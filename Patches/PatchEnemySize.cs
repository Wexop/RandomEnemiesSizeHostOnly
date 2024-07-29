using HarmonyLib;
using Unity.Netcode;
using UnityEngine;

namespace RandomEnemiesSize.Patches
{
    [HarmonyPatch(typeof(EnemyAI))]
    internal class PatchEnemySize
    {
        [HarmonyPatch("Start")]
        [HarmonyPostfix]
        private static void PatchStart(EnemyAI __instance)
        {
            if (!__instance.IsServer || !__instance.IsOwner) return;

            var isVannila = new VanillaEnemies().IsAVanillaEnemy(__instance.enemyType.enemyName);
            
            if(__instance.enemyType.enemyName == "Red Locust Bees") return;

            if (isVannila && !RandomEnemiesSizeHostOnly.instance.CustomAffectVanillaEntry.Value ||
                !isVannila && !RandomEnemiesSizeHostOnly.instance.CustomAffectModEntry.Value) return;

            //RANDOM PERCENT
            var randomPercent = Random.Range(0f, 100f);

            if (RandomEnemiesSizeHostOnly.instance.randomPercentChanceEntry.Value < randomPercent)
            {
                if (RandomEnemiesSizeHostOnly.instance.devLogEntry.Value)
                    Debug.Log(
                        $"RANDOM PERCENT NOT RANDOM SIZE : {randomPercent} FOR ENEMY {__instance.gameObject.name}");
                return;
            }


            var scale = Random.Range(RandomEnemiesSizeHostOnly.instance.minSizeOutdoorEntry.Value,
                RandomEnemiesSizeHostOnly.instance.maxSizeOutdoorEntry.Value);

            if (!__instance.isOutside)
                scale = Random.Range(RandomEnemiesSizeHostOnly.instance.minSizeIndoorEntry.Value,
                    RandomEnemiesSizeHostOnly.instance.maxSizeIndoorEntry.Value);

            var customEnemy = RandomEnemiesSizeHostOnly.instance.GetCustomEnemySize(__instance.enemyType.enemyName);
            if (customEnemy.found) scale = Random.Range(customEnemy.minValue, customEnemy.maxValue);


            if (RandomEnemiesSizeHostOnly.instance.LethalLevelLoaderIsHere)
            {
                var interiorName = RandomEnemiesSizeHostOnly.GetDungeonName();
                //Debug.Log($"ACTUAL DUNGEON NAME {interiorName}");
                if (!__instance.isOutside && interiorName != null)
                {
                    var interiorMult = RandomEnemiesSizeHostOnly.instance.GetInteriorMultiplier(__instance.enemyType.enemyName,
                        interiorName);
                    //Debug.Log($"BEFORE INTERIOR MULT, SCALE IS {scale} AND MULT IS {interiorMult}");
                    scale *= interiorMult;

                    //Debug.Log($"AFTER INTERIOR MULT, SCALE IS {scale} ");
                }
            }

            var originalScale = __instance.gameObject.transform.localScale;
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
            
            __instance.gameObject.GetComponent<NetworkObject>().Despawn(false);
            //change size
            __instance.gameObject.transform.localScale = newScale;
            __instance.gameObject.GetComponent<NetworkObject>().Spawn();
            


            if (RandomEnemiesSizeHostOnly.instance.devLogEntry.Value)
                Debug.Log($"ENEMY ({__instance.gameObject.name}) SPAWNED WITH RANDOM SIZE {newScale.ToString()}");
        }
    }
}