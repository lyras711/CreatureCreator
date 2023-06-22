// ----------------------------------------------------
// AI Engine
// Copyright (c) Pampel Games e.K. All Rights Reserved.
// https://www.pampelgames.com
// ----------------------------------------------------

using UnityEngine;

namespace PampelGames.AIEngine.Editor
{
    internal static class CheckLine
    {
        
        /********************************************************************************************************************************/
        
        internal static bool IsForLoop(this string inputString)
        {
            string lastChar = inputString[^1..];
            if (!inputString.Contains("for (") && !inputString.Contains("for(")) return false;
            if (lastChar == ")" && inputString.Contains(";")) return true;
            return false;
        }

        internal static bool IsForeachLoop(this string inputString)
        {
            string lastChar = inputString[^1..];
            if (inputString.Contains(" in ") && inputString.Contains("foreach") && lastChar == ")") return true;
            return false;
        }
        
        /********************************************************************************************************************************/
        
        internal static bool IsConditionalStatement(this string lineString)
        {
            if (IsIfStatement(lineString)) return true;
            if (IsElseIfStatement(lineString)) return true;
            if (IsElseStatement(lineString)) return true;
            return false;
        }
        internal static bool IsIfStatement(this string lineString)
        {
            lineString = lineString.TrimStart();
            if (lineString.StartsWith("if (")) return true;
            if (lineString.StartsWith("if(")) return true;
            return false;
        }
        internal static bool IsElseIfStatement(this string lineString)
        {
            lineString = lineString.TrimStart();
            if (lineString.StartsWith("else if (")) return true;
            if (lineString.StartsWith("else if(")) return true;
            return false;
        }
        internal static bool IsElseStatement(this string lineString)
        {
            lineString = lineString.TrimStart();
            if (IsElseIfStatement(lineString)) return false;
            return lineString.StartsWith("else ");
        }
        
        /********************************************************************************************************************************/

        internal static bool IsCodeMethod(this string inputString, string[] linesString, int lineIndex)
        {
            if (lineIndex + 1 >= linesString.Length) return false;
            if (!linesString[lineIndex + 1].Contains("{")) return false;
            if (inputString.IsForeachLoop() || inputString.IsForLoop()) return false;
            if (inputString.IsConditionalStatement()) return false;
            if (inputString.Contains(Constants.EqualSign)) return false;
            if (inputString.Contains(";")) return false;
            if (!inputString.Contains("(")) return false;
            string lastChar = inputString[^1..];
            if (lastChar != ")") return false;
            if (!inputString.Contains(" ")) return false;
            return true;
        }
        
        
        
    }
}
