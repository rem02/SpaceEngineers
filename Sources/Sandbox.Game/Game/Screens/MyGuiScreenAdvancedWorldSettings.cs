using Sandbox.Engine.Utils;
using Sandbox.Game.Localization;
using Sandbox.Graphics.GUI;
using System;
using System.Diagnostics;
using System.Text;
using VRage;
using VRage.Game;
using VRage.Library.Utils;
using VRage.Utils;
using VRage.Voxels;
using VRageMath;

namespace Sandbox.Game.Gui
{
    class MyGuiScreenAdvancedWorldSettings : MyGuiScreenBase
    {
        public enum MyWorldSizeEnum
        {
            TEN_KM,
            TWENTY_KM,
            FIFTY_KM,
            HUNDRED_KM,
            UNLIMITED,
            CUSTOM,
        }

        public enum MyViewDistanceEnum
        {
            CUSTOM = 0,
            FIVE_KM = 5000,
            SEVEN_KM = 7000, // old default
            TEN_KM = 10000,
            FIFTEEN_KM = 15000,
            TWENTY_KM = 20000, // default
            THIRTY_KM = 30000,
            FORTY_KM = 40000,
            FIFTY_KM = 50000,
        }

        MyGuiScreenWorldSettings m_parent;
        bool m_isNewGame;

        public enum MyAdvancedSettingPageEnum
        {
            Environemnt = 0,
            Realism = 1,
            Advanced = 2,
        }

        MyGuiControlTabControl m_AdvancedSettingTabs;
        MyAdvancedSettingPageEnum m_initialPage;

        bool m_isConfirmed;
        bool m_showWarningForOxygen;
        bool m_recreating_control;

        //MyGuiControlTextbox m_passwordTextbox; //Is not here... 
        MyGuiControlCombobox /*m_onlineMode, m_environment, Is not here... in MyGuiScreenWorldSettings. */ m_worldSizeCombo, m_spawnShipTimeCombo, m_viewDistanceCombo, m_physicsOptionsCombo;
        MyGuiControlCheckbox m_autoHealing, m_clientCanSave, m_enableCopyPaste, m_weaponsEnabled, m_showPlayerNamesOnHud, m_thrusterDamage, m_cargoShipsEnabled, m_enableSpectator,
                             m_trashRemoval, m_respawnShipDelete, m_resetOwnership, m_permanentDeath, m_destructibleBlocks, m_enableIngameScripts, m_enableToolShake, m_enableOxygen, m_enableOxygenPressurization,
                             m_enable3rdPersonCamera, m_enableEncounters, m_disableRespawnShips, m_scenarioEditMode, m_enableConvertToStation, m_enableSunRotation, m_enableJetpack, 
                             m_spawnWithTools, m_startInRespawnScreen, m_enableVoxelDestruction, 
                             m_enableDrones,
                             m_enableWolfs,  
                             m_enableSpiders;

        MyGuiControlButton m_okButton, m_cancelButton, m_survivalModeButton, m_creativeModeButton, m_inventory_x1, m_inventory_x3, m_inventory_x10;
        MyGuiControlButton m_assembler_x1, m_assembler_x3, m_assembler_x10,
                           m_refinery_x1, m_refinery_x3, m_refinery_x10,
                           m_welder_half, m_welder_x1, m_welder_x2, m_welder_x5,
                           m_grinder_half, m_grinder_x1, m_grinder_x2, m_grinder_x5;
        MyGuiControlSlider m_maxPlayersSlider,m_sunRotationIntervalSlider;
        MyGuiControlLabel m_enableCopyPasteLabel, /* m_maxPlayersLabel, Is not here... in MyGuiScreenWorldSettings. */ m_maxFloatingObjectsLabel, 
            m_maxBackupSavesLabel, m_sunRotationPeriod, m_sunRotationPeriodValue, m_enableToolShakeLabel,
            m_enableDronesLabel,
            m_enableWolfsLabel,
            m_enableSpidersLabel;
        MyGuiControlSlider m_maxFloatingObjectsSlider;
        MyGuiControlSlider m_maxBackupSavesSlider;
        StringBuilder m_tempBuilder = new StringBuilder();
        Vector2 jumpLine = new Vector2(0f, 0.050f);
        int m_customWorldSize = 0;
        int m_customViewDistance = 20000;

        const int MIN_DAY_TIME_MINUTES = 1;
        const int MAX_DAY_TIME_MINUTES = 60 * 24;

        public string Password
        {
            get
            {
                return ""; // m_passwordTextbox.Text;
            }
        }

        public bool IsConfirmed
        {
            get
            {
                return m_isConfirmed;
            }
        }

        public MyGuiScreenAdvancedWorldSettings(MyGuiScreenWorldSettings parent)
            : base(new Vector2(0.5f, 0.5f), MyGuiConstants.SCREEN_BACKGROUND_COLOR, new Vector2(0.9f, 0.97f) )
        {
            MySandboxGame.Log.WriteLine("MyGuiScreenAdvancedWorldSettings.ctor START");

            m_parent = parent;
            EnabledBackgroundFade = true;

            m_isNewGame = (parent.Checkpoint == null);
            m_isConfirmed = false;

            RecreateControls(true);

            MySandboxGame.Log.WriteLine("MyGuiScreenAdvancedWorldSettings.ctor END");
        }

        //public static Vector2 CalcSize()
        //{
        //    return new Vector2(0.9f, 0.97f);
        //}

        public override void RecreateControls(bool constructor)
        {
            base.RecreateControls(constructor);
            m_recreating_control = true;
            BuildControls();
            LoadValues();
            m_recreating_control = false;
        }

        public void BuildControls()
        {
            CloseButtonEnabled = true;
            AddCaption(MyCommonTexts.ScreenCaptionAdvancedSettings);

            #region Game Type
            MyGuiControlLabel gameTypeLabel = new MyGuiControlLabel()
            {
                Text = MyTexts.GetString(MyCommonTexts.WorldSettings_GameMode),
                OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_CENTER
            };
            gameTypeLabel.Position = new Vector2(-0.3f, -0.36f);
            Controls.Add(gameTypeLabel);

            m_creativeModeButton = new MyGuiControlButton(visualStyle: MyGuiControlButtonStyleEnum.Small,
                                                          highlightType: MyGuiControlHighlightType.WHEN_ACTIVE,
                                                          text: MyTexts.Get(MyCommonTexts.WorldSettings_GameModeCreative),
                                                          onButtonClick: CreativeClicked);
            m_creativeModeButton.OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_CENTER;
            m_creativeModeButton.Position = gameTypeLabel.Position + new Vector2(0.15f, 0f);
            m_creativeModeButton.SetToolTip(MySpaceTexts.ToolTipWorldSettingsModeCreative);

            Controls.Add(m_creativeModeButton);

            m_survivalModeButton = new MyGuiControlButton(visualStyle: MyGuiControlButtonStyleEnum.Small,
                                                          highlightType: MyGuiControlHighlightType.WHEN_ACTIVE,
                                                          text: MyTexts.Get(MyCommonTexts.WorldSettings_GameModeSurvival),
                                                          onButtonClick: SurvivalClicked);
            m_survivalModeButton.SetToolTip(MySpaceTexts.ToolTipWorldSettingsModeSurvival);
            m_survivalModeButton.OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_CENTER;
            m_survivalModeButton.Position = m_creativeModeButton.Position + new Vector2(0.15f, 0f);
            Controls.Add(m_survivalModeButton);
            #endregion

            CreateTabs();

            // Ok/Cancel
            Vector2 buttonSize = MyGuiConstants.BACK_BUTTON_SIZE;
            Vector2 buttonsOrigin = m_size.Value / 2 - new Vector2(0.23f, 0.03f);
            m_okButton = new MyGuiControlButton(position: buttonsOrigin - new Vector2(0.01f, 0f), size: buttonSize, text: MyTexts.Get(MyCommonTexts.Ok), onButtonClick: OkButtonClicked, originAlign: MyGuiDrawAlignEnum.HORISONTAL_RIGHT_AND_VERTICAL_BOTTOM);
            m_cancelButton = new MyGuiControlButton(position: buttonsOrigin + new Vector2(0.01f, 0f), size: buttonSize, text: MyTexts.Get(MyCommonTexts.Cancel), onButtonClick: CancelButtonClicked, originAlign: MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_BOTTOM);

            Controls.Add(m_okButton);
            Controls.Add(m_cancelButton);
        }

