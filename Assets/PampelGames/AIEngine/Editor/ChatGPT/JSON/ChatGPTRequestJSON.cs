// ----------------------------------------------------
// AI Engine
// Copyright (c) Pampel Games e.K. All Rights Reserved.
// https://www.pampelgames.com
// ----------------------------------------------------

using System;

namespace PampelGames.AIEngine.Editor
{
    [Serializable]
    public class ChatGPTRequestJSON
    {
        public string model;
        public ChatGPTPromptMessageJSON[] messages;
        public float temperature;
        public int max_tokens;
    }
}