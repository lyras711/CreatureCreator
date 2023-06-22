// ----------------------------------------------------
// AI Engine
// Copyright (c) Pampel Games e.K. All Rights Reserved.
// https://www.pampelgames.com
// ----------------------------------------------------

using System.Collections.Generic;
using UnityEngine;

namespace PampelGames.AIEngine.Editor
{
    public class LoopClass
    {
        public bool partOfLoop;
        
        public int loopIterations; 
        public int loopIndex;

        public string indexName; // "i" or "j" or foreach item name etc.
        
        public object startIndexValue; // (only for loop) value when the loop starts (only relevant for inner for loops).
        public object increaseIndexValue; // (only for loop) value added to determine the next currentIndexValue.
        public object currentIndexValue; // (only for loop) accumulated value.

        public object foreachCollection;
        
        // List because inside the loop could be multiple collections
        public readonly List<string> loopItemNames = new(); // For example "reference" or "references[i]".
        
        // Loop within Loop (only 1x supported)
        public LoopClass innerLoopClass;
    }
}
