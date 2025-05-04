using DrakiaXYZ.BigBrain.Brains;
using EFT;
using SAIN.SAINComponent.Classes.EnemyClasses;
using UnityEngine;

namespace SAIN.Layers.Combat.Solo
{
  internal class MoveToEngageAction : CombatAction, ISAINAction
  {
    private const float TargetPose = 1f;
    private const float TargetMoveSpeed = 1f;
    private const float CoverSearchRadius = 0.5f;
    private const float CoverSearchDistance = 3f;
    private const float EnemyVisibleTimeThreshold = 5f;
    private const float LongDistanceThreshold = 40f;
    private const float LookDirectionSpeed = 500f;
    private const float PathRecalculationInterval = 2f;

    private float RecalcPathTimer;

    public MoveToEngageAction(BotOwner bot) : base(bot, nameof(MoveToEngageAction))
    {
    }

    public void Toggle(bool value)
    {
      ToggleAction(value);
    }

    public override void Update(CustomLayer.ActionData data)
    {
      this.StartProfilingSample("Update");
      Enemy enemy = Bot.Enemy;
      if (enemy == null)
      {
        Bot.Steering.SteerByPriority();
        EndProfilingSample();
        return;
      }

      Bot.Mover.SetTargetPose(TargetPose);
      Bot.Mover.SetTargetMoveSpeed(TargetMoveSpeed);

      if (ShouldShootEnemy(enemy))
      {
        Bot.Steering.SteerByPriority();
        Shoot.CheckAimAndFire();
        EndProfilingSample();
        return;
      }

      Vector3? lastKnown = enemy.KnownPlaces.LastKnownPosition;
      Vector3 movePos;
      if (lastKnown.HasValue)
      {
        movePos = lastKnown.Value;
      }
      else if (enemy.TimeSinceSeen < EnemyVisibleTimeThreshold)
      {
        movePos = enemy.EnemyPosition;
      }
      else
      {
        Bot.Steering.SteerByPriority();
        Shoot.CheckAimAndFire();
        EndProfilingSample();
        return;
      }

      var cover = Bot.Cover.FindPointInDirection(movePos - Bot.Position, CoverSearchRadius, CoverSearchDistance);
      if (cover != null)
      {
        movePos = cover.Position;
      }

      float distance = enemy.RealDistance;
      if (distance > LongDistanceThreshold && !BotOwner.Memory.IsUnderFire)
      {
        TryRecalculatePath(movePos, true);
        EndProfilingSample();
        return;
      }

      Bot.Mover.Sprint(false);
      TryRecalculatePath(movePos, false);

      if (!Bot.Steering.SteerByPriority(lookRandom: false))
      {
        Bot.Steering.LookToMovingDirection();
      }
      EndProfilingSample();
    }

    private bool ShouldShootEnemy(Enemy enemy)
    {
      float distance = enemy.RealDistance;
      bool enemyLookAtMe = enemy.EnemyLookingAtMe;
      float effectiveDistance = Bot.Info.WeaponInfo.EffectiveWeaponDistance;

      if (enemy.IsVisible)
      {
        if (enemyLookAtMe || (distance <= effectiveDistance && enemy.CanShoot))
        {
          return true;
        }
      }
      return false;
    }

    private void TryRecalculatePath(Vector3 movePos, bool sprint)
    {
      if (RecalcPathTimer < Time.time)
      {
        RecalcPathTimer = Time.time + PathRecalculationInterval;
        if (sprint)
        {
          BotOwner.BotRun.Run(movePos, false, SAINPlugin.LoadedPreset.GlobalSettings.General.SprintReachDistance);
          Bot.Steering.LookToMovingDirection(LookDirectionSpeed, true);
        }
        else
        {
          BotOwner.MoveToEnemyData.TryMoveToEnemy(movePos);
        }
      }
    }

    public override void Start()
    {
      Toggle(true);
    }

    public override void Stop()
    {
      Toggle(false);
    }
  }
}