        private void CreateTabs()
        {
            m_AdvancedSettingTabs = new MyGuiControlTabControl()
            {
                Position = new Vector2(0.12f, -0.30f),
                Size = new Vector2(Size.Value.X - 0.05f, Size.Value.Y),
                Name = "TerminalTabs",
                OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_TOP
            };

            MyGuiControlTabPage EnvironmentPage = m_AdvancedSettingTabs.GetTabSubControl(0);
            MyGuiControlTabPage RealismPage = m_AdvancedSettingTabs.GetTabSubControl(1);
            MyGuiControlTabPage AdvancedPage = m_AdvancedSettingTabs.GetTabSubControl(2);

            CreateEnvironmentPageControls(EnvironmentPage);
            CreateRealismPagePageControls(RealismPage);
            CreateAdvancedPagePageControls(AdvancedPage);

            m_initialPage = MyAdvancedSettingPageEnum.Environemnt;
            m_AdvancedSettingTabs.SelectedPage = (int)m_initialPage;

            Controls.Add(m_AdvancedSettingTabs);
        }

        /**
         * Set all controls for Environment Page
         */
        private void CreateEnvironmentPageControls(MyGuiControlTabPage page)
        {
            page.Name = "PageEnvironmentPage";
            page.Text = new StringBuilder("Environment");
            page.TextScale = 0.9f;

            float width = 0.284375f + 0.025f;
            Vector2 originL = -m_size.Value * 0.5f + new Vector2(0.03f, 0.08f);

            // First column
            Vector2 currentLine = originL;
            var worldSizeLabel = MakeLabel(page, currentLine, MySpaceTexts.WorldSettings_LimitWorldSize);
            currentLine += jumpLine;
            var viewDistanceLabel = MakeLabel(page, currentLine, MySpaceTexts.WorldSettings_ViewDistance);
            currentLine += jumpLine;
            var spawnShipTimeLabel = MakeLabel(page, currentLine, MySpaceTexts.WorldSettings_RespawnShipCooldown);
            currentLine += jumpLine;
            var enableSunRotationLabel = MakeLabel(page, currentLine, MySpaceTexts.WorldSettings_EnableSunRotation);
            currentLine += jumpLine;
            m_sunRotationPeriod = MakeLabel(page, currentLine, MySpaceTexts.SunRotationPeriod);
            currentLine += jumpLine;
            m_maxFloatingObjectsLabel = MakeLabel(page, currentLine, MySpaceTexts.MaxFloatingObjects);
            currentLine += jumpLine;
            var enableEncountersLabel = MakeLabel(page, currentLine, MySpaceTexts.WorldSettings_Encounters);
            currentLine += jumpLine;
            var destructibleBlocksLabel = MakeLabel(page, currentLine, MySpaceTexts.WorldSettings_DestructibleBlocks);            
            currentLine += jumpLine;
            var enableStationVoxelLabel = MakeLabel(page, currentLine, MySpaceTexts.WorldSettings_EnableConvertToStation);
            currentLine += jumpLine;
            var enableVoxelDestructionLabel = MakeLabel(page, currentLine, MySpaceTexts.WorldSettings_EnableVoxelDestruction);
            if (MyFakes.ENABLE_CARGO_SHIPS)
            {
                currentLine += jumpLine;
                var shipsEnabledLabel = MakeLabel(page, currentLine, MySpaceTexts.WorldSettings_EnableCargoShips);
            }

            // second column 
            m_enableDronesLabel = MakeLabel(page, currentLine, MySpaceTexts.WorldSettings_EnableDrones);
            m_enableSpidersLabel = MakeLabel(page, currentLine, MySpaceTexts.WorldSettings_EnableSpiders);
            m_enableWolfsLabel = MakeLabel(page, currentLine, MySpaceTexts.WorldSettings_EnableWolfs);

            // Find padding for next columm
            Vector2 paddingX = FindMaxSizeXLabel(page.Controls);

            // reset line position
            currentLine = originL;
            m_worldSizeCombo = new MyGuiControlCombobox(size: new Vector2(width, 0.04f));
            m_worldSizeCombo.AddItem((int)MyWorldSizeEnum.TEN_KM, MySpaceTexts.WorldSettings_WorldSize10Km);
            m_worldSizeCombo.AddItem((int)MyWorldSizeEnum.TWENTY_KM, MySpaceTexts.WorldSettings_WorldSize20Km);
            m_worldSizeCombo.AddItem((int)MyWorldSizeEnum.FIFTY_KM, MySpaceTexts.WorldSettings_WorldSize50Km);
            m_worldSizeCombo.AddItem((int)MyWorldSizeEnum.HUNDRED_KM, MySpaceTexts.WorldSettings_WorldSize100Km);
            m_worldSizeCombo.AddItem((int)MyWorldSizeEnum.UNLIMITED, MySpaceTexts.WorldSettings_WorldSizeUnlimited);
            m_worldSizeCombo.SetToolTip(MyTexts.GetString(MySpaceTexts.ToolTipWorldSettingsLimitWorldSize));
            m_worldSizeCombo.Position = ComputeControlPosition(currentLine, m_worldSizeCombo, paddingX);
            page.Controls.Add(m_worldSizeCombo);

            currentLine += jumpLine;
            m_viewDistanceCombo = new MyGuiControlCombobox(size: new Vector2(width, 0.04f));
            m_viewDistanceCombo.AddItem((int)MyViewDistanceEnum.FIVE_KM, MySpaceTexts.WorldSettings_ViewDistance_5_Km);
            m_viewDistanceCombo.AddItem((int)MyViewDistanceEnum.SEVEN_KM, MySpaceTexts.WorldSettings_ViewDistance_7_Km);
            m_viewDistanceCombo.AddItem((int)MyViewDistanceEnum.TEN_KM, MySpaceTexts.WorldSettings_ViewDistance_10_Km);
            m_viewDistanceCombo.AddItem((int)MyViewDistanceEnum.FIFTEEN_KM, MySpaceTexts.WorldSettings_ViewDistance_15_Km);
            m_viewDistanceCombo.AddItem((int)MyViewDistanceEnum.TWENTY_KM, MySpaceTexts.WorldSettings_ViewDistance_20_Km);
            m_viewDistanceCombo.AddItem((int)MyViewDistanceEnum.THIRTY_KM, MySpaceTexts.WorldSettings_ViewDistance_30_Km);
            m_viewDistanceCombo.AddItem((int)MyViewDistanceEnum.FORTY_KM, MySpaceTexts.WorldSettings_ViewDistance_40_Km);
            m_viewDistanceCombo.AddItem((int)MyViewDistanceEnum.FIFTY_KM, MySpaceTexts.WorldSettings_ViewDistance_50_Km);
            m_viewDistanceCombo.SetToolTip(MyTexts.GetString(MySpaceTexts.ToolTipWorldSettingsViewDistance));
            m_viewDistanceCombo.Position = ComputeControlPosition(currentLine, m_viewDistanceCombo, paddingX);
            page.Controls.Add(m_viewDistanceCombo);

            currentLine += jumpLine;
            m_spawnShipTimeCombo = new MyGuiControlCombobox(size: new Vector2(width, 0.04f));
            m_spawnShipTimeCombo.AddItem((int)0, MySpaceTexts.WorldSettings_RespawnShip_CooldownsDisabled);
            m_spawnShipTimeCombo.AddItem((int)1, MySpaceTexts.WorldSettings_RespawnShip_x01);
            m_spawnShipTimeCombo.AddItem((int)2, MySpaceTexts.WorldSettings_RespawnShip_x02);
            m_spawnShipTimeCombo.AddItem((int)5, MySpaceTexts.WorldSettings_RespawnShip_x05);
            m_spawnShipTimeCombo.AddItem((int)10, MySpaceTexts.WorldSettings_RespawnShip_Default);
            m_spawnShipTimeCombo.AddItem((int)20, MySpaceTexts.WorldSettings_RespawnShip_x2);
            m_spawnShipTimeCombo.AddItem((int)50, MySpaceTexts.WorldSettings_RespawnShip_x5);
            m_spawnShipTimeCombo.AddItem((int)100, MySpaceTexts.WorldSettings_RespawnShip_x10);
            m_spawnShipTimeCombo.AddItem((int)200, MySpaceTexts.WorldSettings_RespawnShip_x20);
            m_spawnShipTimeCombo.AddItem((int)500, MySpaceTexts.WorldSettings_RespawnShip_x50);
            m_spawnShipTimeCombo.AddItem((int)1000, MySpaceTexts.WorldSettings_RespawnShip_x100);
            m_spawnShipTimeCombo.Position = ComputeControlPosition(currentLine, m_spawnShipTimeCombo, paddingX);
            page.Controls.Add(m_spawnShipTimeCombo);

            currentLine += jumpLine;
            m_enableSunRotation = new MyGuiControlCheckbox();
            m_enableSunRotation.IsCheckedChanged = (control) =>
            {
                m_sunRotationIntervalSlider.Enabled = control.IsChecked;
                m_sunRotationPeriodValue.Visible = control.IsChecked;
            };
            m_enableSunRotation.Position = ComputeControlPosition(currentLine, m_enableSunRotation, paddingX);
            page.Controls.Add(m_enableSunRotation);

            currentLine += jumpLine;
            m_sunRotationIntervalSlider = new MyGuiControlSlider(width: width, labelSpaceWidth: 0.05f);
            m_sunRotationIntervalSlider.MinValue = 0;
            m_sunRotationIntervalSlider.MaxValue = 1;
            m_sunRotationIntervalSlider.DefaultValue = 0;
            m_sunRotationIntervalSlider.ValueChanged += (MyGuiControlSlider s) =>
            {
                m_tempBuilder.Clear();
                MyValueFormatter.AppendTimeInBestUnit(MathHelper.Clamp(MathHelper.InterpLog(s.Value, MIN_DAY_TIME_MINUTES, MAX_DAY_TIME_MINUTES), MIN_DAY_TIME_MINUTES, MAX_DAY_TIME_MINUTES) * 60, m_tempBuilder);
                m_sunRotationPeriodValue.Text = m_tempBuilder.ToString();
            };
            m_sunRotationIntervalSlider.Position = ComputeControlPosition(currentLine, m_sunRotationIntervalSlider, paddingX);
            page.Controls.Add(m_sunRotationIntervalSlider);
            m_sunRotationPeriodValue = MakeLabel(page, m_sunRotationIntervalSlider.Position + new Vector2(0.12f, 0f), MySpaceTexts.SunRotationPeriod);

            currentLine += jumpLine;
            m_maxFloatingObjectsSlider = new MyGuiControlSlider(
                width: width,
                minValue: 16,
                maxValue: 1024,
                labelText: new StringBuilder("{0}").ToString(),
                labelDecimalPlaces: 0,
                labelSpaceWidth: 0.05f,
                intValue: true
                );
            m_maxFloatingObjectsSlider.Position = ComputeControlPosition(currentLine, m_maxFloatingObjectsSlider, paddingX);
            m_maxFloatingObjectsSlider.SetToolTip(MyTexts.GetString(MySpaceTexts.ToolTipWorldSettingsMaxFloatingObjects));
            page.Controls.Add(m_maxFloatingObjectsSlider);
       
            currentLine += jumpLine;
            m_enableEncounters = new MyGuiControlCheckbox();
            m_enableEncounters.Position = ComputeControlPosition(currentLine, m_enableEncounters, paddingX);
            page.Controls.Add(m_enableEncounters);

            Vector2 startColum1 = m_enableEncounters.Position + new Vector2(0.05f, 0f);
            m_enableDronesLabel.Position = startColum1;
            m_enableDrones = new MyGuiControlCheckbox();
            m_enableDrones.SetToolTip(MyTexts.GetString(MySpaceTexts.ToolTipWorldSettings_EnableDrones));
            m_enableDrones.Position = ComputeControlPosition(startColum1, m_enableDrones, paddingX);
            page.Controls.Add(m_enableDrones);

            currentLine += jumpLine;
            m_destructibleBlocks = new MyGuiControlCheckbox();
            m_destructibleBlocks.SetToolTip(MyTexts.GetString(MySpaceTexts.ToolTipWorldSettingsDestructibleBlocks));
            m_destructibleBlocks.Position = ComputeControlPosition(currentLine, m_destructibleBlocks, paddingX);
            page.Controls.Add(m_destructibleBlocks);

            startColum1 += jumpLine;
            m_enableSpidersLabel.Position = startColum1;
            m_enableSpiders = new MyGuiControlCheckbox();
            m_enableSpiders.SetToolTip(MyTexts.GetString(MySpaceTexts.ToolTipWorldSettings_EnableSpiders));
            m_enableSpiders.Position = ComputeControlPosition(startColum1, m_enableSpiders, paddingX);
            page.Controls.Add(m_enableSpiders);

            currentLine += jumpLine;
            m_enableConvertToStation = new MyGuiControlCheckbox();
            m_enableConvertToStation.SetToolTip(MyTexts.GetString(MySpaceTexts.ToolTipWorldSettings_EnableConvertToStation));
            m_enableConvertToStation.Position = ComputeControlPosition(currentLine, m_enableConvertToStation, paddingX);
            page.Controls.Add(m_enableConvertToStation);

            startColum1 += jumpLine;
            m_enableWolfsLabel.Position = startColum1;
            m_enableWolfs = new MyGuiControlCheckbox();
            m_enableWolfs.SetToolTip(MyTexts.GetString(MySpaceTexts.ToolTipWorldSettings_EnableWolfs));
            m_enableWolfs.Position = ComputeControlPosition(startColum1, m_enableWolfs, paddingX);
            page.Controls.Add(m_enableWolfs);

            currentLine += jumpLine;
            m_enableVoxelDestruction = new MyGuiControlCheckbox();
            m_enableVoxelDestruction.SetToolTip(MyTexts.GetString(MySpaceTexts.ToolTipWorldSettings_EnableVoxelDestruction));
            m_enableVoxelDestruction.Position = ComputeControlPosition(currentLine, m_enableVoxelDestruction, paddingX);
            page.Controls.Add(m_enableVoxelDestruction);

            if (MyFakes.ENABLE_CARGO_SHIPS)
            {
                currentLine += jumpLine;
                m_cargoShipsEnabled = new MyGuiControlCheckbox();
                m_cargoShipsEnabled.SetToolTip(MyTexts.GetString(MySpaceTexts.ToolTipWorldSettingsEnableCargoShips));
                m_cargoShipsEnabled.Position = ComputeControlPosition(currentLine, m_cargoShipsEnabled, paddingX);
                page.Controls.Add(m_cargoShipsEnabled);
            }

        }

