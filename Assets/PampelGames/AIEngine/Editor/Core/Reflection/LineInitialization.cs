// ----------------------------------------------------
// AI Engine
// Copyright (c) Pampel Games e.K. All Rights Reserved.
// https://www.pampelgames.com
// ----------------------------------------------------

using System;
using PampelGames.Shared.Utility;
using UnityEngine;
using Object = UnityEngine.Object;

namespace PampelGames.AIEngine.Editor
{
    internal static class LineInitialization
    {
        internal static void InitializeLine(ExecutionClass executionClass, int lineIndex)
        {
            executionClass.linesDetailClasses[lineIndex] = new LinesDetailClass();
            CreateBasics(executionClass, lineIndex);
            executionClass.linesDetailClasses[lineIndex].CreateRightSideLevels(executionClass, executionClass.linesString[lineIndex]);
            executionClass.linesDetailClasses[lineIndex].CreateSplittedStrings();
        }

        private static void CreateBasics(ExecutionClass executionClass, int lineIndex)
        {
            if (CreateCodeBasics(executionClass, lineIndex)) return;
            if (CreateReferences(executionClass, lineIndex)) return;
            if (Collections.CreateList(executionClass, lineIndex)) return;
            if (Collections.CreateArray(executionClass, lineIndex)) return;
            if (CreateValueType(executionClass, lineIndex)) return;
            if (Loops.CreateLoop(executionClass, lineIndex)) return;
        }

        /********************************************************************************************************************************/

        private static bool CreateCodeBasics(ExecutionClass executionClass, int lineIndex)
        {
            if (lineIndex == 0 && executionClass.parentClass == "MonoBehaviour")
                executionClass.MonoBehaviourObject = new GameObject(Constants.MonoBehaviourObject);

            var linesString = executionClass.linesString[lineIndex];
            
            if (string.IsNullOrWhiteSpace(linesString)) return true;
            
            if (linesString.Contains("using "))
            {
                executionClass.linesDetailClasses[lineIndex].leftSideName = "namespace";
                return true;
            }

            if (linesString.Contains(" : ") && !linesString.Contains("?"))
            {
                executionClass.linesDetailClasses[lineIndex].leftSideName = "class";
                return true;
            }

            if (linesString.Contains("void ") && linesString.Contains("("))
            {
                executionClass.linesDetailClasses[lineIndex].leftSideName = "method";
                return true;
            }

            return false;
        }

        private static bool CreateReferences(ExecutionClass executionClass, int lineIndex)
        {
            var stringValue = executionClass.linesString[lineIndex];

            if (stringValue.Contains("List<"))
            {
                var listName = stringValue.PGCutBefore(">", true).PGCutAfter(";", false).PGCutAfter("=", true).Trim();
                var listTypeName = stringValue.PGCutBefore("<", true).PGCutAfter(">", true);
                var listType = PGReflectionUtility.GetTypeByName(listTypeName, executionClass.namespaces);
                if (listName == Constants.Prefabs && listType == typeof(GameObject))
                {
                    executionClass.linesDetailClasses[lineIndex].leftSideName = listName;
                    executionClass.linesDetailClasses[lineIndex].SetLeftSideValue(executionClass, executionClass.prefabs);
                    return true;
                }

                if (listName == Constants.References && listType == typeof(GameObject))
                {
                    executionClass.linesDetailClasses[lineIndex].leftSideName = listName;
                    executionClass.linesDetailClasses[lineIndex].SetLeftSideValue(executionClass, executionClass.references);
                    return true;
                }
            }
            else if (stringValue.Contains("GameObject ") && stringValue.Contains(" " + Constants.Prefab)
                                                         && !stringValue.Contains(Constants.EqualSign) && !stringValue.Contains("("))
            {
                var prefabName = stringValue.PGCutBefore(Constants.Prefab, false).PGCutAfter(";", true).Trim();
                executionClass.linesDetailClasses[lineIndex].leftSideName = prefabName;
                executionClass.linesDetailClasses[lineIndex].SetLeftSideValue(executionClass, executionClass.prefabs);
                return true;
            }
            else if (stringValue.Contains("GameObject ") && stringValue.Contains(" " + Constants.Reference)
                                                         && !stringValue.Contains(Constants.EqualSign) && !stringValue.Contains("("))
            {
                var referenceName = stringValue.PGCutBefore(Constants.Reference, false).PGCutAfter(";", true).Trim();
                executionClass.linesDetailClasses[lineIndex].leftSideName = referenceName;
                executionClass.linesDetailClasses[lineIndex].SetLeftSideValue(executionClass, executionClass.references);
                return true;
            }

            return false;
        }


        private static bool CreateValueType(ExecutionClass executionClass, int lineIndex)
        {
            if (executionClass.linesString[lineIndex].IsForLoop() || executionClass.linesString[lineIndex].IsForeachLoop()) return false;
            
            var tokens = executionClass.linesString[lineIndex].Split(new[] {' '}, StringSplitOptions.RemoveEmptyEntries);

            if (tokens.Length < 2) return false;
            if (tokens[0].Contains(".")) return false;
            if (tokens[1].Contains(".")) return false;

            if (tokens.Length == 2)
            {
                executionClass.linesDetailClasses[lineIndex].leftSideName = tokens[1];
                var leftSideValue = ReflectionUtility.CreateValueTypeFromString(tokens[0], executionClass.namespaces);
                // Could be component from the scene.
                if (leftSideValue == null && tokens[0] != "GameObject")
                {
                    Type type = PGReflectionUtility.GetTypeByName(tokens[0], executionClass.namespaces);
                    if(type != null) leftSideValue = Object.FindObjectOfType(type);
                }
                // Sometimes AI makes empty prefab values when creating primitives
                else if (leftSideValue == null && tokens[0] == "GameObject")
                {
                    if (tokens[1] == Constants.PlanePrefabName) leftSideValue = GameObject.CreatePrimitive(PrimitiveType.Plane);
                    else if (tokens[1] == Constants.CapsulePrefabName) leftSideValue = GameObject.CreatePrimitive(PrimitiveType.Capsule);
                    else if (tokens[1] == Constants.CubePrefabName) leftSideValue = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    else if (tokens[1] == Constants.CylinderPrefabName) leftSideValue = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                    else if (tokens[1] == Constants.QuadPrefabName) leftSideValue = GameObject.CreatePrimitive(PrimitiveType.Quad);
                    else if (tokens[1] == Constants.SpherePrefabName) leftSideValue = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                    if(leftSideValue != null) executionClass.defaultPrimitive = (GameObject) leftSideValue;
                }
                executionClass.linesDetailClasses[lineIndex].SetLeftSideValue(executionClass, leftSideValue);
            }
            else if (tokens[2] == "=")
            {
                string connectedString = "";
                for (int i = 3; i < tokens.Length; i++) connectedString += tokens[i];
                LinesDetailClass.LevelsClass levelsClass = new LinesDetailClass.LevelsClass();
                var leftSideValue = LineDetermination.DetermineValueFromConnectedString(executionClass, levelsClass, lineIndex, connectedString);
                executionClass.linesDetailClasses[lineIndex].leftSideName = tokens[1];
                executionClass.linesDetailClasses[lineIndex].SetLeftSideValue(executionClass, leftSideValue);
            }

            var _leftSideValue = executionClass.linesDetailClasses[lineIndex].leftSideValue;

            if (_leftSideValue == null) return false;
            
            return true;
        }
    }
}