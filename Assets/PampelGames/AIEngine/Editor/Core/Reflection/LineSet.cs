// ----------------------------------------------------
// AI Engine
// Copyright (c) Pampel Games e.K. All Rights Reserved.
// https://www.pampelgames.com
// ----------------------------------------------------

using System;
using PampelGames.Shared.Utility;
using UnityEngine;

namespace PampelGames.AIEngine.Editor
{
    internal static class LineSet
    {
        internal static void SetLeftSideValue(ExecutionClass executionClass, LinesDetailClass.LevelsClass levelsClass,
            int lineIndex)
        {
            if (!Validate(executionClass, levelsClass, lineIndex)) return;
            if (SetValueInObject(executionClass, levelsClass, lineIndex, executionClass.loopClasses[lineIndex])) return;
            if (SetValueInMethod(executionClass, levelsClass, lineIndex, executionClass.loopClasses[lineIndex])) return;
            if (SetValueInStaticProperty(executionClass, levelsClass, lineIndex)) return;
            Loops.SetValueFromLoopCollectionItem(executionClass, levelsClass, lineIndex);
        }

        internal static void SetLeftSideBasicValue(ExecutionClass executionClass, string valueName, object value)
        {
            for (int i = 0; i < executionClass.linesDetailClasses.Length; i++)
            {
                if (executionClass.linesString[i].IsCodeMethod(executionClass.linesString, i)) break;
                if (executionClass.linesDetailClasses[i].leftSideName == valueName)
                {
                    executionClass.linesDetailClasses[i].leftSideValue = value;
                    break;
                }
            }    
        }
        
        /********************************************************************************************************************************/

        private static bool Validate(ExecutionClass executionClass, LinesDetailClass.LevelsClass levelsClass,
            int lineIndex)
        {
            if (!executionClass.linesString[lineIndex].Contains(Constants.EqualSign)) return false;
            if (levelsClass.parameterValues.Count == 0) return false;

            return true;
        }

        /********************************************************************************************************************************/

        internal static bool IsLeftSideMethod(ExecutionClass executionClass, LinesDetailClass.LevelsClass levelsClass,
            int lineIndex)
        {
            var lineString = executionClass.linesString[lineIndex];
            if (!lineString.Contains(Constants.EqualSign)) return false;
            var leftSideString = lineString.PGCutAfter(Constants.EqualSign, true).Trim();
            if (!leftSideString.Contains("(") || levelsClass.level != 100) return false;
            return true;
        }

        /********************************************************************************************************************************/

        private static bool SetValueInObject(ExecutionClass executionClass, LinesDetailClass.LevelsClass levelsClass,
            int lineIndex, LoopClass loopClass)
        {
            if (levelsClass.level != 0 && levelsClass.level != 100) return false;

            var linesDetailClass = executionClass.linesDetailClasses[lineIndex];
            var leftSideString = executionClass.linesString[lineIndex].PGCutAfter(Constants.EqualSign, true).Trim();

            if (leftSideString.Contains("(")) return false;

            if (!leftSideString.Contains("."))
            {
                var tokens = leftSideString.Split(new[] {' '}, StringSplitOptions.RemoveEmptyEntries);
                if (tokens.Length != 2) return false;
                if (string.IsNullOrWhiteSpace(linesDetailClass.leftSideName)) linesDetailClass.leftSideName = tokens[1];
                linesDetailClass.SetLeftSideValue(executionClass, levelsClass.parameterValues[^1]);
            }
            else
            {
                var tokens = leftSideString.Split(new[] {'.'}, StringSplitOptions.RemoveEmptyEntries);
                if (tokens.Length == 0) return false;
                object reference = null;
                reference = executionClass.GetValueFromName(tokens[0]);
                
                if (reference == null && loopClass != null) reference = Loops.GetValueAsLoopCollectionItem(loopClass, tokens[0], executionClass);
                if (reference == null) reference = Collections.GetValueFromCollection(executionClass, levelsClass, lineIndex, tokens[0]);
                
                if (reference == null) return false;

                var connectedPropertyName = leftSideString.PGCutBefore(".", true).Trim();

                ReflectionGetSet.SetValueByName(reference.GetType(), connectedPropertyName, levelsClass.parameterValues[^1], reference);
                linesDetailClass.SetLeftSideValue(executionClass, levelsClass.parameterValues[^1]);
            }

            return true;
        }


        // Left side contains a method, for example: cube.AddComponent<Rigidbody>().mass = 3; 
        private static bool SetValueInMethod(ExecutionClass executionClass, LinesDetailClass.LevelsClass levelsClass,
            int lineIndex, LoopClass loopClass)
        {
            var linesDetailClass = executionClass.linesDetailClasses[lineIndex];
            var leftSideString = executionClass.linesString[lineIndex].PGCutAfter(Constants.EqualSign, true).Trim();

            if (!leftSideString.Contains("(") || levelsClass.level != 100) return false;

            var splittedString = levelsClass.splittedStrings[^1];
            if (!splittedString.Contains("(") || !splittedString.Contains(".")) return SetValueInMethodError(executionClass, lineIndex);
            if (splittedString.EndsWith(")")) return SetValueInMethodError(executionClass, lineIndex);


            var propertyName = splittedString.PGCutBeforeLast(")", true).Trim();
            propertyName = propertyName.PGCutBefore(".", true).Trim();
            var methodString = splittedString.PGCutAfterLast(")", false).Trim();

            var methodReference = Methods.GetValueFromMethod(executionClass, levelsClass, lineIndex, methodString);
            if (methodReference == null) return SetValueInMethodError(executionClass, lineIndex);

            LinesDetailClass.LevelsClass level0 = null;
            for (var i = 0; i < linesDetailClass.levelsClasses.Count; i++)
            {
                if (linesDetailClass.levelsClasses[i].level != 0) continue;
                level0 = linesDetailClass.levelsClasses[i];
                break;
            }

            if (level0 == null) return false;
            ReflectionGetSet.SetValueByName(methodReference.GetType(),
                propertyName, level0.parameterValues[0], methodReference);

            return true;
        }

        private static bool SetValueInMethodError(ExecutionClass executionClass, int lineIndex)
        {
            DebugHandler.SendDebugError(executionClass.aIEngine, "Expected to set left side value in method but not able to determine.\n" +
                                                                 executionClass.linesString[lineIndex]);
            return false;
        }

        private static bool SetValueInStaticProperty(ExecutionClass executionClass, LinesDetailClass.LevelsClass levelsClass,
            int detailIndex)
        {
            if (levelsClass.level != 0 && levelsClass.level != 100) return false;

            var leftSideString = executionClass.linesString[detailIndex].PGCutAfter(Constants.EqualSign, true).Trim();
            if (!leftSideString.Contains(".")) return false;

            var typeName = leftSideString.PGCutAfter(".", true);
            var type = PGReflectionUtility.GetTypeByName(typeName, executionClass.namespaces);

            ReflectionGetSet.SetValueByName(type, leftSideString.PGCutBefore(".", true),
                levelsClass.parameterValues[^1]);

            return true;
        }
    }
}
