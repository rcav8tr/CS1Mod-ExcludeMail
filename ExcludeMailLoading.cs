using ICities;
using UnityEngine;
using Harmony;
using System;
using ColossalFramework.UI;
using ColossalFramework;

namespace ExcludeMail
{
    /// <summary>
    /// handle game loading and unloading
    /// </summary>
    /// <remarks>A new instance of ExcludeMailLoading is NOT created when loading a game from the Pause Menu.</remarks>
    public class ExcludeMailLoading : LoadingExtensionBase
    {
        // the UI objects that will be added
        private static UISprite _includeMailCheckBox;
        private static UILabel _includeMailLabel;

        // UI objects that will be updated by this class
        private static UISprite _importLegendMailBox;
        private static UISprite _exportLegendMailBox;
        private static UILabel _importLegendMailLabel;
        private static UILabel _exportLegendMailLabel;

        // save original legend colors
        private static bool _legendColorsInitialized = false;
        private static Color32 _importLegendMailBoxOriginalColor;
        private static Color32 _exportLegendMailBoxOriginalColor;
        private static Color32 _importLegendMailLabelOriginalColor;
        private static Color32 _exportLegendMailLabelOriginalColor;

        public override void OnLevelLoaded(LoadMode mode)
        {
            // do base processing
            base.OnLevelLoaded(mode);

            try
            {
                // initialize only if user has Industries DLC
                if (SteamHelper.IsDLCOwned(SteamHelper.DLC.IndustryDLC))
                {
                    // check for new or loaded game
                    if (mode == LoadMode.NewGame || mode == LoadMode.NewGameFromScenario || mode == LoadMode.LoadGame)
                    {
                        // display warning message
                        ExceptionPanel panel = UIView.library.ShowModal<ExceptionPanel>("ExceptionPanel");
                        panel.SetMessage(
                            "Exclude Mail",
                            "The Exclude Mail mod is deprecated and will be deleted from the Steam workshop." + Environment.NewLine + Environment.NewLine +
                            "Please use the Enhanced Outside Connections View mod instead.",
                            true);

                        // initialize Harmony
                        ExcludeMail.harmony = HarmonyInstance.Create("com.github.rcav8tr.ExcludeMail");
                        if (ExcludeMail.harmony == null)
                        {
                            Debug.LogError("Unable to create Harmony instance.");
                            return;
                        }

                        // find Ingame atlas
                        UITextureAtlas ingameAtlas = null;
                        UITextureAtlas[] atlases = Resources.FindObjectsOfTypeAll(typeof(UITextureAtlas)) as UITextureAtlas[];
                        for (int i = 0; i < atlases.Length; i++)
                        {
                            if (atlases[i] != null)
                            {
                                if (atlases[i].name == "Ingame")
                                {
                                    ingameAtlas = atlases[i];
                                    break;
                                }
                            }
                        }
                        if (ingameAtlas == null)
                        {
                            Debug.LogError("Unable to find atlas [Ingame].");
                            return;
                        }

                        // get the OutsideConnectionsInfoViewPanel panel (displayed when the user clicks on the Outside Connections info view button)
                        OutsideConnectionsInfoViewPanel ocPanel = UIView.library.Get<OutsideConnectionsInfoViewPanel>(typeof(OutsideConnectionsInfoViewPanel).Name);
                        if (ocPanel == null)
                        {
                            Debug.LogError("Unable to find [OutsideConnectionsInfoViewPanel].");
                            return;
                        }

                        // find the import and export legend mail panels
                        UIPanel importLegendMailPanel = ocPanel.Find<UIPanel>("ResourceLegendMail");
                        if (importLegendMailPanel == null)
                        {
                            Debug.LogError($"Unable to find panel [ResourceLegendMail] on [OutsideConnectionsInfoViewPanel]");
                            return;
                        }
                        UIPanel exportLegendMailPanel = ocPanel.Find<UIPanel>("ResourceLegendMail2");
                        if (exportLegendMailPanel == null)
                        {
                            Debug.LogError($"Unable to find panel [ResourceLegendMail2] on [OutsideConnectionsInfoViewPanel]");
                            return;
                        }

                        // find the import and export legend mail boxes
                        _importLegendMailBox = importLegendMailPanel.Find<UISprite>("MailColor");
                        if (_importLegendMailBox == null)
                        {
                            Debug.LogError($"Unable to find import sprite [MailColor] on [ResourceLegendMail] on [OutsideConnectionsInfoViewPanel]");
                            return;
                        }
                        _exportLegendMailBox = exportLegendMailPanel.Find<UISprite>("MailColor");
                        if (_exportLegendMailBox == null)
                        {
                            Debug.LogError($"Unable to find export sprite [MailColor] on [ResourceLegendMail2] on [OutsideConnectionsInfoViewPanel]");
                            return;
                        }

                        // find the import and export legend mail labels
                        _importLegendMailLabel = importLegendMailPanel.Find<UILabel>("Type");
                        if (_importLegendMailLabel == null)
                        {
                            Debug.LogError($"Unable to find import label [Type] on [ResourceLegendMail] on [OutsideConnectionsInfoViewPanel]");
                            return;
                        }
                        _exportLegendMailLabel = exportLegendMailPanel.Find<UILabel>("Type");
                        if (_exportLegendMailLabel == null)
                        {
                            Debug.LogError($"Unable to find export label [Type] on [ResourceLegendMail2] on [OutsideConnectionsInfoViewPanel]");
                            return;
                        }

                        // find import total label
                        UILabel totalLabel = ocPanel.Find<UILabel>("ImportTotal");
                        if (totalLabel == null)
                        {
                            Debug.LogError("Unable to find label [ImportTotal] on [OutsideConnectionsInfoViewPanel].");
                            return;
                        }

                        // create the check box (i.e. a sprite)
                        _includeMailCheckBox = ocPanel.component.AddUIComponent<UISprite>();
                        if (_includeMailCheckBox == null)
                        {
                            Debug.LogError("Unable to create check box sprite on [OutsideConnectionsInfoViewPanel].");
                            return;
                        }
                        _includeMailCheckBox.name = "IncludeMailCheckBox";
                        _includeMailCheckBox.autoSize = false;
                        _includeMailCheckBox.size = new Vector2(totalLabel.size.y, totalLabel.size.y);    // width is same as height
                        _includeMailCheckBox.relativePosition = new Vector3(8f, 82f);
                        _includeMailCheckBox.atlas = ingameAtlas;
                        SetCheckBox(_includeMailCheckBox, true);
                        _includeMailCheckBox.isVisible = true;
                        _includeMailCheckBox.BringToFront();
                        _includeMailCheckBox.eventClicked += CheckBox_eventClicked;

                        // create the label and right align it to the info view
                        _includeMailLabel = ocPanel.component.AddUIComponent<UILabel>();
                        if (_includeMailLabel == null)
                        {
                            Debug.LogError("Unable to create label on [OutsideConnectionsInfoViewPanel].");
                            return;
                        }
                        _includeMailLabel.name = "IncludeMailLabel";
                        _includeMailLabel.text = "Include Mail";
                        _includeMailLabel.textAlignment = UIHorizontalAlignment.Left;
                        _includeMailLabel.verticalAlignment = UIVerticalAlignment.Top;
                        _includeMailLabel.font = totalLabel.font;
                        _includeMailLabel.textScale = totalLabel.textScale;
                        _includeMailLabel.textColor = totalLabel.textColor;
                        _includeMailLabel.autoSize = true;
                        _includeMailLabel.relativePosition = new Vector3(_includeMailCheckBox.relativePosition.x + _includeMailCheckBox.size.x + 5f, _includeMailCheckBox.relativePosition.y + 2.5f);
                        _includeMailLabel.isVisible = true;
                        _includeMailLabel.BringToFront();
                        _includeMailLabel.eventClicked += Label_eventClicked;

                        // create the patches
                        PostOfficeAIPatch.CreateGetColorPatch();
                        PostVanAIPatch.CreateGetColorPatch();
                        OutsideConnectionsInfoViewPanelPatch.CreateUpdatePanelPatch();

                        // legend colors are not initialized
                        _legendColorsInitialized = false;
                    }
                }
                else
                {
                    // show a nice message
                    ExceptionPanel panel = UIView.library.ShowModal<ExceptionPanel>("ExceptionPanel", true);
                    panel.SetMessage(
                        "Exclude Mail Mod",
                        "Industries DLC is not installed.  Having the Exclude Mail mod enabled without Industries DLC will not cause an error in the mod, but the functionality of the mod will not be available.",
                        false);
                }
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }
        }

