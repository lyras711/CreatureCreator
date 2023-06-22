// ----------------------------------------------------
// AI Engine
// Copyright (c) Pampel Games e.K. All Rights Reserved.
// https://www.pampelgames.com
// ----------------------------------------------------

using System;

namespace PampelGames.AIEngine.Editor
{
    [Serializable]
    public class HistoryClass
    {
        public float startTime;
        public float endTime;
        public float responseTime;

        public int tokensUsed;
        
        public bool testRequest;
        
        public string dateTime;
        public string task;
        public string response;

        public bool failed;
        public string failText;
    }
}