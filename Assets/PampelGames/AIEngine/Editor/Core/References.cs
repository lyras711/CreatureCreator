// ----------------------------------------------------
// AI Engine
// Copyright (c) Pampel Games e.K. All Rights Reserved.
// https://www.pampelgames.com
// ----------------------------------------------------

using System.Collections.Generic;
using PampelGames.Shared.Editor;
using PampelGames.Shared.Utility;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace PampelGames.AIEngine.Editor
{
    internal static class References
    {
        internal static void AddReferences(AIEngine aiEngine, List<GameObject> objectList, Enums.ReferencesBehaviour referencesBehaviour, bool manual)
        {
            if (objectList == null || objectList.Count == 0) return;
            if (referencesBehaviour == Enums.ReferencesBehaviour.Locked)
            {
                DebugHandler.SendDebugLog(aiEngine,objectList.Count +  " new GameObjects have not been added because they are locked.");
                return;
            }

            if (!manual)
            {
                aiEngine.references.Clear();
                EditorUtility.SetDirty(aiEngine);
            }

            ClearEmptyReferences(aiEngine);
            
            for (var i = 0; i < objectList.Count; i++)
            {
                if(objectList[i] == null) continue;
                if (PGAssetUtility.IsPrefab(objectList[i]))
                {
                    if (aiEngine.prefabs.Contains(objectList[i])) continue;
                    aiEngine.prefabs.Add(objectList[i]);
                }
                else
                {
                    if (aiEngine.references.Contains(objectList[i])) continue;
                    aiEngine.references.Add(objectList[i]);    
                }
            }

            EditorUtility.SetDirty(aiEngine);
            UpdateReferencesTooltip(aiEngine);
        }

        internal static void CreateReferences(AIEngine aiEngine)
        {
            if (aiEngine == null) return;
            aiEngine.input.RegisterCallback<DragUpdatedEvent>(evt => { DragAndDrop.visualMode = DragAndDropVisualMode.Copy; });
            aiEngine.input.RegisterCallback<DragPerformEvent>(e =>
            {
                var newReferences = new List<GameObject>();
                foreach (var reference in DragAndDrop.objectReferences)
                {
                    if (reference.GetType() != typeof(GameObject)) continue;
                    newReferences.Add(reference as GameObject);
                }

                AddReferences(aiEngine, newReferences, Enums.ReferencesBehaviour.Unlocked, true);
            });

            aiEngine.selectPrefabs.tooltip = "Select all prefabs.";
            aiEngine.selectPrefabs.PGSetupClickableIcon();
            aiEngine.selectPrefabs.RegisterCallback<ClickEvent>(x =>
            {
                ClearEmptyReferences(aiEngine);
                UpdateReferencesTooltip(aiEngine);
                SelectAllPrefabs(aiEngine);
            });
            
            aiEngine.selectReferences.tooltip = "Select all references.";
            aiEngine.selectReferences.PGSetupClickableIcon();
            aiEngine.selectReferences.RegisterCallback<ClickEvent>(x =>
            {
                ClearEmptyReferences(aiEngine);
                UpdateReferencesTooltip(aiEngine);
                SelectAllReferences(aiEngine);
            });

            aiEngine.clearPrefabs.tooltip = "Clear prefabs list.";
            aiEngine.clearPrefabs.PGSetupClickableIcon();
            aiEngine.clearPrefabs.RegisterCallback<ClickEvent>(x =>
            {
                aiEngine.prefabs.Clear();
                EditorUtility.SetDirty(aiEngine);
                DebugHandler.SendDebugLog(aiEngine, "Prefabs cleared.");
                UpdateReferencesTooltip(aiEngine);
            });
            
            aiEngine.clearReferences.tooltip = "Clear references list.";
            aiEngine.clearReferences.PGSetupClickableIcon();
            aiEngine.clearReferences.RegisterCallback<ClickEvent>(x =>
            {
                aiEngine.references.Clear();
                EditorUtility.SetDirty(aiEngine);
                DebugHandler.SendDebugLog(aiEngine, "References cleared.");
                UpdateReferencesTooltip(aiEngine);
            });

            ClearEmptyReferences(aiEngine);
            UpdateReferencesTooltip(aiEngine);
            CreateReferencesBehaviour(aiEngine);
        }

        /********************************************************************************************************************************/

        internal static void UpdateReferencesTooltip(AIEngine aiEngine)
        {
            if (aiEngine == null) return;
            
            var currentPrefabs = aiEngine.prefabs;
            var prefabsTooltip = "Current \"prefabs\": " + "\n";
            for (var i = 0; i < currentPrefabs.Count; i++)
            {
                if (currentPrefabs[i] == null) continue;
                prefabsTooltip = prefabsTooltip + "\n" + currentPrefabs[i].name;
            }

            aiEngine.prefabsLabel.tooltip = "Project prefabs (\"prefabs\")\n" + "\n" +
                                               "To instantiate prefabs, they have to be referenced.\n" + "\n" +
                                               "To add prefabs, simply drag them onto the text input field.\n" + "\n"+
                                               "When mentioning prefabs in your prompts, call them \"prefab(s)\". For example: \"Instantiate 10 prefabs\"";
            aiEngine.prefabsAmount.tooltip = prefabsTooltip;
            aiEngine.prefabsAmount.value = currentPrefabs.Count;
            
            /********************************************************************************************************************************/
            
            var currentReferences = aiEngine.references;
            var referencesTooltip = "Current \"refs\": " + "\n";
            for (var i = 0; i < currentReferences.Count; i++)
            {
                if (currentReferences[i] == null) continue;
                referencesTooltip = referencesTooltip + "\n" + currentReferences[i].name;
            }

            aiEngine.referencesLabel.tooltip = "Scene references (\"refs\")\n" + "\n" +
                                               "To use or manipulate GameObjects, they have to be referenced.\n" + "\n" +
                                               "To add references, simply drag them onto the text input field. " +
                                               "Objects created by the AI are added automatically (if not locked).\n" + "\n"+
                                               "When mentioning references in your prompts, call them \"ref(s)\". For example: \"Add a rigidbody to the refs\"\n" +
                                               "\n" +
                                               "However, you can also do things like: \"Get the terrain from the scene and ...\"";
            aiEngine.referencesAmount.tooltip = referencesTooltip;
            aiEngine.referencesAmount.value = currentReferences.Count;
        }

        internal static void ClearEmptyReferences(AIEngine aiEngine)
        {
            for (var i = aiEngine.references.Count - 1; i >= 0; i--)
                if (aiEngine.references[i] == null)
                    aiEngine.references.RemoveAt(i);
            
            for (var i = aiEngine.prefabs.Count - 1; i >= 0; i--)
                if (aiEngine.prefabs[i] == null)
                    aiEngine.prefabs.RemoveAt(i);
            
            EditorUtility.SetDirty(aiEngine);
        }

        private static void SelectAllReferences(AIEngine aiEngine)
        {
            if (aiEngine.references.Count == 0) return;
            var selectedObjects = new List<GameObject>();

            foreach (var reference in aiEngine.references) selectedObjects.Add(reference);

            Selection.objects = selectedObjects.ToArray();
        }
        
        private static void SelectAllPrefabs(AIEngine aiEngine)
        {
            if (aiEngine.prefabs.Count == 0) return;
            var selectedObjects = new List<GameObject>();

            foreach (var reference in aiEngine.prefabs) selectedObjects.Add(reference);

            Selection.objects = selectedObjects.ToArray();
        }

        private static void CreateReferencesBehaviour(AIEngine aiEngine)
        {
            aiEngine.referencesUnlocked.PGSetupClickableIcon();
            aiEngine.referencesUnlocked.tooltip = "Newly created GameObjects will replace existing references.";
            aiEngine.referencesUnlocked.RegisterCallback<ClickEvent>(evt =>
            {
                aiEngine.referencesBehaviour = Enums.ReferencesBehaviour.Locked;
                EditorUtility.SetDirty(aiEngine);
                ReferencesBehaviourVisibility(aiEngine);
            });

            aiEngine.referencesLocked.PGSetupClickableIcon();
            aiEngine.referencesLocked.tooltip = "References are locked and can only be added manually.";
            aiEngine.referencesLocked.RegisterCallback<ClickEvent>(evt =>
            {
                aiEngine.referencesBehaviour = Enums.ReferencesBehaviour.Unlocked;
                EditorUtility.SetDirty(aiEngine);
                ReferencesBehaviourVisibility(aiEngine);
            });
            ReferencesBehaviourVisibility(aiEngine);
        }

        private static void ReferencesBehaviourVisibility(AIEngine aiEngine)
        {
            aiEngine.referencesLocked.style.display = DisplayStyle.None;
            aiEngine.referencesUnlocked.style.display = DisplayStyle.None;

            if (aiEngine.referencesBehaviour == Enums.ReferencesBehaviour.Locked)
                aiEngine.referencesLocked.style.display = DisplayStyle.Flex;
            else if (aiEngine.referencesBehaviour == Enums.ReferencesBehaviour.Unlocked)
                aiEngine.referencesUnlocked.style.display = DisplayStyle.Flex;
        }
    }
}