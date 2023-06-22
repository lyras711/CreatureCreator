// ----------------------------------------------------
// AI Engine
// Copyright (c) Pampel Games e.K. All Rights Reserved.
// https://www.pampelgames.com
// ----------------------------------------------------

using System;
using System.Collections.Generic;
using PampelGames.Shared.Utility;
using UnityEngine;

namespace PampelGames.AIEngine.Editor
{
    public class ExecutionClass
    {
        public AIEngine aIEngine;

        public string parentClass;
        public GameObject MonoBehaviourObject;
        public GameObject defaultPrimitive;

        public object prefabs;
        public object references;
        public List<string> namespaces;

        public string[] linesString;
        public LoopClass[] loopClasses;
        public ConditionalClass[] conditionalClasses;
        public LinesDetailClass[] linesDetailClasses;

        public CodeMethodClass[] codeMethodClasses; // Methods within the code lines created by the AI.

    }

    
    public static class ExecutionTasksClassExtensions
    {
        public static void Initialize(this ExecutionClass executionClass, AIEngine aiEngine, string[] linesString,
            List<GameObject> prefabs, List<GameObject> references, List<string> namespaces, string parentClass)
        {
            executionClass.aIEngine = aiEngine;
            if(prefabs.Count == 1) executionClass.prefabs = prefabs[0];
            else executionClass.prefabs = prefabs;
            if(references.Count == 1) executionClass.references = references[0];
            else executionClass.references = references;
            executionClass.namespaces = namespaces;
            executionClass.parentClass = parentClass;
            executionClass.linesString = linesString;
            executionClass.loopClasses = new LoopClass[linesString.Length];
            for (int i = 0; i < executionClass.loopClasses.Length; i++)
                executionClass.loopClasses[i] = new LoopClass();
            executionClass.conditionalClasses = new ConditionalClass[linesString.Length];
            for (int i = 0; i < executionClass.conditionalClasses.Length; i++)
                executionClass.conditionalClasses[i] = new ConditionalClass();
            executionClass.linesDetailClasses = new LinesDetailClass[linesString.Length];
            for (var i = 0; i < executionClass.linesDetailClasses.Length; i++)
                executionClass.linesDetailClasses[i] = new LinesDetailClass();
            executionClass.codeMethodClasses = new CodeMethodClass[linesString.Length];
        }
        public static int GetObjectIndexFromName(this ExecutionClass executionClass, string name)
        {
            for (var i = 0; i < executionClass.linesDetailClasses.Length; i++)
            {
                if (executionClass.linesDetailClasses[i].leftSideName == name)
                    return i;
            }
            return -1;
        }
        public static object GetValueFromName(this ExecutionClass executionClass, string name)
        {
            if(name.Contains(".")) return null;
            if(name.Contains("(")) return null;
            if(name.Contains(")")) return null;
            if(name.Contains(",")) return null;
            
            bool minus = false;
            if (name.Substring(0, 1).Equals("-"))
            {
                name = name.PGCutBefore("-", true);
                minus = true;
            }

            for (var i = 0; i < executionClass.linesDetailClasses.Length; i++)
            {
                if (executionClass.linesDetailClasses[i].leftSideName == name)
                {
                    var leftSideValue = executionClass.linesDetailClasses[i].leftSideValue;
                    if (minus) leftSideValue = ReflectionMath.CalculateObjects((-1), leftSideValue, " * ");
                    return leftSideValue;
                }
            }
            return null;
        }
        
        
        public static object GetValueFromObjectProperty(this ExecutionClass executionClass,
            LinesDetailClass.LevelsClass levelsClass, int lineIndex, string stringValue, LoopClass loopClass)
        {
            object value = null;
            if (!stringValue.Contains("."))
            {
                value = ReflectionGetSet.GetValueByName(executionClass.MonoBehaviourObject.GetType(), stringValue, executionClass.MonoBehaviourObject);
                return value;
            }
            if (stringValue.Contains("(")) return null;

            // Object value (for example: sphere.transform.position)
            var objName = stringValue.PGCutAfter(".", true);
            
            var objValue = GetValueFromName(executionClass, objName);
            if (objValue == null) objValue = Loops.GetValueAsLoopCollectionItem(loopClass, objName, executionClass);
            if (objValue == null) objValue = Collections.GetValueFromCollection(executionClass, levelsClass, lineIndex, stringValue);
            if (objValue != null)
            {
                value = ReflectionGetSet.GetValueByName(objValue.GetType(), stringValue.PGCutBefore(".",true), objValue);
                return value;
            }
            
            // Monobehaviour object itself (for example: transform.position)
            if (executionClass.MonoBehaviourObject == null) return null;
            value = ReflectionGetSet.GetValueByName(executionClass.MonoBehaviourObject.GetType(), stringValue, executionClass.MonoBehaviourObject);

            return value;
        }
        
        public static object GetValueFromStaticProperty(this ExecutionClass executionClass, string stringValue)
        {
            if (!stringValue.Contains(".")) return null;
            bool minus = false;
            if (stringValue.Substring(0, 1).Equals("-"))
            {
                stringValue = stringValue.PGCutBefore("-", true).Trim();
                minus = true;
            }

            Type classType = PGReflectionUtility.GetTypeByName(stringValue, executionClass.namespaces);
            var connectedValueName = stringValue.PGCutBefore(".", true);
            
            var value = ReflectionGetSet.GetValueByName(classType, connectedValueName);
            
            if (minus) value = ReflectionMath.SetNegative(value);
            return value;
        }
        
        public static object GetValueAsOutParameter(this ExecutionClass executionClass, string stringValue)
        {
            if (!stringValue.StartsWith("out ")) return null;
            string stringName = stringValue.PGCutBefore("out ", true).Trim();
            var value = executionClass.GetValueFromName(stringName);
            return value;
        }
        
    }
    
}