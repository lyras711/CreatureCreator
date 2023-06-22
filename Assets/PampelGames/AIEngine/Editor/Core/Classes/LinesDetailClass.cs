// ----------------------------------------------------
// AI Engine
// Copyright (c) Pampel Games e.K. All Rights Reserved.
// https://www.pampelgames.com
// ----------------------------------------------------

using System.Collections.Generic;
using PampelGames.Shared.Utility;
using UnityEngine;

namespace PampelGames.AIEngine.Editor
{
    public class LinesDetailClass
    {
        public string leftSideName;
        public object leftSideValue;

        // Levels = inner brackets, level 0 = most outer bracket
        // Levels on the left side start with 100 = most outer bracket
        public readonly List<LevelsClass> levelsClasses = new();

        public class LevelsClass
        {
            public int level;
            public string levelString;
            public List<object> parameterValues;
            public string[] splittedStrings;
            public object[] splittedValues;

            public MethodClass methodClass;
            public ConstructorClass constructorClass;
        }
    }

    public static class LinesDetailClassExtensions
    {
        public static void CreateRightSideLevels(this LinesDetailClass linesDetailClass, ExecutionClass executionClass, string inputString)
        {
            if (inputString.IsForLoop()) return;
            if (inputString.IsForeachLoop()) return;
            if (inputString.Contains("{")) return;
            if (inputString.Contains("}")) return;
            if (inputString.Contains("void ") && inputString.Contains("(")) return;
            if (inputString.Contains(" : ") && !inputString.Contains("?")) return;
            if (inputString.Contains("using ")) return;
            
            /********************************************************************************************************************************/
            
            var inputStringRight = inputString.PGCutBefore(Constants.EqualSign, true);
            inputStringRight = inputStringRight.TrimStart();

            var innerBracketsRight = CreateInnerBrackets(executionClass.aIEngine, inputStringRight);

            foreach (var (s, l) in innerBracketsRight)
            {
                var _rightSideLevelsClass = new LinesDetailClass.LevelsClass();
                _rightSideLevelsClass.level = l;
                _rightSideLevelsClass.levelString = s;
                linesDetailClass.levelsClasses.Add(_rightSideLevelsClass);
            }
            
            var rightSideLevelsClass = new LinesDetailClass.LevelsClass();
            rightSideLevelsClass.level = 0;
            rightSideLevelsClass.levelString = inputStringRight;
            linesDetailClass.levelsClasses.Add(rightSideLevelsClass);

            
            // In case methods are on the left side. For example: cube.AddComponent<Rigidbody>().mass = 3; 
            // Levels on the left start with 100
            if (inputString.Contains(Constants.EqualSign))
            {
                var inputStringLeft = inputString.PGCutAfter(Constants.EqualSign, true);
                inputStringLeft = inputStringLeft.TrimStart();
                if (inputStringLeft.Contains("("))
                {
                    var innerBracketsLeft = CreateInnerBrackets(executionClass.aIEngine, inputStringLeft);
                
                    foreach (var (s, l) in innerBracketsLeft)
                    {
                        var _leftSideLevelsClass = new LinesDetailClass.LevelsClass();
                        _leftSideLevelsClass.level = 100+l;
                        _leftSideLevelsClass.levelString = s;
                        linesDetailClass.levelsClasses.Add(_leftSideLevelsClass);
                    }
                
                    var leftSideLevelsClass = new LinesDetailClass.LevelsClass();
                    leftSideLevelsClass.level = 100;
                    leftSideLevelsClass.levelString = inputStringLeft;
                    linesDetailClass.levelsClasses.Add(leftSideLevelsClass);
                }
            }
            
            /********************************************************************************************************************************/
            // Multiple inner levels may not be the same (add another number to the int)
            
            for (int i = 0; i < linesDetailClass.levelsClasses.Count; i++)
            {
                if(linesDetailClass.levelsClasses[i].level == 0) continue;
                if(linesDetailClass.levelsClasses[i].level == 100) continue;

                int counter = 1;
                for (int j = i; j < linesDetailClass.levelsClasses.Count -1; j++)
                {
                    if (linesDetailClass.levelsClasses[j].level == linesDetailClass.levelsClasses[j + 1].level)
                    {
                        linesDetailClass.levelsClasses[j + 1].level = linesDetailClass.levelsClasses[j + 1].level * 10 + counter;
                        counter++;
                    }
                }
            }
        }

        public static void CreateSplittedStrings(this LinesDetailClass linesDetailClass)
        {
            // Inner Brackets
            for (var i = 0; i < linesDetailClass.levelsClasses.Count; i++)
            {
                var levelString = linesDetailClass.levelsClasses[i].levelString;
                
                for (var j = 0; j < i; j++)
                {
                    var replaceString = linesDetailClass.levelsClasses[j].levelString;
                    if (levelString == replaceString) continue;
                    replaceString = "(" + replaceString + ")";

                    var newValue = "(" + Constants.InnerBracketLevel + linesDetailClass.levelsClasses[j].level + ")";
                    if (levelString.Contains(replaceString))
                    {
                        levelString = levelString.Replace(replaceString, newValue);
                        linesDetailClass.levelsClasses[i].levelString = levelString;
                    }
                }
            }

            // Split by operators
            for (var i = 0; i < linesDetailClass.levelsClasses.Count; i++)
            {
                var levelString = linesDetailClass.levelsClasses[i].levelString;
                linesDetailClass.levelsClasses[i].splittedStrings = ReflectionMath.SplitByOperators(levelString);
            }
            
        }

        public static void SetLeftSideValue(this LinesDetailClass linesDetailClass, ExecutionClass executionClass, object value)
        {
            linesDetailClass.leftSideValue = value;
            if (string.IsNullOrWhiteSpace(linesDetailClass.leftSideName)) return;
            for (int i = 0; i < executionClass.linesDetailClasses.Length; i++)
            {
                if(executionClass.linesDetailClasses[i] == linesDetailClass) continue;
                if(executionClass.linesDetailClasses[i].leftSideName != linesDetailClass.leftSideName) continue;
                executionClass.linesDetailClasses[i].leftSideValue = linesDetailClass.leftSideValue;
            }
        }
        
        /********************************************************************************************************************************/

        private static List<(string, int)> CreateInnerBrackets(AIEngine aiEngine, string inputString)
        {
            var innerBrackets = new List<(string, int)>();

            var stack = new Stack<int>();
            var level = 0;

            for (var i = 0; i < inputString.Length; i++)
                if (inputString[i] == '(')
                {
                    stack.Push(i);
                    level++;
                }
                else if (inputString[i] == ')')
                {
                    if (stack.Count == 0)
                    {
                        DebugHandler.SendDebugError(aiEngine, "Unequal parenthesis!\n" + inputString);
                        return null;
                    }

                    var startIndex = stack.Pop();
                    var length = i - startIndex - 1;
                    var innerBracket = inputString.Substring(startIndex + 1, length);
                    innerBrackets.Add((innerBracket, level));
                    level--;
                }

            return innerBrackets;
        }
    }
}