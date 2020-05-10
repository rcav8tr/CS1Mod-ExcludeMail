using Harmony;
using UnityEngine;
using System.Reflection;
using ColossalFramework.UI;
using ColossalFramework;

namespace ExcludeMail
{
    /// <summary>
    /// Harmony patching for OutsideConnectionsInfoViewPanel
    /// </summary>
    public class OutsideConnectionsInfoViewPanelPatch
    {
        // labels and charts that will be updated by this class
        private static UILabel _importTotalLabel;
        private static UILabel _exportTotalLabel;
        private static UIRadialChart _importChart;
        private static UIRadialChart _exportChart;

        /// <summary>
        /// create patch for OutsideConnectionsInfoViewPanel.UpdatePanel
        /// </summary>
        public static void CreateUpdatePanelPatch()
        {
            // get the OutsideConnectionsInfoViewPanel panel (displayed when the user clicks on the Outside Connections info view button)
            OutsideConnectionsInfoViewPanel ocPanel = UIView.library.Get<OutsideConnectionsInfoViewPanel>(typeof(OutsideConnectionsInfoViewPanel).Name);
            if (ocPanel == null)
            {
                Debug.LogError("Unable to find [OutsideConnectionsInfoViewPanel].");
                return;
            }

            // find import total label
            string componentName = "ImportTotal";
            _importTotalLabel = ocPanel.Find<UILabel>(componentName);
            if (_importTotalLabel == null)
            {
                Debug.LogError($"Unable to find label [{componentName}] on [OutsideConnectionsInfoViewPanel].");
                return;
            }

            // find export total label
            componentName = "ExportTotal";
            _exportTotalLabel = ocPanel.Find<UILabel>(componentName);
            if (_exportTotalLabel == null)
            {
                Debug.LogError($"Unable to find label [{componentName}] on [OutsideConnectionsInfoViewPanel].");
                return;
            }

            // find import chart
            componentName = "ImportChart";
            _importChart = ocPanel.Find<UIRadialChart>(componentName);
            if (_importChart == null)
            {
                Debug.LogError($"Unable to find chart [{componentName}] on [OutsideConnectionsInfoViewPanel].");
                return;
            }

            // find export chart
            componentName = "ExportChart";
            _exportChart = ocPanel.Find<UIRadialChart>(componentName);
            if (_exportChart == null)
            {
                Debug.LogError($"Unable to find chart [{componentName}] on [OutsideConnectionsInfoViewPanel].");
                return;
            }

            // get the original UpdatePanel method
            MethodInfo original = typeof(OutsideConnectionsInfoViewPanel).GetMethod("UpdatePanel", BindingFlags.Instance | BindingFlags.NonPublic);
            if (original == null)
            {
                Debug.LogError($"Unable to find OutsideConnectionsInfoViewPanel.UpdatePanel method.");
                return;
            }

            // find the Prefix method
            MethodInfo prefix = typeof(OutsideConnectionsInfoViewPanelPatch).GetMethod("Prefix", BindingFlags.Static | BindingFlags.Public);
            if (prefix == null)
            {
                Debug.LogError($"Unable to find OutsideConnectionsInfoViewPanelPatch.Prefix method.");
                return;
            }

            // create the patch
            ExcludeMail.harmony.Patch(original, new HarmonyMethod(prefix), null, null);
        }

        /// <summary>
        /// either update of the panel or let base processing update the panel
        /// </summary>
        /// <returns>whether or not to do base processing</returns>
        public static bool Prefix()
        {
            // assume do base processing
            bool doBaseProcessing = true;

            // check if should exclude mail
            if (!ExcludeMailLoading.IncludeMail())
            {
                // do processing with mail set to zero
                // logic copied from OutsideConnectionsInfoViewPanel.UpdatePanel and then mail was set to zero

                // do imports
                DistrictManager instance = Singleton<DistrictManager>.instance;
                int importOil      = (int)(instance.m_districts.m_buffer[0].m_importData.m_averageOil          + 99) / 100;
                int importOre      = (int)(instance.m_districts.m_buffer[0].m_importData.m_averageOre          + 99) / 100;
                int importForestry = (int)(instance.m_districts.m_buffer[0].m_importData.m_averageForestry     + 99) / 100;
                int importGoods    = (int)(instance.m_districts.m_buffer[0].m_importData.m_averageGoods        + 99) / 100;
                int importFarming  = (int)(instance.m_districts.m_buffer[0].m_importData.m_averageAgricultural + 99) / 100;
                int importMail     = 0;
                int importTotal = importOil + importOre + importForestry + importGoods + importFarming + importMail;
                _importTotalLabel.text = StringUtils.SafeFormat(ColossalFramework.Globalization.Locale.Get(_importTotalLabel.localeID), importTotal);
                _importChart.SetValues(
                    GetValue(importOil,      importTotal),
                    GetValue(importOre,      importTotal), 
                    GetValue(importForestry, importTotal), 
                    GetValue(importGoods,    importTotal), 
                    GetValue(importFarming,  importTotal), 
                    GetValue(importMail,     importTotal));
                
                // do exports
                int exportOil      = (int)(instance.m_districts.m_buffer[0].m_exportData.m_averageOil          + 99) / 100;
                int exportOre      = (int)(instance.m_districts.m_buffer[0].m_exportData.m_averageOre          + 99) / 100;
                int exportForestry = (int)(instance.m_districts.m_buffer[0].m_exportData.m_averageForestry     + 99) / 100;
                int exportGoods    = (int)(instance.m_districts.m_buffer[0].m_exportData.m_averageGoods        + 99) / 100;
                int exportFarming  = (int)(instance.m_districts.m_buffer[0].m_exportData.m_averageAgricultural + 99) / 100;
                int exportMail     = 0;
                int exportFish     = (int)(instance.m_districts.m_buffer[0].m_exportData.m_averageFish         + 99) / 100;
                int exportTotal = exportOil + exportOre + exportForestry + exportGoods + exportFarming + exportMail + exportFish;
                _exportTotalLabel.text = StringUtils.SafeFormat(ColossalFramework.Globalization.Locale.Get(_exportTotalLabel.localeID), exportTotal);
                _exportChart.SetValues(
                    GetValue(exportOil,      exportTotal), 
                    GetValue(exportOre,      exportTotal), 
                    GetValue(exportForestry, exportTotal), 
                    GetValue(exportGoods,    exportTotal), 
                    GetValue(exportFarming,  exportTotal), 
                    GetValue(exportMail,     exportTotal), 
                    GetValue(exportFish,     exportTotal));

                // everything was performed here, skip base processing
                doBaseProcessing = false;
            }

            // return whether or not to do the base processing
            return doBaseProcessing;
        }

        /// <summary>
        /// return the percent for the given value
        /// </summary>
        private static int GetValue(int value, int total)
        {
            // logic copied from OutsideConnectionsInfoViewPanel.GetValue
            if (total == 0) return 0;
            float num = (float)value / (float)total;
            return Mathf.CeilToInt(num * 100f);
        }

    }
}
