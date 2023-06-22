// ----------------------------------------------------
// AI Engine
// Copyright (c) Pampel Games e.K. All Rights Reserved.
// https://www.pampelgames.com
// ----------------------------------------------------

using System;
using System.Collections.Generic;
using PampelGames.Shared.Utility;
using UnityEngine;

namespace PampelGames.AIEngine.Editor
{
    internal static class Constructors
    {
        internal static object GetValueFromConstructor(ExecutionClass executionClass, LinesDetailClass linesDetailClass,
            LinesDetailClass.LevelsClass levelsClass, string splittedString)
        {

            if (!CheckIfConstructor(splittedString)) return null;

            levelsClass.constructorClass = new ConstructorClass();

            if (!DetermineClass(levelsClass.constructorClass, executionClass.namespaces, splittedString)) return null;
            if (!DetermineParameters(levelsClass.constructorClass, linesDetailClass, splittedString)) return null;
            if (!DetermineConstructorInfo(levelsClass.constructorClass)) return null;
            
            var obj = levelsClass.constructorClass.constructorInfo.Invoke(levelsClass.constructorClass.parameters);
            return obj;
        }

        
        /********************************************************************************************************************************/
        
        
        private static bool CheckIfConstructor(string splittedString)
        {
            if (splittedString.Length >= 4)
            {
                var firstFourChars = splittedString.Substring(0, 4);
                if (firstFourChars != "new ") return false;
            }

            if (!splittedString.Contains(Constants.InnerBracketLevel)) return false;
            return true;
        }
        
        private static bool DetermineClass(ConstructorClass constructor, List<string> namespaces, string splittedString)
        {
            var className = splittedString.Substring(4);
            constructor.className = className.PGCutAfter("(", true);
            Type staticClassType = null;
            foreach (var _namespace in namespaces)
            {
                var staticClassName = _namespace + "." + constructor.className + "," + _namespace;
                staticClassType = Type.GetType(staticClassName);
                if (staticClassType != null) break;
            }

            constructor.classType = staticClassType;
            return constructor.classType != null;
        }
        
        private static bool DetermineParameters(ConstructorClass constructor, LinesDetailClass linesDetailClass, string splittedString)
        {
            constructor.parameterTypes = new List<Type>();
            var innerBracketLevel = ReflectionUtility.GetIntegerAfterString(splittedString, Constants.InnerBracketLevel);
            for (var i = 0; i < linesDetailClass.levelsClasses.Count; i++)
                if (linesDetailClass.levelsClasses[i].level == innerBracketLevel)
                {
                    constructor.parameters = linesDetailClass.levelsClasses[i].parameterValues.ToArray();
                    if (constructor.parameters != null)
                    {
                        for (var j = 0; j < constructor.parameters.Length; j++)
                        {
                            constructor.parameterTypes.Add(constructor.parameters[j].GetType());
                        }
                        
                        return true;
                    }
                }
            return false;
        }
        
        private static bool DetermineConstructorInfo(ConstructorClass constructorClass)
        {
            constructorClass.constructorInfo = constructorClass.classType.GetConstructor(constructorClass.parameterTypes.ToArray());
            return constructorClass.constructorInfo != null;
        }
    }
}