        /// <summary>
        /// set the check box (i.e. sprite) status
        /// </summary>
        private void SetCheckBox(UISprite checkBox, bool value)
        {
            // change sprite based on value
            if (value)
            {
                // set check box to checked
                checkBox.spriteName = "check-checked";

                // set mail legends to original colors
                if (_legendColorsInitialized)
                {
                    _importLegendMailBox.color = _importLegendMailBoxOriginalColor;
                    _exportLegendMailBox.color = _exportLegendMailBoxOriginalColor;
                    _importLegendMailLabel.textColor = _importLegendMailLabelOriginalColor;
                    _exportLegendMailLabel.textColor = _exportLegendMailLabelOriginalColor;
                }
            }
            else
            {
                // set check box to unchecked
                checkBox.spriteName = "check-unchecked";

                // set mail legends to partial brightness of original colors
                if (_legendColorsInitialized)
                {
                    Color32 black = new Color32(0, 0, 0, 255);
                    float colorMultiplier = 0.5f;
                    _importLegendMailBox.color = Color32.Lerp(black, _importLegendMailBoxOriginalColor, colorMultiplier);
                    _exportLegendMailBox.color = Color32.Lerp(black, _exportLegendMailBoxOriginalColor, colorMultiplier);
                    _importLegendMailLabel.textColor = Color32.Lerp(black, _importLegendMailLabelOriginalColor, colorMultiplier);
                    _exportLegendMailLabel.textColor = Color32.Lerp(black, _exportLegendMailLabelOriginalColor, colorMultiplier);
                }
            }
        }

