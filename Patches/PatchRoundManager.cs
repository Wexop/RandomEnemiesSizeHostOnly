using HarmonyLib;
using UnityEngine;

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
            Debug.Log($"NETWORK OBJECT DESPAWNING {networkObject.name}");
            networkObject.Despawn();
        }

        RandomEnemiesSizeHostOnly.instance.mapHazardsInLevel.Clear();
    }
}