﻿using EFT;
using SAIN.Components;
using SAIN.Preset.GlobalSettings;
using UnityEngine;

namespace SAIN.SAINComponent.Classes.EnemyClasses
{
  public class EnemyGainSightClass : EnemyBase
  {
    public float GainSightModifier
    {
      get
      {
        if (_nextCheckVisTime < Time.time)
        {
          _nextCheckVisTime = Time.time + 0.05f;
          _gainSightModifier = CalcModifier() * CalcRepeatSeenCoef();
        }
        return _gainSightModifier;
      }
    }

    private const float UNDER_FIRE_FROM_ME_COEF = 0.4f;

    private const float DIST_SEEN_MIN_COEF = 0.01f;
    private const float DIST_SEEN_MIN_DIST = 1f;
    private const float DIST_SEEN_MAX_DIST = 25f;

    private const float DIST_HEARD_MIN_COEF = 0.2f;
    private const float DIST_HEARD_MIN_DIST = 1f;
    private const float DIST_HEARD_MAX_DIST = 20f;

    private const float TIME_MAX_DIST_CLAMP = 200f;
    private const float TIME_MAX_DIST_CLAMP_NVGS = 250f;
    private const float TIME_MIN_DIST_CLAMP = 10f;
    private const float TIME_MIN_DIST_CLAMP_NVGS = 65f;

    private const float ENEMYLIGHT_WHITELIGHT_MOD = 0.75f;
    private const float ENEMYLIGHT_LASER_MOD = 0.95f;
    private const float ENEMYLIGHT_NVGS_IR_LASER_MOD = 0.7f;
    private const float ENEMYLIGHT_NVGS_IR_LIGHT_MOD = 0.85f;

    private const float PARTS_VISIBLE_MIN_DIST = 12.5f;
    private const float PARTS_VISIBLE_MAX_PARTS = 6;
    private const float PARTS_VISIBLE_MIN_PARTS = 2;

    private static float PARTS_VISIBLE_MAX_COEF => Settings.PartsVisibility.PARTS_VISIBLE_MAX_COEF;
    private static float PARTS_VISIBLE_MIN_COEF => Settings.PartsVisibility.PARTS_VISIBLE_MIN_COEF;

    private const float PARTS_VISIBLE_MAX_TIME_SINCE_CHECKED = 2f;
    private const float PARTS_VISIBLE_MAX_TIME_SINCE_VISIBLE = 1f;

    private const float ELEVATION_LASTKNOWN_MAX_DIST = 1.5f;
    private const float ELEVATION_MIN_ANGLE = 5f;

    private static float THIRDPARTY_VISION_START_ANGLE => Settings.ThirdParty.THIRDPARTY_VISION_START_ANGLE;
    private static float THIRDPARTY_VISION_MAX_COEF => Settings.ThirdParty.THIRDPARTY_VISION_MAX_COEF;

    private const float THIRDPARTY_VISION_MAX_DIST_LASTKNOWN = 50f;

    private static float PERIPHERAL_VISION_START_ANGLE => Settings.Peripheral.PERIPHERAL_VISION_START_ANGLE;
    private static float PERIPHERAL_VISION_MAX_REDUCTION_COEF => Settings.Peripheral.PERIPHERAL_VISION_MAX_REDUCTION_COEF;

    private const float PERIPHERAL_VISION_SPEED_DIRECT_FRONT_ANGLE = 3f;
    private const float PERIPHERAL_VISION_SPEED_DIRECT_FRONT_MOD = 0.66f;
    private const float PERIPHERAL_VISION_SPEED_CLOSE_FRONT_ANGLE = 6f;
    private const float PERIPHERAL_VISION_SPEED_CLOSE_FRONT_MOD = 0.8f;
    private const float PERIPHERAL_VISION_SPEED_ENEMY_CLOSE_DIST = 10;
    private const float PERIPHERAL_VISION_SPEED_ENEMY_CLOSE_MOD = 0.9f;
    private const float PERIPHERAL_VISION_SPEED_ENEMY_VERYCLOSE_DIST = 5;
    private const float PERIPHERAL_VISION_SPEED_ENEMY_VERYCLOSE_MOD = 0.8f;

