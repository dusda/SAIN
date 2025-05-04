using EFT;
using SAIN.Components.PlayerComponentSpace.PersonClasses;
using SAIN.Helpers;
using SAIN.Models.Enums;
using SAIN.Preset.GlobalSettings;
using SAIN.Preset.GlobalSettings.Categories;
using SAIN.SAINComponent;
using SAIN.SAINComponent.Classes;
using SAIN.SAINComponent.Classes.Debug;
using SAIN.SAINComponent.Classes.Decision;
using SAIN.SAINComponent.Classes.EnemyClasses;
using SAIN.SAINComponent.Classes.Info;
using SAIN.SAINComponent.Classes.Memory;
using SAIN.SAINComponent.Classes.Mover;
using SAIN.SAINComponent.Classes.Search;
using SAIN.SAINComponent.Classes.Talk;
using SAIN.SAINComponent.Classes.WeaponFunction;
using UnityEngine;

namespace SAIN.Components.BotComponentSpace;

public class BotComponent : BotComponentBase
{
  private const float CheaterMoveSpeed = 350f;
  private const float CheaterSprintSpeed = 50f;
  private const float CheaterSpeedLimit = 100f;
  private const float DefaultMaxShootDist = float.MaxValue;

  private readonly Dictionary<Type, IBotClass> _botClasses = [];
  private float _defaultMoveSpeed;
  private float _defaultSprintSpeed;

  public bool IsCheater { get; private set; }

  public bool BotActive => BotActivation?.BotActive ?? false;
  public bool BotInStandBy => BotActivation?.BotInStandBy ?? false;
  public AILimitSetting CurrentAILimit => AILimit?.CurrentAILimit ?? default;

  public bool HasEnemy => EnemyController?.ActiveEnemy?.EnemyPerson?.Active == true;
  public bool HasLastEnemy => EnemyController?.LastEnemy?.EnemyPerson?.Active == true;
  public Enemy? Enemy => HasEnemy ? EnemyController?.ActiveEnemy : null;
  public Enemy? LastEnemy => HasLastEnemy ? EnemyController?.LastEnemy : null;

  public Vector3? CurrentTargetPosition => CurrentTarget?.CurrentTargetPosition;
  public Vector3? CurrentTargetDirection => CurrentTarget?.CurrentTargetDirection;
  public float CurrentTargetDistance => CurrentTarget?.CurrentTargetDistance ?? 0f;

  public BotGlobalEventsClass? GlobalEvents { get; private set; }
  public BotBusyHandsDetector? BusyHandsDetector { get; private set; }
  public ShootDeciderClass? Shoot { get; private set; }
  public BotWeightManagement? WeightManagement { get; private set; }
  public SAINBotMedicalClass? Medical { get; private set; }
  public SAINActivationClass? BotActivation { get; private set; }
  public DoorOpener? DoorOpener { get; private set; }
  public ManualShootClass? ManualShoot { get; private set; }
  public CurrentTargetClass? CurrentTarget { get; private set; }
  public BotBackpackDropClass? BackpackDropper { get; private set; }
  public BotLightController? BotLight { get; private set; }
  public SAINBotSpaceAwareness? SpaceAwareness { get; private set; }
  public AimDownSightsController? AimDownSightsController { get; private set; }
  public SAINAILimit? AILimit { get; private set; }
  public SAINBotSuppressClass? Suppression { get; private set; }
  public SAINVaultClass? Vault { get; private set; }
  public SAINSearchClass? Search { get; private set; }
  public SAINMemoryClass? Memory { get; private set; }
  public SAINEnemyController? EnemyController { get; private set; }
  public SAINNoBushESP? NoBushESP { get; private set; }
  public SAINFriendlyFireClass? FriendlyFire { get; private set; }
  public SAINVisionClass? Vision { get; private set; }
  public SAINMoverClass? Mover { get; private set; }
  public SAINBotUnstuckClass? BotStuck { get; private set; }
  public SAINHearingSensorClass? Hearing { get; private set; }
  public SAINBotTalkClass? Talk { get; private set; }
  public SAINDecisionClass? Decision { get; private set; }
  public SAINCoverClass? Cover { get; private set; }
  public SAINBotInfoClass? Info { get; private set; }
  public SAINSquadClass? Squad { get; private set; }
  public SAINSelfActionClass? SelfActions { get; private set; }
  public BotGrenadeManager? Grenade { get; private set; }
  public SAINSteeringClass? Steering { get; private set; }
  public AimClass? Aim { get; private set; }

