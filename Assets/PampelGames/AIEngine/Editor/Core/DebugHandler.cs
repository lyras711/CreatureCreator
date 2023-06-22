// ----------------------------------------------------
// AI Engine
// Copyright (c) Pampel Games e.K. All Rights Reserved.
// https://www.pampelgames.com
// ----------------------------------------------------

using UnityEngine;

namespace PampelGames.AIEngine.Editor
{
    
    internal static class DebugHandler
    {

        internal static void SendDebugLog(AIEngine aiEngine, string message)
        {
            if (!aiEngine.aiEngineSettingsSo.logMessages) return;
            Debug.Log(message + " \n" + Constants.DebugLogSuffix);
        } 
        internal static void SendDebugError(AIEngine aiEngine, string message)
        {
            if (aiEngine.aiEngineSettingsSo.logErrors)
                Debug.LogError(message + " \n" + Constants.DebugErrorSuffix);
            aiEngine.ScriptExecutionError(message);
        }

    }
    
}
