// ----------------------------------------------------
// AI Engine
// Copyright (c) Pampel Games e.K. All Rights Reserved.
// https://www.pampelgames.com
// ----------------------------------------------------

using System.Collections.Generic;
using UnityEngine;

namespace PampelGames.AIEngine.Editor
{
    public class AIEngineSettingsSO : ScriptableObject
    {

        // Settings
        public bool autoConnect = true;
        public bool storeHistory = true;
        public int storeHistoryAmount = 10;
        public KeyCode confirmTextKeycode = KeyCode.Return;
        public bool clearText = true;
        
        // Details
        public bool logMessages;
        public bool logErrors;
        public bool logConvertedLines;
        public bool logLineDetermination;
        

        /********************************************************************************************************************************/
        
        public void ResetSettings()
        {
            // Settings
            autoConnect = true;
            storeHistory = true;
            storeHistoryAmount = 10;
            confirmTextKeycode = KeyCode.Return;
            clearText = true;
            
            logMessages = false;
            logErrors = false;
            logConvertedLines = false;
            logLineDetermination = false;
        }
        
    }
    
}