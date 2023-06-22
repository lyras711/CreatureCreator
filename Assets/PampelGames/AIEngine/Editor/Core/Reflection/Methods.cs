// ----------------------------------------------------
// AI Engine
// Copyright (c) Pampel Games e.K. All Rights Reserved.
// https://www.pampelgames.com
// ----------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using PampelGames.Shared.Utility;
using UnityEngine;

namespace PampelGames.AIEngine.Editor
{
    internal static class Methods
    {
        internal static object GetValueFromMethod(ExecutionClass executionClass, LinesDetailClass.LevelsClass levelsClass,
            int lineIndex, string splittedString)
        {
            var linesDetailClass = executionClass.linesDetailClasses[lineIndex];
            levelsClass.methodClass = new MethodClass();
            var directBracketObject = GetInnerBracketDirect(levelsClass.methodClass, executionClass, linesDetailClass, splittedString);
            if (directBracketObject != null) return directBracketObject;
            if (!CheckIfMethod(splittedString)) return null;
            GetMethodObject(levelsClass.methodClass, executionClass, lineIndex, splittedString);
            var objectEndValue = CheckForObjectEndValue(levelsClass.methodClass, splittedString);
            if (objectEndValue != null) return objectEndValue;
            if (!DetermineClassType(levelsClass.methodClass, executionClass.namespaces, splittedString)) return null;
            if (!DetermineMethod(levelsClass.methodClass, splittedString)) return null;
            if (!DetermineGenericTypeArguments(levelsClass.methodClass, executionClass, splittedString)) return null;
            if (!DetermineParameters(levelsClass.methodClass, executionClass, linesDetailClass, splittedString)) return null;
            if (!DetermineMethodInfo(levelsClass.methodClass)) return null;
            var obj = InvokeMethod(levelsClass.methodClass, executionClass);
            SetOutRefValues(levelsClass.methodClass, executionClass);
            if (obj == null) obj = new string(Constants.EmptyReturnValue);
            return obj;
        }

        /********************************************************************************************************************************/

        private static bool CheckIfMethod(string splittedString)
        {
            if (splittedString.Length >= 4)
            {
                var firstFourChars = splittedString.Substring(0, 4);
                if (firstFourChars == "new ") return false;
            }

            if (!splittedString.Contains(Constants.InnerBracketLevel)) return false;
            if (!splittedString.EndsWith(")")) return false;

            return true;
        }

        // No method but brackets
        private static object GetInnerBracketDirect(MethodClass methodClass, ExecutionClass executionClass,
            LinesDetailClass linesDetailClass, string splittedString)
        {
            if (splittedString.Length == 0) return null;
            if (!splittedString.Contains(Constants.InnerBracketLevel)) return null;
            if (!splittedString.Contains(")")) return null;
            var bracket = splittedString[0].ToString();
            if (bracket != "(") return null;
            
            DetermineParameters(methodClass, executionClass, linesDetailClass, splittedString);
            if (methodClass.parameters == null || methodClass.parameters.Length != 1)
            {
                DebugHandler.SendDebugError(executionClass.aIEngine, "Couldn't determine non-method bracket value.\n" +
                                                                     splittedString);
                return null;
            }

            object parameterValue = methodClass.parameters[0];
            
            // with value after brackets, for example: (Vector3.one + transform.position).normalized;
            string endString = splittedString.PGCutBefore(")", true).PGCutBefore(".", true).Trim();
            if (!string.IsNullOrWhiteSpace(endString))
            {
                var value = ReflectionGetSet.GetValueByName(parameterValue.GetType(), endString, parameterValue);
                if (value == null)
                    DebugHandler.SendDebugError(executionClass.aIEngine, "Couldn't determine non-method bracket value.\n" + splittedString);
                return value;
            }
            
            return methodClass.parameters[0];
        }


        private static void GetMethodObject(MethodClass methodClass, ExecutionClass executionClass, int lineIndex, string splittedString)
        {
            if (!splittedString.Contains(".")) return;
            var objectString = splittedString.PGCutAfterLast(".", true).Trim();

            var methodObj = LineDetermination.DetermineValue(executionClass, new LinesDetailClass.LevelsClass(), lineIndex, objectString);

            methodClass.obj = methodObj;
        }

        // For cases like this: obj.GetComponent<Renderer>().material
        private static object CheckForObjectEndValue(MethodClass methodClass, string splittedString)
        {
            var endString = splittedString.PGCutBeforeLast(".", true).Trim();
            if (methodClass.obj == null || endString.Contains("(")) return null;
            var propertyValue = ReflectionGetSet.GetValueByName(methodClass.obj.GetType(), endString, methodClass.obj);
            return propertyValue;
        }

        private static bool DetermineClassType(MethodClass methodClass, List<string> namespaces, string splittedString)
        {
            if (methodClass.obj != null)
            {
                methodClass.classType = methodClass.obj.GetType();
                return true;
            }

            var className = splittedString.PGCutAfter(".", true);
            Type staticClassType = null;
            foreach (var _namespace in namespaces)
            {
                var staticClassName = _namespace + "." + className + "," + _namespace;
                staticClassType = Type.GetType(staticClassName);
                if (staticClassType != null) break;
            }

            methodClass.classType = staticClassType;
            return methodClass.classType != null;
        }

        private static bool DetermineMethod(MethodClass methodClass, string splittedString)
        {
            if (!splittedString.Contains(".")) return false;
            if (!splittedString.Contains("(")) return false;
            var methodName = splittedString.PGCutBeforeLast(".", true).PGCutAfter("(", true);
            methodName = methodName.PGCutAfter("<", true);
            methodClass.methodName = methodName;
            return true;
        }

