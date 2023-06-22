// ----------------------------------------------------
// AI Engine
// Copyright (c) Pampel Games e.K. All Rights Reserved.
// https://www.pampelgames.com
// ----------------------------------------------------

using UnityEngine;

namespace PampelGames.AIEngine.Editor
{
    public class ChatGPTSettingsSO : ScriptableObject
    {
        public string apiURL;
        public string apiKey;
        public string apiOrganization;
        public string apiModel;
        public int apiMaxTokens = 500;
        public bool showTokenInfo;
        
        /********************************************************************************************************************************/
        
        public void ResetValues()
        {
            apiURL = ChatGPTConstants.apiURL;
            apiModel = ChatGPTConstants.apiModel;
            apiMaxTokens = 500;
            showTokenInfo = false;
        }
    }
}
