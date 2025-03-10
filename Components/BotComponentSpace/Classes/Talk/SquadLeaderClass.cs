namespace SAIN.SAINComponent.Classes.Talk
{
    public class SquadLeaderClass : BotBase
    {
        public SquadLeaderClass(BotComponent owner) : base(owner)
        {
        }

        public void Init()
        {
        }

        public void Update()
        {
            if (!Bot.BotActive || Bot.GameEnding)
            {
                return;
            }

            if (!Bot.Squad.BotInGroup)
            {
                return;
            }
        }

        public void Dispose()
        {
        }
    }
}
