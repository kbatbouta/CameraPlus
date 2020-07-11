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
        private readonly static float gainMultiplier = 0.08f;
        private readonly static float decayMultiplier = 0.025f;

        public static float tickMultiplierOld = 0.0f;
        public static float viewActivityLevel = 0.0f;
        public static float nMinusCameraScale = 0.0f;


        private static int ticksAbs = 0;

        static Patch_TickRateMultiplier()
        {
            var harmony = new Harmony("net.cameraplus.speed");

            var tickPost = new HarmonyMethod(typeof(Patch_TickRateMultiplier), nameof(TickManager_Postfix));
            var driverDollyPost = new HarmonyMethod(typeof(Patch_TickRateMultiplier), nameof(CameraDriver_Dolly_Postfix));
            var driverOnGuiPost = new HarmonyMethod(typeof(Patch_TickRateMultiplier), nameof(CameraDriver_OnGUI_Postfix));

            harmony.Patch(AccessTools.PropertyGetter(typeof(TickManager), nameof(TickManager.TickRateMultiplier)), null, tickPost);
            harmony.Patch(AccessTools.Method(typeof(CameraDriver), nameof(CameraDriver.CalculateCurInputDollyVect)), null, driverDollyPost);
            harmony.Patch(AccessTools.Method(typeof(CameraDriver), nameof(CameraDriver.CameraDriverOnGUI)), null, driverOnGuiPost);
        }

        static void TickManager_Postfix(ref float __result, TickManager __instance)
        {
            if (CameraPlusMain.Settings.experimentalTicking)
            {
                if (__instance.CurTimeSpeed != TimeSpeed.Ultrafast)
                    return;

                var deltaT = Time.deltaTime;

                if (deltaT < 0.021f)
                    return;

                __result = __result * 0.98f;
                __result = __result * gainMultiplier + tickMultiplierOld * (1 - gainMultiplier);
                tickMultiplierOld = __result;
            }
            else if (CameraPlusMain.Settings.dynamicSpeedControl)
            {
                if (Find.CameraDriver.CurrentViewRect.Area != nMinusCameraScale)
                {
                    nMinusCameraScale = Find.CameraDriver.CurrentViewRect.Area;

                    switch (Find.CameraDriver.CurrentZoom)
                    {
                        case CameraZoomRange.Furthest:
                            viewActivityLevel += 150f; break;
                        case CameraZoomRange.Far:
                            viewActivityLevel += 80f; break;
                        case CameraZoomRange.Middle:
                            viewActivityLevel += 40f; break;
                        case CameraZoomRange.Close:
                            viewActivityLevel += 5f; break;
                    }
                }

                if (viewActivityLevel <= 15f)
                {
                    __result = __result * gainMultiplier + tickMultiplierOld * (1f - gainMultiplier);
                    tickMultiplierOld = __result;
                }
                else
                {
                    var tickMultiplier = 0f;

                    switch (__instance.CurTimeSpeed)
                    {
                        case TimeSpeed.Normal:
                            tickMultiplier = 1.0f; break;
                        case TimeSpeed.Fast:
                            tickMultiplier = Mathf.Max(2.0f - viewActivityLevel / 128f, 1.0f); break;
                        case TimeSpeed.Superfast:
                            tickMultiplier = Mathf.Max(3.0f - viewActivityLevel / 96f, 1.5f); break;
                        case TimeSpeed.Ultrafast:
                            tickMultiplier = Mathf.Max(4.0f - viewActivityLevel / 64f, 2.0f); break;
                    }

                    __result = __result * decayMultiplier + tickMultiplier * (1f - decayMultiplier);
                    tickMultiplierOld = __result;
                }
            }
        }

        static void CameraDriver_Dolly_Postfix(ref Vector2 __result)
        {
            if (!CameraPlusMain.Settings.dynamicSpeedControl) return;

            viewActivityLevel = __result.magnitude;
        }

        static void CameraDriver_OnGUI_Postfix(CameraDriver __instance)
        {
            if (!CameraPlusMain.Settings.dynamicSpeedControl) return;

            if (Find.TickManager.Paused || Find.TickManager.NotPlaying)
                return;

            if (KeyBindingDefOf.MapDolly_Left.IsDown || KeyBindingDefOf.MapDolly_Up.IsDown || KeyBindingDefOf.MapDolly_Right.IsDown || KeyBindingDefOf.MapDolly_Down.IsDown)
            {
                var modifer = 0f;

                switch (Find.CameraDriver.CurrentZoom)
                {
                    case CameraZoomRange.Furthest:
                        modifer = 150f; break;
                    case CameraZoomRange.Far:
                        modifer = 80f; break;
                    case CameraZoomRange.Middle:
                        modifer = 40f; break;
                    case CameraZoomRange.Close:
                        modifer = 5f; break;
                }

                viewActivityLevel = modifer;
            }
        }
    }

}
