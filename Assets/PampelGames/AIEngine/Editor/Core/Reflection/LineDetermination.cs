// ----------------------------------------------------
// AI Engine
// Copyright (c) Pampel Games e.K. All Rights Reserved.
// https://www.pampelgames.com
// ----------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using PampelGames.Shared.Utility;
using UnityEngine;

namespace PampelGames.AIEngine.Editor
{
    internal static class LineDetermination
    {
        internal static List<GameObject> DetermineLine(ExecutionClass executionClass, int lineIndex)
        {
            var objectList = new List<GameObject>();
            var linesDetailClass = executionClass.linesDetailClasses[lineIndex];

            for (var i = 0; i < linesDetailClass.levelsClasses.Count; i++)
            {
                var rightSideLevelsClass = linesDetailClass.levelsClasses[i];
                if(!ValidateLine(executionClass.linesString[lineIndex])) continue;
                var newObjects = DetermineValues(executionClass, rightSideLevelsClass, lineIndex);
                objectList.AddRange(newObjects);
                
                if (executionClass.linesString[lineIndex].Contains(Constants.EqualSign) || i + 1 != linesDetailClass.levelsClasses.Count)
                {
                    if (!LineSet.IsLeftSideMethod(executionClass, rightSideLevelsClass, lineIndex))
                        CalculateParameterValues(executionClass, rightSideLevelsClass, lineIndex);
                    else rightSideLevelsClass.parameterValues = new List<object> {null};
                    
                    LineSet.SetLeftSideValue(executionClass, rightSideLevelsClass, lineIndex);
                }
                if(executionClass.aIEngine.aiEngineSettingsSo.logLineDetermination)
                    LogLineDetermination(executionClass, linesDetailClass, rightSideLevelsClass, lineIndex);
            }

            return objectList;
        }

        /********************************************************************************************************************************/

        private static bool ValidateLine(string levelString)
        {
            if (levelString.Contains("public ") || levelString.Contains("private ") || 
                levelString.Contains("protected ") || levelString.Contains("internal ")) return false;
            if (levelString.Contains("using ")) return false;
            if (levelString.Contains(" : ") && !levelString.Contains("?")) return false;
            if (levelString.Contains("void ") && levelString.Contains("(")) return false;
            return true;
        }
        
        /********************************************************************************************************************************/
        
        private static List<GameObject> DetermineValues(ExecutionClass executionClass,
            LinesDetailClass.LevelsClass levelsClass, int lineIndex)
        {

            var objectList = new List<GameObject>();
            
            var splittedStrings = levelsClass.splittedStrings;
            levelsClass.splittedValues = new object[splittedStrings.Length];
            
            for (var i = 0; i < splittedStrings.Length; i++)
            {
                levelsClass.splittedValues[i] = DetermineValue(executionClass, levelsClass,
                    lineIndex, splittedStrings[i]);

                if(levelsClass.splittedValues[i] != null && levelsClass.splittedValues[i] is GameObject)
                    objectList.Add(levelsClass.splittedValues[i] as GameObject);
            }
            return objectList;
        }

        internal static object DetermineValue(ExecutionClass executionClass, LinesDetailClass.LevelsClass levelsClass, 
            int lineIndex, string splittedString)
        {
            var linesDetailClass = executionClass.linesDetailClasses[lineIndex];
            
            object splittedValue = null;
            if (splittedString.StartsWith("!")) splittedString = splittedString.PGCutBefore("!", true).Trim();
            
            splittedValue = ReflectionUtility.DetermineBasicValueFromString(splittedString);
            if (splittedValue != null) return splittedValue;
            
            splittedValue = executionClass.GetValueFromName(splittedString);
            if (splittedValue != null) return splittedValue;

            splittedValue = Collections.GetCollectionSizeFromName(executionClass, splittedString);
            if (splittedValue != null) return splittedValue;
            
            splittedValue = executionClass.GetValueFromObjectProperty(levelsClass, lineIndex, splittedString, executionClass.loopClasses[lineIndex]);
            if (splittedValue != null) return splittedValue;
            
            splittedValue = Collections.GetValueFromCollection(executionClass, levelsClass, lineIndex, splittedString);
            if (splittedValue != null) return splittedValue;
            
            splittedValue = executionClass.GetValueFromStaticProperty(splittedString);
            if (splittedValue != null) return splittedValue;
           
            splittedValue = executionClass.GetValueAsOutParameter(splittedString);
            if (splittedValue != null) return splittedString;

            splittedValue = Methods.GetValueFromMethod(executionClass, levelsClass, lineIndex, splittedString);
            if (splittedValue != null) return splittedValue;

            splittedValue = Constructors.GetValueFromConstructor(executionClass, linesDetailClass, levelsClass, splittedString);
            if (splittedValue != null) return splittedValue;

            splittedValue = Loops.GetForLoopCurrentIndexValue(executionClass.loopClasses[lineIndex], splittedString);
            if (splittedValue != null) return splittedValue;

            splittedValue = Loops.GetValueAsLoopCollectionItem(executionClass.loopClasses[lineIndex], splittedString, executionClass);
            if (splittedValue != null) return splittedValue;
            
            splittedValue = CodeMethods.GetValueFromCodeMethod(executionClass, lineIndex, splittedString);
            if (splittedValue != null) return splittedValue;
          
            return null;
        }

