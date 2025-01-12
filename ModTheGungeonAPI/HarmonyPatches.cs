﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Dungeonator;
using HarmonyLib;
using UnityEngine;

[HarmonyPatch]
internal class HarmonyPatches
{
    // aiactor patches
    [HarmonyPatch(typeof(AIActor), nameof(AIActor.Start))]
    [HarmonyPrefix]
    private static void AIActor_Start_Prefix(AIActor __instance)
    {
        ETGMod.AIActor.OnPreStart?.Invoke(__instance);
    }

    [HarmonyPatch(typeof(AIActor), nameof(AIActor.Start))]
    [HarmonyPostfix]
    private static void AIActor_Start_Postfix(AIActor __instance)
    {
        ETGMod.AIActor.OnPostStart?.Invoke(__instance);
    }

    [HarmonyPatch(typeof(AIActor), nameof(AIActor.CheckForBlackPhantomness))]
    [HarmonyPrefix]
    private static void AIActor_CheckForBlackPhantomness(AIActor __instance)
    {
        ETGMod.AIActor.OnBlackPhantomnessCheck?.Invoke(__instance);
    }

    // chest patches
    [HarmonyPatch(typeof(Chest), nameof(Chest.Spawn), typeof(Chest), typeof(Vector3), typeof(RoomHandler), typeof(bool))]
    [HarmonyPostfix]
    private static void Chest_Spawn(Chest __instance)
    {
        ETGMod.Chest.OnPostSpawn?.Invoke(__instance);
    }

    [HarmonyPatch(typeof(Chest), nameof(Chest.Open))]
    [HarmonyPrefix]
    private static bool Chest_Open_Prefix(Chest __instance, ref bool __state, PlayerController player)
    {
        return __state = ETGMod.Chest.OnPreOpen.RunHook(true, new object[]
        {
            __instance,
            player
        });
    }

    [HarmonyPatch(typeof(Chest), nameof(Chest.Open))]
    [HarmonyPostfix]
    private static void Chest_Open_Postfix(Chest __instance, bool __state, PlayerController player)
    {
        ETGMod.Chest.OnPostOpen?.Invoke(__instance, player);
    }

    //gamemanager patches
    [HarmonyPatch(typeof(GameManager), MethodType.Constructor)]
    [HarmonyPostfix]
    internal static void AddLevelLoadListener(GameManager __instance)
    {
        try
        {
            __instance.BraveLevelLoadedListeners = __instance.BraveLevelLoadedListeners.Concat(
                AppDomain.CurrentDomain.GetAssemblies().Select(x => x.GetTypes().Where(x => x?.GetInterfaces() != null && !x.IsInterface && x.GetInterfaces().Contains(typeof(ILevelLoadedListener)))).SelectMany(x => x)).Distinct().ToArray();
        }
        catch { }
    }

    [HarmonyPatch(typeof(GameManager), nameof(GameManager.Awake))]
    [HarmonyPrefix]
    internal static void InvokeOnAwakeBehaviours(GameManager __instance)
    {
        if(GameUIRoot.Instance == null)
        {
            return;
        }
        if(GameManager.mr_manager != null && GameManager.mr_manager != __instance)
        {
            return;
        }
        try
        {
            foreach (var act in ETGModMainBehaviour.OnGameManagerAwake)
            {
                if (__instance == null)
                {
                    break;
                }
                try
                {
                    act?.Invoke(__instance);
                }
                catch
                {
                }
            }
            ETGModMainBehaviour.OnGameManagerAwake.Clear();
        }
        catch
        {
        }
    }

    [HarmonyPatch(typeof(GameManager), nameof(GameManager.Start))]
    [HarmonyPostfix]
    internal static void InvokeOnStartBehaviours(GameManager __instance)
    {
        if (GameUIRoot.Instance == null)
        {
            return;
        }
        if (GameManager.mr_manager != null && GameManager.Instance != __instance)
        {
            return;
        }
        try
        {
            foreach (var act in ETGModMainBehaviour.OnGameManagerStart)
            {
                if (__instance == null)
                {
                    break;
                }
                try
                {
                    act?.Invoke(__instance);
                }
                catch
                {
                }
            }
            ETGModMainBehaviour.OnGameManagerStart.Clear();
        }
        catch
        {
        }
    }

    //gameuiroot patches
    [HarmonyPatch(typeof(dfGUIManager), nameof(dfGUIManager.Awake))]
    [HarmonyPrefix]
    internal static void GMUIRInvokeOnAwakeBehaviours(dfGUIManager __instance)
    {
        if (__instance.GetComponent<GameUIRoot>() == null || (GameUIRoot.Instance != null && GameUIRoot.Instance != __instance.GetComponent<GameUIRoot>()) || !GameManager.HasInstance)
        {
            return;
        }
        try
        {
            foreach (var act in ETGModMainBehaviour.OnGameManagerAwake)
            {
                if (!GameManager.HasInstance)
                {
                    break;
                }
                try
                {
                    act?.Invoke(GameManager.Instance);
                }
                catch
                {
                }
            }
            ETGModMainBehaviour.OnGameManagerAwake.Clear();
        }
        catch
        {
        }
    }

    [HarmonyPatch(typeof(GameUIRoot), nameof(GameUIRoot.Start))]
    [HarmonyPostfix]
    internal static void GMUIRInvokeOnStartBehaviours(GameUIRoot __instance)
    {
        if (!GameManager.HasInstance || (GameUIRoot.Instance != null && GameUIRoot.Instance != __instance))
        {
            if (__instance.GetComponent<GameUIRoot>() != null)
            {
            }
            return;
        }
        try
        {
            foreach (var act in ETGModMainBehaviour.OnGameManagerStart)
            {
                if (!GameManager.HasInstance)
                {
                    break;
                }
                try
                {
                    act?.Invoke(GameManager.Instance);
                }
                catch
                {
                }
            }
            ETGModMainBehaviour.OnGameManagerStart.Clear();
        }
        catch
        {
        }
    }
}