  public bool ShallExecuteRequests
  {
    get
    {
      var currRequest = BotOwner?.BotRequestController?.CurRequest;
      if (currRequest == null || currRequest.Requester == null)
      {
        return false;
      }
      if (HasEnemy && currRequest.Requester.IsAI)
      {
        return false;
      }
      return true;
    }
  }

  public bool IsDead => Person?.ActivationClass?.IsAlive == false;
  public bool GameEnding => BotActivation?.GameEnding ?? false;
  public bool SAINLayersActive => BotActivation?.SAINLayersActive ?? false;

  public float DistanceToAimTarget
  {
    get
    {
      return BotOwner?.AimingManager?.CurrentAiming?.LastDist2Target ?? CurrentTargetDistance;
    }
  }

  public float LastCheckVisibleTime;

  public ESAINLayer ActiveLayer
  {
    get
    {
      return BotActivation?.ActiveLayer ?? default;
    }
    set
    {
      BotActivation?.SetActiveLayer(value);
    }
  }

  public void Update()
  {
    BotActivation?.Update();
    if (!BotActive)
    {
      return;
    }

    AILimit?.Update();
    CurrentTarget?.Update();
    EnemyController?.Update();
    BotStuck?.Update();
    Decision?.Update();
    GlobalEvents?.Update();

    if (BotInStandBy)
    {
      return;
    }

    Info?.Update();
    BusyHandsDetector?.Update();
    WeightManagement?.Update();
    DoorOpener?.Update();
    Aim?.Update();
    Search?.Update();
    Memory?.Update();
    FriendlyFire?.Update();
    Vision?.Update();
    Mover?.Update();
    Hearing?.Update();
    Talk?.Update();
    Cover?.Update();
    Squad?.Update();
    SelfActions?.Update();
    Grenade?.Update();
    Steering?.Update();
    Vault?.Update();
    Suppression?.Update();
    AimDownSightsController?.Update();
    SpaceAwareness?.Update();
    Medical?.Update();
    BotLight?.Update();
    ManualShoot?.Update();
    Shoot?.Update();

    HandleBotCheaterBehavior();
  }

  public bool InitializeBot(PersonClass person)
  {
    if (!base.Init(person))
    {
      return false;
    }
    if (!CreateClasses(person))
    {
      return false;
    }
    if (!AddToSquad())
    {
      return false;
    }
    if (!InitClasses())
    {
      return false;
    }
    return FinishInit();
  }

