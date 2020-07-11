﻿using HarmonyLib;
using System;
using UnityEngine;
using Verse;

namespace CameraPlus
{
    public enum LabelStyle
    {
        IncludeAnimals = 0,
        AnimalsDifferent = 1,
        HideAnimals = 2
    }

    public class SavedViews : MapComponent
    {
        public RememberedCameraPos[] views = new RememberedCameraPos[9];

        public SavedViews(Map map) : base(map)
        {
        }

        public override void ExposeData()
        {
            for (var i = 0; i < 9; i++)
                Scribe_Deep.Look(ref views[i], "view" + (i + 1), new object[] { map });
        }
    }

    public class CameraPlusSettings : ModSettings
    {
        public float zoomedOutPercent = 65;
        public float zoomedInPercent = 1;
        public int exponentiality = 1;
        public float zoomedOutDollyPercent = 1;
        public float zoomedInDollyPercent = 1;
        public float zoomedOutScreenEdgeDollyFactor = 0.5f;
        public float zoomedInScreenEdgeDollyFactor = 0.5f;
        public bool stickyMiddleMouse = false;
        public bool zoomToMouse = true;
        public float soundNearness = 0;
        public bool hideNamesWhenZoomedOut = true;
        public int dotSize = 9;
        public int hidePawnLabelBelow = 9;
        public int hideThingLabelBelow = 32;
        public bool mouseOverShowsLabels = true;
        public bool dynamicSpeedControl = true;
        public bool experimentalTicking = false;
        public LabelStyle customNameStyle = LabelStyle.AnimalsDifferent;
        public bool includeNotTamedAnimals = true;

        public KeyCode[] cameraSettingsMod = new[] { KeyCode.LeftShift, KeyCode.None };
        public KeyCode cameraSettingsKey = KeyCode.Tab;
        public KeyCode[] cameraSettingsSave = new[] { KeyCode.LeftAlt, KeyCode.None };
        public KeyCode[] cameraSettingsLoad = new[] { KeyCode.LeftShift, KeyCode.None };

        public static float minRootResult = 2;
        public static float maxRootResult = 130;

        public static readonly float minRootInput = 11;
        public static readonly float maxRootInput = 60;

        public static readonly float minRootOutput = 15;
        public static readonly float maxRootOutput = 65;

        public static readonly float nearestHeight = 32;
        public static readonly float farOutHeight = 256;

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref zoomedOutPercent, "zoomedOutPercent", 65);
            Scribe_Values.Look(ref zoomedInPercent, "zoomedInPercent", 1);
            Scribe_Values.Look(ref exponentiality, "exponentiality", 1);
            Scribe_Values.Look(ref zoomedOutDollyPercent, "zoomedOutDollyPercent", 1);
            Scribe_Values.Look(ref zoomedInDollyPercent, "zoomedInDollyPercent", 1);
            Scribe_Values.Look(ref zoomedOutScreenEdgeDollyFactor, "zoomedOutScreenEdgeDollyFactor", 0.5f);
            Scribe_Values.Look(ref zoomedInScreenEdgeDollyFactor, "zoomedInScreenEdgeDollyFactor", 0.5f);
            Scribe_Values.Look(ref stickyMiddleMouse, "stickyMiddleMouse", false);
            Scribe_Values.Look(ref zoomToMouse, "zoomToMouse", true);
            Scribe_Values.Look(ref soundNearness, "soundNearness", 0);
            Scribe_Values.Look(ref hideNamesWhenZoomedOut, "hideNamesWhenZoomedOut", true);
            Scribe_Values.Look(ref dotSize, "dotSize", 9);
            Scribe_Values.Look(ref hidePawnLabelBelow, "hidePawnLabelBelow", 0);
            Scribe_Values.Look(ref hideThingLabelBelow, "hideThingLabelBelow", 32);
            Scribe_Values.Look(ref mouseOverShowsLabels, "mouseOverShowsLabels", true);
            Scribe_Values.Look(ref customNameStyle, "customNameStyle", LabelStyle.AnimalsDifferent);
            Scribe_Values.Look(ref includeNotTamedAnimals, "includeNotTamedAnimals", true);
            Scribe_Values.Look(ref cameraSettingsMod[0], "cameraSettingsMod1", KeyCode.LeftShift);
            Scribe_Values.Look(ref cameraSettingsMod[1], "cameraSettingsMod2", KeyCode.None);
            Scribe_Values.Look(ref cameraSettingsKey, "cameraSettingsKey", KeyCode.Tab);
            Scribe_Values.Look(ref cameraSettingsLoad[0], "cameraSettingsLoad1", KeyCode.LeftShift);
            Scribe_Values.Look(ref cameraSettingsLoad[1], "cameraSettingsLoad2", KeyCode.None);
            Scribe_Values.Look(ref cameraSettingsSave[0], "cameraSettingsSave1", KeyCode.LeftAlt);
            Scribe_Values.Look(ref cameraSettingsSave[1], "cameraSettingsSave2", KeyCode.None);
            Scribe_Values.Look(ref dynamicSpeedControl, "dynamicSpeedControl", true);
            Scribe_Values.Look(ref experimentalTicking, "experimentalTicking", false);

