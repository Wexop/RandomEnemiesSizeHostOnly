using HarmonyLib;
using Unity.Netcode;
using UnityEngine;

namespace RandomEnemiesSize.Patches
{
    [HarmonyPatch(typeof(Landmine))]
    public class PatchLandmineSize
    {
        [HarmonyPatch("Start")]
        [HarmonyPostfix]
        private static void PatchStart(Landmine __instance)
        {
            if (!__instance.IsServer || !__instance.IsOwner) return;

            if (!RandomEnemiesSizeHostOnly.instance.customAffectMineEntry.Value) return;

            //RANDOM PERCENT
            var randomPercent = Random.Range(0f, 100f);

            if (RandomEnemiesSizeHostOnly.instance.randomPercentChanceEntry.Value < randomPercent)
            {
                if (RandomEnemiesSizeHostOnly.instance.devLogEntry.Value)
                    Debug.Log(
                        $"RANDOM PERCENT NOT RANDOM SIZE : {randomPercent} FOR ENEMY {__instance.gameObject.name}");
                return;
            }

            var scale = Random.Range(RandomEnemiesSizeHostOnly.instance.minSizeMineEntry.Value,
                RandomEnemiesSizeHostOnly.instance.maxSizeMineEntry.Value);

            var networkObject = __instance.gameObject.GetComponentInParent<NetworkObject>();

            var originalScale = networkObject.transform.localScale;
            var newScale = originalScale * scale;

            if (RandomEnemiesSizeHostOnly.instance.funModeEntry.Value)
            {
                var funXSize = Random.Range(RandomEnemiesSizeHostOnly.instance.funModeHorizontalMinEntry.Value,
                    RandomEnemiesSizeHostOnly.instance.funModeHorizontalMaxEntry.Value);
                var funZSize = Random.Range(RandomEnemiesSizeHostOnly.instance.funModeHorizontalMinEntry.Value,
                    RandomEnemiesSizeHostOnly.instance.funModeHorizontalMaxEntry.Value);

                if (RandomEnemiesSizeHostOnly.instance.funModeLockHorizontalEnrty.Value) funZSize = funXSize;

                newScale = new Vector3(newScale.x * funXSize, newScale.y, newScale.z * funZSize);
            }

            RandomEnemiesSizeHostOnly.instance.mapHazardsInLevel.Add(__instance.NetworkObject);

            __instance.NetworkObject.SynchronizeTransform = true;
            __instance.gameObject.transform.parent.localScale = newScale;
            __instance.NetworkObject.Despawn(false);
            //change size

            __instance.NetworkObject.Spawn();
        }
    }
}