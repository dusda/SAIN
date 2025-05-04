using SAIN.Components;
using SAIN.Components.BotComponentSpace;
using SAIN.SAINComponent.SubComponents.CoverFinder;
using System.Collections;
using UnityEngine;

namespace SAIN.BotController.Classes
{
  public class GlobalCoverfinder : SAINControllerBase
  {
    public GlobalCoverfinder(SAINBotController botController) : base(botController)
    {
    }

    public void FindCoverForBots()
    {
      if (_findCoverCoroutine == null)
      {
        _findCoverCoroutine = BotController.StartCoroutine(Main());
      }
    }

    private Coroutine _findCoverCoroutine;

    private static IEnumerator Main()
    {
      while (true)
      {
        yield return null;
      }
    }

    public void Dispose()
    {
      if (_findCoverCoroutine != null)
      {
        BotController.StopCoroutine(_findCoverCoroutine);
        _findCoverCoroutine = null;
      }
    }

    private readonly List<BotComponent> _localBotList = [];
    public readonly Dictionary<int, CoverObject> ActiveCoverObjects = [];
  }

  public class CoverObject
  {
    public int id;
    public Collider? collider;
    public Dictionary<string, CoverPoint> BotCoverPointDictionary = [];
  }
}