        /********************************************************************************************************************************/
        internal static object DetermineValueFromConnectedString(ExecutionClass executionClass,
            LinesDetailClass.LevelsClass levelsClass, int lineIndex, string connectedString)
        {
            var splittedStrings = ReflectionMath.SplitByOperators(connectedString);

            object[] splittedValues = new object[splittedStrings.Length];
            for (int i = 0; i < splittedStrings.Length; i++)
            {
                splittedValues[i] = DetermineValue(executionClass, levelsClass, lineIndex, splittedStrings[i]);
            }

            object accumulatedValue = ReflectionMath.CalculateObjectsFromLists(splittedStrings.ToList(), splittedValues.ToList());
            return accumulatedValue;
        }
        
        /********************************************************************************************************************************/
        
        internal static void CalculateParameterValues(ExecutionClass executionClass, LinesDetailClass.LevelsClass 
            levelsClass, int lineIndex)
        {
            levelsClass.parameterValues = new List<object>();
            var splittedStrings = levelsClass.splittedStrings;
            var splittedValues = levelsClass.splittedValues;

            var numCommas = splittedStrings.Sum(s => s.Count(c => c == ','));
            var arraySize = numCommas + 1;
            var seperatedStrings = new List<string>[arraySize];
            var seperatedValues = new List<object>[arraySize];
            for (var i = 0; i < arraySize; i++)
            {
                seperatedStrings[i] = new List<string>();
                seperatedValues[i] = new List<object>();
            }
            
            var listIndex = 0;
            for (var i = 0; i < splittedStrings.Length; i++)
            {
                var substrings = splittedStrings[i].Split(',');

                for (var j = 0; j < substrings.Length; j++)
                {
                    if (!string.IsNullOrEmpty(substrings[j].Trim()))
                    {
                        seperatedStrings[listIndex].Add(substrings[j].Trim());
                        seperatedValues[listIndex].Add(splittedValues[i]);
                    }

                    if (j < substrings.Length - 1) listIndex++;
                }
            }

            for (var i = 0; i < seperatedStrings.Length; i++)
            {
                for (var j = 0; j < seperatedStrings[i].Count; j++)
                {
                    if (Conditionals.IsTerneryOperator(seperatedStrings[i][j]))
                    {
                        seperatedValues[i][j] = Conditionals.GetValueFromTerneryOperator(executionClass, levelsClass,
                            lineIndex, seperatedStrings[i][j]);
                    }
                }
                
                object accumulatedValue = ReflectionMath.CalculateObjectsFromLists(seperatedStrings[i], seperatedValues[i]);
                
                if (seperatedStrings[i].Count != 0 && accumulatedValue == null)
                {
                    string errorMessage = "Parameter couldn't be generated at index "+i+ ".\n" + "Line: "+ executionClass.linesString[lineIndex] + "\n";
                    for (int j = 0; j < seperatedStrings[i].Count; j++)
                        errorMessage += "String: "+seperatedStrings[i][j]+"\n"+ "Value: "+seperatedValues[i][j] + "\n";
                    
                    DebugHandler.SendDebugError(executionClass.aIEngine, errorMessage);
                    return;
                }
                
                if(accumulatedValue != null) levelsClass.parameterValues.Add(accumulatedValue);
            }
        }


        /********************************************************************************************************************************/
        /********************************************************************************************************************************/
        
        private static void LogLineDetermination(ExecutionClass executionClass, LinesDetailClass linesDetailClass, 
            LinesDetailClass.LevelsClass levelsClass, int lineIndex)
        {

            Debug.Log("### LINE " +lineIndex+ " / LEVEL " +levelsClass.level+ " ###");
            if (levelsClass.level == 0)
            {
                Debug.Log(executionClass.linesString[lineIndex]);
                Debug.Log(linesDetailClass.leftSideName);
                Debug.Log(linesDetailClass.leftSideValue);
            }
            
            Debug.Log(levelsClass.levelString);    
            
            for (var i = 0; i < levelsClass.splittedStrings.Length; i++)
            {
                Debug.Log(levelsClass.splittedStrings[i]);
                Debug.Log(levelsClass.splittedValues[i]);
            }

            if (levelsClass.parameterValues == null)
                Debug.Log(levelsClass.parameterValues);
            else
                for (var i = 0; i < levelsClass.parameterValues.Count; i++)
                    Debug.Log(levelsClass.parameterValues[i]);
        }
    }
}