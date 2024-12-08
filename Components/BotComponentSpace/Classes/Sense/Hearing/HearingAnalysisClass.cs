﻿using EFT;
using SAIN.Helpers;
using SAIN.Preset;
using SAIN.Preset.GlobalSettings;
using UnityEngine;

namespace SAIN.SAINComponent.Classes
{
    public class HearingAnalysisClass : BotSubClass<SAINHearingSensorClass>, IBotClass
    {
        private const float BUNKER_REDUCTION_COEF = 0.2f;
        private const float BUNKER_ELEV_DIFF_COEF = 0.66f;
        private const float GUNSHOT_OCCLUSION_MOD = 0.8f;
        private const float FOOTSTEP_OCCLUSION_MOD = 0.8f;
        private const float GUNSHOT_ENVIR_MOD = 0.65f;
        private const float FOOTSTEP_ENVIR_MOD = 0.7f;
        private const float MIN_ENVIRONMENT_MOD = 0.05f;

        private const float HEAR_MODIFIER_NO_EARS = 0.6f;
        private const float HEAR_MODIFIER_HEAVY_HELMET = 0.8f;
        private const float HEAR_MODIFIER_DYING = 0.8f;
        private const float HEAR_MODIFIER_SPRINT = 0.85f;
        private const float HEAR_MODIFIER_HEAVYBREATH = 0.65f;

        private const float HEAR_MODIFIER_MIN_CLAMP = 0.01f;
        private const float HEAR_MODIFIER_MAX_CLAMP = 5f;
        private const float HEAR_MODIFIER_MAX_AFFECT_DIST = 3f;

        private const float HEAR_CHANCE_MIN_DIST = 0.25f;
        private const float HEAR_CHANCE_MIN_DIST_HEADPHONES = 1;
        private const float HEAR_CHANCE_MIDRANGE_COEF = 0.66f;
        private const float HEAR_CHANCE_MIDRANGE_MINCHANCE_HEADPHONES = 3;
        private const float HEAR_CHANCE_LONGRANGE_MINCHANCE_HEADPHONES = 1;
        private const float HEAR_CHANCE_NOTMOVING_VELOCITY = 0.05f;
        private const float HEAR_CHANCE_NOTMOVING_MINCHANCE = 2;
        private const float HEAR_CHANCE_NOTMOVING_MINCHANCE_HEADPHONES = 4;
        private const float HEAR_CHANCE_HEADPHONES_OTHERSOUNDS = 3;
        private const float HEAR_CHANCE_CURRENTENEMY_MINCHANCE = 2;
        private const float HEAR_CHANCE_CURRENTENEMY_MINCHANCE_HEADPHONES = 3;

        public HearingAnalysisClass(SAINHearingSensorClass hearing) : base(hearing)
        {
        }

        public void Init()
        {
            base.SubscribeToPreset(updateSettings);
        }

        public void Update()
        {
        }

        public void Dispose()
        {
        }

        public bool CheckIfSoundHeard(BotSound sound)
        {
            if (shallLimitAI(sound)) {
                sound.Results.LimitedByAI = true;
                return false;
            }
            if (!doIDetectFootsteps(sound)) {
                return false;
            }

            bool farFromPlayer = checkDistToPlayer(sound);
            sound.Results.SoundFarFromPlayer = farFromPlayer;
            if (farFromPlayer) {
                return false;
            }

            sound.Range.FinalRange = sound.Range.BaseRange * calcModifiers(sound);
            if (sound.Distance > sound.Range.FinalRange) {
                return false;
            }

            if (!sound.Enemy.Player.IsAI) {
                //Logger.LogDebug($"Heard Sound : Final Range [{sound.Range.FinalRange}] : Modifier {sound.Range.Modifiers.FinalModifier}");
            }
            return true;
        }

        private float calcModifiers(BotSound sound)
        {
            var mods = sound.Range.Modifiers;
            mods.EnvironmentModifier = calcEnvironmentMod(sound);
            mods.ConditionModifier = calcConditionMod(sound);
            mods.OcclusionModifier = calcOcclusionMod(sound);
            mods.FinalModifier = mods.CalcFinalModifier(HEAR_MODIFIER_MIN_CLAMP, HEAR_MODIFIER_MAX_CLAMP) * Bot.Info.Difficulty.HearingDistanceModifier;
            return mods.FinalModifier;
        }

