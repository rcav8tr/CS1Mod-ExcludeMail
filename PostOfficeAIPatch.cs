using Harmony;
using UnityEngine;
using System.Reflection;
using ColossalFramework;

namespace ExcludeMail
{
    /// <summary>
    /// Harmony patching for PostOfficeAI
    /// </summary>
    public class PostOfficeAIPatch
    {
        /// <summary>
        /// create a patch of the GetColor method
        /// </summary>
        public static void CreateGetColorPatch()
        {
            // get the original GetColor method
            MethodInfo original = typeof(PostOfficeAI).GetMethod("GetColor");
            if (original == null)
            {
                Debug.LogError("Unable to find GetColor method for [PostOfficeAI].");
                return;
            }

            // find the Prefix method
            MethodInfo prefix = typeof(PostOfficeAIPatch).GetMethod("Prefix", BindingFlags.Public | BindingFlags.Static);
            if (prefix == null)
            {
                Debug.LogError("Unable to find PostOfficeAI.Prefix method.");
                return;
            }

            // create the patch
            ExcludeMail.harmony.Patch(original, new HarmonyMethod(prefix), null, null);
        }

        /// <summary>
        /// return the color of the building
        /// </summary>
        /// <returns>whether or not to do base processing</returns>
        public static bool Prefix(ushort buildingID, ref Building data, InfoManager.InfoMode infoMode, ref Color __result)
        {
            // assume do base processing
            bool doBaseProcessing = true;

            // do processing for this mod only for Outside Connections info view
            if (infoMode == InfoManager.InfoMode.Connections)
            {
                // if excluding mail, set building to neutral color
                if (!ExcludeMailLoading.IncludeMail())
                {
                    __result = Singleton<InfoManager>.instance.m_properties.m_neutralColor;
                    doBaseProcessing = false;
                }
            }

            // return whether or not to do the base processing
            return doBaseProcessing;
        }
    }
}
