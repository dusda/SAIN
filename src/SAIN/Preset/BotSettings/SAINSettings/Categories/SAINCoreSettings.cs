﻿using Newtonsoft.Json;
using SAIN.Attributes;
using SAIN.Preset.GlobalSettings;

namespace SAIN.Preset.BotSettings.SAINSettings.Categories
{
  public class SAINCoreSettings : SAINSettingsBase<SAINCoreSettings>, ISAINSettings
  {
    [Category("Vision")]
    [Name("Field of View")]
    [MinMax(45f, 180f)]
    [CopyValue]
    public float VisibleAngle = 160f;

    [Category("Vision")]
    [Name("Base Vision Distance")]
    [MinMax(50f, 500f)]
    [CopyValue]
    public float VisibleDistance = 150f;

    [Category("Vision")]
    [Name("Gain Sight Coeficient")]
    [Description("Default EFT Config. Affects how quickly this bot will notice their enemies. Small changes to this have dramatic affects on bot vision speed.")]
    [MinMax(0.001f, 0.999f, 10000f)]
    [Advanced]
    [CopyValue]
    public float GainSightCoef = 0.2f;

    [Category("Aim and Shoot")]
    [Name("Accuracy Speed")]
    [Description("Default EFT Config. Affects how quickly this bot will aim at targets.")]
    [MinMax(0.01f, 0.95f, 100f)]
    [Advanced]
    [CopyValue]
    public float AccuratySpeed = 0.3f;

    [Category("Aim and Shoot")]
    [Description("Default EFT Config. I do not know what this does exactly.")]
    [MinMax(0.001f, 1f, 1000f)]
    [Advanced]
    [CopyValue]
    public float ScatteringPerMeter = 0.08f;

    [Category("Aim and Shoot")]
    [Description("Default EFT Config. I do not know what this does exactly.")]
    [MinMax(0.001f, 1f, 1000f)]
    [Advanced]
    [CopyValue]
    public float ScatteringClosePerMeter = 0.12f;

    [Category("Hearing")]
    [Name("Hearing Distance Multiplier")]
    [Description("Modifies the distance that this bot can hear sounds")]
    [MinMax(0.1f, 3f, 1000f)]
    public float HearingDistanceMulti = 1f;

    [Name("Can Use Grenades")]
    public bool CanGrenade = true;

    [Hidden]
    [JsonIgnore]
    public bool CanRun = true;

    [Hidden]
    [JsonIgnore]
    public float DamageCoeff = 1f;
  }
}