  private bool CreateClasses(PersonClass person)
  {
    try
    {
      // Must be first, other classes use it
      Info = InitBotClass<SAINBotInfoClass>();

      NoBushESP = gameObject.AddComponent<SAINNoBushESP>();

      Squad =
          InitBotClass<SAINSquadClass>();
      BusyHandsDetector =
          InitBotClass<BotBusyHandsDetector>();
      GlobalEvents =
          InitBotClass<BotGlobalEventsClass>();
      Shoot =
          InitBotClass<ShootDeciderClass>();
      WeightManagement =
          InitBotClass<BotWeightManagement>();
      Memory =
          InitBotClass<SAINMemoryClass>();
      BotStuck =
          InitBotClass<SAINBotUnstuckClass>();
      Hearing =
          InitBotClass<SAINHearingSensorClass>();
      Talk =
          InitBotClass<SAINBotTalkClass>();
      Decision =
          InitBotClass<SAINDecisionClass>();
      Cover =
          InitBotClass<SAINCoverClass>();
      SelfActions =
          InitBotClass<SAINSelfActionClass>();
      Steering =
          InitBotClass<SAINSteeringClass>();
      Grenade =
          InitBotClass<BotGrenadeManager>();
      Mover =
          InitBotClass<SAINMoverClass>();
      EnemyController =
          InitBotClass<SAINEnemyController>();
      FriendlyFire =
          InitBotClass<SAINFriendlyFireClass>();
      Vision =
          InitBotClass<SAINVisionClass>();
      Search =
          InitBotClass<SAINSearchClass>();
      Vault =
          InitBotClass<SAINVaultClass>();
      Suppression =
          InitBotClass<SAINBotSuppressClass>();
      AILimit =
          InitBotClass<SAINAILimit>();
      AimDownSightsController =
          InitBotClass<AimDownSightsController>();
      SpaceAwareness =
          InitBotClass<SAINBotSpaceAwareness>();
      DoorOpener =
          InitBotClass<DoorOpener>();
      Medical =
          InitBotClass<SAINBotMedicalClass>();
      BotLight =
          InitBotClass<BotLightController>();
      BackpackDropper =
          InitBotClass<BotBackpackDropClass>();
      CurrentTarget =
          InitBotClass<CurrentTargetClass>();
      ManualShoot =
          InitBotClass<ManualShootClass>();
      BotActivation =
          InitBotClass<SAINActivationClass>();
      Aim =
          InitBotClass<AimClass>();
    }
    catch (Exception ex)
    {
      Logger.LogError($"Error When Creating Classes, Disposing... : {ex}");
      return false;
    }
    return true;
  }

  private T InitBotClass<T>() where T : BotBase
  {
    T botClass = (T)Activator.CreateInstance(typeof(T), this)!
        ?? throw new InvalidOperationException($"Failed to create an instance of {typeof(T)}.");
    var botClassInterface = botClass as IBotClass ?? throw new InvalidOperationException($"Failed to cast {typeof(T)} to IBotClass.");
    _botClasses.Add(typeof(T), botClassInterface);
    return botClass;
  }

  private bool AddToSquad()
  {
    try
    {
      if (Squad?.SquadInfo != null)
      {
        Squad.SquadInfo.AddMember(this);
      }
      else
      {
        Logger.LogError("Squad or SquadInfo is null, cannot add member to squad.");
        return false;
      }
    }
    catch (Exception ex)
    {
      Logger.LogError($"Error adding member to squad!: {ex}");
      return false;
    }
    return true;
  }

  private bool InitClasses()
  {
    try
    {
      if (NoBushESP != null && Person?.AIInfo?.BotOwner != null)
      {
        NoBushESP.Init(Person.AIInfo.BotOwner, this);
      }
    }
    catch (Exception ex)
    {
      Logger.LogError($"Error When Initializing Components, Disposing... : {ex}");
      return false;
    }
    foreach (var botClass in _botClasses)
    {
      try
      {
        botClass.Value.Init();
      }
      catch (Exception ex)
      {
        Logger.LogError($"Error When Initializing Class [{botClass.Key}], Disposing... : {ex}");
        return false;
      }
    }
    return true;
  }

