// ----------------------------------------------------
// AI Engine
// Copyright (c) Pampel Games e.K. All Rights Reserved.
// https://www.pampelgames.com
// ----------------------------------------------------

using System.Collections.Generic;
using System.Linq;
using PampelGames.Shared.Utility;
using UnityEngine;

namespace PampelGames.AIEngine.Editor
{
    /// <summary>
    ///     Methods within the code lines created by the AI.
    /// </summary>
    internal static class CodeMethods
    {
        internal static void CreateCodeMethods(ExecutionClass executionClass)
        {
            var overMainMethod = false;
            for (var i = 0; i < executionClass.linesString.Length; i++)
            {
                if (!executionClass.linesString[i].IsCodeMethod(executionClass.linesString, i)) continue;
                
                if (!overMainMethod)
                {
                    overMainMethod = true;
                    continue;
                }

                var methodName = executionClass.linesString[i].PGCutBefore(" ", true).Trim();
                var methodNameRaw = methodName.PGCutBetween("(", ")", false).Trim();
                var parameterStrings = methodName.PGCutBefore("(", true).PGCutAfter(")", true).Trim();

                executionClass.codeMethodClasses[i] = new CodeMethodClass();
                executionClass.codeMethodClasses[i].methodNameRaw = methodNameRaw;
                executionClass.codeMethodClasses[i].parameterStrings = parameterStrings;
                var bracket = 0;
                for (var j = i + 1; j < executionClass.linesString.Length; j++)
                {
                    if (executionClass.linesString[j] == "{") bracket++;
                    else if (executionClass.linesString[j] == "}") bracket--;
                    if (bracket <= 0) break;
                    executionClass.codeMethodClasses[j] = new CodeMethodClass();
                    executionClass.codeMethodClasses[j].methodNameRaw = methodNameRaw;
                    executionClass.codeMethodClasses[j].parameterStrings = parameterStrings;
                    i++;
                }
            }
        }

        /********************************************************************************************************************************/

        internal static object GetValueFromCodeMethod(ExecutionClass executionClass, int lineIndex, string splittedString)
        {
            if (!splittedString.Contains(Constants.InnerBracketLevel)) return null;

            var methodName = splittedString.PGCutBefore(executionClass.parentClass + ".", true).Trim();
            var methodNameRaw = methodName.PGCutBetween("(", ")", false).Trim();
            var startIndex = 0;

            bool codeMethodExists = false;
            bool codeMethodFound = false;
            for (var i = 0; i < executionClass.codeMethodClasses.Length; i++)
            {
                startIndex = i;
                if (executionClass.codeMethodClasses[i] == null) continue;
                codeMethodExists = true;
                if (executionClass.codeMethodClasses[i].methodNameRaw == methodNameRaw)
                {
                    codeMethodFound = true;
                    break;
                }
                if (i >= executionClass.codeMethodClasses.Length - 1) return null;
            }

            if (!codeMethodExists) return null;
            if (!codeMethodFound) return null;
            var endIndex = 0;
            
            for (var i = startIndex; i < executionClass.codeMethodClasses.Length; i++)
            {
                if (executionClass.codeMethodClasses[i] == null) continue;
                if (executionClass.codeMethodClasses[i].methodNameRaw != methodNameRaw) break;
                endIndex = i;
            }

            if (!executionClass.linesString[endIndex].Contains("return "))
            {
                if(!executionClass.linesString[startIndex].Contains("void "))
                  return CodeMethodError(executionClass, lineIndex);
            }
            
            DetermineParameters(executionClass, lineIndex, startIndex, splittedString, out var parameterNames, out var parameters);

            if (parameters != null)
            {
                if (parameters.Length != parameterNames.Length) return CodeMethodError(executionClass, lineIndex);

                for (int i = 0; i < parameterNames.Length; i++)
                {
                    bool found = false;
                    var newLineEnd = parameterNames[i] + " = " + parameters[i];

                    for (int j = 0; j < startIndex; j++)
                    {
                        if (executionClass.linesString[j].IsCodeMethod(executionClass.linesString, j)) break;
           
                        var currentName = executionClass.linesString[j].PGCutBefore(" ", true).Trim();
                        currentName = currentName.PGCutAfter(" = ", true).Trim();
                        var currentNameStart = executionClass.linesString[j].PGCutAfter(" ", true).Trim();
                        
                        if (newLineEnd.Contains(currentName))
                        {
                            executionClass.linesString[j] = currentNameStart + " " + newLineEnd;
                            executionClass.linesDetailClasses[j].leftSideValue = parameters[i];
                            found = true;
                            break;
                        }
                    }
                    // Name of the variable not found. Should have been created in ConversionResponse from the method parameters.
                    // For example: private Vector3 GetHelixPosition(int index, float johan) -> int index & float johan
                    if(!found) return CodeMethodError(executionClass, lineIndex);
                }
            }
            
            var objectList = new List<GameObject>();

            for (var i = startIndex + 1; i < endIndex; i++) Execution.ExecuteLinesDetailClass(executionClass, i, ref objectList);
            

            var originalReturnString = executionClass.linesString[endIndex];
            var returnString = originalReturnString.Replace("return ", "object return = ");
            
            executionClass.linesString[endIndex] = returnString;
            LineInitialization.InitializeLine(executionClass, endIndex);
            Execution.ExecuteLinesDetailClass(executionClass, endIndex, ref objectList);
            executionClass.linesString[endIndex] = originalReturnString;
            
            var returnValue = executionClass.linesDetailClasses[endIndex].leftSideValue;
            if (returnValue == null) returnValue = Constants.EmptyReturnValue;
            return returnValue;
        }

        /********************************************************************************************************************************/

        private static object CodeMethodError(ExecutionClass executionClass, int lineIndex)
        {
            DebugHandler.SendDebugError(executionClass.aIEngine, "Couldn't determine value from Code Method in line " + lineIndex + ".\n" +
                                                                 executionClass.linesString[lineIndex]);
            return null;
        }
        
        /********************************************************************************************************************************/
        
        private static void DetermineParameters(ExecutionClass executionClass, int lineIndex, int startIndex, string splittedString, 
            out string[] parameterNames, out object[] parameters)
        {
            parameterNames = null;
            parameters = null;
            
            var linesDetailClass = executionClass.linesDetailClasses[lineIndex];
            var innerBracketLevel = ReflectionUtility.GetIntegerAfterString(splittedString, Constants.InnerBracketLevel);
            for (var i = 0; i < linesDetailClass.levelsClasses.Count; i++)
                if (linesDetailClass.levelsClasses[i].level == innerBracketLevel)
                {
                    if (linesDetailClass.levelsClasses[i].parameterValues.Count > 0)
                    {
                        parameters = linesDetailClass.levelsClasses[i].parameterValues.ToArray();
                        parameterNames = new string[linesDetailClass.levelsClasses[i].parameterValues.Count];
                        for (int j = 0; j < parameterNames.Length; j++)
                        {
                            parameterNames = executionClass.codeMethodClasses[startIndex].parameterStrings.Split(",");
                            for (int k = 0; k < parameterNames.Length; k++) parameterNames[k] = parameterNames[k].Trim();
                            for (int k = 0; k < parameterNames.Length; k++) parameterNames[k] = parameterNames[k].PGCutBefore(" ", true).Trim();
                        }
                    }
                    return;
                }
        }
    }
}