        private bool checkDistToPlayer(BotSound sound)
        {
            // The sound originated from somewhere far from the player's position, typically from a grenade explosion, which is handled elsewhere
            return (sound.Info.Position - sound.Info.SourcePlayer.Position).sqrMagnitude > 5f * 5f;
        }

        private float calcBunkerVolumeReduction(BotSound sound)
        {
            var botLocation = Bot.PlayerComponent.AIData.PlayerLocation;
            var enemyLocation = sound.Info.SourcePlayer.AIData.PlayerLocation;

            bool botinBunker = botLocation.InBunker;
            bool playerinBunker = enemyLocation.InBunker;
            if (botinBunker != playerinBunker) {
                return BUNKER_REDUCTION_COEF;
            }
            if (botinBunker) {
                float diff = Mathf.Abs(botLocation.BunkerDepth - enemyLocation.BunkerDepth);
                if (diff > 0) {
                    return BUNKER_ELEV_DIFF_COEF;
                }
            }
            return 1f;
        }

        private bool doIDetectFootsteps(BotSound sound)
        {
            if (sound.Info.IsGunShot) {
                return true;
            }

            bool hasheadPhones = Bot.PlayerComponent.Equipment.GearInfo.HasEarPiece;
            float closehearing = hasheadPhones ? HEAR_CHANCE_MIN_DIST_HEADPHONES : HEAR_CHANCE_MIN_DIST;
            float distance = sound.Distance;
            if (distance <= closehearing) {
                return true;
            }

            float farhearing = hasheadPhones ? SAINPlugin.LoadedPreset.GlobalSettings.Hearing.MaxFootstepAudioDistance : SAINPlugin.LoadedPreset.GlobalSettings.Hearing.MaxFootstepAudioDistanceNoHeadphones;
            if (distance > farhearing) {
                return false;
            }

            float minimumChance = 0f;
            if (hasheadPhones) {
                if (distance < farhearing * HEAR_CHANCE_MIDRANGE_COEF) {
                    minimumChance += HEAR_CHANCE_MIDRANGE_MINCHANCE_HEADPHONES;
                }
                else {
                    minimumChance += HEAR_CHANCE_LONGRANGE_MINCHANCE_HEADPHONES;
                }
                if (sound.Info.SoundType != SAINSoundType.FootStep) {
                    minimumChance += HEAR_CHANCE_HEADPHONES_OTHERSOUNDS;
                }
            }

            if (Bot.PlayerComponent.Transform.VelocityMagnitudeNormal < HEAR_CHANCE_NOTMOVING_VELOCITY) {
                minimumChance += hasheadPhones ? HEAR_CHANCE_NOTMOVING_MINCHANCE_HEADPHONES : HEAR_CHANCE_NOTMOVING_MINCHANCE;
            }

            if (Bot.HasEnemy &&
                Bot.Enemy.EnemyProfileId == sound.Info.SourcePlayer.ProfileId) {
                minimumChance += hasheadPhones ? HEAR_CHANCE_CURRENTENEMY_MINCHANCE_HEADPHONES : HEAR_CHANCE_CURRENTENEMY_MINCHANCE;
            }

            float num = farhearing - closehearing;
            float num2 = distance - closehearing;
            float chanceToHear = 1f - num2 / num;
            chanceToHear *= 100f;

            chanceToHear = Mathf.Clamp(chanceToHear, minimumChance, 100f);
            sound.Results.ChanceToHear = chanceToHear;
            if (!sound.Enemy.Player.IsAI) {
                //Logger.LogDebug($"chanceToHear [{chanceToHear}] : minChance [{minimumChance}] : distance [{distance}]");
            }
            return EFTMath.RandomBool(chanceToHear);
        }

        private float calcOcclusionMod(BotSound sound)
        {
            bool isGunshot = sound.Info.IsGunShot;
            if (sound.Enemy.InLineOfSight) {
                sound.Results.VisibleSource = true;
                return 1f;
            }
            return isGunshot ? GUNSHOT_OCCLUSION_MOD : FOOTSTEP_OCCLUSION_MOD;
        }

        private float calcEnvironmentMod(BotSound sound)
        {
            if (Player.AIData.EnvironmentId == sound.Info.SourcePlayer.Player.AIData.EnvironmentId) {
                return 1f;
            }
            float envMod = sound.Info.IsGunShot ? GUNSHOT_ENVIR_MOD : FOOTSTEP_ENVIR_MOD;
            float bunkerMod = calcBunkerVolumeReduction(sound);
            float result = envMod * bunkerMod;
            result = Mathf.Clamp(result, MIN_ENVIRONMENT_MOD, 1f);
            return result;
        }

