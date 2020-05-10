using ColossalFramework;
using Harmony;
using UnityEngine;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace ExcludeMail
{
    /// <summary>
    /// Harmony patching for PostVanAI
    /// </summary>
    public class PostVanAIPatch
    {
        /// <summary>
        /// create a patch of the GetColor method for the specified vehicle AI type
        /// </summary>
        /// <remarks>
        /// Cannot use HarmonyPatch attribute because the PostVanAI class has two GetColor routines:
        /// There is a GetColor routine in the PostVanAI class which has Vehicle as a parameter.
        /// There is a GetColor routine in the base class VehicleAI which has VehicleParked as a parameter.
        /// Furthermore, MakeByRefType cannot be specified in the HarmonyPatch attribute (or any attribute) to allow the patch to be created automatically.
        /// This routine manually finds the GetColor routine with Vehicle as a ref type parameter and creates the patch for it.
        /// </remarks>
        public static void CreateGetColorPatch()
        {
            // get the original GetColor method that takes ref Vehicle parameter
            MethodInfo original = typeof(PostVanAI).GetMethod("GetColor", new Type[] { typeof(ushort), typeof(Vehicle).MakeByRefType(), typeof(InfoManager.InfoMode) });
            if (original == null)
            {
                Debug.LogError("Unable to find GetColor method for [PostVanAI].");
                return;
            }

            // find the Prefix method
            MethodInfo prefix = typeof(PostVanAIPatch).GetMethod("Prefix", BindingFlags.Static | BindingFlags.Public);
            if (prefix == null)
            {
                Debug.LogError($"Unable to find PostVanAIPatch.Prefix method.");
                return;
            }

            // create the patch
            ExcludeMail.harmony.Patch(original, new HarmonyMethod(prefix), null, null);
        }

        /// <summary>
        /// return the color of the vehicle
        /// </summary>
        /// <returns>whether or not to do base processing</returns>
        public static bool Prefix(ushort vehicleID, ref Vehicle data, InfoManager.InfoMode infoMode, ref Color __result)
        {
            // assume do base processing
            bool doBaseProcessing = true;

            // do processing for this mod only for Outside Connections info view
            if (infoMode == InfoManager.InfoMode.Connections)
            {
                // if excluding mail, set vehicle to neutral color
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