            if (Scribe.mode == LoadSaveMode.ResolvingCrossRefs)
            {
                minRootResult = zoomedInPercent * 2;
                maxRootResult = zoomedOutPercent * 2;
            }
        }

        public void DoWindowContents(Rect inRect)
        {
            float previous;
            Rect rect;
            var map = Current.Game?.CurrentMap;
            const float buttonWidth = 80f;
            const float buttonSpace = 4f;

            var list = new Listing_Standard { ColumnWidth = (inRect.width - 34f) / 2f };
            list.Begin(inRect);

            list.Gap(16f);

            _ = list.Label("Zoom".Translate());

            previous = zoomedInPercent;
            list.Slider(ref zoomedInPercent, 0.1f, 20f, () => "Near".Translate() + ": " + Math.Round(zoomedInPercent, 1) + "%");
            minRootResult = zoomedInPercent * 2;
            if (previous != zoomedInPercent && map != null)
            {
                var val = Traverse.Create(Find.CameraDriver).Field("rootSize").GetValue<float>();
                if (val != minRootInput)
                    Find.CameraDriver.SetRootPosAndSize(map.rememberedCameraPos.rootPos, minRootInput);
            }

            previous = zoomedOutPercent;
            list.Slider(ref zoomedOutPercent, 25f, 100f, () => "Far".Translate() + ": " + Math.Round(zoomedOutPercent, 1) + "%");
            maxRootResult = zoomedOutPercent * 2;
            if (previous != zoomedOutPercent && map != null)
            {
                var val = Traverse.Create(Find.CameraDriver).Field("rootSize").GetValue<float>();
                if (val != maxRootInput)
                    Find.CameraDriver.SetRootPosAndSize(map.rememberedCameraPos.rootPos, maxRootInput);
            }

            list.Gap(12f);

            _ = list.Label("Exponentiality".Translate());
            if (list.RadioButton("Off", exponentiality == 0, 8f)) exponentiality = 0;
            for (var i = 1; i <= 3; i++)
                if (list.RadioButton(i + "x", exponentiality == i, 8f)) exponentiality = i;

            list.Gap(16f);

            _ = list.Label("SoundNearness".Translate() + ": " + Math.Round(soundNearness * 100, 1) + "%");
            list.Slider(ref soundNearness, 0f, 1f, null);

            list.Gap(6f);

            list.CheckboxLabeled("ZoomToMouse".Translate(), ref zoomToMouse);
            list.CheckboxLabeled("MouseRevealsLabels".Translate(), ref mouseOverShowsLabels);

            list.Gap(24f);

            _ = list.Label("HotKeys".Translate());
            list.Gap(6f);

            rect = list.GetRect(28f);
            GenUI.SetLabelAlign(TextAnchor.MiddleLeft);
            Widgets.Label(rect, "SettingsKey".Translate());
            GenUI.ResetLabelAlign();
            rect.xMin = rect.xMax - buttonWidth;
            Tools.KeySettingsButton(rect, false, cameraSettingsKey, code => cameraSettingsKey = code);
            rect.xMin -= buttonWidth + buttonSpace;
            rect.xMax = rect.xMin + buttonWidth;
            Tools.KeySettingsButton(rect, false, cameraSettingsMod[1], code => cameraSettingsMod[1] = code);
            rect.xMin -= buttonWidth + buttonSpace;
            rect.xMax = rect.xMin + buttonWidth;
            Tools.KeySettingsButton(rect, false, cameraSettingsMod[0], code => cameraSettingsMod[0] = code);
            list.Gap(6f);

            rect = list.GetRect(28f);
            GenUI.SetLabelAlign(TextAnchor.MiddleLeft);
            Widgets.Label(rect, "LoadModifier".Translate());
            GenUI.ResetLabelAlign();
            rect.xMin = rect.xMax - buttonWidth;
            GenUI.SetLabelAlign(TextAnchor.MiddleCenter);
            Widgets.Label(rect, "1 - 9");
            GenUI.ResetLabelAlign();
            rect.xMin -= buttonWidth + buttonSpace;
            rect.xMax = rect.xMin + buttonWidth;
            Tools.KeySettingsButton(rect, false, cameraSettingsLoad[1], code => cameraSettingsLoad[1] = code);
            rect.xMin -= buttonWidth + buttonSpace;
            rect.xMax = rect.xMin + buttonWidth;
            Tools.KeySettingsButton(rect, false, cameraSettingsLoad[0], code => cameraSettingsLoad[0] = code);
            list.Gap(6f);

            rect = list.GetRect(28f);
            GenUI.SetLabelAlign(TextAnchor.MiddleLeft);
            Widgets.Label(rect, "SaveModifier".Translate());
            GenUI.ResetLabelAlign();
            rect.xMin = rect.xMax - buttonWidth;
            GenUI.SetLabelAlign(TextAnchor.MiddleCenter);
            Widgets.Label(rect, "1 - 9");
            GenUI.ResetLabelAlign();
            rect.xMin -= buttonWidth + buttonSpace;
            rect.xMax = rect.xMin + buttonWidth;
            Tools.KeySettingsButton(rect, false, cameraSettingsSave[1], code => cameraSettingsSave[1] = code);
            rect.xMin -= buttonWidth + buttonSpace;
            rect.xMax = rect.xMin + buttonWidth;
            Tools.KeySettingsButton(rect, false, cameraSettingsSave[0], code => cameraSettingsSave[0] = code);
            list.Gap(6f);

            list.NewColumn();
            list.Gap(16f);

            _ = list.Label("DollyPercentLabel".Translate());
            list.Slider(ref zoomedInDollyPercent, 0f, 4f, () => "Near".Translate() + ": " + Math.Round(zoomedInDollyPercent * 100, 1) + "%");
            list.Slider(ref zoomedOutDollyPercent, 0f, 4f, () => "Far".Translate() + ": " + Math.Round(zoomedOutDollyPercent * 100, 1) + " % ");

            list.Gap(12f);

            _ = list.Label("ScreenEdgeDollyFrictionLabel".Translate());
            list.Slider(ref zoomedInScreenEdgeDollyFactor, 0f, 1f, () => "Near".Translate() + ": " + Math.Round(0.5 + zoomedInScreenEdgeDollyFactor, 1) + "x");
            list.Slider(ref zoomedOutScreenEdgeDollyFactor, 0f, 1f, () => "Far".Translate() + ": " + Math.Round(0.5 + zoomedOutScreenEdgeDollyFactor, 1) + "x");

            list.Gap(24f);

            list.CheckboxLabeled("The Smooziatron 5001", ref dynamicSpeedControl);
            list.CheckboxLabeled("The ticketron 1000", ref experimentalTicking);

            if (CameraPlusMain.Settings.experimentalTicking)
                dynamicSpeedControl = false;

            if (CameraPlusMain.Settings.dynamicSpeedControl)
                experimentalTicking = false;
#if LEGACY
            if (hideNamesWhenZoomedOut)
            {
            	var pixel = "Pixel".Translate();
            	var label1 = "HidePawnLabelBelow".Translate();
            	list.Slider(ref hidePawnLabelBelow, 0, 128, () => label1 + (hidePawnLabelBelow == 0 ? "Never".Translate() : hidePawnLabelBelow + " " + pixel));
            	var label2 = "HideThingLabelBelow".Translate();
            	list.Slider(ref hideThingLabelBelow, 0, 128, () => label2 + (hideThingLabelBelow == 0 ? "Never".Translate() : hideThingLabelBelow + " " + pixel));
            	list.Slider(ref dotSize, 1, 32, () => "ShowMarkerBelow".Translate() + dotSize + " " + "Pixel".Translate());
            	foreach (var label in Enum.GetNames(typeof(LabelStyle)))
            	{
            		var val = (LabelStyle)Enum.Parse(typeof(LabelStyle), label);
            		if (list.RadioButton(label.Translate(), customNameStyle == val, 8f)) customNameStyle = val;
            	}
            	list.CheckboxLabeled("IncludeNotTamedAnimals".Translate(), ref includeNotTamedAnimals);
            }
#endif

            if (CameraPlusMain.Settings.experimentalTicking)
                CameraPlusMain.Settings.dynamicSpeedControl = false;

            if (CameraPlusMain.Settings.dynamicSpeedControl)
                CameraPlusMain.Settings.experimentalTicking = false;

            list.Gap(28f);

            if (list.ButtonText("RestoreToDefaultSettings".Translate()))
            {
                zoomedOutPercent = 65;
                zoomedInPercent = 1;
                exponentiality = 1;
                zoomedOutDollyPercent = 1;
                zoomedInDollyPercent = 1;
                zoomedOutScreenEdgeDollyFactor = 0.5f;
                zoomedInScreenEdgeDollyFactor = 0.5f;
                stickyMiddleMouse = false;
                soundNearness = 0;
                hideNamesWhenZoomedOut = true;
                dotSize = 9;
                hidePawnLabelBelow = 9;
                hideThingLabelBelow = 32;
                mouseOverShowsLabels = true;
                customNameStyle = LabelStyle.AnimalsDifferent;
                includeNotTamedAnimals = true;
                cameraSettingsMod[0] = KeyCode.LeftShift;
                cameraSettingsMod[1] = KeyCode.None;
                cameraSettingsKey = KeyCode.Tab;
                cameraSettingsLoad[0] = KeyCode.LeftShift;
                cameraSettingsLoad[1] = KeyCode.None;
                cameraSettingsSave[0] = KeyCode.LeftAlt;
                cameraSettingsSave[1] = KeyCode.None;
            }

            list.End();
        }
    }
}