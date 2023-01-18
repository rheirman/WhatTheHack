using System;
using System.Collections.Generic;
using System.Linq;
using HugsLib.Settings;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace WhatTheHack;

public class GUIDrawUtility
{
    private const float ContentPadding = 5f;

    private const float TextMargin = 20f;
    private const float BottomMargin = 2f;
    private const float buttonHeight = 28f;
    private static readonly Color iconBaseColor = new Color(0.5f, 0.5f, 0.5f, 1f);
    private static readonly Color iconMouseOverColor = new Color(0.6f, 0.6f, 0.4f, 1f);

    private static readonly Color SelectedOptionColor = new Color(0.5f, 1f, 0.5f, 1f);
    private static readonly Color constGrey = new Color(0.8f, 0.8f, 0.8f, 1f);

    private static readonly Color background = new Color(0.5f, 0, 0, 0.1f);
    private static readonly Color selectedBackground = new Color(0f, 0.5f, 0, 0.1f);
    private static readonly Color notSelectedBackground = new Color(0.5f, 0, 0, 0.1f);


    private static void DrawBackground(Rect rect, Color background)
    {
        var save = GUI.color;
        GUI.color = background;
        GUI.DrawTexture(rect, TexUI.FastFillTex);
        GUI.color = save;
    }

    private static void DrawLabel(string labelText, Rect textRect, float offset)
    {
        var labelHeight = Text.CalcHeight(labelText, textRect.width);
        labelHeight -= 2f;
        var labelRect = new Rect(textRect.x, textRect.yMin - labelHeight + offset, textRect.width, labelHeight);
        GUI.DrawTexture(labelRect, TexUI.GrayTextBG);
        GUI.color = Color.white;
        Text.Anchor = TextAnchor.UpperCenter;
        Widgets.Label(labelRect, labelText);
        Text.Anchor = TextAnchor.UpperLeft;
        GUI.color = Color.white;
    }

    private static Color GetColor(ThingDef mech)
    {
        if (mech.graphicData != null)
        {
            return mech.graphicData.color;
        }

        return Color.white;
    }

    private static bool DrawTileForPawn(KeyValuePair<string, Record> pawn, Rect contentRect, Vector2 iconOffset,
        int buttonID, float tileHeight)
    {
        var iconRect = new Rect(contentRect.x + iconOffset.x, contentRect.y + iconOffset.y, contentRect.width,
            tileHeight);
        MouseoverSounds.DoRegion(iconRect, SoundDefOf.Mouseover_Command);
        var save = GUI.color;

        if (Mouse.IsOver(iconRect))
        {
            GUI.color = iconMouseOverColor;
        }
        else if (pawn.Value.isSelected)
        {
            GUI.color = selectedBackground;
        }
        else
        {
            GUI.color = notSelectedBackground;
        }

        GUI.DrawTexture(iconRect, TexUI.FastFillTex);
        GUI.color = save;
        Text.Anchor = TextAnchor.MiddleCenter;
        Widgets.Label(iconRect, !pawn.Value.label.NullOrEmpty() ? pawn.Value.label : pawn.Key);
        Text.Anchor = TextAnchor.UpperLeft;

        if (!Widgets.ButtonInvisible(iconRect))
        {
            return false;
        }

        Event.current.button = buttonID;
        return true;
    }


    public static bool CustomDrawer_Button(Rect rect, SettingHandle<bool> setting, string activateText,
        string deactivateText, int xOffset = 0, int yOffset = 0)
    {
        var labelWidth = (int)rect.width - 20;
        var horizontalOffset = 0;
        var verticalOffset = 0;
        var buttonRect = new Rect(rect)
        {
            width = labelWidth
        };
        buttonRect.position = new Vector2(buttonRect.position.x + horizontalOffset + xOffset,
            buttonRect.position.y + verticalOffset + yOffset);
        var activeColor = GUI.color;
        var isSelected = setting.Value;
        var text = setting ? deactivateText : activateText;

        if (isSelected)
        {
            GUI.color = SelectedOptionColor;
        }

        var clicked = Widgets.ButtonText(buttonRect, text);
        if (isSelected)
        {
            GUI.color = activeColor;
        }

        if (!clicked)
        {
            return false;
        }

        setting.Value = !setting.Value;

        return true;
    }

    public static bool CustomDrawer_Tabs(Rect rect, SettingHandle<string> setting, string[] defaultValues,
        bool vertical = false, int xOffset = 0, int yOffset = 0)
    {
        var labelWidth = (int)rect.width - 20;

        var horizontalOffset = 0;
        var verticalOffset = 0;

        var change = false;

        foreach (var tab in defaultValues)
        {
            var buttonRect = new Rect(rect)
            {
                width = labelWidth
            };
            buttonRect.position = new Vector2(buttonRect.position.x + horizontalOffset + xOffset,
                buttonRect.position.y + verticalOffset + yOffset);
            var activeColor = GUI.color;
            var isSelected = tab == setting.Value;
            if (isSelected)
            {
                GUI.color = SelectedOptionColor;
            }

            var clicked = Widgets.ButtonText(buttonRect, tab);
            if (isSelected)
            {
                GUI.color = activeColor;
            }


            if (clicked)
            {
                if (setting.Value != tab)
                {
                    setting.Value = tab;
                }

                //else
                //{
                //    setting.Value = "none";
                //}
                change = true;
            }

            if (vertical)
            {
                verticalOffset += (int)buttonRect.height;
            }
            else
            {
                horizontalOffset += labelWidth;
            }
        }

        if (vertical)
        {
            //setting.CustomDrawerHeight = verticalOffset;
        }

        return change;
    }


