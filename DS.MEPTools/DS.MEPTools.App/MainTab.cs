using Autodesk.Revit.UI;
using DS.MEPCurveTraversability;
using System;

namespace DS.MEPTools.App
{
    internal class MainTab
    {
        private readonly static string TabName = "DS.MEPTools";

        public MainTab(UIControlledApplication uIControlledApplication)
        {
            //check if ribbon tab with current name exist
            try
            {
                uIControlledApplication.CreateRibbonTab(TabName);
            }
            catch (Exception)
            { }
            CreateCollisionSelectorPanel(uIControlledApplication);

        }

        private RibbonPanel CreateCollisionSelectorPanel(UIControlledApplication uIControlledApplication)
        {
            var ribbonPanel = uIControlledApplication.CreateRibbonPanel(TabName, "Устранение коллизий");

            SplitButtonData sbData = new SplitButtonData("FindCollisionsOpt", "Найти коллизии");
            SplitButton sb = ribbonPanel.AddItem(sbData) as SplitButton;

            var button = new ButtonCreator()
            {
                LargeImageName = "pipe-circle-check32x32.png",
                ImageName = "pipe-circle-check16x16.png",
                ButtonName = "CheckTraversability",
                ButtonText = "Проверить\nэлемент",
                ClassName = typeof(ExternalCommand).FullName,                
            }.Create();
            sb.AddPushButton(button);

            button = new ButtonCreator()
            {
                LargeImageName = "block-brick32x32.png",
                ImageName = "block-brick16x16.png",
                ButtonName = "WallIntersectionSettingsAR",
                ButtonText = "Настройки пересечения\nстен АР",
                ClassName = typeof(ShowARSettingsExternalCommand).FullName,
            }.Create();
            sb.AddPushButton(button);

            button = new ButtonCreator()
            {
                LargeImageName = "block-brick32x32.png",
                ImageName = "block-brick16x16.png",
                ButtonName = "WallIntersectionSettingsKR",
                ButtonText = "Настройки пересечения\nстен КР",
                ClassName = typeof(ShowKRSettingsExternalCommand).FullName,
            }.Create();
            sb.AddPushButton(button);

            button = new ButtonCreator()
            {
                LargeImageName = "square-xmark32x32.png",
                ImageName = "square-xmark16x16.png",
                ButtonName = "RoomIntersectionSettings",
                ButtonText = "Настройки пересечения\nпомещений.",
                ClassName = typeof(RoomSettingsExternalCommand).FullName,
            }.Create();
            sb.AddPushButton(button);

            return ribbonPanel;
        }
    }
}
