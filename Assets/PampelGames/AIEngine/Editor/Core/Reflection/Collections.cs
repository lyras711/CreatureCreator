// ----------------------------------------------------
// AI Engine
// Copyright (c) Pampel Games e.K. All Rights Reserved.
// https://www.pampelgames.com
// ----------------------------------------------------

using System;
using System.Collections.Generic;
using System.Globalization;
using PampelGames.Shared.Utility;
using UnityEngine;

namespace PampelGames.AIEngine.Editor
{
    internal static class Collections
    {
        
        internal static bool CreateList(ExecutionClass executionClass, int index)
        {
            var stringValue = executionClass.linesString[index];
            if (!stringValue.Contains("List<")) return false;
            
            string listName = stringValue.PGCutBefore(">", true).PGCutAfter(";", false).PGCutAfter("=", true).Trim();
            executionClass.linesDetailClasses[index].leftSideName = listName;
            string listTypeName = stringValue.PGCutBefore("<", true).PGCutAfter(">", true);
            var value = ReflectionUtility.CreateValueTypeFromString(listTypeName, new List<string>());
            Type listType = null;
            if (value != null) listType = value.GetType();
            if(listType == null) listType = PGReflectionUtility.GetTypeByName(listTypeName, executionClass.namespaces);
            if (listType == null)
            {
                DebugHandler.SendDebugError(executionClass.aIEngine, "Couldn't create list in line "+index +".\n" + stringValue);
                return false;
            }
            var list = PGCollectionsUtility.CreateListOfType(listType);
            var listValues = CreateValueList(stringValue, listType);
            for (int i = 0; i < listValues.Count; i++) list.Add(listValues[i]);
            
            executionClass.linesDetailClasses[index].SetLeftSideValue(executionClass, list);
            
            return true;
        }
        
        internal static bool CreateArray(ExecutionClass executionClass, int lineIndex)
        {
            var stringValue = executionClass.linesString[lineIndex];
            if (!stringValue.Contains("[] ")) return false;
            
            string arrayName = stringValue.PGCutBefore("]", true).PGCutAfter(";", false).PGCutAfter("=", true).Trim();
            executionClass.linesDetailClasses[lineIndex].leftSideName = arrayName;
            string listTypeName = stringValue.PGCutAfter("[", true).Trim();
            if (listTypeName.Contains(" ")) listTypeName = listTypeName.PGCutBefore(" ", true).Trim();
            var value = ReflectionUtility.CreateValueTypeFromString(listTypeName, new List<string>());
            Type arrayType = null;
            if (value != null) arrayType = value.GetType();
            
            if(arrayType == null) arrayType = PGReflectionUtility.GetTypeByName(listTypeName, executionClass.namespaces);
            if (arrayType == null)
            {
                DebugHandler.SendDebugError(executionClass.aIEngine, "Couldn't create array in line "+lineIndex +".\n" + stringValue);
                return false;
            }
            var arrayValues = CreateValueList(stringValue, arrayType);
            var array = PGCollectionsUtility.CreateArrayOfType(arrayType, arrayValues.Count);
            for (int i = 0; i < arrayValues.Count; i++) array.SetValue(arrayValues[i], i);
            executionClass.linesDetailClasses[lineIndex].SetLeftSideValue(executionClass, array);

            return true;
        }

        /********************************************************************************************************************************/
        
        internal static object GetCollectionSizeFromName(ExecutionClass executionClass, string splittedString)
        {
            if (!splittedString.Contains(".Length") && !splittedString.Contains(".Count")) return null;
            
            splittedString = splittedString.PGCutAfter(".", true);
            for (var i = 0; i < executionClass.linesDetailClasses.Length; i++)
            {
                if (executionClass.linesDetailClasses[i].leftSideName == splittedString)
                {
                    if (executionClass.linesDetailClasses[i].leftSideValue == null) break;
                    int length = PGCollectionsUtility.GetCollectionSize(executionClass.linesDetailClasses[i].leftSideValue);
                    return length;
                }
            }
            return null;
        }
        
        internal static object GetCollection(ExecutionClass executionClass, string collectionName)
        {
            for (var i = 0; i < executionClass.linesDetailClasses.Length; i++)
            {
                if (executionClass.linesDetailClasses[i].leftSideName == collectionName)
                {
                    if (executionClass.linesDetailClasses[i].leftSideValue == null || 
                        !PGCollectionsUtility.IsCollection(executionClass.linesDetailClasses[i].leftSideValue)) break;
                    return executionClass.linesDetailClasses[i].leftSideValue;
                }
            }
            
            return null;
        }
        
        internal static object GetValueFromCollection(ExecutionClass executionClass, LinesDetailClass.LevelsClass levelsClass, 
            int lineIndex, string stringValue)
        {
            string collectionName = "";
            if(stringValue.Contains("[")) collectionName = stringValue.PGCutAfter("[", true);
            if (collectionName == "") return null;
            var collection = executionClass.GetValueFromName(collectionName);
            if (collection == null) return null;
            var integerString = stringValue.PGCutBefore("[", true).PGCutAfter("]", true);
            object integerValue =
                LineDetermination.DetermineValueFromConnectedString(executionClass, levelsClass, lineIndex, integerString);
            if (integerValue == null) return null;
            if (integerValue is float) integerValue = Mathf.RoundToInt((float) integerValue);
            if (!(integerValue is int)) return null;

            var item = PGCollectionsUtility.GetCollectionItemAtIndex(collection, (int) integerValue);
            return item;
        }
        
        /********************************************************************************************************************************/
        /********************************************************************************************************************************/
        
        private static List<object> CreateValueList(string stringValue, Type listType)
        {
            List<object> valueList = new List<object>();

            if (!(stringValue.Contains("{") && stringValue.Contains("}"))) return valueList;
            string namesCombined = stringValue.PGCutBefore("{", true).PGCutAfter("}", true);
            string[] namesSeperated = namesCombined.Split(",");
            for (int i = 0; i < namesSeperated.Length; i++)
            {
                namesSeperated[i] = namesSeperated[i].Trim();
                if (listType == typeof(string))
                {
                    string removeCurly = namesSeperated[i].Replace("\"", " ").Trim();
                    valueList.Add(removeCurly);
                }
                else if (listType == typeof(int))
                {
                    int.TryParse(namesSeperated[i], out var myInt);
                    valueList.Add(myInt);
                }
                else if (listType == typeof(float))
                {
                    var floatName = namesSeperated[i].PGCutAfter("f", true);
                    float.TryParse(floatName, NumberStyles.Float, CultureInfo.InvariantCulture, out var myFloat);
                    valueList.Add(myFloat);
                }
            }

            return valueList;
        }
    }
}