    private static float PRONE_VISION_SPEED_COEF => Settings.Pose.PRONE_VISION_SPEED_COEF;
    private static float DUCK_VISION_SPEED_COEF => Settings.Pose.DUCK_VISION_SPEED_COEF;

    private const float UNKNOWN_ENEMY_HAS_ENEMY_COEF = 1.5f;

    private float CalcUnknownMod()
    {
      if (Enemy.EnemyKnown)
      {
        return 1f;
      }
      if (Enemy.Bot.HasEnemy)
      {
        return UNKNOWN_ENEMY_HAS_ENEMY_COEF;
      }
      return 1f;
    }

    private float CalcModifier()
    {
      float partMod = CalcPartsMod();
      float gearMod = CalcGearMod();

      bool flareEnabled = EnemyPlayer.AIData?.GetFlare == true &&
          Enemy.EnemyPlayerComponent?.Equipment.CurrentWeapon?.HasSuppressor == false;

      bool underFire = Bot.BotOwner.Memory.IsUnderFire && Bot.Memory.LastUnderFireEnemy == Enemy;

      float underFireMod = underFire ? UNDER_FIRE_FROM_ME_COEF : 1f;
      float weatherMod = CalcWeatherMod(flareEnabled);
      float timeMod = CalcTimeModifier(flareEnabled);
      float moveMod = CalcMoveModifier();
      float elevMod = CalcElevationModifier();
      float thirdPartyMod = CalcThirdPartyMod();
      float angleMod = CalcAngleMod();
      float poseMod = PoseModifier();
      float unknownMod = CalcUnknownMod();

      float notLookMod = 1f;
      if (!Enemy.IsAI)
        notLookMod = SAINNotLooking.GetVisionSpeedDecrease(Enemy.EnemyInfo);

      float result =
          1f *
          underFireMod *
          partMod *
          gearMod *
          weatherMod *
          timeMod *
          moveMod *
          elevMod *
          thirdPartyMod *
          angleMod *
          notLookMod *
          unknownMod *
          poseMod;

      //if (EnemyPlayer.IsYourPlayer && result != 1f)
      //{
      //    Logger.LogWarning($"GainSight Time Result: [{result}] : partMod {partMod} : gearMod {gearMod} : flareMod {flareMod} : moveMod {moveMod} : elevMod {elevMod} : posFlareMod {posFlareMod} : thirdPartyMod {thirdPartyMod} : angleMod {angleMod} : notLookMod {notLookMod} ");
      //}

      return result;
    }

    private float PoseModifier()
    {
      if (Settings.Pose.Enabled == false)
      {
        return 1f;
      }
      float result = 1f;
      if (EnemyPlayer.IsInPronePose)
      {
        result *= PRONE_VISION_SPEED_COEF;
      }
      else if (EnemyPlayer.Pose == EPlayerPose.Duck)
      {
        result *= DUCK_VISION_SPEED_COEF;
      }
      return result;
    }

    public EnemyGainSightClass(Enemy enemy) : base(enemy)
    {
    }

    private float CalcRepeatSeenCoef()
    {
      EnemyPlace lastSeen = Enemy.KnownPlaces.LastSeenPlace;
      float result = 1f;
      if (lastSeen != null)
      {
        result *= CalcVisionSpeedPositional(
            lastSeen.DistanceToEnemyRealPosition,
            DIST_SEEN_MIN_COEF,
            DIST_SEEN_MIN_DIST,
            DIST_SEEN_MAX_DIST,
            SeenSpeedCheck.Vision);
      }
      EnemyPlace lastHeard = Enemy.KnownPlaces.LastHeardPlace;
      if (lastHeard != null)
      {
        result *= CalcVisionSpeedPositional(
            lastHeard.DistanceToEnemyRealPosition,
            DIST_HEARD_MIN_COEF,
            DIST_HEARD_MIN_DIST,
            DIST_HEARD_MAX_DIST,
            SeenSpeedCheck.Audio);
      }
      return result;
    }

