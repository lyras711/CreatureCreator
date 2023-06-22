// ----------------------------------------------------
// AI Engine
// Copyright (c) Pampel Games e.K. All Rights Reserved.
// https://www.pampelgames.com
// ----------------------------------------------------

using System;
using System.Linq;
using System.Reflection;
using PampelGames.Shared.Utility;
using UnityEngine;

namespace PampelGames.AIEngine.Editor
{
    internal static class Conditionals
    {
        
        /********************************************************************************************************************************/
        /* Ternery Operator *************************************************************************************************************/
        
        internal static bool IsTerneryOperator(string inputString)
        {
            return inputString.Contains("?");
        }

        internal static object GetValueFromTerneryOperator(ExecutionClass executionClass, LinesDetailClass.LevelsClass levelsClass, 
            int lineIndex, string inputString)
        {
            string leftSideString = inputString.PGCutAfter("?", true).Trim();
            string rightSideString = inputString.PGCutBefore("?", true).Trim();

            string leftSideStringValue1 = leftSideString.PGCutAfter("<", true).PGCutAfter(">", true).PGCutAfter("=", true).Trim();
            string leftSideStringValue2 = leftSideString.PGCutBefore("<", true).PGCutBefore(">", true).
                PGCutBefore("=", true).PGCutBefore("=", true).Trim();
            string leftSideOperator = leftSideString.PGCutBefore(leftSideStringValue1, true).PGCutAfter(leftSideStringValue2, true).Trim();

            string rightSideStringValue1 = rightSideString.PGCutAfter(":", true);
            string rightSideStringValue2 = rightSideString.PGCutBefore(":", true);
            if (!rightSideString.Contains(":")) rightSideStringValue1 = rightSideStringValue2;

            object leftSideValue1 =
                LineDetermination.DetermineValueFromConnectedString(executionClass, levelsClass, lineIndex, leftSideStringValue1);
            object leftSideValue2 =
                LineDetermination.DetermineValueFromConnectedString(executionClass, levelsClass, lineIndex, leftSideStringValue2);

            if(leftSideValue1 == null || leftSideValue2 == null) return TerneryOperatorError(executionClass, lineIndex, inputString);
            
            object rightSideValue1 =
                LineDetermination.DetermineValueFromConnectedString(executionClass, levelsClass, lineIndex, rightSideStringValue1);
            if(rightSideValue1 == null) return TerneryOperatorError(executionClass, lineIndex, inputString);

            object rightSideValue2 = null;
            
            // Nested Ternery operator
            if (rightSideString.Contains("?") && rightSideString.Contains(":"))
            {
                var nestedRightSideString = rightSideString.PGCutBefore(":", true).Trim();
                rightSideValue2 = GetValueFromTerneryOperator(executionClass, levelsClass, lineIndex, nestedRightSideString);
            }
            else
            {
                rightSideValue2 = LineDetermination.DetermineValueFromConnectedString(executionClass, levelsClass, lineIndex, rightSideStringValue2);
            }

            if(rightSideValue2 == null) return TerneryOperatorError(executionClass, lineIndex, inputString);

            bool condition = CheckCondition(leftSideValue1, leftSideValue2, leftSideOperator);

            if (condition) return rightSideValue1;
            return rightSideValue2;

        }

        private static object TerneryOperatorError(ExecutionClass executionClass, int lineIndex, string inputString)
        {
            DebugHandler.SendDebugError(executionClass.aIEngine, "Couldn't determine Ternery Operator in line "+lineIndex+".\n" +
                                                                      inputString);
            return null;
        }
        
        /********************************************************************************************************************************/
        /* Statements *****************************************************************************************************************/
        
        
        internal static bool CreateConditionals(ExecutionClass executionClass, int index)
        {
            if (!executionClass.linesString[index].IsIfStatement()) return false;
            
            bool conditionTrue = DetermineConditionTrue(executionClass, index);
            int newIndex = SetConditionalInfos(executionClass, index, conditionTrue);
            newIndex++;

            if(executionClass.linesString[newIndex].IsElseIfStatement())
            {
                conditionTrue = DetermineConditionTrue(executionClass, newIndex);
                newIndex = SetConditionalInfos(executionClass, newIndex, conditionTrue);
                newIndex++;
            }
            
            if(executionClass.linesString[newIndex].IsElseStatement())
            {
                conditionTrue = DetermineConditionTrue(executionClass, newIndex);
                SetConditionalInfos(executionClass, newIndex, conditionTrue);
            }
            

            return true;
        }

        /********************************************************************************************************************************/
        