        /**
         * Set all controls for Realism Page
         */
        private void CreateRealismPagePageControls(MyGuiControlTabPage page)
        {
            page.Name = "PageRealism";
            page.Text = new StringBuilder("Realism");
            page.TextScale = 0.9f;

            Vector2 originL = -m_size.Value * 0.5f + new Vector2(0.03f, 0.08f);

            // Column 1
            Vector2 currentLine = originL;
            var inventorySizeLabel = MakeLabel(page, currentLine, MySpaceTexts.WorldSettings_InventorySize);
            currentLine += jumpLine;
            var assemblerEfficiencyLabel = MakeLabel(page, currentLine, MySpaceTexts.WorldSettings_AssemblerEfficiency);
            currentLine += jumpLine;
            var refineryEfficiencyLabel = MakeLabel(page, currentLine, MySpaceTexts.WorldSettings_RefinerySpeed);
            currentLine += jumpLine;
            var weldingSpeedLabel = MakeLabel(page, currentLine, MySpaceTexts.WorldSettings_WelderSpeed);
            currentLine += jumpLine;
            var grindingSpeedLabel = MakeLabel(page, currentLine, MySpaceTexts.WorldSettings_GrinderSpeed);
            currentLine += jumpLine;
            var permanentDeathLabel = MakeLabel(page, currentLine, MySpaceTexts.WorldSettings_PermanentDeath);
            currentLine += jumpLine;
            var autoHealingLabel = MakeLabel(page, currentLine, MySpaceTexts.WorldSettings_AutoHealing);
            currentLine += jumpLine;
            var oxygenLabel = MakeLabel(page, currentLine, MySpaceTexts.World_Settings_EnableOxygen);
            currentLine += jumpLine;
            var enableWeaponsLabel = MakeLabel(page, currentLine, MySpaceTexts.WorldSettings_EnableWeapons);
            currentLine += jumpLine;
            var thrusterDamageLabel = MakeLabel(page, currentLine, MySpaceTexts.WorldSettings_ThrusterDamage);

            // Colunm 2
            var resetOwnershipLabel = MakeLabel(page, currentLine, MySpaceTexts.WorldSettings_ResetOwnership);
            var enableJetpackLabel = MakeLabel(page, currentLine, MySpaceTexts.WorldSettings_EnableJetpack);
            var oxygenPressurizationLabel = MakeLabel(page, currentLine, MySpaceTexts.World_Settings_EnableOxygenPressurization);
            var spawnWithToolsLabel = MakeLabel(page, currentLine, MySpaceTexts.WorldSettings_SpawnWithTools);
            if (MyFakes.ENABLE_TOOL_SHAKE)
            {
                m_enableToolShakeLabel = MakeLabel(page, currentLine, MySpaceTexts.WorldSettings_EnableToolShake);
            }


            // Find padding for next columm
            Vector2 paddingX = FindMaxSizeXLabel(page.Controls);

            // reset line position
            currentLine = originL;
            m_inventory_x1 = new MyGuiControlButton(visualStyle: MyGuiControlButtonStyleEnum.Small, highlightType: MyGuiControlHighlightType.WHEN_ACTIVE, text: MyTexts.Get(MySpaceTexts.WorldSettings_Realistic), onButtonClick: OnInventoryClick, originAlign: MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_CENTER);
            m_inventory_x3 = new MyGuiControlButton(visualStyle: MyGuiControlButtonStyleEnum.Tiny, highlightType: MyGuiControlHighlightType.WHEN_ACTIVE, text: MyTexts.Get(MySpaceTexts.WorldSettings_Realistic_x3), onButtonClick: OnInventoryClick, originAlign: MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_CENTER);
            m_inventory_x10 = new MyGuiControlButton(visualStyle: MyGuiControlButtonStyleEnum.Tiny, highlightType: MyGuiControlHighlightType.WHEN_ACTIVE, text: MyTexts.Get(MySpaceTexts.WorldSettings_Realistic_x10), onButtonClick: OnInventoryClick, originAlign: MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_CENTER);
            m_inventory_x1.SetToolTip(MySpaceTexts.ToolTipWorldSettings_Inventory_x1);
            m_inventory_x3.SetToolTip(MySpaceTexts.ToolTipWorldSettings_Inventory_x3);
            m_inventory_x10.SetToolTip(MySpaceTexts.ToolTipWorldSettings_Inventory_x10);
            m_inventory_x1.UserData = 1.0f;
            m_inventory_x3.UserData = 3.0f;
            m_inventory_x10.UserData = 10.0f;
            m_inventory_x1.Position = ComputeControlPosition(currentLine, m_inventory_x1, paddingX, 0f);
            m_inventory_x3.Position = m_inventory_x1.Position + new Vector2(m_inventory_x1.Size.X + 0.017f, 0);
            m_inventory_x10.Position = m_inventory_x3.Position + new Vector2(m_inventory_x3.Size.X + 0.017f, 0);
            page.Controls.Add(m_inventory_x1);
            page.Controls.Add(m_inventory_x3);
            page.Controls.Add(m_inventory_x10);

            currentLine += jumpLine;
            m_assembler_x1 = new MyGuiControlButton(visualStyle: MyGuiControlButtonStyleEnum.Small, highlightType: MyGuiControlHighlightType.WHEN_ACTIVE, text: MyTexts.Get(MySpaceTexts.WorldSettings_Realistic), onButtonClick: OnAssemblerClick, originAlign: MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_CENTER);
            m_assembler_x3 = new MyGuiControlButton(visualStyle: MyGuiControlButtonStyleEnum.Tiny, highlightType: MyGuiControlHighlightType.WHEN_ACTIVE, text: MyTexts.Get(MySpaceTexts.WorldSettings_Realistic_x3), onButtonClick: OnAssemblerClick, originAlign: MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_CENTER);
            m_assembler_x10 = new MyGuiControlButton(visualStyle: MyGuiControlButtonStyleEnum.Tiny, highlightType: MyGuiControlHighlightType.WHEN_ACTIVE, text: MyTexts.Get(MySpaceTexts.WorldSettings_Realistic_x10), onButtonClick: OnAssemblerClick, originAlign: MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_CENTER);
            m_assembler_x1.SetToolTip(MySpaceTexts.ToolTipWorldSettings_Assembler_x1);
            m_assembler_x3.SetToolTip(MySpaceTexts.ToolTipWorldSettings_Assembler_x3);
            m_assembler_x10.SetToolTip(MySpaceTexts.ToolTipWorldSettings_Assembler_x10);
            m_assembler_x1.UserData = 1.0f;
            m_assembler_x3.UserData = 3.0f;
            m_assembler_x10.UserData = 10.0f;
            m_assembler_x1.Position = ComputeControlPosition(currentLine, m_assembler_x1, paddingX, 0f);
            m_assembler_x3.Position = m_assembler_x1.Position + new Vector2(m_assembler_x1.Size.X + 0.017f, 0);
            m_assembler_x10.Position = m_assembler_x3.Position + new Vector2(m_assembler_x3.Size.X + 0.017f, 0);
            page.Controls.Add(m_assembler_x1);
            page.Controls.Add(m_assembler_x3);
            page.Controls.Add(m_assembler_x10);

            currentLine += jumpLine;
            m_refinery_x1 = new MyGuiControlButton(visualStyle: MyGuiControlButtonStyleEnum.Small, highlightType: MyGuiControlHighlightType.WHEN_ACTIVE, text: MyTexts.Get(MySpaceTexts.WorldSettings_Realistic), onButtonClick: OnRefineryClick, originAlign: MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_CENTER);
            m_refinery_x3 = new MyGuiControlButton(visualStyle: MyGuiControlButtonStyleEnum.Tiny, highlightType: MyGuiControlHighlightType.WHEN_ACTIVE, text: MyTexts.Get(MySpaceTexts.WorldSettings_Realistic_x3), onButtonClick: OnRefineryClick, originAlign: MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_CENTER);
            m_refinery_x10 = new MyGuiControlButton(visualStyle: MyGuiControlButtonStyleEnum.Tiny, highlightType: MyGuiControlHighlightType.WHEN_ACTIVE, text: MyTexts.Get(MySpaceTexts.WorldSettings_Realistic_x10), onButtonClick: OnRefineryClick, originAlign: MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_CENTER);
            m_refinery_x1.UserData = 1.0f;
            m_refinery_x3.UserData = 3.0f;
            m_refinery_x10.UserData = 10.0f;
            m_refinery_x1.SetToolTip(MySpaceTexts.ToolTipWorldSettings_Refinery_x1);
            m_refinery_x3.SetToolTip(MySpaceTexts.ToolTipWorldSettings_Refinery_x3);
            m_refinery_x10.SetToolTip(MySpaceTexts.ToolTipWorldSettings_Refinery_x10);
            m_refinery_x1.Position = ComputeControlPosition(currentLine, m_refinery_x1, paddingX, 0f);
            m_refinery_x3.Position = m_refinery_x1.Position + new Vector2(m_refinery_x1.Size.X + 0.017f, 0);
            m_refinery_x10.Position = m_refinery_x3.Position + new Vector2(m_refinery_x3.Size.X + 0.017f, 0);
            page.Controls.Add(m_refinery_x1);
            page.Controls.Add(m_refinery_x3);
            page.Controls.Add(m_refinery_x10);

            currentLine += jumpLine;
            m_welder_half = new MyGuiControlButton(visualStyle: MyGuiControlButtonStyleEnum.Tiny, highlightType: MyGuiControlHighlightType.WHEN_ACTIVE, text: MyTexts.Get(MySpaceTexts.WorldSettings_Realistic_half), onButtonClick: OnWelderClick, originAlign: MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_CENTER, textScale: MyGuiConstants.HUD_TEXT_SCALE);
            m_welder_x1 = new MyGuiControlButton(visualStyle: MyGuiControlButtonStyleEnum.Small, highlightType: MyGuiControlHighlightType.WHEN_ACTIVE, text: MyTexts.Get(MySpaceTexts.WorldSettings_Realistic), onButtonClick: OnWelderClick, originAlign: MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_CENTER);
            m_welder_x2 = new MyGuiControlButton(visualStyle: MyGuiControlButtonStyleEnum.Tiny, highlightType: MyGuiControlHighlightType.WHEN_ACTIVE, text: MyTexts.Get(MySpaceTexts.WorldSettings_Realistic_x2), onButtonClick: OnWelderClick, originAlign: MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_CENTER);
            m_welder_x5 = new MyGuiControlButton(visualStyle: MyGuiControlButtonStyleEnum.Tiny, highlightType: MyGuiControlHighlightType.WHEN_ACTIVE, text: MyTexts.Get(MySpaceTexts.WorldSettings_Realistic_x5), onButtonClick: OnWelderClick, originAlign: MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_CENTER);
            m_welder_half.UserData = 0.5f;
            m_welder_x1.UserData = 1.0f;
            m_welder_x2.UserData = 2.0f;
            m_welder_x5.UserData = 5.0f;
            m_welder_half.SetToolTip(MySpaceTexts.ToolTipWorldSettings_Welder_half);
            m_welder_x1.SetToolTip(MySpaceTexts.ToolTipWorldSettings_Welder_x1);
            m_welder_x2.SetToolTip(MySpaceTexts.ToolTipWorldSettings_Welder_x2);
            m_welder_x5.SetToolTip(MySpaceTexts.ToolTipWorldSettings_Welder_x5);
            m_welder_x1.Position = ComputeControlPosition(currentLine, m_welder_x1, paddingX, 0f);
            m_welder_half.Position = m_welder_x1.Position + new Vector2(m_welder_x1.Size.X + 0.017f, 0);
            m_welder_x2.Position = m_welder_half.Position + new Vector2(m_welder_half.Size.X + 0.017f, 0);
            m_welder_x5.Position = m_welder_x2.Position + new Vector2(m_welder_x2.Size.X + 0.017f, 0);
            page.Controls.Add(m_welder_half);
            page.Controls.Add(m_welder_x1);
            page.Controls.Add(m_welder_x2);
            page.Controls.Add(m_welder_x5);

            currentLine += jumpLine;
            m_grinder_half = new MyGuiControlButton(visualStyle: MyGuiControlButtonStyleEnum.Tiny, highlightType: MyGuiControlHighlightType.WHEN_ACTIVE, text: MyTexts.Get(MySpaceTexts.WorldSettings_Realistic_half), onButtonClick: OnGrinderClick, originAlign: MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_CENTER, textScale: MyGuiConstants.HUD_TEXT_SCALE);
            m_grinder_x1 = new MyGuiControlButton(visualStyle: MyGuiControlButtonStyleEnum.Small, highlightType: MyGuiControlHighlightType.WHEN_ACTIVE, text: MyTexts.Get(MySpaceTexts.WorldSettings_Realistic), onButtonClick: OnGrinderClick, originAlign: MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_CENTER);
            m_grinder_x2 = new MyGuiControlButton(visualStyle: MyGuiControlButtonStyleEnum.Tiny, highlightType: MyGuiControlHighlightType.WHEN_ACTIVE, text: MyTexts.Get(MySpaceTexts.WorldSettings_Realistic_x2), onButtonClick: OnGrinderClick, originAlign: MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_CENTER);
            m_grinder_x5 = new MyGuiControlButton(visualStyle: MyGuiControlButtonStyleEnum.Tiny, highlightType: MyGuiControlHighlightType.WHEN_ACTIVE, text: MyTexts.Get(MySpaceTexts.WorldSettings_Realistic_x5), onButtonClick: OnGrinderClick, originAlign: MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_CENTER);
            m_grinder_half.UserData = 0.5f;
            m_grinder_x1.UserData = 1.0f;
            m_grinder_x2.UserData = 2.0f;
            m_grinder_x5.UserData = 5.0f;
            m_grinder_half.SetToolTip(MySpaceTexts.ToolTipWorldSettings_Grinder_half);
            m_grinder_x1.SetToolTip(MySpaceTexts.ToolTipWorldSettings_Grinder_x1);
            m_grinder_x2.SetToolTip(MySpaceTexts.ToolTipWorldSettings_Grinder_x2);
            m_grinder_x5.SetToolTip(MySpaceTexts.ToolTipWorldSettings_Grinder_x5);
            m_grinder_x1.Position = ComputeControlPosition(currentLine, m_grinder_x1, paddingX, 0f);
            m_grinder_half.Position = m_grinder_x1.Position + new Vector2(m_grinder_x1.Size.X + 0.017f, 0);
            m_grinder_x2.Position = m_grinder_half.Position + new Vector2(m_grinder_half.Size.X + 0.017f, 0);
            m_grinder_x5.Position = m_grinder_x2.Position + new Vector2(m_grinder_x2.Size.X + 0.017f, 0);
            page.Controls.Add(m_grinder_half);
            page.Controls.Add(m_grinder_x1);
            page.Controls.Add(m_grinder_x2);
            page.Controls.Add(m_grinder_x5);

            currentLine += jumpLine;
            m_permanentDeath = new MyGuiControlCheckbox();
            m_permanentDeath.SetToolTip(MyTexts.GetString(MySpaceTexts.ToolTipWorldSettingsPermanentDeath));
            m_permanentDeath.Position = ComputeControlPosition(currentLine, m_permanentDeath, paddingX);
            page.Controls.Add(m_permanentDeath);
   
            Vector2 startColum1 = m_permanentDeath.Position + new Vector2(0.05f, 0f);
            resetOwnershipLabel.Position = startColum1;
            m_resetOwnership = new MyGuiControlCheckbox();
            m_resetOwnership.SetToolTip(MyTexts.GetString(MySpaceTexts.ToolTipWorldSettingsResetOwnership));
            m_resetOwnership.Position = ComputeControlPosition(startColum1, m_resetOwnership, paddingX);
            page.Controls.Add(m_resetOwnership);

            currentLine += jumpLine;
            autoHealingLabel.Position = currentLine;
            m_autoHealing = new MyGuiControlCheckbox();
            m_autoHealing.SetToolTip(MyTexts.GetString(MySpaceTexts.ToolTipWorldSettingsAutoHealing));
            m_autoHealing.Position = ComputeControlPosition(currentLine, m_autoHealing, paddingX);
            page.Controls.Add(m_autoHealing);

            startColum1 += jumpLine;
            enableJetpackLabel.Position = startColum1;
            m_enableJetpack = new MyGuiControlCheckbox();
            m_enableJetpack.SetToolTip(MyTexts.GetString(MySpaceTexts.ToolTipWorldSettings_EnableJetpack));
            m_enableJetpack.Position = ComputeControlPosition(startColum1, m_enableJetpack, paddingX);
            page.Controls.Add(m_enableJetpack);
            
            currentLine += jumpLine;
            oxygenLabel.Position = currentLine;
            m_enableOxygen = new MyGuiControlCheckbox();
            m_enableOxygen.SetToolTip(MyTexts.GetString(MySpaceTexts.ToolTipWorldSettings_EnableOxygen));
            m_enableOxygen.IsCheckedChanged = (x) =>
            {
                if (m_showWarningForOxygen && x.IsChecked)
                {
                    MyGuiSandbox.AddScreen(MyGuiSandbox.CreateMessageBox(
                                buttonType: MyMessageBoxButtonsType.YES_NO,
                                messageText: MyTexts.Get(MySpaceTexts.MessageBoxTextAreYouSureEnableOxygen),
                                messageCaption: MyTexts.Get(MyCommonTexts.MessageBoxCaptionPleaseConfirm),
                                callback: (v) =>
                                {
                                    if (v == MyGuiScreenMessageBox.ResultEnum.NO)
                                    {
                                        x.IsChecked = false;
                                    }
                                }));
                }
                if (!x.IsChecked)
                {
                    m_enableOxygenPressurization.IsChecked = false;
                    m_enableOxygenPressurization.Enabled = false;
                    oxygenPressurizationLabel.Enabled = false;
                }
                else
                {
                    m_enableOxygenPressurization.Enabled = true;
                    oxygenPressurizationLabel.Enabled = true;
                }
            };
            m_enableOxygen.Position = ComputeControlPosition(currentLine, m_enableOxygen, paddingX);
            page.Controls.Add(m_enableOxygen);

            startColum1 += jumpLine;
            oxygenPressurizationLabel.Position = startColum1;
            m_enableOxygenPressurization = new MyGuiControlCheckbox();
            m_enableOxygenPressurization.IsCheckedChanged = (x) =>
            {
                if (x.IsChecked && !m_recreating_control)
                {
                    MyGuiSandbox.AddScreen(MyGuiSandbox.CreateMessageBox(
                        buttonType: MyMessageBoxButtonsType.YES_NO,
                        messageText: MyTexts.Get(MySpaceTexts.MessageBoxTextAreYouSureEnableOxygenPressurization),
                        messageCaption: MyTexts.Get(MyCommonTexts.MessageBoxCaptionPleaseConfirm),
                        callback: (v) =>
                        {
                            if (v == MyGuiScreenMessageBox.ResultEnum.NO)
                            {
                                x.IsChecked = false;
                            }
                        }
                        ));
                }
            };
            if (!m_enableOxygen.IsChecked)
            {
                m_enableOxygenPressurization.Enabled = false;
                oxygenPressurizationLabel.Enabled = false;
            }
            m_enableOxygenPressurization.SetToolTip(MyTexts.GetString(MySpaceTexts.ToolTipWorldSettings_EnableOxygenPressurization));
            m_enableOxygenPressurization.Position = ComputeControlPosition(startColum1, m_enableOxygenPressurization, paddingX);
            page.Controls.Add(m_enableOxygenPressurization);

            currentLine += jumpLine;
            enableWeaponsLabel.Position = currentLine;
            m_weaponsEnabled = new MyGuiControlCheckbox();
            m_weaponsEnabled.SetToolTip(MyTexts.GetString(MySpaceTexts.ToolTipWorldSettingsWeapons));
            m_weaponsEnabled.Position = ComputeControlPosition(currentLine, m_weaponsEnabled, paddingX);
            page.Controls.Add(m_weaponsEnabled);
            
            startColum1 += jumpLine;
            spawnWithToolsLabel.Position = startColum1;
            m_spawnWithTools = new MyGuiControlCheckbox();
            m_spawnWithTools.SetToolTip(MyTexts.GetString(MySpaceTexts.ToolTipWorldSettings_SpawnWithTools));
            m_spawnWithTools.Position = ComputeControlPosition(startColum1, m_spawnWithTools, paddingX);
            page.Controls.Add(m_spawnWithTools);

            currentLine += jumpLine;
            thrusterDamageLabel.Position = currentLine;
            m_thrusterDamage = new MyGuiControlCheckbox();
            m_thrusterDamage.SetToolTip(MyTexts.GetString(MySpaceTexts.ToolTipWorldSettingsThrusterDamage));
            m_thrusterDamage.Position = ComputeControlPosition(currentLine, m_thrusterDamage, paddingX);
            page.Controls.Add(m_thrusterDamage);

            if (MyFakes.ENABLE_TOOL_SHAKE)
            {
                startColum1 += jumpLine;
                m_enableToolShakeLabel.Position = startColum1;
                m_enableToolShake = new MyGuiControlCheckbox();
                m_enableToolShake.SetToolTip(MyTexts.GetString(MySpaceTexts.ToolTipWorldSettings_ToolShake));
                m_enableToolShake.Position = ComputeControlPosition(startColum1, m_enableToolShake, paddingX);
                page.Controls.Add(m_enableToolShake);
            }
        }