    private enum SeenSpeedCheck
    {
      None = 0,
      Vision = 1,
      Audio = 2,
    }

    private static float CalcVisionSpeedPositional(float distance, float minSpeedCoef, float minDist, float maxDist, SeenSpeedCheck check)
    {
      if (distance <= minDist)
      {
        return minSpeedCoef;
      }
      if (distance >= maxDist)
      {
        return 1f;
      }

      float num = maxDist - minDist;
      float num2 = distance - minDist;
      float ratio = num2 / num;
      float result = Mathf.Lerp(minSpeedCoef, 1f, ratio);
      //Logger.LogInfo($"{check} Distance from Position: {distance} Result: {result}");
      return result;
    }

    private float _gainSightModifier;
    private float _nextCheckVisTime;

    private float CalcGearMod()
    {
      return Enemy.EnemyPlayerComponent.AIData.AIGearModifier.StealthModifier(Enemy.RealDistance);
    }

    private float CalcTimeModifier(bool flareEnabled)
    {
      float baseModifier = BaseTimeModifier(flareEnabled);

      if (baseModifier <= 1f)
      {
        return 1f;
      }

      if (EnemyUsingLight(out float lightModifier))
      {
        return lightModifier;
      }

      bool usingNVGS = BotOwner.NightVision.UsingNow;
      float enemyDist = Enemy.RealDistance;

      if (EnemyInRangeOfLight(enemyDist, usingNVGS))
        return 1f;

      float max = 1f + baseModifier;
      float min = 1f;
      float maxDist = usingNVGS ? TIME_MAX_DIST_CLAMP_NVGS : TIME_MAX_DIST_CLAMP;
      float minDist = usingNVGS ? TIME_MIN_DIST_CLAMP_NVGS : TIME_MIN_DIST_CLAMP;

      if (enemyDist >= maxDist)
        return max;
      if (enemyDist < minDist)
        return min;

      float enemyVelocity = Enemy.Vision.EnemyVelocity;
      bool moving = enemyVelocity > 0.1f;
      if (!moving)
      {
        max += 1f;
      }

      float num = maxDist - minDist;
      float num2 = enemyDist - minDist;
      float ratio = num2 / num;
      float result = Mathf.Lerp(min, max, ratio);
      return result;
    }

    private bool EnemyUsingLight(out float modifier)
    {
      var flashlight = Enemy.EnemyPlayerComponent.Flashlight;
      if (flashlight.WhiteLight)
      {
        modifier = ENEMYLIGHT_WHITELIGHT_MOD;
        return true;
      }
      if (flashlight.Laser)
      {
        modifier = ENEMYLIGHT_LASER_MOD;
        return true;
      }
      bool usingNVGS = BotOwner.NightVision.UsingNow;
      if (usingNVGS)
      {
        if (flashlight.IRLaser)
        {
          modifier = ENEMYLIGHT_NVGS_IR_LASER_MOD;
          return true;
        }
        if (flashlight.IRLight)
        {
          modifier = ENEMYLIGHT_NVGS_IR_LIGHT_MOD;
          return true;
        }
      }
      modifier = 1f;
      return false;
    }

    private bool EnemyInRangeOfLight(float enemyDist, bool usingNVGS)
    {
      var settings = Bot.Info.FileSettings.Look;
      if (Bot.PlayerComponent.Flashlight.WhiteLight &&
          enemyDist <= settings.VISIBLE_DISNACE_WITH_LIGHT)
      {
        return true;
      }
      if (usingNVGS && Bot.PlayerComponent.Flashlight.IRLight && enemyDist <= settings.VISIBLE_DISNACE_WITH_IR_LIGHT)
      {
        return true;
      }
      return false;
    }

