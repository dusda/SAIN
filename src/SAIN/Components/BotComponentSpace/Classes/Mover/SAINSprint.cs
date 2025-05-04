using EFT;
using SAIN.Helpers;
using SAIN.Preset.GlobalSettings;
using SAIN.SAINComponent;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;

namespace SAIN.Components.BotComponentSpace.Classes.Mover
{
  public enum RunStatus
  {
    None,
    FirstTurn,
    Running,
    Turning,
    ShortCorner,
    NoStamina,
    InteractingWithDoor,
    ArrivingAtDestination,
    CantSprint,
    LookAtEnemyNoSprint,
    Canceling,
  }

  public class SAINSprint : BotBase, IBotClass
  {
    public event Action<Vector3, Vector3> OnNewCornerMoveTo = delegate { };

    public event Action<Vector3, Vector3> OnNewSprint = delegate { };

    public event Action OnSprintEnd = delegate { };

    public SAINSprint(BotComponent sain) : base(sain)
    {
    }

    public void Init()
    {
      base.SubscribeToPreset(null);
    }

    public void Update()
    {
    }

    public bool Running => _runToPointCoroutine != null;

    public bool Canceling { get; set; }

    public void CancelRun(float afterTime = -1f)
    {
      if (Running)
      {
        if (afterTime <= 0)
        {
          StopRunCoroutine();
          return;
        }
        if (!Canceling)
        {
          Canceling = true;
          Bot.StartCoroutine(CancelRunAfterTime(afterTime));
        }
      }
    }

    void StopRunCoroutine()
    {
      if (!Running)
      {
        return;
      }
      OnSprintEnd?.Invoke();
      Canceling = false;
      Bot.StopCoroutine(_runToPointCoroutine);
      _runToPointCoroutine = null;
      Bot.Mover?.Sprint(false);
      _path.Clear();
    }

    IEnumerator CancelRunAfterTime(float afterTime)
    {
      yield return new WaitForSeconds(afterTime);
      StopRunCoroutine();
    }

    public bool RunToPointByWay(NavMeshPath way, ESprintUrgency urgency, bool stopSprintEnemyVisible, bool checkSameWay = true, Action? callback = null)
    {
      if (!GetLastCorner(way, out Vector3 point))
      {
        return false;
      }
      ShallStopSprintWhenSeeEnemy = stopSprintEnemyVisible;
      if (checkSameWay && IsPointSameWay(point))
      {
        return true;
      }
      StartRun(way, point, urgency, callback ?? (() => { }));
      return true;
    }

    static bool GetLastCorner(NavMeshPath way, out Vector3 result)
    {
      Vector3[]? corners = way?.corners;
      if (corners == null)
      {
        result = Vector3.zero;
        return false;
      }
      if (way?.status != NavMeshPathStatus.PathComplete)
      {
        result = Vector3.zero;
        return false;
      }

      Vector3? last = corners.LastElement();
      if (last == null)
      {
        result = Vector3.zero;
        return false;
      }

      result = last.Value;
      return true;
    }

    public bool RunToPoint(Vector3 point, ESprintUrgency urgency, bool stopSprintEnemyVisible, bool checkSameWay = true, Action? callback = null)
    {
      if (checkSameWay && IsPointSameWay(point))
      {
        return true;
      }

      if (Bot.Mover == null || !Bot.Mover.CanGoToPoint(point, out NavMeshPath path))
      {
        return false;
      }
      ShallStopSprintWhenSeeEnemy = stopSprintEnemyVisible;
      StartRun(path, point, urgency, callback ?? (() => { }));
      return true;
    }

    bool ShallStopSprintWhenSeeEnemy;

    bool IsPointSameWay(Vector3 point, float minDistSqr = 0.5f)
    {
      return Running && (LastRunDestination - point).sqrMagnitude < minDistSqr;
    }

    void StartRun(NavMeshPath path, Vector3 point, ESprintUrgency urgency, Action callback)
    {
      StopRunCoroutine();
      BotOwner.AimingManager.CurrentAiming.LoseTarget();
      LastRunDestination = point;
      CurrentPath = path;
      _lastUrgency = urgency;
      _runToPointCoroutine = Bot.StartCoroutine(RunToPointCoroutine(path.corners, urgency, callback));
      OnNewSprint?.Invoke(path.corners[1], point);
    }

