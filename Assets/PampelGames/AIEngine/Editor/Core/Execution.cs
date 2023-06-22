// ----------------------------------------------------
// AI Engine
// Copyright (c) Pampel Games e.K. All Rights Reserved.
// https://www.pampelgames.com
// ----------------------------------------------------

using System;
using System.Collections.Generic;
using UnityEngine;

namespace PampelGames.AIEngine.Editor
{
    internal static class Execution
    {
        internal static List<GameObject> ExecuteTasks(ExecutionClass executionClass)
        {
            var objectList = new List<GameObject>();

            for (var lineIndex = 0; lineIndex < executionClass.linesDetailClasses.Length; lineIndex++)
                LineInitialization.InitializeLine(executionClass, lineIndex);

            for (var lineIndex = 0; lineIndex < executionClass.linesDetailClasses.Length; lineIndex++)
                if(executionClass.codeMethodClasses[lineIndex] == null)
                    ExecuteLinesDetailClass(executionClass, lineIndex, ref objectList);
            
            return objectList;
        }

        /********************************************************************************************************************************/
        
        internal static void ExecuteLinesDetailClass(ExecutionClass executionClass, int lineIndex, ref List<GameObject> objectList)
        {
            // If its a loop, does all loops and inner loops in one go and then skips those for subsequent lines. 
            
            var loopClass = executionClass.loopClasses[lineIndex];
            
            if (!loopClass.partOfLoop)
            {
                objectList.AddRange(DetermineLine(executionClass, lineIndex));
                return;
            }

            if (!executionClass.linesString[lineIndex].IsForeachLoop() &&
                !executionClass.linesString[lineIndex].IsForLoop()) return;

            if (loopClass.innerLoopClass != null) return;

            if (!Loops.DetermineLoopIterations(executionClass, lineIndex)) return;
            
            for (int i = 0; i < loopClass.loopIterations; i++)
            {
                for (int j = lineIndex; j < executionClass.linesString.Length; j++)
                {
                    if (!executionClass.loopClasses[j].partOfLoop) break;
                    executionClass.loopClasses[j].loopIndex = i;
                    var innerLoopClass = executionClass.loopClasses[j].innerLoopClass;
                    if (innerLoopClass == null)
                    {
                        LineSet.SetLeftSideBasicValue(executionClass, executionClass.loopClasses[j].indexName, 
                            executionClass.loopClasses[j].currentIndexValue);
                        objectList.AddRange(DetermineLine(executionClass, j));    
                    }
                    else
                    {
                        int currentIndex = j;
                        
                        for (int k = 0; k < innerLoopClass.loopIterations; k++)
                        {
                            for (int l = currentIndex; l < executionClass.linesString.Length; l++)
                            {
                                if (executionClass.loopClasses[l].innerLoopClass == null) break;
                                if (k == 0)
                                {
                                    executionClass.loopClasses[l].innerLoopClass.currentIndexValue =
                                        executionClass.loopClasses[l].innerLoopClass.startIndexValue;
                                }
                                executionClass.loopClasses[l].innerLoopClass.loopIndex = k;
                                LineSet.SetLeftSideBasicValue(executionClass, executionClass.loopClasses[l].innerLoopClass.indexName, 
                                    executionClass.loopClasses[l].innerLoopClass.currentIndexValue);
                                objectList.AddRange(DetermineLine(executionClass, l));
                                if (executionClass.loopClasses[l].innerLoopClass.currentIndexValue is float)
                                {
                                    executionClass.loopClasses[l].innerLoopClass.currentIndexValue =
                                        (float)Convert.ToDouble(executionClass.loopClasses[l].innerLoopClass.currentIndexValue) +
                                        (float)Convert.ToDouble(executionClass.loopClasses[l].innerLoopClass.increaseIndexValue);
                                }
                                else if (executionClass.loopClasses[l].innerLoopClass.currentIndexValue is int)
                                {
                                    executionClass.loopClasses[l].innerLoopClass.currentIndexValue =
                                        (int) executionClass.loopClasses[l].innerLoopClass.currentIndexValue +
                                        (int) executionClass.loopClasses[l].innerLoopClass.increaseIndexValue;
                                }
                               
                            }
                            j++;
                        }
                    }
                }

                for (int j = lineIndex; j < executionClass.linesString.Length; j++)
                {
                    if (!executionClass.loopClasses[j].partOfLoop) break;
                    if (executionClass.loopClasses[j].currentIndexValue is float)
                    {
                        executionClass.loopClasses[j].currentIndexValue = (float)Convert.ToDouble(executionClass.loopClasses[j].currentIndexValue) +
                                                                          (float)Convert.ToDouble(executionClass.loopClasses[j].increaseIndexValue);
                    }
                    else if (executionClass.loopClasses[j].currentIndexValue is int)
                    {
                        executionClass.loopClasses[j].currentIndexValue = (int) executionClass.loopClasses[j].currentIndexValue +
                                                                          (int) executionClass.loopClasses[j].increaseIndexValue;
                    }
                }
            }
        }
        
        private static List<GameObject> DetermineLine(ExecutionClass executionClass, int lineIndex)
        {
            if (Conditionals.CreateConditionals(executionClass, lineIndex)) 
                return new List<GameObject>();

            if (executionClass.linesString[lineIndex].IsConditionalStatement())
                return new List<GameObject>();

            if (executionClass.conditionalClasses[lineIndex].conditional && !executionClass.conditionalClasses[lineIndex].conditionTrue)
                return new List<GameObject>();

            return LineDetermination.DetermineLine(executionClass, lineIndex);
        }
    }
}