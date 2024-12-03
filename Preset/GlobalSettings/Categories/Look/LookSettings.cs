﻿using RootMotion.FinalIK;
using SAIN.Attributes;
using SAIN.Helpers;
using System.Threading.Tasks;
using System;
using UnityEngine.UIElements.Experimental;
using System.Collections.Generic;

namespace SAIN.Preset.GlobalSettings
{
    public class LookSettings : SAINSettingsBase<LookSettings>, ISAINSettings
    {
        [Category("Core Settings")]
        [Name("Vision Speed Settings")]
        public VisionSpeedSettings VisionSpeed = new VisionSpeedSettings();

        [Category("Core Settings")]
        [Name("Vision Distance Settings")]
        public VisionDistanceSettings VisionDistance = new VisionDistanceSettings();

        [Category("Core Settings")]
        [Name("Time Settings")]
        public TimeSettings Time = new TimeSettings();

        [Category("Core Settings")]
        [Name("Flashlights and NVGs Settings")]
        public LightNVGSettings Light = new LightNVGSettings();

        [Category("Extra")]
        [Name("Not Looking At Bot Settings")]
        public NotLookingSettings NotLooking = new NotLookingSettings();

        [Category("Extra")]
        [Name("No Bush ESP")]
        public NoBushESPSettings NoBushESP = new NoBushESPSettings();

        public override void Init(List<ISAINSettings> list)
        {
            VisionSpeed.Init(list);
            list.Add(VisionDistance);
            list.Add(NotLooking);
            list.Add(NoBushESP);
            list.Add(Time);
            list.Add(Light);
        }
    }
}