    float _timeStartRun;

    ESprintUrgency _lastUrgency;

    public NavMeshPath CurrentPath = new NavMeshPath();

    public bool RecalcPath()
    {
      return RunToPoint(LastRunDestination, _lastUrgency, false);
    }

    public Vector3 LastRunDestination { get; set; }

    Coroutine? _runToPointCoroutine = null;

    public RunStatus CurrentRunStatus { get; set; }

    public Vector3 CurrentCornerDestination()
    {
      if (_path.Count <= _currentIndex)
      {
        return Vector3.zero;
      }
      return _path[_currentIndex];
    }

    int _currentIndex = 0;

    IEnumerator RunToPointCoroutine(Vector3[] corners, ESprintUrgency urgency, Action? callback = null)
    {
      _path.Clear();
      _path.AddRange(corners);

      isShortCorner = false;
      _timeStartCorner = Time.time;
      positionMoving = true;
      _timeNotMoving = -1f;
      _timeStartRun = Time.time;

      BotOwner.Mover.Stop();
      _currentIndex = 1;

      // First step, look towards the path we want to run
      //yield return firstTurn(path.corners[1]);

      // Start running!
      yield return RunPath(urgency);

      callback?.Invoke();

      CurrentRunStatus = RunStatus.None;
      StopRunCoroutine();
    }

    readonly List<Vector3> _path = [];

    void MoveToNextCorner()
    {
      if (TotalCorners() > _currentIndex)
      {
        CheckCornerLength();
        _currentIndex++;
        Vector3 currentCorner = _path[_currentIndex];
        OnNewCornerMoveTo?.Invoke(currentCorner, LastRunDestination);
      }
    }

    void CheckCornerLength()
    {
      Vector3 current = _path[_currentIndex];
      Vector3 next = _path[_currentIndex + 1];
      isShortCorner = (current - next).magnitude < 0.25f;
      _timeStartCorner = Time.time;
    }

    float _timeStartCorner;

    int TotalCorners()
    {
      return _path.Count - 1;
    }

    Vector3 LastCorner()
    {
      int count = _path.Count;
      if (count == 0)
      {
        return Vector3.zero;
      }
      return _path[count - 1];
    }

    static MoveSettings MoveSettings => SAINPlugin.LoadedPreset.GlobalSettings.Move;

    IEnumerator RunPath(ESprintUrgency urgency)
    {
      int total = TotalCorners();
      for (int i = 1; i <= total; i++)
      {
        // Track distance to target corner in the path.
        float distToCurrent = float.MaxValue;
        while (distToCurrent > MoveSettings.BotSprintCornerReachDist)
        {
          distToCurrent = DistanceToCurrentCornerSqr();
          DistanceToCurrentCorner = distToCurrent;

          Vector3 current = CurrentCornerDestination();
          if (SAINPlugin.DebugMode)
          {
            //DebugGizmos.Sphere(current, 0.1f);
            //DebugGizmos.Line(current, Bot.Position, 0.1f, 0.1f);
          }

          // Start or stop sprinting with a buffer
          HandleSprinting(distToCurrent, urgency);

          Vector3 destination = CurrentCornerDestination();
          if (Bot.DoorOpener != null && !Bot.DoorOpener.Interacting &&
              !Bot.DoorOpener.BreachingDoor)
          {
            TrackMovement();
            float timeSinceNoMove = TimeSinceNotMoving;
            if (timeSinceNoMove > MoveSettings.BotSprintRecalcTime && Time.time - _timeStartRun > 2f)
            {
              RecalcPath();
              yield break;
            }
            //else if (timeSinceNoMove > _moveSettings.BotSprintTryJumpTime)
            //{
            //    SAINBot.Mover.TryJump();
            //}
            else if (Bot.Info?.FileSettings?.Move?.VAULT_TOGGLE == true
                && GlobalSettingsClass.Instance?.Move?.VAULT_TOGGLE == true
                && timeSinceNoMove > MoveSettings.BotSprintTryVaultTime)
            {
              Bot.Mover?.TryVault();
            }

            Move((destination - Bot.Position).normalized);
          }

          float speed = IsSprintEnabled ? MoveSettings.BotSprintTurnSpeedWhileSprint : MoveSettings.BotSprintTurningSpeed;
          float dotProduct = Steer(destination, speed);

          //if (onLastCorner() &&
          //    distToCurrent <= _moveSettings.BotSprintFinalDestReachDist)
          //{
          //    yield break;
          //}

          yield return null;
        }
        MoveToNextCorner();
      }
    }

