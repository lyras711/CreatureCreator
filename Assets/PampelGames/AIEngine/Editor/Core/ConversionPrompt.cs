// ----------------------------------------------------
// AI Engine
// Copyright (c) Pampel Games e.K. All Rights Reserved.
// https://www.pampelgames.com
// ----------------------------------------------------

using System;
using System.Collections.Generic;
using UnityEngine;

namespace PampelGames.AIEngine.Editor
{
    public static class ConversionPrompt
    {
        
        public static string GeneratePrompt(string promptRaw, AffixesSO affixesSo, List<GameObject> prefabs, List<GameObject> references)
        {
            var unityVersionPrefix = GetUnityVersionString();
            var referencesPrefix = GetReferencesPrefix(prefabs, references);

            string prefix = affixesSo.GetPrefix();
            string suffix = affixesSo.GetSuffix();
            
            string fullPrompt = unityVersionPrefix + referencesPrefix + prefix + " \"" + promptRaw + "\". " + suffix; 
            
            return fullPrompt;
        }
        
        
        /********************************************************************************************************************************/

        private static string GetUnityVersionString()
        {
            var unityVersion = Application.unityVersion;
            int yearIndex = unityVersion.LastIndexOf("20", StringComparison.Ordinal);
            if (yearIndex >= 0)
            {
                string unityYear = unityVersion.Substring(yearIndex, 4);
                unityVersion = "Unity " + unityYear + " C#. ";
            }
            
            return unityVersion;
        }

        private static string GetReferencesPrefix(List<GameObject> prefabs, List<GameObject> references)
        {
            string prefabsPrefix = "";
            string referencesPrefix = "";

            if (prefabs != null && prefabs.Count > 0)
            {
                if (prefabs.Count == 1) prefabsPrefix = "I have a prefab: \"GameObject prefab;\". ";
                else prefabsPrefix = "I have a list of prefabs: \"List<GameObject> prefabs;\" of length " + prefabs.Count + ". ";
            }
            
            if (references != null && references.Count > 0)
            {
                if (references.Count == 1) referencesPrefix = "In the scene, I have a \"GameObject ref;\". ";
                else referencesPrefix = "In the scene, I have a \"List<GameObject> refs;\" of length " + references.Count + ". ";
            }

            return prefabsPrefix + referencesPrefix;
        }
    }
    
}