        /**
         * Set all controls for Advanced Page
         */
        private void CreateAdvancedPagePageControls(MyGuiControlTabPage page)
        {
            page.Name = "PageAdvanced";
            page.Text = new StringBuilder("Advanced");
            page.TextScale = 0.9f;
            float width = 0.284375f + 0.025f;
            Vector2 originL = -m_size.Value * 0.5f + new Vector2(0.03f, 0.08f);

            // Label first !
            Vector2 currentLine = originL;
            m_maxBackupSavesLabel = MakeLabel(page, currentLine, MySpaceTexts.MaxBackupSaves);
            currentLine += jumpLine;
            var enableSpectatorLabel = MakeLabel(page, currentLine, MySpaceTexts.WorldSettings_EnableSpectator);
            if (MyFakes.ENABLE_PROGRAMMABLE_BLOCK)
            {
                currentLine += jumpLine;
                var enableIngameScriptsLabel = MakeLabel(page, currentLine, MySpaceTexts.WorldSettings_EnableIngameScripts);
            }
            currentLine += jumpLine;
            m_enableCopyPasteLabel = MakeLabel(page, currentLine, MySpaceTexts.WorldSettings_EnableCopyPaste);
            currentLine += jumpLine;
            var enable3rdPersonCameraLabel = MakeLabel(page, currentLine, MySpaceTexts.WorldSettings_Enable3rdPersonCamera);
            currentLine += jumpLine;
            var showPlayerNamesOnHudLabel = MakeLabel(page, currentLine, MySpaceTexts.WorldSettings_ShowPlayerNamesOnHud);
            currentLine += jumpLine;
            var clientCanSaveLabel = MakeLabel(page, currentLine, MySpaceTexts.WorldSettings_ClientCanSave);
            currentLine += jumpLine;
            var disableRespawnShipsLabel = MakeLabel(page, currentLine, MySpaceTexts.WorldSettings_DisableRespawnShips);
            currentLine += jumpLine;
            var respawnShipDeleteLabel = MakeLabel(page, currentLine, MySpaceTexts.WorldSettings_RespawnShipDelete);
            if (MyFakes.ENABLE_TRASH_REMOVAL)
            {
                currentLine += jumpLine;
                var trashRemovalLabel = MakeLabel(page, currentLine, MySpaceTexts.WorldSettings_RemoveTrash);
            }
            currentLine += jumpLine;
            var startInRespawnScreenLabel = MakeLabel(page, currentLine, MySpaceTexts.WorldSettings_StartInRespawnScreen);

            // Find padding for next columm
            Vector2 paddingX = FindMaxSizeXLabel(page.Controls);

            // reset line position
            currentLine = originL;
            // Add control
            m_maxBackupSavesSlider = new MyGuiControlSlider(
                width: width, 
                minValue: 0,
                maxValue: 1000,
                labelText: new StringBuilder("{0}").ToString(),
                labelDecimalPlaces: 0,
                labelSpaceWidth: 0.05f,
                intValue: true
                );
            m_maxBackupSavesSlider.SetToolTip(MyTexts.GetString(MySpaceTexts.MaxBackupSaves));
            m_maxBackupSavesSlider.Position = ComputeControlPosition(currentLine, m_maxBackupSavesSlider, paddingX);
            page.Controls.Add(m_maxBackupSavesSlider);

            currentLine += jumpLine;
            m_enableSpectator = new MyGuiControlCheckbox();
            m_enableSpectator.SetToolTip(MyTexts.GetString(MySpaceTexts.ToolTipWorldSettingsEnableSpectator));
            m_enableSpectator.Position = ComputeControlPosition(currentLine, m_enableSpectator, paddingX);
            page.Controls.Add(m_enableSpectator);

            if (MyFakes.ENABLE_PROGRAMMABLE_BLOCK)
            {
                currentLine += jumpLine;
                m_enableIngameScripts = new MyGuiControlCheckbox();
                m_enableIngameScripts.SetToolTip(MyTexts.GetString(MySpaceTexts.WorldSettings_EnableIngameScripts));
                m_enableIngameScripts.Position = ComputeControlPosition(currentLine, m_enableIngameScripts, paddingX);
                page.Controls.Add(m_enableIngameScripts);
            }

            currentLine += jumpLine;
            m_enableCopyPaste = new MyGuiControlCheckbox();
            m_enableCopyPaste.SetToolTip(MyTexts.GetString(MySpaceTexts.ToolTipWorldSettingsEnableCopyPaste));
            m_enableCopyPaste.Position = ComputeControlPosition(currentLine, m_enableCopyPaste, paddingX);
            page.Controls.Add(m_enableCopyPaste);

            currentLine += jumpLine;
            m_enable3rdPersonCamera = new MyGuiControlCheckbox();
            m_enable3rdPersonCamera.Position = ComputeControlPosition(currentLine, m_enable3rdPersonCamera, paddingX);
            page.Controls.Add(m_enable3rdPersonCamera);

            currentLine += jumpLine;
            m_showPlayerNamesOnHud = new MyGuiControlCheckbox();
            m_showPlayerNamesOnHud.SetToolTip(MyTexts.GetString(MySpaceTexts.ToolTipWorldSettingsShowPlayerNamesOnHud));
            m_showPlayerNamesOnHud.Position = ComputeControlPosition(currentLine, m_showPlayerNamesOnHud, paddingX);
            page.Controls.Add(m_showPlayerNamesOnHud);

            currentLine += jumpLine;
            m_clientCanSave = new MyGuiControlCheckbox();
            m_clientCanSave.SetToolTip(MyTexts.GetString(MySpaceTexts.WorldSettings_ClientCanSave));
            m_clientCanSave.Position = ComputeControlPosition(currentLine, m_clientCanSave, paddingX);
            page.Controls.Add(m_clientCanSave);

            currentLine += jumpLine;
            m_disableRespawnShips = new MyGuiControlCheckbox();
            m_disableRespawnShips.SetToolTip(MyTexts.GetString(MySpaceTexts.ToolTipWorldSettings_DisableRespawnShips));
            m_disableRespawnShips.Position = ComputeControlPosition(currentLine, m_disableRespawnShips, paddingX);
            page.Controls.Add(m_disableRespawnShips);

            currentLine += jumpLine;
            m_respawnShipDelete = new MyGuiControlCheckbox();
            m_respawnShipDelete.SetToolTip(MyTexts.GetString(MySpaceTexts.TooltipWorldSettingsRespawnShipDelete));
            m_respawnShipDelete.Position = ComputeControlPosition(currentLine, m_respawnShipDelete, paddingX);
            page.Controls.Add(m_respawnShipDelete);

            if (MyFakes.ENABLE_TRASH_REMOVAL)
            {
                currentLine += jumpLine;
                m_trashRemoval = new MyGuiControlCheckbox();
                m_trashRemoval.SetToolTip(MyTexts.GetString(MySpaceTexts.ToolTipWorldSettingsRemoveTrash));
                m_trashRemoval.Position = ComputeControlPosition(currentLine, m_trashRemoval, paddingX);
                page.Controls.Add(m_trashRemoval);
            }

            currentLine += jumpLine;
            m_startInRespawnScreen = new MyGuiControlCheckbox();
            m_startInRespawnScreen.SetToolTip(MyTexts.GetString(MySpaceTexts.ToolTipWorldSettings_StartInRespawnScreen));
            m_startInRespawnScreen.Position = ComputeControlPosition(currentLine, m_startInRespawnScreen, paddingX);
            page.Controls.Add(m_startInRespawnScreen);
        }