    bool isShortCorner;
    public float DistanceToCurrentCorner { get; set; }

    static float FindStartSprintStamina(ESprintUrgency urgency)
    {
      return urgency switch
      {
        ESprintUrgency.None or ESprintUrgency.Low => 0.75f,
        ESprintUrgency.Middle => 0.5f,
        ESprintUrgency.High => 0.2f,
        _ => 0.5f,
      };
    }

    static float FindEndSprintStamina(ESprintUrgency urgency)
    {
      return urgency switch
      {
        ESprintUrgency.None or ESprintUrgency.Low => 0.4f,
        ESprintUrgency.Middle => 0.2f,
        ESprintUrgency.High => 0.01f,
        _ => 0.25f,
      };
    }

    bool ShallLookAtEnemy()
    {
      return ShallStopSprintWhenSeeEnemy && Bot.Enemy?.IsVisible == true;
    }

    void HandleSprinting(float distToCurrent, ESprintUrgency urgency)
    {
      // I cant sprint :(
      if (!Player.MovementContext.CanSprint)
      {
        CurrentRunStatus = RunStatus.CantSprint;
        return;
      }

      if (Canceling)
      {
        CurrentRunStatus = RunStatus.Canceling;
        Bot.Mover?.EnableSprintPlayer(false);
        return;
      }

      if (ShallLookAtEnemy())
      {
        CurrentRunStatus = RunStatus.LookAtEnemyNoSprint;
        Bot.Mover?.EnableSprintPlayer(false);
        return;
      }

      if (isShortCorner)
      {
        CurrentRunStatus = RunStatus.ShortCorner;
        Bot.Mover?.EnableSprintPlayer(false);
        return;
      }

      if (Player.IsSprintEnabled)
      {
        if (Bot.IsCheater)
        {
          Player.MovementContext.SprintSpeed = 50f;
        }
        else if (MoveSettings.EditSprintSpeed)
        {
          Player.MovementContext.SprintSpeed = 1.5f;
        }
      }

      // Were messing with a door, dont sprint
      if (Bot.DoorOpener != null && Bot.DoorOpener.ShallPauseSprintForOpening())
      {
        CurrentRunStatus = RunStatus.InteractingWithDoor;
        Bot.Mover?.EnableSprintPlayer(false);
        return;
      }

      // We are arriving to our destination, stop sprinting when you get close.
      if ((LastCorner() - BotPosition).magnitude <= MoveSettings.BotSprintDistanceToStopSprintDestination)
      {
        Bot.Mover?.EnableSprintPlayer(false);
        CurrentRunStatus = RunStatus.ArrivingAtDestination;
        return;
      }

      float staminaValue = Player.Physical.Stamina.NormalValue;

      // We are out of stamina, stop sprinting.
      if (ShallPauseSprintStamina(staminaValue, urgency))
      {
        Bot.Mover?.EnableSprintPlayer(false);
        CurrentRunStatus = RunStatus.NoStamina;
        return;
      }

      // We are approaching a sharp corner, or we are currently not looking in the direction we need to go, stop sprinting
      if (ShallPauseSprintAngle())
      {
        Bot.Mover?.EnableSprintPlayer(false);
        CurrentRunStatus = RunStatus.Turning;
        return;
      }

      // If we arne't already sprinting, and our corner were moving to is far enough away, and I have enough stamina, and the angle isn't too sharp... enable sprint
      if (ShallStartSprintStamina(staminaValue, urgency) &&
          _timeStartCorner + 0.25f < Time.time)
      {
        Bot.Mover?.EnableSprintPlayer(true);
        CurrentRunStatus = RunStatus.Running;
        return;
      }
    }

    bool ShallPauseSprintStamina(float stamina, ESprintUrgency urgency) => stamina <= FindEndSprintStamina(urgency);

    bool ShallStartSprintStamina(float stamina, ESprintUrgency urgency) => stamina >= FindStartSprintStamina(urgency);

