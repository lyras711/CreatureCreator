// ----------------------------------------------------
// AI Engine
// Copyright (c) Pampel Games e.K. All Rights Reserved.
// https://www.pampelgames.com
// ----------------------------------------------------

using System.Collections.Generic;
using UnityEngine;

namespace PampelGames.AIEngine.Editor
{
    public class HistorySO : ScriptableObject
    {
        public List<HistoryClass> historyClasses = new();
    }
}