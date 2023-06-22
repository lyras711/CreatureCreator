// ----------------------------------------------------
// Copyright (c) Pampel Games e.K. All Rights Reserved.
// https://www.pampelgames.com
// ----------------------------------------------------

using System;
using System.Collections.Generic;
using UnityEngine;

namespace PampelGames.Shared.Utility
{
    public static class PGArrayUtility
    {
        public static void Insert<T>(ref T[] array, int index, T item)
        {
            if (index < 0 || index > array.Length) return;
            T[] newArray = new T[array.Length + 1];
            Array.Copy(array, 0, newArray, 0, index);
            newArray[index] = item;
            Array.Copy(array, index, newArray, index + 1, array.Length - index);
            array = newArray;
        }
        
        public static void Add<T>(ref T[] array, T item)
        {
            Array.Resize(ref array, array.Length + 1);
            array[^1] = item;
        }

        public static void RemoveAt<T>(ref T[] array, int index)
        {
            if (index < 0 || index >= array.Length) return;
            T[] newArray = new T[array.Length - 1];
            Array.Copy(array, 0, newArray, 0, index);
            Array.Copy(array, index + 1, newArray, index, array.Length - index - 1);
            array = newArray;
        }
        
        
    }
}
