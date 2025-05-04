﻿using Newtonsoft.Json;
using SAIN.Attributes;
using SAIN.Helpers;
using SAIN.Preset.GlobalSettings.Categories;
using SAIN.Preset.Personalities;
using static SAIN.Helpers.JsonUtility;

namespace SAIN.Preset.GlobalSettings
{
  public class GlobalSettingsClass : SettingsGroupBase<GlobalSettingsClass>
  {
    [Hidden]
    [JsonIgnore]
    public static GlobalSettingsClass Instance;

    public GlobalSettingsClass()
    {
      Instance = this;
    }

    public static GlobalSettingsClass ImportGlobalSettings(SAINPresetDefinition Preset)
    {
      string fileName = FileAndFolderNames[JsonEnum.GlobalSettings];
      string presetsFolder = FileAndFolderNames[JsonEnum.Presets];

      if (!Load.LoadObject(out GlobalSettingsClass result, fileName, presetsFolder, Preset.Name))
      {
        result = new GlobalSettingsClass();
        SaveObjectToJson(result, fileName, presetsFolder, Preset.Name);
      }
      return result;
    }

    public override void Init()
    {
      InitList();
      CreateDefaults();
      Update();
    }

    public DifficultySettings Difficulty = new();

    public GeneralSettings General = new();

    public AimSettings Aiming = new();

    public HearingSettings Hearing = new();

    public LocationSettingsClass Location = new();

    public LookSettings Look = new();

    public MindSettings Mind = new();

    public MoveSettings Move = new();

    public SteeringSettings Steering = new();

    public ShootSettings Shoot = new();

    public TalkSettings Talk = new();

    [Name("Squad Talk")]
    public SquadTalkSettings SquadTalk = new();

    [Name("Power Level Calculation")]
    public PowerCalcSettings PowerCalc = new();

    public override void InitList()
    {
      SettingsList.Clear();

      Difficulty.Init(SettingsList);
      General.Init(SettingsList);
      Aiming.Init(SettingsList);
      Hearing.Init(SettingsList);
      Location.Init(SettingsList);
      Look.Init(SettingsList);
      Mind.Init(SettingsList);
      Move.Init(SettingsList);
      Shoot.Init(SettingsList);
      Talk.Init(SettingsList);
      SquadTalk.Init(SettingsList);
      PowerCalc.Init(SettingsList);
      Steering.Init(SettingsList);
    }
  }
}
