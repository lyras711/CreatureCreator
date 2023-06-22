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
    internal static class Loops
    {
        internal static bool CreateLoop(ExecutionClass executionClass, int lineIndex)
        {
            var inputString = executionClass.linesString[lineIndex];
            
            if (inputString.IsForLoop())
            {
                var loopClass = GetLoopClass(executionClass, lineIndex);
                SetLoops(executionClass, lineIndex, loopClass);
                SetForLoopItemNames(executionClass, lineIndex, inputString, loopClass);
                SetIndexNames(executionClass, lineIndex, inputString);
                return true;
            }

            if (executionClass.linesString[lineIndex].IsForeachLoop())
            {
                var loopClass = GetLoopClass(executionClass, lineIndex);
                SetLoops(executionClass, lineIndex, loopClass);
                SetForeachLoopItemName(executionClass, lineIndex, inputString, loopClass);
                SetIndexNames(executionClass, lineIndex, inputString);
                return true;
            }

            return false;
        }

        /********************************************************************************************************************************/
        
        private static LoopClass GetLoopClass(ExecutionClass executionClass, int lineIndex)
        {
            var loopClass = executionClass.loopClasses[lineIndex];
            if (loopClass.partOfLoop)
            {
                loopClass = loopClass.innerLoopClass = new LoopClass();

                int brackets = 1;
                for (int i = lineIndex +1; i < executionClass.linesString.Length; i++)
                {
                    if (executionClass.linesString[i].Contains("{")) brackets++;
                    if (executionClass.linesString[i].Contains("}")) brackets--;
                    if (brackets <= 0) break;
                    executionClass.loopClasses[i].innerLoopClass = new LoopClass();
                }
            }
            return loopClass;
        }

        /********************************************************************************************************************************/
        
        internal static bool DetermineLoopIterations(ExecutionClass executionClass, int lineIndex)
        {
            var loopIterations = GetLoopIterations(executionClass, lineIndex, out var increaseIndexValue, out var currentIndexValue,
                out var foreachCollection);
            if (loopIterations == 0) return LoopIterationsError(executionClass, lineIndex);
            return SetLoopIterations(executionClass, lineIndex, loopIterations, increaseIndexValue, currentIndexValue, foreachCollection);
        }

        private static int GetLoopIterations(ExecutionClass executionClass, int lineIndex, 
            out object increaseIndexValue, out object currentIndexValue, out object foreachCollection)
        {
            int loopIterations = 0;
            increaseIndexValue = null;
            currentIndexValue = null;
            foreachCollection = null;

            var inputString = executionClass.linesString[lineIndex];

            if (inputString.IsForLoop())
            {
                var levelsClass = new LinesDetailClass.LevelsClass();

                var objString1 = inputString.PGCutAfter(";", true).Trim().PGCutBefore("=", true).Trim();
                var objString2 = inputString.PGCutBefore(";", true).PGCutAfter(";", true).
                    PGCutBefore("<", true).PGCutBefore(">", true).Trim();
                
                var obj1 = LineDetermination.DetermineValueFromConnectedString(executionClass, levelsClass, lineIndex, objString1);
                var obj2 = LineDetermination.DetermineValueFromConnectedString(executionClass, levelsClass, lineIndex, objString2);

                if (obj1 == null || obj2 == null) return 0;

                var increaseString = inputString.PGCutBeforeLast(";", true).PGCutAfterLast(")", true).Trim();

                currentIndexValue = obj1;
                
                if (increaseString.Contains("++"))
                {
                    increaseIndexValue = 1;
                }
                else if (increaseString.Contains("--"))
                {
                    increaseIndexValue = -1;
                }
                else
                {
                    var originalLine = executionClass.linesString[lineIndex];
                    executionClass.linesString[lineIndex] = increaseString;
                    LineInitialization.InitializeLine(executionClass, lineIndex);
                    LineDetermination.DetermineLine(executionClass, lineIndex);
                    var value = executionClass.linesDetailClasses[lineIndex].leftSideValue;
                    executionClass.linesString[lineIndex] = originalLine;
                    increaseIndexValue = value;
                }

                if (increaseIndexValue is int intValue && intValue == 0) return 0;
                if (increaseIndexValue is float floatValue && floatValue == 0) return 0;


                var loopOperatorString = inputString.PGCutAfterLast(";", true).PGCutBefore(";", true).Trim();

                if (increaseIndexValue is float)
                {
                    if (obj1 is int) obj1 = Convert.ToSingle(obj1);
                    if (obj2 is int) obj2 = Convert.ToSingle(obj2);

                    if(loopOperatorString.Contains("<"))
                        for (float i = (float) obj1; i < (float) obj2; i += (float) increaseIndexValue) loopIterations++;
                    else if(loopOperatorString.Contains(">"))
                        for (float i = (float) obj1; i > (float) obj2; i += (float) increaseIndexValue) loopIterations++;  
                }
                else if (increaseIndexValue is int)
                {
                    if (obj1 is float) obj1 = Convert.ToInt32(obj1);
                    if (obj2 is float) obj2 = Convert.ToInt32(obj2);
                    
                    if(loopOperatorString.Contains("<"))
                        for (int i = (int) obj1; i < (int) obj2; i += (int) increaseIndexValue) loopIterations++;
                    else if(loopOperatorString.Contains(">"))
                        for (int i = (int) obj1; i > (int) obj2; i += (int) increaseIndexValue) loopIterations++;  
                }
                currentIndexValue = obj1;
            }

            else if (inputString.IsForeachLoop())
            {
                var lastWhitespaceIndex = inputString.LastIndexOf(' ');
                var lastParenthesisIndex = inputString.LastIndexOf(')');
                var collectionName = inputString.Substring(lastWhitespaceIndex + 1, lastParenthesisIndex - lastWhitespaceIndex - 1);
                var objectIndex = executionClass.GetObjectIndexFromName(collectionName);
                
                if (objectIndex < 0)
                {
                    // Could be method, for example: foreach (var light in FindObjectsOfType<Light>())
                    var originalLine = executionClass.linesString[lineIndex];
                    executionClass.linesString[lineIndex] = collectionName;
                    LineInitialization.InitializeLine(executionClass, lineIndex);
                    LineDetermination.DetermineLine(executionClass, lineIndex);
                    executionClass.linesString[lineIndex] = originalLine;
                    for (int i = 0; i < executionClass.linesDetailClasses[lineIndex].levelsClasses.Count; i++)
                    {
                        if(executionClass.linesDetailClasses[lineIndex].levelsClasses[i].splittedValues.Length > 0)
                            foreachCollection = executionClass.linesDetailClasses[lineIndex].levelsClasses[i].splittedValues[^1];
                    }
                }
                else
                {
                    foreachCollection = executionClass.linesDetailClasses[objectIndex].leftSideValue;
                }
                
                loopIterations = PGCollectionsUtility.GetCollectionSize(foreachCollection);
            }

            return loopIterations;
        }

        private static bool SetLoopIterations(ExecutionClass executionClass, int lineIndex, int loopIterations, 
            object increaseIndexValue, object currentIndexValue, object foreachCollection)
        {
            for (int i = lineIndex; i < executionClass.linesString.Length; i++)
            {
                var loopClass = executionClass.loopClasses[i];
                if (!loopClass.partOfLoop) break;
                loopClass.loopIterations = loopIterations;
                loopClass.increaseIndexValue = increaseIndexValue;
                loopClass.currentIndexValue = currentIndexValue;
                loopClass.startIndexValue = currentIndexValue;
                loopClass.foreachCollection = foreachCollection;
            }

            for (int i = lineIndex; i < executionClass.linesString.Length; i++)
            {
                var loopClass = executionClass.loopClasses[i];
                if (!loopClass.partOfLoop) break;
                if (loopClass.innerLoopClass != null)
                {
                    var innerLoopIterations = GetLoopIterations(executionClass, i, out var _increaseIndexValue, out var _currentIndexValue, 
                        out var _foreachCollection);
                    if (innerLoopIterations == 0) return LoopIterationsError(executionClass, i);
                    for (int j = i; j < executionClass.linesString.Length; j++)
                    {
                        if (executionClass.loopClasses[j].innerLoopClass == null) break;
                        executionClass.loopClasses[j].innerLoopClass.loopIterations = innerLoopIterations;
                        executionClass.loopClasses[j].innerLoopClass.increaseIndexValue = _increaseIndexValue;
                        executionClass.loopClasses[j].innerLoopClass.currentIndexValue = _currentIndexValue;
                        executionClass.loopClasses[j].innerLoopClass.startIndexValue = _currentIndexValue;
                        executionClass.loopClasses[j].innerLoopClass.foreachCollection = _foreachCollection;
                    }

                    break;
                }
            }

            return true;
        }
        private static bool LoopIterationsError(ExecutionClass executionClass, int lineIndex)
        {
            DebugHandler.SendDebugError(executionClass.aIEngine, "Loop iterations in line "+lineIndex + " are 0.\n" +
                                                                 executionClass.linesString[lineIndex]);
            return false;
        }
        
        /********************************************************************************************************************************/

        internal static object GetForLoopCurrentIndexValue(LoopClass loopClass, string stringValue)
        {
            if (stringValue.Length != 1) return null;
            if (loopClass.innerLoopClass != null && loopClass.innerLoopClass.indexName == stringValue)
            {
                return loopClass.innerLoopClass.currentIndexValue;
            }
            if (loopClass.indexName == stringValue)
            {
                return loopClass.currentIndexValue;    
            }

            return null;
        }
        
        
        internal static object GetValueAsLoopCollectionItem(LoopClass loopClass, string itemName, ExecutionClass executionClass)
        {
            string collectionName = "";
            int loopIndex = 0;
            
            // For loop
            if (itemName.Contains("["))
            {
                collectionName = itemName.PGCutAfter("[", true);
                loopIndex = loopClass.loopIndex;
            }
            // Foreach loop
            else
            {
                for (int i = 0; i < executionClass.linesString.Length; i++)
                {
                    var line = executionClass.linesString[i];
                    if(!line.IsForeachLoop()) continue;

                    if (executionClass.loopClasses[i].innerLoopClass != null)
                    {
                        if (itemName == executionClass.loopClasses[i].innerLoopClass.indexName)
                        {
                            collectionName = line.PGCutBefore(" in ", true).PGCutAfterLast(")", true).Trim();
                            loopIndex = executionClass.loopClasses[i].innerLoopClass.loopIndex;
                            // If foreachCollection is a method, for example: foreach (var light in FindObjectsOfType<Light>())
                            var foreachCollection = executionClass.loopClasses[i].innerLoopClass.foreachCollection;
                            if (foreachCollection != null) return PGCollectionsUtility.GetCollectionItemAtIndex(foreachCollection, loopIndex);
                        }
                    }
                    else if (itemName == executionClass.loopClasses[i].indexName)
                    {
                        collectionName = line.PGCutBefore(" in ", true).PGCutAfterLast(")", true).Trim();
                        loopIndex = loopClass.loopIndex;
                        // If foreachCollection is a method. For example: foreach (var light in FindObjectsOfType<Light>())
                        var foreachCollection = executionClass.loopClasses[i].foreachCollection;
                        if (foreachCollection != null) return PGCollectionsUtility.GetCollectionItemAtIndex(foreachCollection, loopIndex);
                    }
                }
            }
            
            if (string.IsNullOrWhiteSpace(collectionName)) return null;

            for (int i = 0; i < loopClass.loopItemNames.Count; i++)
            {
                if(itemName != loopClass.loopItemNames[i]) continue;
                
                for (int j = 0; j < executionClass.linesDetailClasses.Length; j++)
                {
                    if(executionClass.linesDetailClasses[j].leftSideName != collectionName) continue;
                    if(executionClass.linesDetailClasses[j].leftSideValue == null) continue;
                    if(!PGCollectionsUtility.IsCollection(executionClass.linesDetailClasses[j].leftSideValue)) continue;
                    var collection = executionClass.linesDetailClasses[j].leftSideValue;
                    var listItem = PGCollectionsUtility.GetCollectionItemAtIndex(collection, loopIndex);
                    return listItem;
                }
            }
            return null;
        }

        internal static void SetValueFromLoopCollectionItem(ExecutionClass executionClass,
            LinesDetailClass.LevelsClass levelsClass, int lineIndex)
        {
            if (levelsClass.level != 0) return;
            
            var leftSideString = executionClass.linesString[lineIndex].PGCutAfter("=", true).Trim();
            
            LinesDetailClass linesDetailClass = executionClass.linesDetailClasses[lineIndex];
            
            if (!leftSideString.Contains("."))
            {
                if (string.IsNullOrWhiteSpace(linesDetailClass.leftSideName)) linesDetailClass.leftSideName = leftSideString;
                linesDetailClass.SetLeftSideValue(executionClass, levelsClass.parameterValues[^1]);
                return;
            }

            var tokens = leftSideString.Split(new[] {'.'}, StringSplitOptions.RemoveEmptyEntries);
            if (tokens.Length == 0) return;
            var reference = GetValueAsLoopCollectionItem(executionClass.loopClasses[lineIndex], tokens[0], executionClass);
            if (reference == null) return;

            ReflectionGetSet.SetValueByName(reference.GetType(), leftSideString.PGCutBefore(".", true),
                levelsClass.parameterValues[^1], reference);
            linesDetailClass.SetLeftSideValue(executionClass, levelsClass.parameterValues[^1]);
        }

        /********************************************************************************************************************************/

        private static void SetLoops(ExecutionClass executionClass, int index, LoopClass loopClass)
        {
            loopClass.partOfLoop = true;
            int openBrackets = 0;
            for (;;)
            {
                index++;
                if (index >= executionClass.loopClasses.Length) break;
                if (executionClass.linesString[index].Contains("{")) openBrackets++;
                else if (executionClass.linesString[index].Contains("}")) openBrackets--;
                if (openBrackets <= 0) break;
                executionClass.loopClasses[index].partOfLoop = true;
            }
        }

        private static void SetForLoopItemNames(ExecutionClass executionClass, int index, string inputString, LoopClass loopClass)
        {
            var loopIndexName = inputString.PGCutBefore(";", true).Trim()[0];
            var forLoopInstanceName = "";
            
            if (inputString.Contains(".Count")) forLoopInstanceName = inputString.PGCutAfter(".Count", true);
            else if (inputString.Contains(".Length")) forLoopInstanceName = inputString.PGCutAfter(".Length", true).Trim();
            
            forLoopInstanceName = forLoopInstanceName.PGCutBeforeLast(" ", false).Trim();
            
            // Name of items not mentioned in loop header.
            if (string.IsNullOrWhiteSpace(forLoopInstanceName))
            {
                List<string> collectionNames = new List<string>();
                collectionNames.Add("Default_Used_for_LINQ");
                for (int i = 0; i < index + 1; i++)
                {
                    if(executionClass.linesDetailClasses[i].leftSideValue == null) continue;
                    if(!PGCollectionsUtility.IsCollection(executionClass.linesDetailClasses[i].leftSideValue)) continue;
                    collectionNames.Add(executionClass.linesDetailClasses[i].leftSideName);
                }
                for (int i = index +1; i < executionClass.linesString.Length; i++)
                {
                    int collectionNamesIndex = collectionNames.Select((s, i) => new { Value = s, Index = i })
                        .Where(x => executionClass.linesString[i].Contains(x.Value))
                        .Select(x => x.Index)
                        .FirstOrDefault();
                    if(collectionNamesIndex == 0) continue;
                    forLoopInstanceName = collectionNames[collectionNamesIndex];
                    break;
                }
            }
            
            forLoopInstanceName = forLoopInstanceName + "[" + loopIndexName + "]";
            SetItemNames(executionClass, index, forLoopInstanceName);
        }

        private static void SetIndexNames(ExecutionClass executionClass, int lineIndex, string inputString)
        {
            string indexName = "";
            if(inputString.IsForLoop())
                indexName = inputString.PGCutAfter("=", true).PGCutBefore("int", true).PGCutBefore("float", true).Trim();
            else if (inputString.IsForeachLoop())
            {
                indexName = inputString.PGCutAfter(" in ", true).Trim();
                indexName = indexName.PGGetSubstringBeforeIndex(indexName.Length - 1).Trim();
            }
            
            if (executionClass.loopClasses[lineIndex].innerLoopClass == null)
            {
                for (int i = lineIndex; i < executionClass.linesString.Length; i++)
                {
                    if (!executionClass.loopClasses[i].partOfLoop) break;
                    executionClass.loopClasses[i].indexName = indexName;
                }
            }
            else
            {
                for (int i = lineIndex; i < executionClass.linesString.Length; i++)
                {
                    if (executionClass.loopClasses[i].innerLoopClass == null) break;
                    executionClass.loopClasses[i].innerLoopClass.indexName = indexName;
                }
            }
        }
        private static void SetForeachLoopItemName(ExecutionClass executionClass, int index, string inputString, LoopClass loopClass)
        {
            var innerBrackets = inputString.PGCutBefore("(", true).PGCutAfter(")", true);
            var words = innerBrackets.Split(' ');
            var foreachLoopInstanceName = words[1].Trim();
            SetItemNames(executionClass, index, foreachLoopInstanceName);
            
        }

        private static void SetItemNames(ExecutionClass executionClass, int index, string itemName)
        {
            int openBrackets = 0;
            for (;;)
            {
                index++;
                if (index >= executionClass.loopClasses.Length) break;
                if (executionClass.linesString[index].Contains("{")) openBrackets++;
                else if (executionClass.linesString[index].Contains("}")) openBrackets--;
                if (openBrackets <= 0) break;
                executionClass.loopClasses[index].loopItemNames.Add(itemName);

                // Line could have multiple collections in it.
                string input = executionClass.linesString[index];
                var stringIndexes = PGStringUtility.GetIndexesOfString(input, "]");
                for (int i = 0; i < stringIndexes.Length; i++)
                {
                    var subName = input.PGGetSubstringBeforeIndex(stringIndexes[i]);
                    if(subName == itemName) continue;
                    executionClass.loopClasses[index].loopItemNames.Add(subName);
                }
            }
        }
        
    }
}