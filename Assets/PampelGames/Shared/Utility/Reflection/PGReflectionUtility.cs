// ----------------------------------------------------
// Copyright (c) Pampel Games e.K. All Rights Reserved.
// https://www.pampelgames.com
// ----------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;

namespace PampelGames.Shared.Utility
{
    public static class PGReflectionUtility
    {
        
        /// <summary>
        ///     Generates a list of all inheritors of a class type.
        /// </summary>
        /// <param name="assemblies">var assemblies = AppDomain.CurrentDomain.GetAssemblies();</param>
        /// <typeparam name="T">Type of the base class.</typeparam>
        public static List<T> CreateInstances<T>(Assembly[] assemblies)
            where T : class
        {
            var instances = new List<T>();

            foreach (var type in assemblies.SelectMany(a => a.GetTypes())
                         .Where(t => typeof(T).IsAssignableFrom(t) && t.IsClass && !t.IsAbstract))
            {
                var instance = (T)FormatterServices.GetUninitializedObject(type);
                instances.Add(instance);
            }

            return instances;
        }
        
        /// <summary>
        ///     Gets a class type by name.
        /// </summary>
        /// <param name="stringName">Name of the class. Can also be connected, for example: Mathf.Cos()</param>
        /// <param name="namespaces">List of namespaces to check for. If null, checks automatically for "UnityEngine".</param>
        /// <returns>Class type.</returns>
        public static Type GetTypeByName(string stringName, List<string> namespaces = null)
        {
            if (namespaces == null || namespaces.Count == 0) namespaces = new List<string> {"UnityEngine"};
            var classString = stringName.PGCutAfter(".", true);
            Type classType = null;
            foreach (var _namespace in namespaces)
            {
                var staticClassName = _namespace + "." + classString + "," + _namespace;
                classType = Type.GetType(staticClassName);
                if (classType != null) break;
            }

            return classType;
        }
        
    }
}