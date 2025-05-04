using DrakiaXYZ.BigBrain.Brains;
using SAIN.Layers;
using SAIN.Layers.Combat.Run;
using SAIN.Layers.Combat.Solo;
using SAIN.Layers.Combat.Squad;
using SAIN.Preset.GlobalSettings;
using SAIN.Preset.GlobalSettings.Categories;

namespace SAIN
{
  public class BigBrainHandler
  {
    public static void Init()
    {
      BrainAssignment.Init();
    }

    public static bool BigBrainInitialized;

    public class BrainAssignment
    {
      public static void Init()
      {
        HandlePMCandRaiders();
        HandleScavs();
        HandleRogues();
        HandleBloodHounds();
        HandleBosses();
        HandleFollowers();
        HandleGoons();
        HandleOthers();
      }

      private static void HandlePMCandRaiders()
      {
        var settings = SAINPlugin.LoadedPreset.GlobalSettings.General.Layers;
        List<string> pmcBrain = [Brain.PMC.ToString()];

        BrainManager.AddCustomLayer(typeof(DebugLayer), pmcBrain, 99);
        BrainManager.AddCustomLayer(typeof(SAINAvoidThreatLayer), pmcBrain, 80);
        BrainManager.AddCustomLayer(typeof(ExtractLayer), pmcBrain, settings.SAINExtractLayerPriority);
        BrainManager.AddCustomLayer(typeof(CombatSquadLayer), pmcBrain, settings.SAINCombatSquadLayerPriority);
        BrainManager.AddCustomLayer(typeof(CombatSoloLayer), pmcBrain, settings.SAINCombatSoloLayerPriority);

        List<string> LayersToRemove =
                [
                    "Help",
                    "AdvAssaultTarget",
                    "Hit",
                    "Simple Target",
                    "Pmc",
                    "AssaultHaveEnemy",
                    "Request",
                    //"FightReqNull",
                    //"PeacecReqNull",
                    "Assault Building",
                    "Enemy Building",
                    "KnightFight",
                    //"PtrlBirdEye",
                    "PmcBear",
                    "PmcUsec",
                ];
        CheckExtractEnabled(LayersToRemove);
        BrainManager.RemoveLayers(LayersToRemove, pmcBrain);
      }

      private static void HandleScavs()
      {
        if (_vanillaBotSettings.VanillaScavs)
        {
          return;
        }

        List<string> brainList = GetBrainList(AIBrains.Scavs);
        var settings = SAINPlugin.LoadedPreset.GlobalSettings.General.Layers;

        //BrainManager.AddCustomLayer(typeof(BotUnstuckLayer), stringList, 98);
        BrainManager.AddCustomLayer(typeof(DebugLayer), brainList, 99);
        BrainManager.AddCustomLayer(typeof(SAINAvoidThreatLayer), brainList, 80);
        BrainManager.AddCustomLayer(typeof(ExtractLayer), brainList, settings.SAINExtractLayerPriority);
        BrainManager.AddCustomLayer(typeof(CombatSquadLayer), brainList, settings.SAINCombatSquadLayerPriority);
        BrainManager.AddCustomLayer(typeof(CombatSoloLayer), brainList, settings.SAINCombatSoloLayerPriority);

        List<string> LayersToRemove =
                [
                    "Help",
                    "AdvAssaultTarget",
                    "Hit",
                    "Simple Target",
                    "Pmc",
                    //"FightReqNull",
                    //"PeacecReqNull",
                    "AssaultHaveEnemy",
                    "Assault Building",
                    "Enemy Building",
                    "PmcBear",
                    "PmcUsec",
                ];
        CheckExtractEnabled(LayersToRemove);
        BrainManager.RemoveLayers(LayersToRemove, brainList);
      }

