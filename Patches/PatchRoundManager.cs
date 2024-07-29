using HarmonyLib;

namespace RandomEnemiesSize.Patches;

[HarmonyPatch(typeof(RoundManager))]
public class PatchRoundManager
{
    [HarmonyPatch(nameof(RoundManager.LoadNewLevel))]
    [HarmonyPrefix]
    public static void PatchLoadLevel()
    {
        foreach (var networkObject in RandomEnemiesSizeHostOnly.instance.mapHazardsInLevel)
        {
            networkObject.Despawn();
        }

        RandomEnemiesSizeHostOnly.instance.mapHazardsInLevel.Clear();
    }
}