        private MyGuiControlLabel MakeLabel(MyGuiControlParent page, Vector2 position, MyStringId textEnum)
        {
            MyGuiControlLabel label = new MyGuiControlLabel()
            {
                Text = MyTexts.GetString(textEnum),
                OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_CENTER
            };
            label.Position = position;
            page.Controls.Add(label);
            return label;
        }

        private Vector2 FindMaxSizeXLabel(MyGuiControls labels)
        {
            float max = 0f;
            foreach (var l in labels)
            {
                if (l is MyGuiControlLabel)
                {
                    if (l.Size.X > max)
                        max = l.Size.X;
                }
            }
            return new Vector2(max, 0f);
        }

        private Vector2 ComputeControlPosition(Vector2 currentLine, MyGuiControlBase control, Vector2 paddingX, float margin = 0.5f)
        {
            return currentLine + paddingX + new Vector2(control.Size.X * margin + 0.015f, 0f);
        }

        private void LoadValues()
        {
            //if (m_isNewGame)
            //{
            //    m_passwordTextbox.Text = "";
            //}
            //else
            //{
            //    m_passwordTextbox.Text = m_parent.Checkpoint.Password;
            //}
            SetSettings(m_parent.Settings);
        }

        private void CheckButton(float value, params MyGuiControlButton[] allButtons)
        {
            bool any = false;
            foreach (var btn in allButtons)
            {
                if (btn.UserData is float)
                {
                    if ((float)btn.UserData == value && !btn.Checked)
                    {
                        any = true;
                        btn.Checked = true;
                    }
                    else if ((float)btn.UserData != value && btn.Checked)
                        btn.Checked = false;
                }
            }

            if (!any)
                allButtons[0].Checked = true;
        }