    bool ShallPauseSprintAngle()
    {
      Vector3? currentCorner = CurrentCornerDestination();
      return currentCorner != null && CheckShallPauseSprintFromTurn(currentCorner.Value, MoveSettings.BotSprintCurrentCornerAngleMax);
    }

    bool CheckShallPauseSprintFromTurn(Vector3 currentCorner, float angleThresh = 25f)
    {
      return FindAngleFromLook(currentCorner) >= angleThresh;
    }

    float FindAngleFromLook(Vector3 end)
    {
      Vector3 origin = BotOwner.WeaponRoot.position;
      Vector3 aDir = Bot.LookDirection;
      Vector3 bDir = end - origin;
      aDir.y = 0;
      bDir.y = 0;
      return Vector3.Angle(aDir, bDir);
    }

    void TrackMovement()
    {
      if (nextCheckPosTime < Time.time)
      {
        nextCheckPosTime = Time.time + MoveSettings.BotSprintNotMovingCheckFreq;
        Vector3 botPos = BotPosition;
        positionMoving = (botPos - lastCheckPos).sqrMagnitude > MoveSettings.BotSprintNotMovingThreshold;
        if (positionMoving)
        {
          _timeNotMoving = -1f;
          lastCheckPos = botPos;
        }
        else if (_timeNotMoving < 0)
        {
          _timeNotMoving = Time.time;
        }
      }
    }

    bool positionMoving;
    Vector3 lastCheckPos;
    float nextCheckPosTime;
    float TimeSinceNotMoving => positionMoving ? 0f : Time.time - _timeNotMoving;
    float _timeNotMoving;

    Vector3 BotPosition
    {
      get
      {
        Vector3 botPos = Bot.Position;
        if (NavMesh.SamplePosition(botPos, out var hit, 0.5f, -1))
        {
          botPos = hit.position;
        }
        return botPos;
      }
    }

    float DistanceToCurrentCornerSqr()
    {
      Vector3 destination = CurrentCornerDestination();
      Vector3 testPoint = destination + Vector3.up;
      if (Physics.Raycast(testPoint, Vector3.down, out var hit, 1.5f, LayerMaskClass.HighPolyWithTerrainMask))
      {
        destination = hit.point;
      }
      return (destination - BotPosition).sqrMagnitude;
    }

    float Steer(Vector3 target, float turnSpeed)
    {
      Vector3 playerPosition = Bot.Position;
      Vector3 currentLookDirNormal = Bot.Person!.Transform.LookDirection.normalized;
      target += Vector3.up;
      Vector3 targetLookDir = target - playerPosition;
      Vector3 targetLookDirNormal = targetLookDir.normalized;

      if (Bot.DoorOpener != null && !Bot.DoorOpener.Interacting)
      {
        if (ShallLookAtEnemy())
        {
          Bot.Steering?.LookToEnemy(Bot.Enemy!);
        }
        else if (!ShallSteerbyPriority() || !Bot.Steering!.SteerByPriority(null, false, true))
        {
          Bot.Steering!.LookToDirection(targetLookDirNormal, true, turnSpeed);
        }
      }
      float dotProduct = Vector3.Dot(targetLookDirNormal, currentLookDirNormal);
      return dotProduct;
    }

    bool ShallSteerbyPriority()
    {
      return CurrentRunStatus switch
      {
        RunStatus.Turning or
        RunStatus.FirstTurn or
        RunStatus.Running or
        RunStatus.ShortCorner => false,
        _ => true,
      };
    }

    void Move(Vector3 direction)
    {
      if (Bot.IsCheater)
      {
        direction *= 10f;
      }
      Player.CharacterController.SetSteerDirection(direction);
      BotOwner.AimingManager.CurrentAiming.Move(Player.Speed);
      if (BotOwner.Mover != null)
      {
        BotOwner.Mover.IsMoving = true;
      }
      Player.Move(FindMoveDirection(direction));
    }

    public Vector2 FindMoveDirection(Vector3 direction)
    {
      Vector3 vector = Quaternion.Euler(0f, 0f, Player.Rotation.x) * new Vector2(direction.x, direction.z);
      return new Vector2(vector.x, vector.y);
    }

    public void Dispose()
    {
    }

    bool IsSprintEnabled => Player.IsSprintEnabled;
  }

  public enum ESprintUrgency
  {
    None = 0,
    Low = 1,
    Middle = 2,
    High = 3,
  }
}
