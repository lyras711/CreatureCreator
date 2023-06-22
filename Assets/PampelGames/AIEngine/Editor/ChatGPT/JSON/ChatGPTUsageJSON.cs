// ----------------------------------------------------
// AI Engine
// Copyright (c) Pampel Games e.K. All Rights Reserved.
// https://www.pampelgames.com
// ----------------------------------------------------

using System;

namespace PampelGames.AIEngine.Editor
{
    [Serializable]
    public class ChatGPTUsageJSON
    {
        public int prompt_tokens;
        public int completion_tokens;
        public int total_tokens;
    }
}