        private void CheckButton(MyGuiControlButton active, params MyGuiControlButton[] allButtons)
        {
            foreach (var btn in allButtons)
            {
                if (btn == active && !btn.Checked)
                    btn.Checked = true;
                else if (btn != active && btn.Checked)
                    btn.Checked = false;
            }
        }

        public void UpdateSurvivalState(bool survivalEnabled)
        {
            m_creativeModeButton.Checked = !survivalEnabled;
            m_survivalModeButton.Checked = survivalEnabled;

            m_inventory_x1.Enabled = m_inventory_x3.Enabled = m_inventory_x10.Enabled = survivalEnabled;
            m_assembler_x1.Enabled = m_assembler_x3.Enabled = m_assembler_x10.Enabled = survivalEnabled;
            m_refinery_x1.Enabled = m_refinery_x3.Enabled = m_refinery_x10.Enabled = survivalEnabled;

            if (survivalEnabled)
            {
                m_enableCopyPaste.IsChecked = false;
            }

            m_enableCopyPaste.Enabled = !survivalEnabled;
            m_enableCopyPasteLabel.Enabled = !survivalEnabled;
        }

        private MyGameModeEnum GetGameMode()
        {
            return m_survivalModeButton.Checked ? MyGameModeEnum.Survival : MyGameModeEnum.Creative;
        }

