using CameraPlus;
using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using UnityEngine;
using Verse;

namespace CameraPlus
{

    [StaticConstructorOnStartup]
    public static class Patch_TickRateMultiplier
    {

        public static float viewDollyLevel = 0.0f;
        public static float viewActivityLevel = 0.0f;

        public static float nMinusCameraScale = 0.0f;
        public static float rootSize = 0;

        private static float oldTickRateMultiplier = 0f;

        public static CameraDriver cameraDriver = null;


        static Patch_TickRateMultiplier()
        {
            var harmony = new Harmony("net.cameraplus.speed");

            var tickPost = new HarmonyMethod(typeof(Patch_TickRateMultiplier),
                nameof(TickManager_Postfix));
            var driverDollyPost = new HarmonyMethod(typeof(Patch_TickRateMultiplier),
                nameof(CameraDriver_Dolly_Postfix));
            var driverOnGuiPost = new HarmonyMethod(typeof(Patch_TickRateMultiplier),
                nameof(CameraDriver_OnGUI_Postfix));

            harmony.Patch(AccessTools.PropertyGetter(typeof(TickManager), nameof(TickManager.TickRateMultiplier)), null, tickPost);
            harmony.Patch(AccessTools.Method(typeof(CameraDriver), nameof(CameraDriver.CalculateCurInputDollyVect)), null, driverDollyPost);
            harmony.Patch(AccessTools.Method(typeof(CameraDriver), nameof(CameraDriver.CameraDriverOnGUI)), null, driverOnGuiPost);
        }

        static void TickManager_Postfix(ref float __result, TickManager __instance)
        {
            if (!CameraPlusMain.Settings.dynamicSpeedControl) { return; }

            if (cameraDriver == null) { cameraDriver = Find.CameraDriver; }

            var gameSpeed = __instance.CurTimeSpeed;
            if (gameSpeed == TimeSpeed.Paused) { return; }

            var deltaTime = Time.deltaTime;
            if (Mathf.Abs(rootSize - nMinusCameraScale) > 0.001f)
            {
                nMinusCameraScale = rootSize; viewActivityLevel = 2 * rootSize; deltaTime *= 10;
            }

            if (viewActivityLevel + viewDollyLevel > 5f)
            {
                switch (gameSpeed)
                {
                    case TimeSpeed.Normal:
                        __result = 1.0f;
                        break;
                    case TimeSpeed.Fast:
                        __result = Mathf.Max(oldTickRateMultiplier - 0.5f * deltaTime, CameraPlusMain.Settings.speedLimitFast);
                        break;
                    case TimeSpeed.Superfast:
                        __result = Mathf.Max(oldTickRateMultiplier - 1.25f * deltaTime, CameraPlusMain.Settings.speedLimitSuperFast);
                        break;
                    case TimeSpeed.Ultrafast:
                        __result = Mathf.Max(oldTickRateMultiplier - 4.5f * deltaTime, CameraPlusMain.Settings.speedLimitUltraFast);
                        break;
                }
            }
            else { __result = Mathf.Min(oldTickRateMultiplier + CameraPlusMain.Settings.speedGainSpeed * deltaTime, __result); }

            oldTickRateMultiplier = __result;
        }

        static void CameraDriver_Dolly_Postfix(ref Vector2 __result)
        {
            if (!CameraPlusMain.Settings.dynamicSpeedControl) return;

            viewDollyLevel = __result.magnitude;
        }

        static void CameraDriver_OnGUI_Postfix(CameraDriver __instance, float ___rootSize)
        {
            if (!CameraPlusMain.Settings.dynamicSpeedControl) return;

            Patch_TickRateMultiplier.rootSize = ___rootSize;

            if (Find.TickManager.Paused || Find.TickManager.NotPlaying)
                return;

            if (KeyBindingDefOf.MapDolly_Left.IsDown || KeyBindingDefOf.MapDolly_Up.IsDown || KeyBindingDefOf.MapDolly_Right.IsDown || KeyBindingDefOf.MapDolly_Down.IsDown)
            {
                switch (cameraDriver.CurrentZoom)
                {
                    case CameraZoomRange.Furthest:
                        viewActivityLevel = 150f;
                        break;
                    case CameraZoomRange.Far:
                        viewActivityLevel = 80f;
                        break;
                    case CameraZoomRange.Middle:
                        viewActivityLevel = 40f;
                        break;
                    case CameraZoomRange.Close:
                        viewActivityLevel = 5f;
                        break;
                }
            }
            else { viewActivityLevel = 0f; }
        }
    }

}
