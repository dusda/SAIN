﻿using Newtonsoft.Json;
using SAIN.Attributes;

namespace SAIN.Preset.GlobalSettings
{

  public abstract class SAINSettingsBase<T> : ISAINSettings
  {
    public virtual void Update()
    {
    }

    public object GetDefaults()
    {
      return Defaults;
    }

    public void CreateDefault()
    {
      Defaults = (T)Activator.CreateInstance(typeof(T));
    }

    public void UpdateDefaults(object values)
    {
      CloneSettingsClass.CopyFields(values, Defaults);
    }

    [Hidden]
    [JsonIgnore]
    public T? Defaults;

    public virtual void Init(List<ISAINSettings> list)
    {
    }
  }
}
