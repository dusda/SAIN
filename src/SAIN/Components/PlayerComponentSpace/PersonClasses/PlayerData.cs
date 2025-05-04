﻿using EFT;

namespace SAIN.Components.PlayerComponentSpace.PersonClasses
{
  public class PlayerData
  {
    public ProfileData Profile { get; }
    public PlayerComponent PlayerComponent { get; }
    public IPlayer IPlayer { get; }
    public Player Player { get; }

    public PlayerData(PlayerComponent component, Player player, IPlayer iPlayer)
    {
      PlayerComponent = component;
      Player = player;
      IPlayer = iPlayer;
      Profile = new ProfileData(player);
    }
  }
}
