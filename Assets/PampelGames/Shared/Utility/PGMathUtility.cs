// ---------------------------------------------------
// Copyright (c) Pampel Games e.K. All Rights Reserved.
// https://www.pampelgames.com
// ---------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

namespace PampelGames.Shared.Utility
{
    /// <summary>
    ///     Math helpers.
    /// </summary>
    public static class PGMathUtility
    {

        /// <summary>
        ///     Generates a unique integer value.
        /// </summary>
        public static int UniqueGUID()
        {
            int uniqueId = Guid.NewGuid().GetHashCode();
            return uniqueId;
        }
        
        /// <summary>
        ///     Gets a random list or array entry using instances weights.
        /// </summary>
        /// <param name="arrayLenght">Lenght of list or array.</param>
        /// <param name="instancesWeights">Weights of the array in the same order. If entry does not exist, will be set to 1.</param>
        /// <returns></returns>
        public static int GetRandomArrayEntry(int arrayLenght, float[] instancesWeights)
        {
            var currentWeight = 0f;
            var totalWeight = 0f;
            for (var i = 0; i < arrayLenght; i++)
                totalWeight += i < instancesWeights.Length ? instancesWeights[i] : 1;

            var randomWeight = Random.Range(0f, totalWeight);

            for (var i = 0; i < arrayLenght; i++)
            {
                currentWeight += i < instancesWeights.Length ? instancesWeights[i] : 1;
                if (randomWeight <= currentWeight) return i;
            }

            return 0;
        }

        public static int GetRandomArrayEntry(int arrayLenght, List<float> instancesWeights)
        {
            return GetRandomArrayEntry(arrayLenght, instancesWeights.ToArray());
        }

        /// <summary>
        ///     Computes and returns the direction between two vector3, used to check if a vector is pointing left or right of another one
        /// </summary>
        /// <param name="vectorA">Vector a.</param>
        /// <param name="vectorB">Vector b.</param>
        /// <param name="up"></param>
        /// <returns>The <see cref="System.Single" />.</returns>
        public static float AngleDirection(Vector3 vectorA, Vector3 vectorB, Vector3 up)
        {
            var cross = Vector3.Cross(vectorA, vectorB);
            var direction = Vector3.Dot(cross, up);

            return direction;
        }

        /// <summary>
        /// Returns all integers chained as one int that are inside the string.
        /// </summary>
        public static int GetIntegerFromString(string stringValue)
        {
            var currentIntegerString = new String(stringValue.Where(Char.IsDigit).ToArray());
            return Int32.Parse(currentIntegerString);
        }
    }
}