    private float CalcWeatherMod(bool flareEnabled)
    {
      float baseModifier = BaseWeatherMod(flareEnabled);

      if (baseModifier <= 1f)
        return 1f;

      float max = 1f + baseModifier;
      float min = 1f;
      float maxDist = 200f;
      float minDist = 30f;
      float enemyDist = Enemy.RealDistance;

      if (enemyDist >= maxDist)
        return max;
      if (enemyDist < minDist)
        return min;
      if (EnemyUsingLight(out _))
        return min;

      bool moving = Enemy.Vision.EnemyVelocity > 0.1f;
      if (!moving)
        max += 1f;

      float num = maxDist - minDist;
      float num2 = enemyDist - minDist;
      float ratio = num2 / num;
      float result = Mathf.Lerp(min, max, ratio);
      return result;
    }

    private float BaseWeatherMod(bool flareEnabled)
    {
      if (flareEnabled && Enemy.RealDistance < 100f)
      {
        return 1f;
      }
      return SAINBotController.Instance.WeatherVision.GainSightModifier;
    }

    private static float BaseTimeModifier(bool flareEnabled)
    {
      if (flareEnabled)
      {
        return 1f;
      }
      return SAINBotController.Instance.TimeVision.TimeGainSightModifier;
    }

    // private static float _nextLogTime;

    private static VisionSpeedSettings Settings => GlobalSettingsClass.Instance.Look.VisionSpeed;

    private float CalcPartsMod()
    {
      if (Settings.PartsVisibility.Enabled == false)
      {
        return 1f;
      }
      if (Enemy.IsAI)
      {
        return 1f;
      }
      if (Enemy.RealDistance < PARTS_VISIBLE_MIN_DIST)
      {
        return 1f;
      }
      float max = PARTS_VISIBLE_MAX_COEF;
      float min = PARTS_VISIBLE_MIN_COEF;

      float partRatio = GetRatioPartsVisible(out int visibleCount);
      if (visibleCount <= PARTS_VISIBLE_MIN_PARTS)
      {
        return max;
      }
      if (visibleCount >= PARTS_VISIBLE_MAX_PARTS)
      {
        return min;
      }

      if (partRatio >= 1f)
      {
        return min;
      }
      float result = Mathf.Lerp(max, min, partRatio);
      return result;
    }

    private float GetRatioPartsVisible(out int visibleCount)
    {
      int partCount = 0;
      visibleCount = 0;
      var parts = Enemy.Vision.VisionChecker.EnemyParts.Parts.Values;
      foreach (EnemyPartDataClass part in parts)
      {
        if (part.TimeSinceLastVisionCheck > PARTS_VISIBLE_MAX_TIME_SINCE_CHECKED)
          continue;
        partCount++;
        if (part.TimeSinceLastVisionSuccess < PARTS_VISIBLE_MAX_TIME_SINCE_VISIBLE)
          visibleCount++;
      }
      return (float)visibleCount / (float)partCount;
    }

    private float CalcMoveModifier()
    {
      if (Settings.Movement.Enabled == false)
      {
        return 1f;
      }
      var look = SAINPlugin.LoadedPreset.GlobalSettings.Look.VisionSpeed;
      return Mathf.Lerp(1, Settings.Movement.MOVEMENT_VISION_MULTIPLIER, Enemy.Vision.EnemyVelocity);
    }

    private bool IsLastKnownAtSameElev()
    {
      var lastKnown = Enemy.LastKnownPosition;
      if (lastKnown != null)
      {
        Vector3 enemyPosition = EnemyCurrentPosition;
        if (Mathf.Abs(enemyPosition.y - lastKnown.Value.y) < ELEVATION_LASTKNOWN_MAX_DIST)
        {
          return true;
        }
      }
      return false;
    }