        // Returns userobject as float from first checked button
        private float GetMultiplier(params MyGuiControlButton[] buttons)
        {
            foreach (var btn in buttons)
            {
                if (btn.Checked && btn.UserData is float)
                    return (float)btn.UserData;
            }
            Debug.Fail("No button is active");
            return 1.0f;
        }

        private float GetInventoryMultiplier()
        {
            return GetMultiplier(m_inventory_x1, m_inventory_x3, m_inventory_x10);
        }

        private float GetRefineryMultiplier()
        {
            return GetMultiplier(m_refinery_x1, m_refinery_x3, m_refinery_x10);
        }

        private float GetAssemblerMultiplier()
        {
            return GetMultiplier(m_assembler_x1, m_assembler_x3, m_assembler_x10);
        }

        private float GetWelderMultiplier()
        {
            return GetMultiplier(m_welder_x1, m_welder_half, m_welder_x2, m_welder_x5);
        }

        private float GetGrinderMultiplier()
        {
            return GetMultiplier(m_grinder_x1, m_grinder_half, m_grinder_x2, m_grinder_x5);
        }

        private float GetSpawnShipTimeMultiplier()
        {
            return (float)m_spawnShipTimeCombo.GetSelectedKey() / 10.0f;
        }

        public int GetWorldSize()
        {
            var asd = m_parent.Settings.WorldSizeKm;
            switch (m_worldSizeCombo.GetSelectedKey())
            {
                case (int)MyWorldSizeEnum.TEN_KM:
                    return 10;
                    break;
                case (int)MyWorldSizeEnum.TWENTY_KM:
                    return 20;
                    break;
                case (int)MyWorldSizeEnum.FIFTY_KM:
                    return 50;
                    break;
                case (int)MyWorldSizeEnum.HUNDRED_KM:
                    return 100;
                    break;
                case (int)MyWorldSizeEnum.UNLIMITED:
                    return 0;
                    break;
                case (int)MyWorldSizeEnum.CUSTOM:
                    return m_customWorldSize;
                    break;
                default:
                    Debug.Assert(false, "Unhandled MyWorldSizeEnum value");
                    return 0;
                    break;
            }
        }

        private MyWorldSizeEnum WorldSizeEnumKey(int worldSize)
        {
            switch (worldSize)
            {
                case 0:
                    return MyWorldSizeEnum.UNLIMITED;
                case 10:
                    return MyWorldSizeEnum.TEN_KM;
                case 20:
                    return MyWorldSizeEnum.TWENTY_KM;
                case 50:
                    return MyWorldSizeEnum.FIFTY_KM;
                case 100:
                    return MyWorldSizeEnum.HUNDRED_KM;
                default:
                    m_worldSizeCombo.AddItem((int)MyWorldSizeEnum.CUSTOM, MySpaceTexts.WorldSettings_WorldSizeCustom);
                    m_customWorldSize = worldSize;
                    //Debug.Assert(false, "non-standard world size");
                    return MyWorldSizeEnum.CUSTOM;
            }
        }

        public int GetViewDistance()
        {
            var key = m_viewDistanceCombo.GetSelectedKey();
            if (key == (int)MyViewDistanceEnum.CUSTOM)
            {
                return m_customViewDistance;
            }
            return (int)key;
        }

        private MyViewDistanceEnum ViewDistanceEnumKey(int viewDistance)
        {
            var value = (MyViewDistanceEnum)viewDistance;
            if (value != MyViewDistanceEnum.CUSTOM && Enum.IsDefined(typeof(MyViewDistanceEnum), value))
            {
                return (MyViewDistanceEnum)viewDistance;
            }
            else
            {
                m_viewDistanceCombo.AddItem((int)MyWorldSizeEnum.CUSTOM, MySpaceTexts.WorldSettings_ViewDistance_Custom);
                m_viewDistanceCombo.SelectItemByKey((int)MyWorldSizeEnum.CUSTOM);
                m_customViewDistance = viewDistance;
                return MyViewDistanceEnum.CUSTOM;
            }
        }

        public void GetSettings(MyObjectBuilder_SessionSettings output)
        {
            //output.OnlineMode = (MyOnlineModeEnum)m_onlineMode.GetSelectedKey();
            //output.EnvironmentHostility = (MyEnvironmentHostilityEnum)m_environment.GetSelectedKey();

            output.AutoHealing = m_autoHealing.IsChecked;
            output.CargoShipsEnabled = m_cargoShipsEnabled.IsChecked;
            output.EnableCopyPaste = m_enableCopyPaste.IsChecked;
            output.EnableSpectator = m_enableSpectator.IsChecked;
            output.ResetOwnership = m_resetOwnership.IsChecked;
            output.PermanentDeath = m_permanentDeath.IsChecked;
            output.DestructibleBlocks = m_destructibleBlocks.IsChecked;
            output.EnableIngameScripts = m_enableIngameScripts.IsChecked;
            output.Enable3rdPersonView = m_enable3rdPersonCamera.IsChecked;
            output.EnableEncounters = m_enableEncounters.IsChecked;
            output.EnableToolShake = m_enableToolShake.IsChecked;
            output.ShowPlayerNamesOnHud = m_showPlayerNamesOnHud.IsChecked;
            output.ThrusterDamage = m_thrusterDamage.IsChecked;
            output.WeaponsEnabled = m_weaponsEnabled.IsChecked;
            output.RemoveTrash = m_trashRemoval.IsChecked;
            output.EnableOxygen = m_enableOxygen.IsChecked;
            if (output.EnableOxygen && output.VoxelGeneratorVersion < MyVoxelConstants.VOXEL_GENERATOR_MIN_ICE_VERSION)
            {
                output.VoxelGeneratorVersion = MyVoxelConstants.VOXEL_GENERATOR_MIN_ICE_VERSION;
            }
            output.EnableOxygenPressurization = m_enableOxygenPressurization.IsChecked;
            output.RespawnShipDelete = m_respawnShipDelete.IsChecked;

			output.EnableConvertToStation = m_enableConvertToStation.IsChecked;
            output.DisableRespawnShips = m_disableRespawnShips.IsChecked;
            output.EnableWolfs = m_enableWolfs.IsChecked;
            output.EnableSunRotation = m_enableSunRotation.IsChecked;
            output.EnableJetpack = m_enableJetpack.IsChecked;
            output.SpawnWithTools = m_spawnWithTools.IsChecked;
            output.StartInRespawnScreen = m_startInRespawnScreen.IsChecked;
            output.EnableVoxelDestruction = m_enableVoxelDestruction.IsChecked;
            output.EnableDrones = m_enableDrones.IsChecked;

            output.EnableSpiders = m_enableSpiders.IsChecked;

            //output.MaxPlayers = (short)m_maxPlayersSlider.Value;
            output.MaxFloatingObjects = (short)m_maxFloatingObjectsSlider.Value;

            output.MaxBackupSaves = (short)m_maxBackupSavesSlider.Value;

            output.SunRotationIntervalMinutes = MathHelper.Clamp(MathHelper.InterpLog(m_sunRotationIntervalSlider.Value, MIN_DAY_TIME_MINUTES, MAX_DAY_TIME_MINUTES), MIN_DAY_TIME_MINUTES, MAX_DAY_TIME_MINUTES);

            output.AssemblerEfficiencyMultiplier = GetAssemblerMultiplier();
            output.AssemblerSpeedMultiplier = GetAssemblerMultiplier();
            output.InventorySizeMultiplier = GetInventoryMultiplier();
            output.RefinerySpeedMultiplier = GetRefineryMultiplier();
            output.WelderSpeedMultiplier = GetWelderMultiplier();
            output.GrinderSpeedMultiplier = GetGrinderMultiplier();
            output.SpawnShipTimeMultiplier = GetSpawnShipTimeMultiplier();

            output.WorldSizeKm = GetWorldSize();
            output.ViewDistance = GetViewDistance();

            //output.PhysicsIterations = (int)m_physicsOptionsCombo.GetSelectedKey();

            output.GameMode = GetGameMode();
        }

