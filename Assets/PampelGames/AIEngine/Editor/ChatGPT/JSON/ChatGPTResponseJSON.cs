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
    [Serializable]
    public class ChatGPTResponseJSON
    {
        public ChatGPTUsageJSON usage;
        public ChatGPTChoiceJSON[] choices;
    }
    
    


    

}
