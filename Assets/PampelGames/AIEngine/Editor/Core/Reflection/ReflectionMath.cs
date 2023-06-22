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
    
    internal static class ReflectionMath
    {

        internal static string[] SplitByOperators(string inputString)
        {
            string[] inputStringsByComma = inputString.Split(',');
            
            List<string> substrings = new List<string>();
            
            for (int i = 0; i < inputStringsByComma.Length; i++)
            {
                if (Conditionals.IsTerneryOperator(inputStringsByComma[i]))
                {
                    substrings.Add(inputStringsByComma[i].Trim());   
                }
                else if (inputStringsByComma[i].Contains("[") && inputStringsByComma[i].Contains("]"))
                {
                    inputStringsByComma[i] = inputStringsByComma[i].Trim();
                    var brackets = inputStringsByComma[i].PGCutBefore("[", false).PGCutAfter("]", false);
                    var leftSide = inputStringsByComma[i].PGCutAfter("[", true);
                    var rightSide = inputStringsByComma[i].PGCutBefore("]", true);
                    
                    var leftSideByOperators = CreateSubstringsByOperators(leftSide);
                    for (int j = 0; j < leftSideByOperators.Length; j++)
                    {
                        if(j == leftSideByOperators.Length - 1)
                            substrings.Add(leftSideByOperators[j] + brackets);
                        else 
                            substrings.Add(leftSideByOperators[j].Trim());
                    }

                    var rightSideByOperators = CreateSubstringsByOperators(rightSide);
                    for (int j = 0; j < rightSideByOperators.Length; j++)
                    {
                        if (j == 0 && !rightSideByOperators[j].StartsWith(" "))
                            substrings[^1] += rightSideByOperators[j].Trim();
                        else
                            substrings.Add(rightSideByOperators[j].Trim());
                    }
                }
                else
                {
                    var substringsByOperators = CreateSubstringsByOperators(inputStringsByComma[i]);
                    substrings.AddRange(substringsByOperators);    
                }
                if (i == inputStringsByComma.Length - 1) break;
                substrings.Add(",");
            }
            
            return substrings.ToArray();
        }

        private static string[] CreateSubstringsByOperators(string inputString)
        {
            string[] singleCharSeparators = {"*", "/", "+", "-", "%", "?", ":"};
            string[] multiCharSeparators = {"==", ">=", "<=", " > ", " < ", "&&", "||"};

            List<string> substrings = new List<string>();
            int j = 0;
            while (j < inputString.Length)
            {
                int separatorIndex = inputString.Length;
                foreach (string separator in singleCharSeparators)
                {
                    int index = inputString.IndexOf(separator, j, StringComparison.Ordinal);
                    if (index >= 0 && index < separatorIndex)
                    {
                        separatorIndex = index;
                    }
                }

                foreach (string separator in multiCharSeparators)
                {
                    int index = inputString.IndexOf(separator, j, StringComparison.Ordinal);
                    if (index >= 0 && index < separatorIndex)
                    {
                        separatorIndex = index;
                    }
                }

                string currentSubstring = inputString.Substring(j, separatorIndex - j);
                substrings.Add(currentSubstring);
    
                if (separatorIndex < inputString.Length)
                {
                    bool foundSeparator = false;
                    foreach (string separator in multiCharSeparators)
                    {
                        if (inputString.Substring(separatorIndex).StartsWith(separator))
                        {
                            substrings.Add(separator);
                            j = separatorIndex + separator.Length;
                            foundSeparator = true;
                            break;
                        }
                    }

                    if (!foundSeparator)
                    {
                        string separator = inputString.Substring(separatorIndex, 1);
                        substrings.Add(separator);
                        j = separatorIndex + separator.Length;
                    }
                }
                else
                {
                    j = inputString.Length;
                }
            }

            for (int i = 0; i < substrings.Count; i++) substrings[i] = substrings[i].Trim();
            for (int i = substrings.Count -1; i >=0; i--) if(string.IsNullOrWhiteSpace(substrings[i])) substrings.RemoveAt(i);
            
            if (substrings.Count <= 1) return substrings.ToArray();
            
            
            /********************************************************************************************************************************/
            // Check for negative values that are not seperators
            if (substrings[0] == "-")
            {
                substrings[1] = "-" + substrings[1];
                substrings.RemoveAt(0);
            }

            for (int i = 1; i < substrings.Count - 1; i++)
            {
                if (substrings[i + 1] != "-") continue;

                if (singleCharSeparators.Any(substrings[i].Contains))
                {
                    substrings[i + 2] = "-" + substrings[i + 2];
                    substrings[i + 1] = "";
                    i++;
                }
                else if (multiCharSeparators.Any(substrings[i].Contains))
                {
                    substrings[i + 2] = "-" + substrings[i + 2];
                    substrings[i + 1] = "";
                    i++;
                }
            }
            
            for (int i = substrings.Count -1; i >=0; i--) if(string.IsNullOrWhiteSpace(substrings[i])) substrings.RemoveAt(i);
            
            return substrings.ToArray();
        }
        
        
        /********************************************************************************************************************************/
        
        internal static object CalculateObjectsFromLists(List<string> stringList, List<object> valueList)
        {
            // Seperate conditionals && and ||
            List<List<string>> listOfStringLists = new List<List<string>>();
            List<List<object>> listOfValueLists = new List<List<object>>();
            List<string> conditionalSeperators = new List<string>();
            List<object> accumulatedValues = new List<object>();
            
            listOfStringLists.Add(new List<string>());
            listOfValueLists.Add(new List<object>());
            
            for (int i = 0; i < stringList.Count; i++)
            {
                if (stringList[i] == "&&" || stringList[i] == "||")
                {
                    listOfStringLists.Add(new List<string>());
                    listOfValueLists.Add(new List<object>());
                    conditionalSeperators.Add(stringList[i]);
                    continue;
                }
                
                listOfStringLists[^1].Add(stringList[i]);
                listOfValueLists[^1].Add(valueList[i]);
            }

            for (int i = 0; i < listOfStringLists.Count; i++)
            {
                accumulatedValues.Add(null);
                if(listOfValueLists[i].Count > 0) accumulatedValues[^1] = listOfValueLists[i][0];

                var intermediateResults = new List<object> { accumulatedValues[^1] };

                // Split so *,/,% calculated first
                for (var j = 1; j < listOfStringLists[i].Count; j+=2)
                {
                    var mathOperator = listOfStringLists[i][j];
                    if (mathOperator == "*" || mathOperator == "/" || mathOperator == "%")
                    {
                        var leftObj = intermediateResults.Last();
                        var rightObj = listOfValueLists[i][j + 1];
                        if (leftObj == null || rightObj == null) continue;
                        var result = CalculateObjects(leftObj, rightObj, mathOperator);
                        intermediateResults[^1] = result;
                    }
                    else
                    {
                        intermediateResults.Add(mathOperator);
                        intermediateResults.Add(listOfValueLists[i][j + 1]);
                    }
                }

                accumulatedValues[^1] = intermediateResults[0];

                for (var j = 1; j < intermediateResults.Count; j+=2)
                {
                    var mathOperator = (string)intermediateResults[j];
                    var leftObj = accumulatedValues[^1];
                    var rightObj = intermediateResults[j + 1];
                    if (leftObj == null || rightObj == null) continue;
                    accumulatedValues[^1] = CalculateObjects(leftObj, rightObj, mathOperator);
                }
            }

            object finalValue = accumulatedValues[^1];
            
            
            // && and ||
            if (conditionalSeperators.Count > 0)
            {
                for (int i = 0; i < accumulatedValues.Count; i++)
                {
                    if (i + 1 >= accumulatedValues.Count) break;
                    if (conditionalSeperators[i] == "&&")
                    {
                        if ((bool) accumulatedValues[i] == true && (bool) accumulatedValues[i + 1] == true)
                            finalValue = true;
                        else
                        {
                            finalValue = false;
                            break;
                        }
                    }
                    else if (conditionalSeperators[i] == "||")
                    {
                        if ((bool) accumulatedValues[i] == true || (bool) accumulatedValues[i + 1] == true)
                            finalValue = true;
                        else
                        {
                            finalValue = false;
                        }
                    }
                }    
            }

            return finalValue;

        }

        /********************************************************************************************************************************/

        
        internal static bool IsMathOperator(string stringValue)
        {
            if (stringValue.Contains("+")) return true;
            if (stringValue.Contains("-")) return true;
            if (stringValue.Contains("*")) return true;
            if (stringValue.Contains("/")) return true;
            if (stringValue.Contains("%")) return true;
            return false;
        }

        internal static object CalculateObjects(object obj1, object obj2, string mathOperator)
        {
            if (mathOperator.Contains("+"))
                return AddObjects(obj1, obj2);
            if (mathOperator.Contains("-"))
                return SubtractObjects(obj1, obj2);
            if (mathOperator.Contains("*"))
                return MultiplyObjects(obj1, obj2);
            if (mathOperator.Contains("/"))
                return DivideObjects(obj1, obj2);
            if (mathOperator.Contains("%"))
                return ModuloObjects(obj1, obj2);
            if (Conditionals.IsConditionalOperator(mathOperator))
                return Conditionals.CheckCondition(obj1, obj2, mathOperator);
            return null;
        }
        
        private static object AddObjects(object obj1, object obj2)
        {
            object result = null;
            if (obj1 is string)
            {
                result = (string) obj1 + obj2;
            }
            else if (obj2 is string)
            {
                result = obj1 + (string) obj2;
            }
            else if (obj1 is int && obj2 is int)
            {
                result = (int) obj1 + (int) obj2;
            }
            else if (obj1 is float && obj2 is float)
            {
                result = (float) obj1 + (float) obj2;
            }
            else if (obj1 is Vector2 && obj2 is Vector2)
            {
                result = (Vector2) obj1 + (Vector2) obj2;
            }
            else if (obj1 is Color && obj2 is Color)
            {
                result = (Color) obj1 + (Color) obj2;
            }
            else if (obj1 is Quaternion && obj2 is Quaternion)
            {
                result = (Quaternion) obj1 * (Quaternion) obj2;
            }
            else if ((obj1 is int && obj2 is float) || (obj1 is float && obj2 is int))
            {
                result = Convert.ToSingle(obj1) + Convert.ToSingle(obj2);
            }
            else if (obj1 is Vector3 && obj2 is Vector3)
            {
                result = (Vector3) obj1 + (Vector3) obj2;
            }

            return result;
        }

        private static object SubtractObjects(object obj1, object obj2)
        {
            object result = null;
            if (obj1 is int && obj2 is int)
            {
                result = (int) obj1 - (int) obj2;
            }
            else if (obj1 is float && obj2 is float)
            {
                result = (float) obj1 - (float) obj2;
            }
            else if (obj1 is Vector2 && obj2 is Vector2)
            {
                result = (Vector2) obj1 - (Vector2) obj2;
            }
            else if (obj1 is Color && obj2 is Color)
            {
                result = (Color) obj1 - (Color) obj2;
            }
            else if (obj1 is Quaternion && obj2 is Quaternion)
            {
                result = (Quaternion) obj1 * Quaternion.Inverse((Quaternion) obj2);
            }
            else if ((obj1 is int && obj2 is float) || (obj1 is float && obj2 is int))
            {
                result = Convert.ToSingle(obj1) - Convert.ToSingle(obj2);
            }
            else if (obj1 is Vector3 && obj2 is Vector3)
            {
                result = (Vector3) obj1 - (Vector3) obj2;
            }

            return result;
        }

        private static object MultiplyObjects(object obj1, object obj2)
        {
            object result = null;
            if (obj1 is int && obj2 is int)
            {
                result = (int) obj1 * (int) obj2;
            }
            else if (obj1 is float && obj2 is float)
            {
                result = (float) obj1 * (float) obj2;
            }
            else if (obj1 is Vector2 && obj2 is Vector2)
            {
                result = (Vector2) obj1 * (Vector2) obj2;
            }
            else if (obj1 is Color && obj2 is float)
            {
                result = (Color) obj1 * (float) obj2;
            }
            else if (obj1 is Quaternion && obj2 is Quaternion)
            {
                result = (Quaternion) obj1 * (Quaternion) obj2;
            }
            else if ((obj1 is int && obj2 is float) || (obj1 is float && obj2 is int))
            {
                result = Convert.ToSingle(obj1) * Convert.ToSingle(obj2);
            }
            else if (obj1 is Vector2 && (obj2 is float || obj2 is int))
            {
                result = (Vector2) obj1 * Convert.ToSingle(obj2);
            }
            else if (obj2 is Vector2 && (obj1 is float || obj1 is int))
            {
                result = Convert.ToSingle(obj1) * (Vector2) obj2;
            }
            else if (obj1 is Vector3 && (obj2 is float || obj2 is int))
            {
                result = (Vector3) obj1 * Convert.ToSingle(obj2);
            }
            else if (obj2 is Vector3 && (obj1 is float || obj1 is int))
            {
                result = Convert.ToSingle(obj1) * (Vector3) obj2;
            }
            else if (obj1 is Quaternion && obj2 is Vector3)
            {
                var rotation = (Quaternion) obj1;
                var vector = (Vector3) obj2;
                result = rotation * vector;
            }

            return result;
        }
        
        private static object DivideObjects(object obj1, object obj2)
        {
            object result = null;
            if (obj1 is int && obj2 is int)
            {
                result = (int) obj1 / (int) obj2;
            }
            else if (obj1 is float && obj2 is float)
            {
                result = (float) obj1 / (float) obj2;
            }
            else if (obj1 is Vector2 && obj2 is Vector2)
            {
                result = new Vector2(((Vector2) obj1).x / ((Vector2) obj2).x, ((Vector2) obj1).y / ((Vector2) obj2).y);
            }
            else if (obj1 is Color && obj2 is float)
            {
                result = (Color) obj1 / (float) obj2;
            }
            else if ((obj1 is int && obj2 is float) || (obj1 is float && obj2 is int))
            {
                result = Convert.ToSingle(obj1) / Convert.ToSingle(obj2);
            }
            else if (obj1 is Vector3 && (obj2 is float || obj2 is int))
            {
                result = (Vector3) obj1 / Convert.ToSingle(obj2);
            }
            else if (obj1 is Quaternion && obj2 is Vector3)
            {
                var rotation = (Quaternion) obj1;
                var vector = (Vector3) obj2;
                result = Quaternion.Inverse(rotation) * vector;
            }

            return result;
        }

        private static object ModuloObjects(object obj1, object obj2)
        {
            object result = null;
            if (obj1 is int && obj2 is int)
            {
                result = (int)obj1 % (int)obj2;
            }
            else if (obj1 is float && obj2 is float)
            {
                result = (float)obj1 % (float)obj2;
            }
            else if (obj1 is Color && obj2 is float)
            {
                var color = (Color)obj1;
                var divisor = (float)obj2;
                result = new Color(color.r % divisor, color.g % divisor, color.b % divisor, color.a % divisor);
            }
            else if ((obj1 is int && obj2 is float) || (obj1 is float && obj2 is int))
            {
                result = Convert.ToSingle(obj1) % Convert.ToSingle(obj2);
            }

            return result;
        }
        
        /********************************************************************************************************************************/

        internal static object SetNegative(object obj)
        {
            object result = null;
            if (obj is int)
            {
                result = (int) obj * (-1);
            }
            else if (obj is float)
            {
                result = (float) obj * (-1);
            }
            else if (obj is Vector2)
            {
                result = (Vector2) obj * (-1);
            }
            else if (obj is Vector3)
            {
                result = (Vector3) obj * (-1);
            }
            return result;
        }
        
        
    }
}