        public void SetSettings(MyObjectBuilder_SessionSettings settings)
        {
            //m_onlineMode.SelectItemByKey((int)settings.OnlineMode);
            //m_environment.SelectItemByKey((int)settings.EnvironmentHostility);
            m_worldSizeCombo.SelectItemByKey((int)WorldSizeEnumKey(settings.WorldSizeKm));
            m_spawnShipTimeCombo.SelectItemByKey((int)(settings.SpawnShipTimeMultiplier * 10));
            m_viewDistanceCombo.SelectItemByKey((int)ViewDistanceEnumKey(settings.ViewDistance));
            //if (m_physicsOptionsCombo.TryGetItemByKey(settings.PhysicsIterations) != null)
            //    m_physicsOptionsCombo.SelectItemByKey(settings.PhysicsIterations);
            //else
            //    m_physicsOptionsCombo.SelectItemByKey((int)MyPhysicsPerformanceEnum.Fast);

            m_autoHealing.IsChecked = settings.AutoHealing;
            m_cargoShipsEnabled.IsChecked = settings.CargoShipsEnabled;
            m_enableCopyPaste.IsChecked = settings.EnableCopyPaste;
            m_enableSpectator.IsChecked = settings.EnableSpectator;
            m_resetOwnership.IsChecked = settings.ResetOwnership;
            m_permanentDeath.IsChecked = settings.PermanentDeath.Value;
            m_destructibleBlocks.IsChecked = settings.DestructibleBlocks;
            m_enableEncounters.IsChecked = settings.EnableEncounters;
            m_enable3rdPersonCamera.IsChecked = settings.Enable3rdPersonView;
            m_enableIngameScripts.IsChecked = settings.EnableIngameScripts;
            m_enableToolShake.IsChecked = settings.EnableToolShake;
            m_showPlayerNamesOnHud.IsChecked = settings.ShowPlayerNamesOnHud;
            m_thrusterDamage.IsChecked = settings.ThrusterDamage;
            m_weaponsEnabled.IsChecked = settings.WeaponsEnabled;
            m_trashRemoval.IsChecked = settings.RemoveTrash;
            m_enableOxygen.IsChecked = settings.EnableOxygen;
            if (settings.VoxelGeneratorVersion < MyVoxelConstants.VOXEL_GENERATOR_MIN_ICE_VERSION)
            {
                m_showWarningForOxygen = true;
            }
            m_enableOxygenPressurization.IsChecked = settings.EnableOxygenPressurization;
            m_disableRespawnShips.IsChecked = settings.DisableRespawnShips;
            m_respawnShipDelete.IsChecked = settings.RespawnShipDelete;
			m_enableConvertToStation.IsChecked = settings.EnableConvertToStation;
            m_enableSunRotation.IsChecked = settings.EnableSunRotation;

            m_enableJetpack.IsChecked = settings.EnableJetpack;
            m_spawnWithTools.IsChecked = settings.SpawnWithTools;
            m_startInRespawnScreen.IsChecked = settings.StartInRespawnScreen;

            m_sunRotationIntervalSlider.Enabled = m_enableSunRotation.IsChecked;
            m_sunRotationPeriodValue.Visible = m_enableSunRotation.IsChecked;

            m_sunRotationIntervalSlider.Value = 0.03f;//to set value text correctly everytime
            m_sunRotationIntervalSlider.Value = MathHelper.Clamp(MathHelper.InterpLogInv((float)settings.SunRotationIntervalMinutes, MIN_DAY_TIME_MINUTES, MAX_DAY_TIME_MINUTES), 0, 1);
           // m_maxPlayersSlider.Value = settings.MaxPlayers;
            m_maxFloatingObjectsSlider.Value = settings.MaxFloatingObjects;

            m_maxBackupSavesSlider.Value = settings.MaxBackupSaves;

            m_enableVoxelDestruction.IsChecked = settings.EnableVoxelDestruction;
            m_enableDrones.IsChecked = settings.EnableDrones;

            if (settings.EnableWolfs.HasValue)
            {
                m_enableWolfs.IsChecked = settings.EnableWolfs.Value;
            }
            else
            {
                m_enableWolfs.IsChecked = false;
            }

            if (settings.EnableSpiders.HasValue)
            {
                m_enableSpiders.IsChecked = settings.EnableSpiders.Value;
            }
            else
            {
                m_enableSpiders.IsChecked = true;
            }

            CheckButton(settings.AssemblerSpeedMultiplier, m_assembler_x1, m_assembler_x3, m_assembler_x10);
            CheckButton(settings.InventorySizeMultiplier, m_inventory_x1, m_inventory_x3, m_inventory_x10);
            CheckButton(settings.RefinerySpeedMultiplier, m_refinery_x1, m_refinery_x3, m_refinery_x10);
            CheckButton(settings.WelderSpeedMultiplier, m_welder_x1, m_welder_half, m_welder_x2, m_welder_x5);
            CheckButton(settings.GrinderSpeedMultiplier, m_grinder_x1, m_grinder_half, m_grinder_x2, m_grinder_x5);

            UpdateSurvivalState(settings.GameMode == MyGameModeEnum.Survival);
        }

        public override string GetFriendlyName()
        {
            return "MyGuiScreenAdvancedWorldSettings";
        }

        #region Ok and Cancel Button
        private void CancelButtonClicked(object sender)
        {
            this.CloseScreen();
        }

        private void OkButtonClicked(object sender)
        {
            m_isConfirmed = true;

            if (OnOkButtonClicked != null)
            {
                OnOkButtonClicked();
            }

            this.CloseScreen();
        }
        #endregion

        #region Survival and Creative button event
        private void CreativeClicked(object sender)
        {
            UpdateSurvivalState(false);
        }

        private void SurvivalClicked(object sender)
        {
            UpdateSurvivalState(true);
        }
        #endregion

        #region On Event Button
        private void OnInventoryClick(object sender)
        {
            CheckButton((MyGuiControlButton)sender, m_inventory_x1, m_inventory_x3, m_inventory_x10);
            UpdateSurvivalState(m_survivalModeButton.Checked);
        }

        private void OnAssemblerClick(object sender)
        {
            CheckButton((MyGuiControlButton)sender, m_assembler_x1, m_assembler_x3, m_assembler_x10);
            UpdateSurvivalState(m_survivalModeButton.Checked);
        }

        private void OnRefineryClick(object sender)
        {
            CheckButton((MyGuiControlButton)sender, m_refinery_x1, m_refinery_x3, m_refinery_x10);
            UpdateSurvivalState(m_survivalModeButton.Checked);
        }

        private void OnWelderClick(object sender)
        {
            CheckButton((MyGuiControlButton)sender, m_welder_half, m_welder_x1, m_welder_x2, m_welder_x5);
            UpdateSurvivalState(m_survivalModeButton.Checked);
        }

        private void OnGrinderClick(object sender)
        {
            CheckButton((MyGuiControlButton)sender, m_grinder_half, m_grinder_x1, m_grinder_x2, m_grinder_x5);
            UpdateSurvivalState(m_survivalModeButton.Checked);
        }

        public event System.Action OnOkButtonClicked;

        #endregion     
    }
}