    private float CalcElevationModifier()
    {
      if (Settings.Elevation.Enabled == false)
      {
        return 1f;
      }
      if (IsLastKnownAtSameElev())
        return 1f;

      var settings = SAINPlugin.LoadedPreset.GlobalSettings.Look.VisionSpeed.Elevation;
      var angles = Enemy.Vision.Angles;
      float min = ELEVATION_MIN_ANGLE;

      float elevationAngle = angles.AngleToEnemyVertical;
      if (elevationAngle < min)
        return 1f;

      bool enemyAbove = angles.AngleToEnemyVerticalSigned > 0;
      float max = enemyAbove ? settings.HighElevationMaxAngle : settings.LowElevationMaxAngle;
      float targetCoef = enemyAbove ? settings.HighElevationVisionModifier : settings.LowElevationVisionModifier;

      if (elevationAngle > max)
        return targetCoef;

      float num = max - min;
      float diff = elevationAngle - min;
      float ratio = diff / num;
      float result = Mathf.Lerp(1f, targetCoef, ratio);
      return result;
    }

    private float CalcThirdPartyMod()
    {
      if (Settings.ThirdParty.Enabled == false)
      {
        return 1f;
      }
      if (Enemy.IsCurrentEnemy)
      {
        return 1f;
      }
      if (Enemy.EnemyKnown &&
          Enemy.KnownPlaces.EnemyDistanceFromLastKnown > THIRDPARTY_VISION_MAX_DIST_LASTKNOWN)
      {
        return 1f;
      }
      Enemy activeEnemy = Enemy.Bot.Enemy;
      if (activeEnemy == null)
      {
        return 1f;
      }
      Vector3? activeEnemyLastKnown = activeEnemy.LastKnownPosition;
      if (activeEnemyLastKnown == null)
      {
        return 1f;
      }

      Vector3 currentEnemyDir = (activeEnemyLastKnown.Value - Enemy.Bot.Position).normalized;
      currentEnemyDir.y = 0;
      Vector3 myDir = Enemy.EnemyDirectionNormal;
      myDir.y = 0;
      float angle = Vector3.Angle(currentEnemyDir, myDir);

      float minAngle = THIRDPARTY_VISION_START_ANGLE;
      float maxRatio = THIRDPARTY_VISION_MAX_COEF;
      if (angle <= minAngle)
      {
        return 1f;
      }
      float maxAngle = Enemy.Vision.Angles.MaxVisionAngle;
      if (angle >= maxAngle)
      {
        return maxRatio;
      }

      float num = maxAngle - minAngle;
      float num2 = angle - minAngle;
      float ratio = num2 / num;
      float reductionMod = Mathf.Lerp(1f, maxRatio, ratio);

      return reductionMod;
    }

    private float CalcAngleMod()
    {
      if (Settings.Peripheral.Enabled == false)
      {
        return 1f;
      }
      float angle = Enemy.Vision.Angles.AngleToEnemyHorizontal;
      if (angle < PERIPHERAL_VISION_SPEED_DIRECT_FRONT_ANGLE)
      {
        return PERIPHERAL_VISION_SPEED_DIRECT_FRONT_MOD;
      }
      if (angle < PERIPHERAL_VISION_SPEED_CLOSE_FRONT_ANGLE)
      {
        return PERIPHERAL_VISION_SPEED_CLOSE_FRONT_MOD;
      }
      if (Enemy.RealDistance < PERIPHERAL_VISION_SPEED_ENEMY_VERYCLOSE_DIST)
      {
        return PERIPHERAL_VISION_SPEED_ENEMY_VERYCLOSE_MOD;
      }
      if (Enemy.RealDistance < PERIPHERAL_VISION_SPEED_ENEMY_CLOSE_DIST)
      {
        return PERIPHERAL_VISION_SPEED_ENEMY_CLOSE_MOD;
      }
      float minAngle = PERIPHERAL_VISION_START_ANGLE;
      if (angle < minAngle)
      {
        return 1f;
      }
      float maxAngle = Enemy.Vision.Angles.MaxVisionAngle;
      float maxRatio = PERIPHERAL_VISION_MAX_REDUCTION_COEF;
      if (angle > maxAngle)
      {
        return maxRatio;
      }
      float angleDiff = maxAngle - minAngle;
      float enemyAngleDiff = angle - minAngle;
      float ratio = enemyAngleDiff / angleDiff;
      return Mathf.Lerp(1f, maxRatio, ratio);
    }
  }
}
