// ----------------------------------------------------
// AI Engine
// Copyright (c) Pampel Games e.K. All Rights Reserved.
// https://www.pampelgames.com
// ----------------------------------------------------

using System;
using System.Collections.Generic;
using System.Globalization;
using PampelGames.Shared.Utility;
using UnityEngine;

namespace PampelGames.AIEngine.Editor
{
    internal static class ReflectionUtility
    {
        /// <summary>
        ///     Creates a value type, not a reference type.
        /// </summary>
        internal static object CreateValueTypeFromString(string typeName, List<string> namespaces)
        {
            object instance = null;
            if (typeName == "int") return new int();
            if (typeName == "float") return new float();
            if (typeName == "string") return new string("");
            if (typeName == "bool") return new bool();

            foreach (var _namespace in namespaces)
            {
                var staticClassName = _namespace + "." + typeName + "," + _namespace;
                var type = Type.GetType(staticClassName);
                if (type == null) continue;
                if (!type.IsValueType) break;
                instance = Activator.CreateInstance(type);
                break;
            }

            return instance;
        }

        internal static object DetermineBasicValueFromString(string inputString)
        {
            if (inputString.Contains("(")) return null;
            if (inputString.Contains(")")) return null;
            if (inputString.Contains(",")) return null;

            var isInt = int.TryParse(inputString, out var myInt);
            if (isInt) return myInt;

            if (inputString.EndsWith("f"))
            {
                var floatString = inputString.Substring(0, inputString.Length - 1);
                var isFloat = float.TryParse(floatString, NumberStyles.Float, CultureInfo.InvariantCulture, out var myFloat);
                if (isFloat) return myFloat;
            }
            else
            {
                var isFloat = float.TryParse(inputString, NumberStyles.Float, CultureInfo.InvariantCulture, out var myFloat);
                if (isFloat) return myFloat;
            }

            if (inputString == "true") return true;
            if (inputString == "false") return false;

            bool startsAndEndsWithQuotes = inputString.Length > 1 && inputString.Substring(0, 1).Equals("\"") && 
                                           inputString.Substring(inputString.Length - 1, 1).Equals("\"");
            if (startsAndEndsWithQuotes)
            {
                var newString = new string(inputString.PGCutBefore("\"", true).PGCutAfter("\"", true));
                return newString;
            }

            return null;
        }


        internal static int GetIntegerAfterString(string input, string delimiter)
        {
            var index = input.IndexOf(delimiter, StringComparison.Ordinal);
            if (index >= 0 && index < input.Length - delimiter.Length)
            {
                var substring = input.Substring(index + delimiter.Length);

                var endIndex = 0;
                while (endIndex < substring.Length && char.IsDigit(substring[endIndex]))
                    endIndex++;
                var digitString = substring.Substring(0, endIndex);
                int result;
                if (int.TryParse(digitString, out result))
                    return result;
            }

            return -1;
        }
    }
}