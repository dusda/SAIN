﻿using EFT;
using HarmonyLib;
using SAIN.Components;
using SAIN.Components.BotControllerSpace.Classes;
using SAIN.Helpers;
using SPT.Reflection.Patching;
using System.Reflection;
using UnityEngine;
using EFTSettingsLoadClass = GClass598;

namespace SAIN.Patches.Components
{
  internal class AddBotComponentPatch : ModulePatch
  {
    protected override MethodBase GetTargetMethod()
    {
      return AccessTools.Method(typeof(BotOwner), nameof(BotOwner.method_10));
    }

    [PatchPostfix]
    public static void PatchPostfix(ref BotOwner __instance)
    {
      try
      {
        if (__instance.BotState != EBotState.ActiveFail)
        {
          BotSpawnController.Instance.AddBot(__instance);
        }
        else
        {
          Logger.LogDebug($"{__instance.name} failed EFT Init, skipping adding SAIN components");
        }
      }
      catch (Exception ex)
      {
        Logger.LogError($" SAIN Add Bot Error: {ex}");
      }
    }
  }

  internal class AddGameWorldPatch : ModulePatch
  {
    protected override MethodBase GetTargetMethod()
    {
      return AccessTools.Method(typeof(GameWorldUnityTickListener), nameof(GameWorldUnityTickListener.Create));
    }

    [PatchPostfix]
    public static void PatchPostfix(GameObject gameObject, GameWorld gameWorld)
    {
      if (gameWorld is HideoutGameWorld)
        return;

      try
      {
        GameWorldHandler.Create(gameObject);
      }
      catch (Exception ex)
      {
        Logger.LogError($" SAIN Init Gameworld Error: {ex}");
      }
    }
  }

  internal class GetBotController : ModulePatch
  {
    protected override MethodBase GetTargetMethod()
    {
      return AccessTools.Method(typeof(BotsController), nameof(BotsController.method_0));
    }

    [PatchPrefix]
    public static void PatchPrefix(BotsController __instance)
    {
      var controller = SAINBotController.Instance;
      if (controller != null && controller.DefaultController == null)
      {
        controller.DefaultController = __instance;
      }
    }
  }

  internal class GetBotSpawner : ModulePatch
  {
    protected override MethodBase GetTargetMethod()
    {
      return AccessTools.Method(typeof(BotSpawner), nameof(BotSpawner.AddPlayer));
    }

    [PatchPostfix]
    public static void PatchPostfix(BotSpawner __instance)
    {
      var controller = SAINBotController.Instance;
      if (controller != null && controller.BotSpawner == null)
      {
        controller.BotSpawner = __instance;
      }
    }
  }

  internal class UpdateCoreSettingsPatch : ModulePatch
  {
    protected override MethodBase GetTargetMethod()
    {
      return AccessTools.Method(typeof(EFTSettingsLoadClass), nameof(EFTSettingsLoadClass.Load));
    }

    [PatchPostfix]
    public static void Patch()
    {
      try
      {
        EFTCoreSettings.UpdateCoreSettings();
      }
      catch (Exception e)
      {
        Logger.LogError(e);
      }
    }
  }
}
