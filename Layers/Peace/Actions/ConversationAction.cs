﻿using DrakiaXYZ.BigBrain.Brains;
using EFT;

namespace SAIN.Layers.Peace
{
    internal class ConversationAction : CombatAction, ISAINAction
    {
        public ConversationAction(BotOwner bot) : base(bot, "Extract")
        {
        }

        public override void Start()
        {
            Toggle(true);
        }

        public override void Stop()
        {
            Toggle(false);
        }

        public override void Update(CustomLayer.ActionData data)
        {
            this.StartProfilingSample("Update");
            this.EndProfilingSample();
        }

        public void Toggle(bool value)
        {
            ToggleAction(value);
        }
    }
}