      private static void HandleOthers()
      {
        List<string> brainList = GetBrainList(AIBrains.Others);

        var settings = SAINPlugin.LoadedPreset.GlobalSettings.General.Layers;
        //BrainManager.AddCustomLayer(typeof(BotUnstuckLayer), stringList, 98);
        BrainManager.AddCustomLayer(typeof(DebugLayer), brainList, 99);
        BrainManager.AddCustomLayer(typeof(SAINAvoidThreatLayer), brainList, 80);
        BrainManager.AddCustomLayer(typeof(ExtractLayer), brainList, settings.SAINExtractLayerPriority);
        BrainManager.AddCustomLayer(typeof(CombatSquadLayer), brainList, settings.SAINCombatSquadLayerPriority);
        BrainManager.AddCustomLayer(typeof(CombatSoloLayer), brainList, settings.SAINCombatSoloLayerPriority);

        List<string> LayersToRemove =
                [
                    "Help",
                    "AdvAssaultTarget",
                    "Hit",
                    "Simple Target",
                    "Pmc",
                    "AssaultHaveEnemy",
                    "Request",
                    //"FightReqNull",
                    //"PeacecReqNull",
                    "Assault Building",
                    "Enemy Building",
                    "KnightFight",
                    //"PtrlBirdEye",
                    "PmcBear",
                    "PmcUsec",
                ];
        CheckExtractEnabled(LayersToRemove);
        BrainManager.RemoveLayers(LayersToRemove, brainList);
      }

      private static void HandleRogues()
      {
        if (_vanillaBotSettings.VanillaRogues)
        {
          return;
        }

        List<string> brainList = [Brain.ExUsec.ToString()];

        var settings = SAINPlugin.LoadedPreset.GlobalSettings.General.Layers;
        //BrainManager.AddCustomLayer(typeof(BotUnstuckLayer), stringList, 98);
        BrainManager.AddCustomLayer(typeof(DebugLayer), brainList, 99);
        BrainManager.AddCustomLayer(typeof(SAINAvoidThreatLayer), brainList, 80);
        BrainManager.AddCustomLayer(typeof(ExtractLayer), brainList, settings.SAINExtractLayerPriority);
        BrainManager.AddCustomLayer(typeof(CombatSquadLayer), brainList, settings.SAINCombatSquadLayerPriority);
        BrainManager.AddCustomLayer(typeof(CombatSoloLayer), brainList, settings.SAINCombatSoloLayerPriority);

        List<string> LayersToRemove =
                [
                    "Help",
                    "AdvAssaultTarget",
                    "Hit",
                    "Simple Target",
                    "Pmc",
                    "AssaultHaveEnemy",
                    "Request",
                    //"FightReqNull",
                    //"PeacecReqNull",
                    "Assault Building",
                    "Enemy Building",
                    "KnightFight",
                    //"PtrlBirdEye",
                    "PmcBear",
                    "PmcUsec",
                ];
        CheckExtractEnabled(LayersToRemove);
        BrainManager.RemoveLayers(LayersToRemove, brainList);
      }

      private static void HandleBloodHounds()
      {
        if (_vanillaBotSettings.VanillaBloodHounds)
        {
          return;
        }

        List<string> brainList = [Brain.ArenaFighter.ToString()];

        var settings = SAINPlugin.LoadedPreset.GlobalSettings.General.Layers;
        //BrainManager.AddCustomLayer(typeof(BotUnstuckLayer), stringList, 98);
        BrainManager.AddCustomLayer(typeof(DebugLayer), brainList, 99);
        BrainManager.AddCustomLayer(typeof(SAINAvoidThreatLayer), brainList, 80);
        BrainManager.AddCustomLayer(typeof(ExtractLayer), brainList, settings.SAINExtractLayerPriority);
        BrainManager.AddCustomLayer(typeof(CombatSquadLayer), brainList, settings.SAINCombatSquadLayerPriority);
        BrainManager.AddCustomLayer(typeof(CombatSoloLayer), brainList, settings.SAINCombatSoloLayerPriority);

        List<string> LayersToRemove =
                [
                    "Help",
                    "AdvAssaultTarget",
                    "Hit",
                    "Simple Target",
                    "Pmc",
                    "AssaultHaveEnemy",
                    "Request",
                    //"FightReqNull",
                    //"PeacecReqNull",
                    "Assault Building",
                    "Enemy Building",
                    "KnightFight",
                    //"PtrlBirdEye",
                    "PmcBear",
                    "PmcUsec",
                ];
        CheckExtractEnabled(LayersToRemove);
        BrainManager.RemoveLayers(LayersToRemove, brainList);
      }