        /// <summary>
        /// handle clicked on check box
        /// </summary>
        private void CheckBox_eventClicked(UIComponent component, UIMouseEventParameter eventParam)
        {
            // save the original legend colors
            if (!_legendColorsInitialized)
            {
                _importLegendMailBoxOriginalColor = _importLegendMailBox.color;
                _exportLegendMailBoxOriginalColor = _exportLegendMailBox.color;
                _importLegendMailLabelOriginalColor = _importLegendMailLabel.textColor;
                _exportLegendMailLabelOriginalColor = _exportLegendMailLabel.textColor;
                _legendColorsInitialized = true;
            }

            // set check box to its opposite state
            SetCheckBox(_includeMailCheckBox, !IncludeMail());

            // update colors on all buildings
            Singleton<BuildingManager>.instance.UpdateBuildingColors();
        }

        /// <summary>
        /// handle clicked on label
        /// </summary>
        private void Label_eventClicked(UIComponent component, UIMouseEventParameter eventParam)
        {
            // same as clicked on check box
            CheckBox_eventClicked(_includeMailCheckBox, eventParam);
        }

        /// <summary>
        /// return whether or not the Include Mail check box is checked
        /// </summary>
        public static bool IncludeMail()
        {
            return _includeMailCheckBox.spriteName == "check-checked"; ;
        }

        public override void OnLevelUnloading()
        {
            // do base processing
            base.OnLevelUnloading();

            try
            {
                // remove Harmony patches
                if (ExcludeMail.harmony != null)
                {
                    ExcludeMail.harmony.UnpatchAll();
                    ExcludeMail.harmony = null;
                }

                // remove event handlers and destroy objects added directly to the OutsideConnectionsInfoViewPanel
                // must destroy objects explicitly because loading a saved game from the Pause Menu
                // does not destroy the objects implicitly like returning to the Main Menu to load a saved game
                if (_includeMailCheckBox != null)
                {
                    _includeMailCheckBox.eventClicked -= CheckBox_eventClicked;
                    UnityEngine.Object.Destroy(_includeMailCheckBox);
                    _includeMailCheckBox = null;
                }
                if (_includeMailLabel != null)
                {
                    _includeMailLabel.eventClicked -= Label_eventClicked;
                    UnityEngine.Object.Destroy(_includeMailLabel);
                    _includeMailLabel = null;
                }
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }
        }

    }
}