        private static bool DetermineConditionTrue(ExecutionClass executionClass, int lineIndex)
        {
            var linesDetailClass = executionClass.linesDetailClasses[lineIndex];
            
            // Skip last index - Level 0 would be with "if(...)"
            for (int i = 0; i < executionClass.linesDetailClasses[lineIndex].levelsClasses.Count -1; i++)
            {
                var rightSideLevelsClass = executionClass.linesDetailClasses[lineIndex].levelsClasses[i];

                rightSideLevelsClass.splittedValues = new object[rightSideLevelsClass.splittedStrings.Length];
                for (int j = 0; j < rightSideLevelsClass.splittedStrings.Length; j++)
                {
                    rightSideLevelsClass.splittedValues[j] = LineDetermination.DetermineValue(executionClass,
                        rightSideLevelsClass, lineIndex, rightSideLevelsClass.splittedStrings[j]);
                }

                LineDetermination.CalculateParameterValues(executionClass, rightSideLevelsClass, lineIndex);
            }

            if (linesDetailClass.levelsClasses.Count <= 1)
                return IfStatementError(executionClass, lineIndex, executionClass.linesString[lineIndex]);
                
            if(linesDetailClass.levelsClasses[^2].parameterValues.Count != 1)
                return IfStatementError(executionClass, lineIndex, executionClass.linesString[lineIndex]);

            if(linesDetailClass.levelsClasses[^2].parameterValues[0].GetType() != typeof(bool))
                return IfStatementError(executionClass, lineIndex, executionClass.linesString[lineIndex]);

            // "!" sign is removed from the string in the LineDetermination
            bool contitionTrue = (bool) linesDetailClass.levelsClasses[^2].parameterValues[0];
            if (executionClass.linesString[lineIndex].Contains("!")) contitionTrue = !contitionTrue;
            return contitionTrue ;
        }
        
        private static int SetConditionalInfos(ExecutionClass executionClass, int lineIndex, bool conditionTrue)
        {
            int openBrackets = 0;
            for (;;)
            {
                lineIndex++;
                if (lineIndex + 1 >= executionClass.conditionalClasses.Length) break;
                if (executionClass.linesString[lineIndex].Contains("{")) openBrackets++;
                else if (executionClass.linesString[lineIndex].Contains("}")) openBrackets--;
                executionClass.conditionalClasses[lineIndex].conditional = true;
                executionClass.conditionalClasses[lineIndex].conditionTrue = conditionTrue;
                if (openBrackets <= 0) return lineIndex;
            }

            return 0;
        }

        private static bool IfStatementError(ExecutionClass executionClass, int lineIndex, string inputString)
        {
            DebugHandler.SendDebugError(executionClass.aIEngine, "Couldn't determine if statement in line "+lineIndex+".\n" +
                                                                      inputString);
            return false;
        }
        
        /********************************************************************************************************************************/
        /********************************************************************************************************************************/

        
        internal static bool IsConditionalOperator(string condition)
        {
            if (condition == "==") return true;
            if (condition == ">=") return true;
            if (condition == "<=") return true;
            if (condition == ">") return true;
            if (condition == "<") return true;
            return false;
        }

        internal static bool CheckCondition(object obj1, object obj2, string condition)
        {

            if (condition == "==")
            {
                if (obj1.GetType() == obj2.GetType() && obj1.Equals(obj2)) return true;
                return false;
            }
            else if (condition == ">=")
            {
                if (obj1 is int && obj2 is int)
                {
                    return (int) obj1 >= (int) obj2;
                }
                else if (obj1 is float && obj2 is float)
                {
                    return (float) obj1 >= (float) obj2;
                }
                else if ((obj1 is int && obj2 is float) || (obj1 is float && obj2 is int))
                {
                    return Convert.ToSingle(obj1) >= Convert.ToSingle(obj2);
                }
            }
            else if (condition == "<=")
            {
                if (obj1 is int && obj2 is int)
                {
                    return (int) obj1 <= (int) obj2;
                }
                else if (obj1 is float && obj2 is float)
                {
                    return (float) obj1 <= (float) obj2;
                }
                else if ((obj1 is int && obj2 is float) || (obj1 is float && obj2 is int))
                {
                    return Convert.ToSingle(obj1) <= Convert.ToSingle(obj2);
                }
            }
            else if (condition == "<")
            {
                if (obj1 is int && obj2 is int)
                {
                    return (int) obj1 < (int) obj2;
                }
                else if (obj1 is float && obj2 is float)
                {
                    return (float) obj1 < (float) obj2;
                }
                else if ((obj1 is int && obj2 is float) || (obj1 is float && obj2 is int))
                {
                    return Convert.ToSingle(obj1) < Convert.ToSingle(obj2);
                }
            }
            else if (condition == ">")
            {
                if (obj1 is int && obj2 is int)
                {
                    return (int) obj1 > (int) obj2;
                }
                else if (obj1 is float && obj2 is float)
                {
                    return (float) obj1 > (float) obj2;
                }
                else if ((obj1 is int && obj2 is float) || (obj1 is float && obj2 is int))
                {
                    return Convert.ToSingle(obj1) > Convert.ToSingle(obj2);
                }
            }

            return false;
        }
    }
}