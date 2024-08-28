using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace NoSharkyBoy
{
    [BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
    public class Plugin : BaseUnityPlugin
    {
        internal static new ManualLogSource Logger;

        private void Awake()
        {
            Logger = base.Logger;
            Logger.LogInfo($"Plugin {MyPluginInfo.PLUGIN_GUID} is loaded!");

            Harmony.CreateAndPatchAll(typeof(FishAIPatch));
        }
    }

    [HarmonyPatch(typeof(FishAI))]
    [HarmonyPatch("Spawned")]
    public class FishAIPatch
    {
        private static List<string> badFishies = new List<string>()
        {
            "shark",
            "marlin"
        };

        private static GameObject turtleGameObject;
        private static List<FishAI> delayedObjectPool = new List<FishAI>();
        private static GameObject GetFishSpawner()
        {
            if (turtleGameObject != null)
            {
                return turtleGameObject;
            }

            GameObject turtle = GameObject.Find("Sea Turtle_1");
            if (turtle == null)
                return default;

            FishSpawner spawner = turtle.GetComponent<FishSpawner>();
            if (spawner == null)
            {
                return default;
            }

            turtleGameObject = spawner.FishPrefa.gameObject;

            if (delayedObjectPool.Count > 0)
            {
                foreach (FishAI ai in delayedObjectPool)
                {
                    replaceDisplay(ai);
                }
            }

            return turtleGameObject;
        }

        private static void replaceDisplay(FishAI fish)
        {
            Plugin.Logger.LogInfo($"We FishAI {fish.gameObject.name}!!!!!!!!!!!");
            SkinnedMeshRenderer[] renderers = fish.GetComponentsInChildren<SkinnedMeshRenderer>();

            foreach (SkinnedMeshRenderer meshFilter in renderers)
            {
                meshFilter.gameObject.SetActive(false);
            }

            GameObject newDisplayObject = GameObject.Instantiate(turtleGameObject);

            newDisplayObject.transform.SetParent(fish.transform, false);
            newDisplayObject.transform.localPosition = Vector3.zero;
            newDisplayObject.transform.localEulerAngles = Vector3.zero;

            SkinnedMeshRenderer newmeshRenderer = newDisplayObject.GetComponentInChildren<SkinnedMeshRenderer>();
            if (newmeshRenderer != null)
            {
                newmeshRenderer.material.color = new Color(1.2f, .5f, .5f);
            }
        }

        static void Prefix(FishAI __instance)
        {
            if (!badFishies.Where(s => __instance.gameObject.name.ToLower().Contains(s)).Any())
            {
                return;
            }

            GameObject turtle = GetFishSpawner();
            if (turtle == null)
            {
                delayedObjectPool.Add(__instance);
                return;
            }

            replaceDisplay(__instance);
        }
    }
}