        private static bool DetermineGenericTypeArguments(MethodClass methodClass, ExecutionClass executionClass, string splittedString)
        {
            if (!splittedString.Contains("<")) return true;
            if (!splittedString.Contains(">")) return true;

            var innerBrackets = splittedString.PGCutBefore("<", true).PGCutAfter(">", true);
            var parameterStrings = innerBrackets.Split(',');
            for (var i = 0; i < parameterStrings.Length; i++) parameterStrings[i] = parameterStrings[i].Trim();
            var genericTypeArguments = new List<Type>();
            if (!string.IsNullOrWhiteSpace(innerBrackets))
                for (var i = 0; i < parameterStrings.Length; i++)
                {
                    var type = PGReflectionUtility.GetTypeByName(parameterStrings[i]);

                    if (type == null)
                    {
                        DebugHandler.SendDebugError(executionClass.aIEngine, "Couldn't create generic type argument for string: "
                                                                             + parameterStrings[i] + "\n" + "LINE: " + splittedString);
                        return false;
                    }

                    genericTypeArguments.Add(type);
                }

            methodClass.genericTypeArguments = genericTypeArguments.ToArray();

            return true;
        }


        // Get the parameters and parameter types from the inner level
        private static bool DetermineParameters(MethodClass methodClass, ExecutionClass executionClass,
            LinesDetailClass linesDetailClass, string splittedString)
        {
            methodClass.parameterTypes = new List<Type>();
            var innerBracketLevel = ReflectionUtility.GetIntegerAfterString(splittedString, Constants.InnerBracketLevel);
            for (var i = 0; i < linesDetailClass.levelsClasses.Count; i++)
                if (linesDetailClass.levelsClasses[i].level == innerBracketLevel)
                {
                    methodClass.parameters = linesDetailClass.levelsClasses[i].parameterValues.ToArray();
                    methodClass.outRefs = new string[methodClass.parameters.Length];

                    for (var j = 0; j < methodClass.parameters.Length; j++)
                    {
                        if (methodClass.parameters[j] is string && ((string) methodClass.parameters[j]).Contains("out "))
                        {
                            var refParameter = executionClass.GetValueAsOutParameter((string) methodClass.parameters[j]);
                            if (refParameter != null)
                            {
                                methodClass.outRefs[j] = (string) methodClass.parameters[j];
                                methodClass.parameters[j] = refParameter;
                                var refType = refParameter.GetType().MakeByRefType();
                                methodClass.parameterTypes.Add(refType);
                                continue;
                            }
                        }
                        
                        methodClass.parameterTypes.Add(methodClass.parameters[j].GetType());
                    }

                    return true;
                }

            return false;
        }

        private static bool DetermineMethodInfo(MethodClass methodClass)
        {
            if (methodClass.obj != null)
                methodClass.methodInfo = methodClass.classType.GetMethod(methodClass.methodName,
                    BindingFlags.Public | BindingFlags.Instance | BindingFlags.FlattenHierarchy,
                    null, methodClass.parameterTypes.ToArray(), null);
            else
                methodClass.methodInfo = methodClass.classType.GetMethod(methodClass.methodName,
                    BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy,
                    null, methodClass.parameterTypes.ToArray(), null);
            
            // Maybe integer was used as enum value - does not work right away
            if (methodClass.methodInfo == null)
            {
                var methodInfos = methodClass.classType.GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)
                    .Where(mi => mi.Name == methodClass.methodName).ToArray();
                for (int i = 0; i < methodInfos.Length; i++)
                {
                    var methodInfoParameters = methodInfos[i].GetParameters();
                    if(methodInfoParameters.Length != methodClass.parameterTypes.Count) continue;
                    bool parameterMatch = true;
                    for (int j = 0; j < methodClass.parameterTypes.Count; j++)
                    {
                        if(methodClass.parameterTypes[j] == typeof(int)) continue;
                        if (methodInfoParameters[j].ParameterType != methodClass.parameterTypes[j])
                        {
                            parameterMatch = false;
                            break;
                        }
                    }

                    if (parameterMatch)
                    {
                        methodClass.methodInfo = methodInfos[i];
                        break;    
                    }
                }
            }
            
            return methodClass.methodInfo != null;
        }

        private static object InvokeMethod(MethodClass methodClass, ExecutionClass executionClass)
        {
            object newObject = null;
            if (methodClass.genericTypeArguments == null)
                newObject = methodClass.methodInfo.Invoke(methodClass.obj, methodClass.parameters);
            else
                newObject = methodClass.methodInfo.MakeGenericMethod(methodClass.genericTypeArguments)
                    .Invoke(methodClass.obj, methodClass.parameters);

            return newObject;
        }

        private static void SetOutRefValues(MethodClass methodClass, ExecutionClass executionClass)
        {
            for (var i = 0; i < methodClass.outRefs.Length; i++)
            {
                if (methodClass.outRefs[i] == null) continue;
                var outRefName = methodClass.outRefs[i].PGCutBefore("out ", true).Trim();
                for (var j = 0; j < executionClass.linesDetailClasses.Length; j++)
                {
                    if (outRefName != executionClass.linesDetailClasses[j].leftSideName) continue;
                    executionClass.linesDetailClasses[j].SetLeftSideValue(executionClass, methodClass.parameters[i]);
                }
            }
        }
    }
}