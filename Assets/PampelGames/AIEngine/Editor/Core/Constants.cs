// ----------------------------------------------------
// AI Engine
// Copyright (c) Pampel Games e.K. All Rights Reserved.
// https://www.pampelgames.com
// ----------------------------------------------------

using UnityEngine;

namespace PampelGames.AIEngine.Editor
{
    public static class Constants 
    {
        
        public const string DocumentationURL = "https://docs.google.com/document/d/1F5oSPz7oRDVxmDzxDLleifYZqTP0qtb4YvEszmjVozo/edit?usp=sharing";

        public const string AIEngineSettingsSO = nameof(AIEngineSettingsSO);
        public const string HistorySO = nameof(HistorySO);
        public const string AffixesSO = nameof(AffixesSO);

        public const string DebugLogSuffix = "Log message sent by AI Engine.";
        public const string DebugErrorSuffix = "Error message sent by AI Engine.";

        public const string MonoBehaviourObject = "AIEngine - "+ nameof(MonoBehaviourObject);
        
        public const string Prefab = "prefab";
        public const string Prefabs = "prefabs";
        public const string Reference = "ref";
        public const string References = "refs";

        // AI not creating primitives but expecting prefab
        public const string CubePrefabName = "cubePrefab";
        public const string SpherePrefabName = "spherePrefab";
        public const string CapsulePrefabName = "capsulePrefab";
        public const string CylinderPrefabName = "cylinderPrefab";
        public const string PlanePrefabName = "planePrefab";
        public const string QuadPrefabName = "quadPrefab";

        public const string InnerBracketLevel = nameof(InnerBracketLevel)+"_";
        public const string EmptyReturnValue = nameof(EmptyReturnValue);
        public const string CodeMethodParameter = "cmp";
        public const string ForLoopParameter = "loop";

        
        // Mathematics
        public const string EqualSign = " = ";
        
        // Undo
        public const string UndoGroup = nameof(UndoGroup);
        public const string ObjectCreated = nameof(ObjectCreated);
        
        // infoText
        public const string InfoPleaseWait = "Please Wait...";
        public const string InfoSomethingWrong = "Something went wrong. Check history for details.";

    }
}
