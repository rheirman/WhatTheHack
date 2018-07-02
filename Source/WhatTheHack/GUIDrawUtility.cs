using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using Verse.Sound;
using HugsLib.Settings;

namespace WhatTheHack
{
    public class GUIDrawUtility
    {
        
        private const float ContentPadding = 5f;

        private const float TextMargin = 20f;
        private const float BottomMargin = 2f;
        private const float rowHeight = 20f;
        private const float buttonHeight = 28f;
        private static readonly Color iconBaseColor = new Color(0.5f, 0.5f, 0.5f, 1f);
        private static readonly Color iconMouseOverColor = new Color(0.6f, 0.6f, 0.4f, 1f);

        private static readonly Color SelectedOptionColor = new Color(0.5f, 1f, 0.5f, 1f);
        private static readonly Color constGrey = new Color(0.8f, 0.8f, 0.8f, 1f);
        private static Color background = new Color(0.5f, 0, 0, 0.1f);
        private static Color exceptionBackground = new Color(0f, 0.5f, 0, 0.1f);



        private static void drawBackground(Rect rect, Color background)
        {
            Color save = GUI.color;
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
        private static Color getColor(ThingDef Animal)
        {
            if (Animal.graphicData != null)
            {
                return Animal.graphicData.color;
            }
            return Color.white;
        }

        private static bool DrawTileForAnimal(KeyValuePair<String, Record> Animal, Rect contentRect, Vector2 iconOffset, int buttonID)
        {
            var iconRect = new Rect(contentRect.x + iconOffset.x, contentRect.y + iconOffset.y, contentRect.width, rowHeight);
            MouseoverSounds.DoRegion(iconRect, SoundDefOf.Mouseover_Command);
            Color save = GUI.color;

            if (Mouse.IsOver(iconRect))
            {
                GUI.color = iconMouseOverColor;
            }
            else if (Animal.Value.isException == true)
            {
                GUI.color = exceptionBackground;
            }
            else
            {
                GUI.color = background;
            }
            GUI.DrawTexture(iconRect, TexUI.FastFillTex);
            GUI.color = save;
            Text.Anchor = TextAnchor.MiddleCenter;
            Widgets.Label(iconRect, (!Animal.Value.label.NullOrEmpty() ? Animal.Value.label : Animal.Key));
            Text.Anchor = TextAnchor.UpperLeft;

            if (Widgets.ButtonInvisible(iconRect, true))
            {
                Event.current.button = buttonID;
                return true;
            }
            else
                return false;

        }

        public static bool CustomDrawer_Tabs(Rect rect, SettingHandle<String> setting, String[] defaultValues, bool vertical = false, int xOffset = 0, int yOffset = 0)
        {
            int labelWidth = 140;
            int horizontalOffset = 0;
            int verticalOffset = 0;
            
            bool change = false;
            foreach (String tab in defaultValues)
            {
                Rect buttonRect = new Rect(rect);
                buttonRect.width = labelWidth;
                buttonRect.position = new Vector2(buttonRect.position.x + horizontalOffset + xOffset, buttonRect.position.y + verticalOffset + yOffset);
                Color activeColor = GUI.color;
                bool isSelected = tab == setting.Value;
                if (isSelected)
                    GUI.color = SelectedOptionColor;
                bool clicked = Widgets.ButtonText(buttonRect, tab);
                if (isSelected)
                    GUI.color = activeColor;

                if (clicked)
                {
                    if(setting.Value != tab)
                    {
                        setting.Value = tab;
                    }
                    else
                    {
                        setting.Value = "none";
                    }
                    change = true;
                }

                if (vertical)
                {
                    verticalOffset += (int) buttonRect.height;
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


        public static bool CustomDrawer_Filter(Rect rect, SettingHandle<float> slider, bool def_isPercentage, float def_min, float def_max, Color background)
        {
            drawBackground(rect, background);
            int labelWidth = 50;

            Rect sliderPortion = new Rect(rect);
            sliderPortion.width = sliderPortion.width - labelWidth;

            Rect labelPortion = new Rect(rect);
            labelPortion.width = labelWidth;
            labelPortion.position = new Vector2(sliderPortion.position.x + sliderPortion.width + 5f, sliderPortion.position.y + 4f);

            sliderPortion = sliderPortion.ContractedBy(2f);

            if (def_isPercentage)
                Widgets.Label(labelPortion, (Mathf.Round(slider.Value * 100f)).ToString("F0") + "%");
            else
                Widgets.Label(labelPortion, slider.Value.ToString("F2"));

            float val = Widgets.HorizontalSlider(sliderPortion, slider.Value, def_min, def_max, true);
            bool change = false;

            if (slider.Value != val)
                change = true;

            slider.Value = val;
            return change;
        }

        internal static void FilterSelection(ref Dictionary<string, Record> selection, List<ThingDef> allDefs)
        {
            Dictionary<string, Record> shouldSelect = new Dictionary<string, Record>();

            if (selection == null)
            {
                selection = new Dictionary<string, Record>();
            }
            foreach (ThingDef td in allDefs)
            {
                Record value = null;
                bool found = selection.TryGetValue(td.defName, out value);
                if (found && value.isException)
                {
                    shouldSelect.Add(td.defName, value);
                }
                else
                {
                    shouldSelect.Add(td.defName, new Record(false, false, td.label));
                }
                
            }
            selection = shouldSelect.OrderBy(d => d.Value.label).ToDictionary(d => d.Key, d => d.Value);
        }

        /*
        internal static void addNewAnimal(ThingDef animal, Dictionary<string, Record> selection, bool shouldSelect, String settingName)
        {
                CompProperties_Mount prop = animal.GetCompProperties<CompProperties_Mount>();
                float mass = animal.race.baseBodySize;
                if (prop != null && prop.isException && settingName == "Animalselecter")
                {
                    selection.Add(animal.defName, new Record(false, true, animal.label));
                }
                else if (prop != null && prop.drawFront && settingName == "drawSelecter")
                {
                    selection.Add(animal.defName, new Record(true, true, animal.label));
                }
                else
                {
                    selection.Add(animal.defName, new Record(shouldSelect, false, animal.label));
                }
        }
        */


        public static bool CustomDrawer_MatchingAnimals_active(Rect wholeRect, SettingHandle<Dict2DRecordHandler> setting, List<ThingDef> allAnimals, SettingHandle<string> filter = null, int filterCount = 0, string yesText = "Is a mount", string noText = "Is not a mount")
        {
            drawBackground(wholeRect, background);


            GUI.color = Color.white;

            Rect leftRect = new Rect(wholeRect);
            leftRect.width = leftRect.width / 2;
            leftRect.height = wholeRect.height - TextMargin + BottomMargin;
            leftRect.position = new Vector2(leftRect.position.x, leftRect.position.y);
            Rect rightRect = new Rect(wholeRect);
            rightRect.width = rightRect.width / 2;
            leftRect.height = wholeRect.height - TextMargin + BottomMargin;
            rightRect.position = new Vector2(rightRect.position.x + leftRect.width, rightRect.position.y);

            DrawLabel(yesText, leftRect, TextMargin);
            DrawLabel(noText, rightRect, TextMargin);

            leftRect.position = new Vector2(leftRect.position.x, leftRect.position.y + TextMargin);
            rightRect.position = new Vector2(rightRect.position.x, rightRect.position.y + TextMargin);

            int iconsPerRow = 1;
            bool change = false;
            int numSelected = 0;



            if (setting.Value == null)
            {
                setting.Value = new Dict2DRecordHandler();
            }

            if (setting.Value.InnerList == null)
            {
                Dictionary<String, Dictionary<String, Record>> factionRestrictionsDict = new Dictionary<String, Dictionary<String, Record>>();
                foreach (FactionDef factionDef in from td in DefDatabase<FactionDef>.AllDefs
                                                  where !td.isPlayer && td != FactionDefOf.Mechanoid && td != FactionDefOf.Insect
                                                  select td)
                {
                    if (!factionRestrictionsDict.ContainsKey(factionDef.defName))
                    {
                        factionRestrictionsDict.Add(factionDef.defName, new Dictionary<string, Record>());
                    }
                }
                setting.Value.InnerList = factionRestrictionsDict;
            }

            bool factionFound = setting.Value.InnerList.TryGetValue(filter.Value, out Dictionary<string, Record> selection);
            FilterSelection(ref selection, allAnimals);
            foreach (KeyValuePair<String, Record> item in selection)
            {
                if (item.Value.isSelected)
                {
                    numSelected++;
                }
            }
            Dictionary<String, Dictionary<String, Record>> innerDict = setting.Value.InnerList;
            int biggerRows = Math.Max( numSelected/ iconsPerRow, (selection.Count - numSelected) / iconsPerRow);
            float neededHightSelector = biggerRows * (rowHeight + BottomMargin) + TextMargin;
            float neededHightFilter = filterCount * buttonHeight + TextMargin;
            setting.CustomDrawerHeight = Math.Max(neededHightSelector, neededHightFilter);


            int indexLeft = 0;
            int indexRight = 0;
            foreach (KeyValuePair<String, Record> item in selection)
            {
                Rect rect = item.Value.isSelected ? leftRect : rightRect;
                int index = item.Value.isSelected ? indexLeft : indexRight;
                if (item.Value.isSelected)
                {
                    indexLeft++;
                }
                else
                {
                    indexRight++;
                }

                int collum = (index % iconsPerRow);
                int row = (index / iconsPerRow);
                bool interacted = DrawTileForAnimal(item, rect, new Vector2(0, rowHeight * row + row * BottomMargin), index);
                if (interacted)
                {
                    change = true;
                    item.Value.isSelected = !item.Value.isSelected;
                    item.Value.isException = !item.Value.isException;
                }
            }
            if (change)
            {
                setting.Value.InnerList[filter.Value] = selection;
                //setting.Value.InnerList = selection;
            }
            return change;
        }


    }
}