        private bool shallLimitAI(BotSound sound)
        {
            if (!sound.Info.IsAI)
                return false;

            var aiLimit = GlobalSettingsClass.Instance.General.AILimit;
            if (!aiLimit.LimitAIvsAIGlobal)
                return false;

            if (!aiLimit.LimitAIvsAIHearing)
                return false;

            var enemyPlayer = sound.Info.SourcePlayer;
            if (Bot.Enemy?.EnemyProfileId == enemyPlayer.ProfileId)
                return false;

            var enemyBot = enemyPlayer.BotComponent;
            float maxRange;
            if (enemyBot == null) {
                if (enemyPlayer.BotOwner?.Memory.GoalEnemy?.ProfileId == Bot.ProfileId) {
                    return false;
                }
                maxRange = getMaxRange(Bot.CurrentAILimit);
            }
            else {
                if (enemyBot.Enemy?.EnemyProfileId == Bot.ProfileId) {
                    return false;
                }
                maxRange = getMaxRange(enemyBot.CurrentAILimit);
            }

            if (sound.Distance <= maxRange) {
                return false;
            }
            return true;
        }

        private float getMaxRange(AILimitSetting aiLimit)
        {
            switch (aiLimit) {
                case AILimitSetting.Far:
                    return _farDistance;

                case AILimitSetting.VeryFar:
                    return _veryFarDistance;

                case AILimitSetting.Narnia:
                    return _narniaDistance;

                default:
                    return float.MaxValue;
            }
        }

        private float calcConditionMod(BotSound sound)
        {
            var mods = sound.Range.Modifiers;
            float dist = sound.Distance * mods.EnvironmentModifier;
            //float maxAffectDist = HEAR_MODIFIER_MAX_AFFECT_DIST;
            // if (dist <= maxAffectDist) {
            //     return 1f;
            // }

            float modifier = 1f;
            float? currentHearSense = BotOwner?.Settings?.Current?.CurrentHearingSense;
            if (currentHearSense != null) {
                modifier *= currentHearSense.Value;
            }
            if (!sound.Info.IsGunShot) {
                modifier *= GlobalSettings.Hearing.FootstepAudioMultiplier;
            }
            else {
                modifier *= GlobalSettings.Hearing.GunshotAudioMultiplier;
            }
            modifier *= Bot.Info.FileSettings.Core.HearingDistanceMulti;

            //if (dist * modifier < maxAffectDist)
            //    return 1f;

            //  && sound.Distance > HEAR_MODIFIER_MAX_AFFECT_DIST
            if (sound.Info.SoundType != SAINSoundType.Shot) {
                if (!Bot.PlayerComponent.Equipment.GearInfo.HasEarPiece) {
                    modifier *= HEAR_MODIFIER_NO_EARS;
                }
                if (Bot.PlayerComponent.Equipment.GearInfo.HasHeavyHelmet) {
                    modifier *= HEAR_MODIFIER_HEAVY_HELMET;
                }
                if (Bot.Memory.Health.Dying &&
                    !Bot.Memory.Health.OnPainKillers) {
                    modifier *= HEAR_MODIFIER_DYING;
                }
                if (Player.IsSprintEnabled) {
                    modifier *= HEAR_MODIFIER_SPRINT;
                }
                if (Player.HeavyBreath) {
                    modifier *= HEAR_MODIFIER_HEAVYBREATH;
                }
            }

            //if (dist * modifier < maxAffectDist)
            //    return 1f;

            return modifier;
        }

        private void updateSettings(SAINPresetClass preset)
        {
            int frame = Time.frameCount;
            if (_lastCalcFrame == frame) {
                return;
            }
            _lastCalcFrame = frame;
            var maxHeadRanges = preset.GlobalSettings.General.AILimit.MaxHearingRanges;
            _farDistance = maxHeadRanges[AILimitSetting.Far];
            _veryFarDistance = maxHeadRanges[AILimitSetting.VeryFar];
            _narniaDistance = maxHeadRanges[AILimitSetting.Narnia];
        }

        private static int _lastCalcFrame;
        private static float _farDistance;
        private static float _veryFarDistance;
        private static float _narniaDistance;
    }
}