    public static bool CustomDrawer_Filter(Rect rect, SettingHandle<float> slider, bool def_isPercentage, float def_min,
        float def_max, Color background)
    {
        DrawBackground(rect, background);
        var labelWidth = 50;

        var sliderPortion = new Rect(rect);
        sliderPortion.width -= labelWidth;

        var labelPortion = new Rect(rect)
        {
            width = labelWidth,
            position = new Vector2(sliderPortion.position.x + sliderPortion.width + 5f, sliderPortion.position.y + 4f)
        };

        sliderPortion = sliderPortion.ContractedBy(2f);

        Widgets.Label(labelPortion,
            def_isPercentage ? $"{Mathf.Round(slider.Value * 100f):F0}%" : slider.Value.ToString("F2"));

        var val = Widgets.HorizontalSlider_NewTemp(sliderPortion, slider.Value, def_min, def_max, true);
        var change = slider.Value != val;

        slider.Value = val;
        return change;
    }

    internal static void FilterSelection(ref Dictionary<string, Record> selection, List<ThingDef> allDefs,
        string factionName)
    {
        var shouldSelect = new Dictionary<string, Record>();

        if (selection == null)
        {
            selection = new Dictionary<string, Record>();
        }

        foreach (var td in allDefs)
        {
            var found = selection.TryGetValue(td.defName, out var value);
            if (found && value.isException)
            {
                shouldSelect.Add(td.defName, value);
            }
            else
            {
                var factionDef = GetFactionByName(factionName);
                if (factionDef != null && (factionDef.techLevel == TechLevel.Spacer ||
                                           factionDef.techLevel == TechLevel.Archotech ||
                                           factionDef.techLevel == TechLevel.Ultra))
                {
                    shouldSelect.Add(td.defName, new Record(true, false, td.label));
                }
                else
                {
                    shouldSelect.Add(td.defName, new Record(false, false, td.label));
                }
            }
        }

        selection = shouldSelect.OrderBy(d => d.Value.label).ToDictionary(d => d.Key, d => d.Value);
    }

    private static FactionDef GetFactionByName(string name)
    {
        return DefDatabase<FactionDef>.AllDefs.FirstOrDefault(fd => fd.defName == name);
    }

    public static bool CustomDrawer_MatchingPawns_active(Rect wholeRect, SettingHandle<Dict2DRecordHandler> setting,
        List<ThingDef> allPawns, List<string> allFactionNames, SettingHandle<string> filter = null,
        string yesText = "Is a mount", string noText = "Is not a mount")
    {
        //TODO: refactor this mess, remove redundant and quircky things.

        var rowHeight = 20f;
        if (setting.Value == null)
        {
            setting.Value = Base.GetDefaultForFactionRestrictions(new Dict2DRecordHandler(), allPawns, allFactionNames);
        }

        CustomDrawer_Tabs(new Rect(wholeRect.x, wholeRect.y, wholeRect.width, buttonHeight), filter,
            allFactionNames.ToArray(), true, (int)-wholeRect.width);
        DrawBackground(wholeRect, background);


        GUI.color = Color.white;

        var leftRect = new Rect(wholeRect);
        leftRect.width /= 2;
        leftRect.height = wholeRect.height - TextMargin + BottomMargin;
        leftRect.position = new Vector2(leftRect.position.x, leftRect.position.y);
        var rightRect = new Rect(wholeRect);
        rightRect.width /= 2;
        leftRect.height = wholeRect.height - TextMargin + BottomMargin;
        rightRect.position = new Vector2(rightRect.position.x + leftRect.width, rightRect.position.y);

        DrawLabel(yesText, leftRect, TextMargin);
        DrawLabel(noText, rightRect, TextMargin);

        leftRect.position = new Vector2(leftRect.position.x, leftRect.position.y + TextMargin);
        rightRect.position = new Vector2(rightRect.position.x, rightRect.position.y + TextMargin);

        var change = false;
        var unused = setting.Value.InnerList.TryGetValue(filter.Value, out var selection);
        FilterSelection(ref selection, allPawns, filter.Value);

        var innerDict = setting.Value.InnerList;

        var indexLeft = 0;
        var indexRight = 0;
        float leftHeight = 0;
        float rightHeight = 0;
        foreach (var item in selection)
        {
            var rect = item.Value.isSelected ? leftRect : rightRect;
            var index = item.Value.isSelected ? indexLeft : indexRight;
            var tileHeight = item.Value.label.Length > 16 ? 2 * rowHeight : rowHeight;
            leftRect.height = tileHeight;
            rightRect.height = tileHeight;
            var offset = item.Value.isSelected ? leftHeight : rightHeight;

            if (item.Value.isSelected)
            {
                leftHeight += tileHeight + BottomMargin;
                indexLeft++;
            }
            else
            {
                rightHeight += tileHeight + BottomMargin;
                indexRight++;
            }

            var interacted = DrawTileForPawn(item, rect, new Vector2(0, offset), index, tileHeight);
            if (!interacted)
            {
                continue;
            }

            change = true;
            item.Value.isSelected = !item.Value.isSelected;
            item.Value.isException = !item.Value.isException;
        }

        var neededHightSelector = Math.Max(leftHeight, rightHeight) + TextMargin;
        var neededHightFilter = (allFactionNames.Count * buttonHeight) + TextMargin + 5f;
        setting.CustomDrawerHeight = Math.Max(neededHightSelector, neededHightFilter);


        if (change)
        {
            setting.Value.InnerList[filter.Value] = selection;
            //setting.Value.InnerList = selection;
        }

        return change;
    }
}