      private static void HandleBosses()
      {
        if (_vanillaBotSettings.VanillaBosses)
        {
          return;
        }

        List<string> brainList = GetBrainList(AIBrains.Bosses);

        var settings = SAINPlugin.LoadedPreset.GlobalSettings.General;
        //BrainManager.AddCustomLayer(typeof(BotUnstuckLayer), stringList, 98);
        BrainManager.AddCustomLayer(typeof(DebugLayer), brainList, 99);
        BrainManager.AddCustomLayer(typeof(SAINAvoidThreatLayer), brainList, 80);
        BrainManager.AddCustomLayer(typeof(CombatSquadLayer), brainList, 70);
        BrainManager.AddCustomLayer(typeof(CombatSoloLayer), brainList, 69);

        List<string> LayersToRemove =
                [
                    "Help",
                    "AdvAssaultTarget",
                    "Hit",
                    "Simple Target",
                    "Pmc",
                    "AssaultHaveEnemy",
                    "Assault Building",
                    "Enemy Building",
                    "KnightFight",
                    "BirdEyeFight",
                    "BossBoarFight"
                ];
        CheckExtractEnabled(LayersToRemove);
        BrainManager.RemoveLayers(LayersToRemove, brainList);
      }

      private static void HandleFollowers()
      {
        if (_vanillaBotSettings.VanillaFollowers)
        {
          return;
        }

        List<string> brainList = GetBrainList(AIBrains.Followers);

        var settings = SAINPlugin.LoadedPreset.GlobalSettings.General;
        //BrainManager.AddCustomLayer(typeof(BotUnstuckLayer), stringList, 98);
        BrainManager.AddCustomLayer(typeof(DebugLayer), brainList, 99);
        BrainManager.AddCustomLayer(typeof(SAINAvoidThreatLayer), brainList, 80);
        BrainManager.AddCustomLayer(typeof(CombatSquadLayer), brainList, 70);
        BrainManager.AddCustomLayer(typeof(CombatSoloLayer), brainList, 69);

        List<string> LayersToRemove =
                [
                    "Help",
                    "AdvAssaultTarget",
                    "Hit",
                    "Simple Target",
                    "Pmc",
                    "AssaultHaveEnemy",
                    "Assault Building",
                    "Enemy Building",
                    "KnightFight",
                    "BoarGrenadeDanger"
                ];
        CheckExtractEnabled(LayersToRemove);
        BrainManager.RemoveLayers(LayersToRemove, brainList);
      }

      private static void HandleGoons()
      {
        if (_vanillaBotSettings.VanillaGoons)
        {
          return;
        }

        List<string> brainList = GetBrainList(AIBrains.Goons);

        BrainManager.AddCustomLayer(typeof(DebugLayer), brainList, 99);
        BrainManager.AddCustomLayer(typeof(SAINAvoidThreatLayer), brainList, 80);
        BrainManager.AddCustomLayer(typeof(CombatSquadLayer), brainList, 64);
        BrainManager.AddCustomLayer(typeof(CombatSoloLayer), brainList, 62);

        List<string> LayersToRemove =
                [
                    "Help",
                    "AdvAssaultTarget",
                    "Hit",
                    "Simple Target",
                    "Pmc",
                    "AssaultHaveEnemy",
                    //"FightReqNull",
                    //"PeacecReqNull",
                    "Assault Building",
                    "Enemy Building",
                    "KnightFight",
                    "BirdEyeFight",
                    "Kill logic"
                ];
        CheckExtractEnabled(LayersToRemove);
        BrainManager.RemoveLayers(LayersToRemove, brainList);
      }

      private static void CheckExtractEnabled(List<string> layersToRemove)
      {
        if (GlobalSettingsClass.Instance.General.Extract.SAIN_EXTRACT_TOGGLE)
        {
          layersToRemove.Add("Exfiltration");
        }
      }

      private static List<string> GetBrainList(List<Brain> brains)
      {
        List<string> brainList = [];
        for (int i = 0; i < brains.Count; i++)
        {
          brainList.Add(brains[i].ToString());
        }
        return brainList;
      }

      private static VanillaBotSettings _vanillaBotSettings => SAINPlugin.LoadedPreset.GlobalSettings.General.VanillaBots;
    }
  }
}
