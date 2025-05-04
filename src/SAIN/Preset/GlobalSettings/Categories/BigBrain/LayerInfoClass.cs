using EFT;

namespace SAIN.Preset.GlobalSettings.Categories
{
  public sealed class LayerInfoClass
  {
    public string? Name;
    public bool ConvertedToString;
    public string? Description;
    public Dictionary<Brain, int> UsedByBrains = [];
    public WildSpawnType[]? UsedByWildSpawns;
  }
}
