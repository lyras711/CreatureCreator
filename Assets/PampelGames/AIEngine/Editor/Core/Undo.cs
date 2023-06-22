// ----------------------------------------------------
// AI Engine
// Copyright (c) Pampel Games e.K. All Rights Reserved.
// https://www.pampelgames.com
// ----------------------------------------------------

using System.Collections.Generic;
using UnityEngine;

namespace PampelGames.AIEngine.Editor
{
    internal static class Undo
    {
        internal static int SaveCurrentState(List<GameObject> references)
        {
            UnityEditor.Undo.SetCurrentGroupName(Constants.UndoGroup);
            var undoGroupIndex = UnityEditor.Undo.GetCurrentGroup();

            for (var i = 0; i < references.Count; i++)
                if (references[i] != null)
                    UnityEditor.Undo.RegisterCreatedObjectUndo(references[i], Constants.ObjectCreated + i);

            UnityEditor.Undo.CollapseUndoOperations(undoGroupIndex);

            return undoGroupIndex;
        }
    }
}