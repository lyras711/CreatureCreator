// ----------------------------------------------------
// AI Engine
// Copyright (c) Pampel Games e.K. All Rights Reserved.
// https://www.pampelgames.com
// ----------------------------------------------------

using PampelGames.Shared.Editor;
using UnityEditor;
using UnityEngine;

namespace PampelGames.AIEngine.Editor
{
    public static class MenuItems 
    {
        [MenuItem("Tools/Pampel Games/AI Engine")]
        private static void OpenAIEngine()
        {
            var window = EditorWindow.GetWindow<AIEngine>();
            
            window.PGSetWindowSize(350, 260);
            window.PGCenterOnMainWindow();

            window.titleContent = new GUIContent("AI Engine", window.windowIcon);
            window.Show();
        }

        public static void OpenAIEngineSettings()
        {
            var window = EditorWindow.GetWindow<AIEngineSettings>();
            window.PGSetWindowSize(400,500);
            window.PGCenterOnMainWindow();
            window.titleContent = new GUIContent("AI Engine - Settings");
            window.Show();
        }
        
        public static void OpenAIEngineHistory()
        {
            var window = EditorWindow.GetWindow<AIEngineHistory>();
            window.PGSetWindowSize(500, 500);
            window.PGCenterOnMainWindow();
            window.titleContent = new GUIContent("AI Engine - History");
            window.Show();
        }

    }
    
}