  private bool FinishInit()
  {
    try
    {
      if (Person != null && !VerifyBrain(Person))
      {
        Logger.LogError("Init SAIN ERROR, Disposing...");
        return false;
      }

      try
      {
        BotOwner.LookSensor.MaxShootDist = DefaultMaxShootDist;
        if (BotOwner.AIData is GClass567 aiData)
        {
          aiData.IsNoOffsetShooting = false;
        }
      }
      catch (Exception ex)
      {
        Logger.LogError($"Error setting MaxShootDist during init, but continuing with initialization...: {ex}");
      }

      try
      {
        var settings = GlobalSettingsClass.Instance?.General?.Jokes;
        if (settings == null)
        {
          Logger.LogWarning("GlobalSettingsClass.Instance.General.Jokes is null, skipping cheater initialization.");
          return true;
        }
        if (settings.RandomCheaters &&
            (EFTMath.RandomBool(settings.RandomCheaterChance) || Player.Profile.Nickname.ToLower().Contains("solarint")))
        {
          IsCheater = true;
        }
      }
      catch (Exception ex)
      {
        Logger.LogWarning($"Error when initializing dumb shit for this bot, continuing anyways since its some dumb shit. Error: {ex}");
      }
    }
    catch (Exception ex)
    {
      Logger.LogError($"Error When Finishing Bot Initialization, Disposing... : {ex}");
      return false;
    }
    return true;
  }

  private bool VerifyBrain(PersonClass person)
  {
    if (Info != null && Info.Profile != null && Info.Profile.IsPMC &&
        person.AIInfo?.BotOwner?.Brain?.BaseBrain?.ShortName() != Brain.PMC.ToString())
    {
      Logger.LogAndNotifyError($"{BotOwner.name} is a PMC but does not have [PMC] Base Brain! Current Brain Assignment: [{person.AIInfo!.BotOwner!.Brain.BaseBrain.ShortName()}] : SAIN Server mod is either missing or another mod is overwriting it. Destroying SAIN for this bot...");
      return false;
    }
    return true;
  }

  private void OnDisable()
  {
    BotActivation?.SetActive(false);
    StopAllCoroutines();
  }

  public void LateUpdate()
  {
    BotActivation?.LateUpdate();
    EnemyController?.LateUpdate();
  }

  private void HandleBotCheaterBehavior()
  {
    if (!IsCheater) return;

    if (_defaultMoveSpeed == 0)
    {
      _defaultMoveSpeed = Player.MovementContext?.MaxSpeed ?? 0;
      _defaultSprintSpeed = Player.MovementContext?.SprintSpeed ?? 0;
    }

    if (Player.Grounder != null)
    {
      Player.Grounder.enabled = Enemy == null;
    }

    if (Enemy != null)
    {
      SetPlayerSpeed(CheaterMoveSpeed, CheaterSprintSpeed, CheaterSpeedLimit);
    }
    else
    {
      SetPlayerSpeed(_defaultMoveSpeed, _defaultSprintSpeed, _defaultMoveSpeed);
    }
  }

  private void SetPlayerSpeed(float moveSpeed, float sprintSpeed, float speedLimit)
  {
    if (Player.MovementContext != null)
    {
      if (Player.MovementContext != null)
      {
        Player.MovementContext?.SetCharacterMovementSpeed(moveSpeed, false);
      }
      if (Player.MovementContext != null)
      {
        Player.MovementContext.SprintSpeed = sprintSpeed;
      }
      Player.ChangeSpeed(speedLimit);
      Player.UpdateSpeedLimit(speedLimit, Player.ESpeedLimit.SurfaceNormal);
      Player.MovementContext?.ChangeSpeedLimit(speedLimit, Player.ESpeedLimit.SurfaceNormal);
    }
    BotOwner?.SetTargetMoveSpeed(speedLimit);
  }

  public override void Dispose()
  {
    base.Dispose();
    BotActivation?.SetActive(false);
    StopAllCoroutines();

    foreach (var botClass in _botClasses)
    {
      try
      {
        botClass.Value.Dispose();
      }
      catch (Exception ex)
      {
        Logger.LogError($"Dispose Class [{botClass.Key}] Error: {ex}");
      }
    }

    if (NoBushESP != null)
    {
      Destroy(NoBushESP);
    }

    if (BotOwner != null)
    {
      BotOwner.OnBotStateChange -= ResetBot;
    }

    Destroy(this);
  }

  private void ResetBot(EBotState state)
  {
    Decision?.ResetDecisions(false);
  }
}
