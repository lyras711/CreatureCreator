// ----------------------------------------------------
// AI Engine
// Copyright (c) Pampel Games e.K. All Rights Reserved.
// https://www.pampelgames.com
// ----------------------------------------------------

using System;
using System.Reflection;
using UnityEngine;

namespace PampelGames.AIEngine.Editor
{
    public static class ReflectionGetSet
    {
        /// <summary>
        ///     Gets a field or property value from a type by name.
        /// </summary>
        /// <param name="referenceType">Can be a static class, an instance or an enum.</param>
        /// <param name="stringName">
        ///     Name of the property or field. Can also be connected, for example: transform.position.x
        ///     (but not including class, for example: Mathf.Deg2Rad.x, here only Deg2Rad.x).
        /// </param>
        /// <param name="reference">For static null. Otherwise pass in the instance.</param>
        /// <returns>Value as object. Can be null.</returns>
        public static object GetValueByName(Type referenceType, string stringName, object reference = null)
        {
            return GetValueByNameInternal(referenceType, stringName, reference);
        }

        /// <summary>
        ///     Sets a field or property value from a type by name.
        /// </summary>
        /// <param name="referenceType">Can be a static class, an instance or an enum.</param>
        /// <param name="stringName">Name of the property or field. Can also be connected, for example: transform.position.x </param>
        /// <param name="value">Value to set.</param>
        /// <param name="reference">For static null. Otherwise pass in the instance.</param>
        /// <returns>Return true when the value was set.</returns>
        public static bool SetValueByName(Type referenceType, string stringName, object value, object reference = null)
        {
            return SetValueByNameInternal(referenceType, stringName, value, reference);
        }

        /********************************************************************************************************************************/
        /* Private **********************************************************************************************************************/


        private static object GetValueByNameInternal(Type referenceType, string stringName, object reference = null)
        {
            if (referenceType == null) return null;
            object value;

            var tokens = stringName.Split('.');
            if (tokens == null || tokens.Length == 0)
            {
                tokens = Array.Empty<string>();
                tokens[0] = stringName;
            }

            var fieldInfos = new FieldInfo[tokens.Length];
            var propertyInfos = new PropertyInfo[tokens.Length];
            var values = new object[tokens.Length];

            fieldInfos[0] = referenceType.GetField(tokens[0]);
            if (fieldInfos[0] != null)
            {
                values[0] = fieldInfos[0].GetValue(reference);
                value = values[0];
            }
            else
            {
                propertyInfos[0] = referenceType.GetProperty(tokens[0]);
                if (propertyInfos[0] != null)
                {
                    values[0] = propertyInfos[0].GetValue(reference);
                    value = values[0];
                }
                else
                {
                    return null;
                }
            }

            if (tokens.Length == 1) return values[0];

            for (var i = 1; i < tokens.Length; i++)
                if (fieldInfos[i - 1] != null)
                {
                    fieldInfos[i] = fieldInfos[i - 1].FieldType.GetField(tokens[i]);
                    if (fieldInfos[i] != null) values[i] = fieldInfos[i].GetValue(values[i - 1]);
                    propertyInfos[i] = fieldInfos[i - 1].FieldType.GetProperty(tokens[i]);
                    if (propertyInfos[i] != null) values[i] = propertyInfos[i].GetValue(values[i - 1]);
                }
                else if (propertyInfos[i - 1] != null)
                {
                    fieldInfos[i] = propertyInfos[i - 1].PropertyType.GetField(tokens[i]);
                    if (fieldInfos[i] != null) values[i] = fieldInfos[i].GetValue(values[i - 1]);
                    propertyInfos[i] = propertyInfos[i - 1].PropertyType.GetProperty(tokens[i]);
                    if (propertyInfos[i] != null) values[i] = propertyInfos[i].GetValue(values[i - 1]);
                }

            if (fieldInfos[^1] != null) value = fieldInfos[^1].GetValue(values[^2]);
            else if (propertyInfos[^1] != null) value = propertyInfos[^1].GetValue(values[^2]);

            return value;
        }

        private static bool SetValueByNameInternal(Type referenceType, string stringName, object value, object reference = null)
        {
            if (referenceType == null) return false;

            var tokens = stringName.Split('.');

            if (tokens == null || tokens.Length == 0)
            {
                tokens = Array.Empty<string>();
                tokens[0] = stringName;
            }
            
            var fieldInfos = new FieldInfo[tokens.Length];
            var propertyInfos = new PropertyInfo[tokens.Length];
            var values = new object[tokens.Length];
            
            fieldInfos[0] = referenceType.GetField(tokens[0]);
            
            if (fieldInfos[0] != null)
            {
                values[0] = fieldInfos[0].GetValue(reference);
            }
            else
            {
                propertyInfos[0] = referenceType.GetProperty(tokens[0]);
                if (propertyInfos[0] != null)
                    values[0] = propertyInfos[0].GetValue(reference);
                else return false;
            }
            
            for (var i = 1; i < tokens.Length; i++)
                if (fieldInfos[i - 1] != null)
                {
                    fieldInfos[i] = fieldInfos[i - 1].FieldType.GetField(tokens[i]);
                    if (fieldInfos[i] != null) values[i] = fieldInfos[i].GetValue(values[i - 1]);
                    propertyInfos[i] = fieldInfos[i - 1].FieldType.GetProperty(tokens[i]);
                    if (propertyInfos[i] != null) values[i] = propertyInfos[i].GetValue(values[i - 1]);
                }
                else if (propertyInfos[i - 1] != null)
                {
                    fieldInfos[i] = propertyInfos[i - 1].PropertyType.GetField(tokens[i]);
                    if (fieldInfos[i] != null) values[i] = fieldInfos[i].GetValue(values[i - 1]);
                    propertyInfos[i] = propertyInfos[i - 1].PropertyType.GetProperty(tokens[i]);
                    if (propertyInfos[i] != null) values[i] = propertyInfos[i].GetValue(values[i - 1]);
                }
            
            if (values.Length >= 2)
            {
                if (fieldInfos[^1] != null) fieldInfos[^1].SetValue(values[^2], value);
                else if (propertyInfos[^1] != null) propertyInfos[^1].SetValue(values[^2], value);
            }
            else
            {
                if (fieldInfos[^1] != null) fieldInfos[^1].SetValue(reference, value);
                else if (propertyInfos[^1] != null) propertyInfos[^1].SetValue(reference, value); 
            }
            

            return true;
        }
    }
}