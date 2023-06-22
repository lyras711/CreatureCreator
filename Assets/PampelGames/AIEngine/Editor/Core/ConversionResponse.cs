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
    internal static class ConversionResponse
    {
        internal static bool CreateLinesString(AIEngine aiEngine, string responseString, List<GameObject> prefabs, List<GameObject> references, out string[] linesString, 
            out List<string> namespaces, out string parentClass)
        {
            parentClass = "";
            namespaces = null;
            var code = GetInnerCode(aiEngine, responseString);
            linesString = GetCleanLines(code);
            if (!VerifyCode(aiEngine, linesString)) return false;
            linesString = CombineLines(linesString);
            linesString = RewriteInlinedParenthesis(linesString);
            linesString = RemoveSerializeFields(linesString);
            linesString = RemoveCasts(linesString);
            linesString = RemoveAccessModifiers(linesString);
            linesString = RewriteCodeMethodParameter(linesString);
            linesString = ExtractCodeMethodParameter(linesString);
            linesString = RewriteForLoopParameter(linesString);
            linesString = AddPrefabs(linesString, prefabs);
            linesString = AddReferences(linesString, references);
            parentClass = ExtractParentClass(linesString);
            linesString = RewriteIncrementOperators(linesString);
            linesString = RewriteStaticParentClassMethods(linesString, parentClass);
            linesString = RewriteDestroyMethods(linesString);
            linesString = RemoveSemicolons(linesString);
            linesString = RewriteCompoundMathOperators(linesString);
            namespaces = ExtractNamespaces(linesString);
            if(aiEngine.aiEngineSettingsSo.logConvertedLines) LogLines(linesString);
            return true;
        }

        /********************************************************************************************************************************/
        
        private static string GetInnerCode(AIEngine aiEngine, string script)
        {
            var startIndex = script.IndexOf("```", StringComparison.Ordinal);
            var endIndex = script.LastIndexOf("```", StringComparison.Ordinal);
            if (startIndex >= 0 && endIndex >= 0)
            {
                // Extract the script between the markers
                var code = script.Substring(startIndex + 3, endIndex - startIndex - 3);
                code = code.Trim();
                
                var beforeCode = script.Substring(0, startIndex);
                var afterCode = script.Substring(endIndex + 3);

                if (!string.IsNullOrWhiteSpace(beforeCode))
                {
                    DebugHandler.SendDebugError(aiEngine, "AI created unnecessary text before script: \n" +
                                                          beforeCode);
                }
                if (!string.IsNullOrWhiteSpace(afterCode))
                {
                    DebugHandler.SendDebugError(aiEngine, "AI created unnecessary text after script: \n" +
                                                          afterCode);
                }
                
                return code;
            }

            return script;
        }
        
        
        /**********************************************************************************************************************/
        

        // Split code into an array of lines.
        private static string[] GetCleanLines(string code)
        {
            var lines = code.Split(new[] {'\r', '\n'}, StringSplitOptions.RemoveEmptyEntries);
            for (var i = lines.Length - 1; i >= 0; i--) lines[i] = lines[i].Trim();
            lines = lines.Where(line => !string.IsNullOrWhiteSpace(line)).ToArray();

            return lines;
        }

        private static bool VerifyCode(AIEngine aiEngine, string[] lines)
        {
            for (int i = 0; i < lines.Length; i++)
            {
                if (lines[i].Contains("while ") && !lines[i].Contains(";"))
                {
                    DebugHandler.SendDebugError(aiEngine, "While loops not supported.\n" +
                                                          "Line: "+ lines[i]);
                    return false;
                }

                if (lines[i].Contains("IEnumerator ") || lines[i].Contains("StartCoroutine("))
                {
                    DebugHandler.SendDebugError(aiEngine, "Coroutines not supported.\n" +
                                                          "Line: "+ lines[i]);
                    return false;
                }
            }

            return true;
        }
        

        // A single code line may be splitted into multiple lines.
        private static string[] CombineLines(string[] lines)
        {
            for (int i = 0; i < lines.Length; i++)
            {
                if(lines[i].Contains("{")|| lines[i].Contains("}")) continue;
                if(lines[i].IsForLoop() || lines[i].IsForeachLoop()) continue;
                if(lines[i].IsConditionalStatement()) continue;
                if(lines[i].Contains(" : ")) continue;
                if(lines[i].Contains(" class ")) continue;
                if(lines[i].IsCodeMethod(lines, i)) continue;
                if (lines[i].EndsWith(";")) continue;

                string combinedLine = "";
                for (int j = i; j < lines.Length; j++)
                {
                    combinedLine +=  lines[j];
                    if (lines[j].EndsWith(";"))
                    {
                        lines[j] = "";
                        break;
                    }

                    lines[j] = "";
                }

                lines[i] = combinedLine;
                
                for (int j = lines.Length -1; j >=0; j--)  
                    if(lines[j] == "") PGArrayUtility.RemoveAt(ref lines, j);
            }
            return lines;
        }

        // Give each "{" and "}" characters a seperate line. 
        private static string[] RewriteInlinedParenthesis(string[] lines)
        {
            for (int i = 0; i < lines.Length; i++)
            {
                if(lines[i].Length <= 1) continue;
                if(!lines[i].EndsWith("{") && !lines[i].EndsWith("}")) continue;
                string lastChar = lines[i].Substring(lines[i].Length - 1);
                lines[i] = lines[i].Remove(lines[i].Length - 1).Trim();
                PGArrayUtility.Insert(ref lines, i + 1, lastChar);
            }

            return lines;
        }

        
        /********************************************************************************************************************************/

        private static string[] RemoveSerializeFields(string[] lines)
        {
            for (int i = 0; i < lines.Length; i++)
            {
                if (lines[i].StartsWith("[SerializeField] ")) lines[i] = lines[i].Replace("[SerializeField] ", "");
            }
            return lines;
        }
        
        
        // For example (PrimitiveType)Random.Range(0, 4)
        private static string[] RemoveCasts(string[] lines)
        {
            for (int i = 0; i < lines.Length; i++)
            {
                if(!lines[i].Contains("(") || !lines[i].Contains(")")) continue;
                if(!lines[i].StartsWith("(")) if(!lines[i].Contains(" (")) continue;

                var openBrackets = PGStringUtility.GetIndexesOfString(lines[i], "(");
                var closeBrackets = PGStringUtility.GetIndexesOfString(lines[i], ")");
                for (int j = 0; j < openBrackets.Length; j++)
                {
                    if (j >= closeBrackets.Length) break;
                    if (closeBrackets[j] + 1 >= lines[i].Length) break;
                    if(lines[i][closeBrackets[j] + 1] == ';') break;
                    if(lines[i][closeBrackets[j] + 1] == ' ') break;
                    string innerBracket = lines[i].Substring(openBrackets[j], closeBrackets[j] + 1 - openBrackets[j]).Trim();
                    char[] operators = { '+', '-', '*', '/', '%' , ',', ' ' };
                    int index = innerBracket.IndexOfAny(operators);
                    if(index >= 0) continue;
                    string prefix = lines[i].Substring(0, openBrackets[j]);
                    string suffix = lines[i].Substring(closeBrackets[j] + 1); 
                    string replacement = new string('§', closeBrackets[j] - openBrackets[j] + 1); 
                    lines[i] = prefix + replacement + suffix;
                }
                lines[i] = lines[i].Replace("§", "").Trim();
            }
            
            return lines;
        }

        private static string[] RemoveAccessModifiers(string[] lines)
        {
            for (int i = 0; i < lines.Length; i++)
            {
                if (lines[i].StartsWith("public ")) lines[i] = lines[i].Replace("public ", "");
                else if (lines[i].StartsWith("private ")) lines[i] = lines[i].Replace("private ", "");
                else if (lines[i].StartsWith("protected ")) lines[i] = lines[i].Replace("protected ", "");
                else if (lines[i].StartsWith("internal ")) lines[i] = lines[i].Replace("internal ", "");
            }
            for (int i = 0; i < lines.Length; i++)
            {
                if (lines[i].StartsWith("const ")) lines[i] = lines[i].Replace("const ", "");
            }
            return lines;
        }

        
        /********************************************************************************************************************************/


        private static string[] RewriteCodeMethodParameter(string[] lines)
        {
            for (int i = 0; i < lines.Length; i++)
            {
                if (!lines[i].IsCodeMethod(lines, i)) continue;
                string innerBracket = lines[i].PGCutBefore("(", true).PGCutAfter(")", true).Trim();
                string leftSide = lines[i].PGCutAfter("(", false).Trim();
                if (string.IsNullOrWhiteSpace(innerBracket)) continue;
                string[] splitByComma = innerBracket.Split(",");
                string[] newSplitByComma = innerBracket.Split(",");
                for (int j = 0; j < splitByComma.Length; j++) splitByComma[j] = splitByComma[j].Trim();
                for (int j = 0; j < newSplitByComma.Length; j++) newSplitByComma[j] = newSplitByComma[j].Trim();
                for (int j = 0; j < splitByComma.Length; j++)
                {
                    var leftSplit = splitByComma[j].PGCutAfter(" ", false);
                    var rightSplit = splitByComma[j].PGCutBefore(" ", true).Trim();
                    if(leftSplit.Contains("List<") && rightSplit == Constants.Prefabs) continue;
                    if(leftSplit.Contains("List<") && rightSplit == Constants.References) continue;
                    if(leftSplit.Contains("GameObject ") && rightSplit == Constants.Prefab) continue;
                    if(leftSplit.Contains("GameObject ") && rightSplit == Constants.Reference) continue;
                    newSplitByComma[j] = leftSplit + Constants.CodeMethodParameter + i + "_" + rightSplit;
                }
                string newInnerBracket = "";
                for (int j = 0; j < newSplitByComma.Length; j++)
                {
                    newInnerBracket += newSplitByComma[j];
                    if (j == newSplitByComma.Length - 1) break;
                    newSplitByComma[j] += ", ";
                }

                lines[i] = leftSide + newInnerBracket + ")";


                int brackets = 0;
                for (int j = i+1; j < lines.Length; j++)
                {
                    if (lines[j] == "{") brackets++;
                    else if (lines[j] == "}") brackets++;
                    if (brackets <= 0) break;
                    i++;
                    for (int k = 0; k < splitByComma.Length; k++)
                    {
                        var leftSplitOld = splitByComma[k].PGCutBefore(" ", true);
                        var leftSplitNew = newSplitByComma[k].PGCutBefore(" ", true);
                        lines[j] = lines[j].Replace(leftSplitOld, leftSplitNew);
                    }
                }
            }

            return lines;
        }
        
        
        private static string[] ExtractCodeMethodParameter(string[] lines)
        {
            List<string> newLines = new List<string>();
            for (int i = 0; i < lines.Length; i++)
            {
                if(!lines[i].IsCodeMethod(lines, i)) continue;
                string innerBracket = lines[i].PGCutBefore("(", true).PGCutAfter(")", true).Trim();
                if(string.IsNullOrWhiteSpace(innerBracket)) continue;
                string[] splitByComma = innerBracket.Split(",");
                for (int j = 0; j < splitByComma.Length; j++) splitByComma[j] = splitByComma[j].Trim();
                for (int j = 0; j < splitByComma.Length; j++)
                {
                    bool exists = false;
                    for (int k = 0; k < i; k++)
                    {
                        if(lines[k].Contains(splitByComma[j]))
                        {
                            exists = true;
                            break;
                        }
                    }
                    if(exists) continue;
                    newLines.Add(splitByComma[j]);
                }
            }

            for (int i = 0; i < newLines.Count; i++)
            {
                PGArrayUtility.Insert(ref lines, 0, newLines[i] + ";");
            }
            return lines;
        }
        
        /********************************************************************************************************************************/
        
        private static string[] RewriteForLoopParameter(string[] lines)
        {
            List<string> newLines = new List<string>();

            for (int i = 0; i < lines.Length; i++)
            {
                if (!lines[i].IsForLoop()) continue;
                string parameterName = lines[i].PGCutBefore("(", true).Trim().PGCutAfter("=", true).Trim();
                string oldNameLeft = parameterName.PGCutAfter(" ", true).Trim();
                string oldNameRight = parameterName.PGCutBefore(" ", true).Trim();
                string newNameRight = Constants.ForLoopParameter + i + "_" + oldNameRight;

                int brackets = 0;
                lines[i] = ReplaceLoopParameters(lines[i], oldNameRight, newNameRight);
                lines[i] = lines[i].Replace(" " + oldNameRight + " ", " " + newNameRight + " ");
                for (int j = i + 1; j < lines.Length; j++)
                {
                    if (lines[j] == "{") brackets++;
                    else if (lines[j] == "}") brackets++;
                    if (brackets <= 0) break;
                    lines[j] = ReplaceLoopParameters(lines[j], oldNameRight, newNameRight);
                }
                
                newLines.Add(oldNameLeft + " " + newNameRight);
            }

            for (int i = 0; i < newLines.Count; i++)
            {
                PGArrayUtility.Insert(ref lines, 0, newLines[i]);
            }
            
            return lines;
        }

        private static string ReplaceLoopParameters(string line, string stringOld, string stringNew)
        {
            string newLine = "";
            newLine = line.Replace(" " + stringOld + " ", " " + stringNew + " ");
            newLine = newLine.Replace("(" + stringOld + ")", "(" + stringNew + ")");
            newLine = newLine.Replace("(" + stringOld + " ", "(" + stringNew + " ");
            newLine = newLine.Replace(" " + stringOld + ")", " " + stringNew + ")");
            newLine = newLine.Replace(" " + stringOld + "++", " " + stringNew + "++");
            newLine = newLine.Replace(" " + stringOld + "--", " " + stringNew + "--");
            newLine = newLine.Replace(" " + stringOld + ",", " " + stringNew + ",");
            newLine = newLine.Replace("(" + stringOld + ",", "(" + stringNew + ",");
            newLine = newLine.Replace("," + stringOld + ")", "," + stringNew + ")");
            
            newLine = newLine.Replace("[" + stringOld + "]", "[" + stringNew + "]");
            newLine = newLine.Replace("[" + stringOld + " ", "[" + stringNew + " ");
            newLine = newLine.Replace(" " + stringOld + "]", " " + stringNew + "]");
            newLine = newLine.Replace("[" + stringOld + ",", "[" + stringNew + ",");
            newLine = newLine.Replace("," + stringOld + "]", "," + stringNew + "]");
            return newLine;
        }
        
        /********************************************************************************************************************************/
        private static string[] AddPrefabs(string[] lines, List<GameObject> prefabs)
        {

            if (prefabs.Count == 1)
            {
                for (int i = 0; i < lines.Length; i++)
                {
                    if (lines[i].Contains("GameObject ") && lines[i].Contains(" " + Constants.Prefab) &&
                        !lines[i].IsCodeMethod(lines, i))
                        return lines;
                }
                PGArrayUtility.Insert(ref lines, 0, "GameObject " + Constants.Prefab +";");    
            }
            else if (prefabs.Count > 1)
            {
                for (int i = 0; i < lines.Length; i++)
                {
                    if (lines[i].Contains("List<GameObject>") && lines[i].Contains(" " +Constants.Prefabs)
                        && !lines[i].IsCodeMethod(lines, i)) return lines;
                }
                PGArrayUtility.Insert(ref lines, 0, "List<GameObject> " + Constants.Prefabs +";");    
            }
            return lines;
        }
        private static string[] AddReferences(string[] lines, List<GameObject> references)
        {
            if (references.Count == 1)
            {
                for (int i = 0; i < lines.Length; i++)
                {
                    if (lines[i].Contains("GameObject ") && lines[i].Contains(" " +Constants.Reference) && !lines[i].Contains(" " +Constants.Prefab)
                        && !lines[i].IsCodeMethod(lines, i))
                        return lines;                    
                }
                PGArrayUtility.Insert(ref lines, 0, "GameObject " + Constants.Reference +";");    
            }
            else if (references.Count > 1)
            {
                for (int i = 0; i < lines.Length; i++)
                {
                    if (lines[i].Contains("List<GameObject>") && lines[i].Contains(" " +Constants.References) && !lines[i].Contains(" " +Constants.Prefab)
                        && !lines[i].IsCodeMethod(lines, i)) 
                        return lines;
                }
                PGArrayUtility.Insert(ref lines, 0, "List<GameObject> " + Constants.References +";");    
            }
            
            return lines;
        }
        
        /********************************************************************************************************************************/
        
        private static string ExtractParentClass(string[] lines)
        {
            for (int i = 0; i < lines.Length; i++)
            {
                if(!lines[i].Contains(" : ")) continue;
                var parentClass = lines[i].PGCutBefore(" : ", true).Trim();
                char[] charsToTrim = { ';' };
                parentClass = parentClass.TrimEnd(charsToTrim);
                return parentClass;
            }
            return "MonoBehaviour";
        }
        
        /********************************************************************************************************************************/
        
        private static string[] RewriteIncrementOperators(string[] lines)
        {
            for (int i = 0; i < lines.Length; i++)
            {
                if(lines[i].Length < 4) continue;
                if(lines[i].Contains(" ")) continue;
                string lastThreeChars = lines[i].Substring(lines[i].Length - 3);
                if(lastThreeChars != "++;" && lastThreeChars != "--;") continue;
                string mathOperator = lastThreeChars[0].ToString();
                string variable = lines[i].Remove(lines[i].Length - 3);
                lines[i] = variable + " = " + variable + " " + mathOperator + " " + 1;
            }
            
            return lines;
        }
        
        private static string[] RewriteStaticParentClassMethods(string[] lines, string parentClass)
        {
            // Right side of "="
            for (int i = 0; i < lines.Length; i++)
            {
                if(!lines[i].Contains(";")) continue;
                if(lines[i].IsForLoop() || lines[i].IsForeachLoop()) continue;
                if(lines[i].IsConditionalStatement()) continue;
                if(lines[i].StartsWith("return ")) continue;
                var methodName = lines[i].PGCutBefore(Constants.EqualSign, true).Trim();
                if(!methodName.Contains("(")) continue;
                if(methodName[0].ToString() == "(") continue;
                methodName = methodName.PGCutAfter("(", true).Trim();
                methodName = methodName.PGCutAfter("<", true).Trim();
                if(methodName.Contains(" ")) continue;
                if(methodName.Contains(".")) continue;
                
                if (lines[i].Contains(Constants.EqualSign))
                {
                    string[] parts = lines[i].Split(Constants.EqualSign);
                    lines[i] = parts[0] + Constants.EqualSign + parentClass + "." + parts[1].Trim();
                }
                else
                {
                    lines[i] = parentClass + "." + lines[i];
                }
            }
            
            // Left side of "="
            for (int i = 0; i < lines.Length; i++)
            {
                if(!lines[i].Contains(Constants.EqualSign)) continue;
                if(!lines[i].Contains(";")) continue;
                if(lines[i].IsForLoop() || lines[i].IsForeachLoop()) continue;
                if(lines[i].IsConditionalStatement()) continue;
                var methodName = lines[i].PGCutAfter(Constants.EqualSign, true).Trim();
                if(!methodName.Contains("(")) continue;
                methodName = methodName.PGCutAfter("(", true).Trim();
                methodName = methodName.PGCutAfter("<", true).Trim();
                if(methodName.Contains(" ")) continue;
                if(methodName.Contains(".")) continue;
                if (lines[i].Contains(Constants.EqualSign))
                {
                    string[] parts = lines[i].Split(Constants.EqualSign);
                    lines[i] = parentClass + "." + parts[0].Trim() + Constants.EqualSign + parts[1].Trim();
                }
                else
                {
                    lines[i] = parentClass + "." + lines[i];
                }
            }
            
            // foreach loops may contain methods
            for (int i = 0; i < lines.Length; i++)
            {
                if(!lines[i].IsForeachLoop()) continue;
                string methodName = lines[i].PGCutBeforeLast(" ", true).PGCutAfterLast(")", true).Trim();
                if(!methodName.Contains("(") || !methodName.Contains(")")) continue;
                lines[i] = lines[i].Replace(methodName, parentClass + "." + methodName);
            }

            return lines;
        }

        private static string[] RewriteDestroyMethods(string[] lines)
        {
            for (int i = 0; i < lines.Length; i++)
            {
                if (lines[i].Contains("Destroy("))
                    lines[i] = lines[i].Replace("Destroy(", "DestroyImmediate(");
            }

            return lines;
        }

        
        private static string[] RemoveSemicolons(string[] lines)
        {
            for (int i = 0; i < lines.Length; i++)
            {
                char[] charsToTrim = { ';' };
                lines[i] = lines[i].TrimEnd(charsToTrim);
            }
            return lines;
        }
        
        /********************************************************************************************************************************/
        private static string[] RewriteCompoundMathOperators(string[] lines)
        {
            int savetyCheck = 0;
            while(lines.Any(ContainsCompoundMathOperator))
            {
                for (int i = 0; i < lines.Length; i++)
                {
                    if(!ContainsCompoundMathOperator(lines[i])) continue;
                    lines[i] = RewriteCompoundMathOperatorForLine(lines[i]);
                }

                savetyCheck++;
                if (savetyCheck > 10)
                {
                    Debug.LogError("Error in Compounding Math Operators!");
                    break;
                }
            }

            return lines;
        }

        private static bool ContainsCompoundMathOperator(string line)
        {
            if (line.Contains("+=")) return true;
            if (line.Contains("-=")) return true;
            if (line.Contains("*=")) return true;
            if (line.Contains("/=")) return true;
            return false;
        }
        private static string RewriteCompoundMathOperatorForLine(string line)
        {
            var compoundCharacters = "";
            if (line.Contains("+=")) compoundCharacters = "+=";
            else if (line.Contains("-=")) compoundCharacters = "-=";
            else if (line.Contains("*=")) compoundCharacters = "*=";
            else if (line.Contains("/=")) compoundCharacters = "/=";
            if (compoundCharacters == "") return line;

            var leftSide = line.PGCutAfter(compoundCharacters, true).Trim();
            var rightSide = line.PGCutBefore(compoundCharacters, true).Trim();
            
            if (!line.IsForLoop())
            {
                line = leftSide + " = " + leftSide + " " + compoundCharacters[0] + " " + rightSide;
                return line;
            }
            
            var leftSideSub = leftSide.PGGetSubstringBeforeIndex(leftSide.Length - 1).Trim();
            leftSideSub = leftSideSub.PGCutBefore("(", true);
            
            var rightSideSub = rightSide.PGGetSubstringAfterIndex(0).Trim();
            rightSideSub = rightSideSub.PGCutAfter(")", true);

            var originalOperation = leftSideSub + " " + compoundCharacters + " " + rightSideSub;
            var newOperation = leftSideSub + " = " + leftSideSub + " " + compoundCharacters[0] + " " + rightSideSub;

            line = line.Replace(originalOperation, newOperation);
            
            return line;
        }

        /********************************************************************************************************************************/
        
        private static List<string> ExtractNamespaces(string[] lines)
        {
            var namespaces = new List<string>();
            foreach (var line in lines)
                if (line.StartsWith("using "))
                {
                    var parts = line.Split(' ');
                    var ns = parts[1].TrimEnd(';');
                    namespaces.Add(ns);
                }

            // include Unity.Engine in case shortened custom code is used.
            if (namespaces.Count == 0) namespaces.Add("UnityEngine");
            return namespaces;
        }
        

        /********************************************************************************************************************************/
        /********************************************************************************************************************************/
        
        private static void LogLines(string[] lines)
        {
            for (int i = 0; i < lines.Length; i++)
            {
                Debug.Log(lines[i]);
            }
        }
        
    }
}