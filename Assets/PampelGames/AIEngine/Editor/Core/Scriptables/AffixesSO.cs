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
    public class AffixesSO : ScriptableObject
    {

        public string connectionTest = "Greet me in a fun way (max 50 characters).";
        
        public AffixesClass affixes = new();
        

        /********************************************************************************************************************************/

        public string GetPrefix()
        {
            string result = String.Join(" ", affixes.prefixes);
            return result;
        }
        
        public string GetSuffix()
        {
            string result = String.Join(" ", affixes.suffixes);
            return result;
        }
    }

    [Serializable]
    public class AffixesClass
    {
        public List<string> prefixes = new();
        public List<string> suffixes = new();
    }
}