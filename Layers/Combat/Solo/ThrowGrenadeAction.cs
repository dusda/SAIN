﻿using EFT;
using System.Collections;
using UnityEngine;
using UnityEngine.Profiling;

namespace SAIN.Layers.Combat.Solo
{
    public class ThrowGrenadeAction : CombatAction, ISAINAction
    {
        public ThrowGrenadeAction(BotOwner bot) : base(bot, nameof(ThrowGrenadeAction))
        {
        }

        public void Toggle(bool value)
        {
            ToggleAction(value);
        }

        public override void Update()
        {
            this.StartProfilingSample("Update");
            if (!Stopped && Time.time - StartTime > 1f || Bot.Cover.CheckLimbsForCover()) {
                Stopped = true;
                BotOwner.StopMove();
            }
            this.EndProfilingSample();
        }

        private float StartTime = 0f;
        private bool Stopped = false;

        public override void Start()
        {
            StartTime = Time.time;
            Toggle(true);
            if (Bot.Squad.BotInGroup && Bot.Talk.GroupTalk.FriendIsClose) {
                Bot.Talk.Say(EPhraseTrigger.OnGrenade);
            }
        }

        public override void Stop()
        {
            Toggle(false);
        }
    }
}