// ----------------------------------------------------
// AI Engine
// Copyright (c) Pampel Games e.K. All Rights Reserved.
// https://www.pampelgames.com
// ----------------------------------------------------

using System.Globalization;
using PampelGames.Shared.Editor;
using PampelGames.Shared.Utility;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace PampelGames.AIEngine.Editor
{
    public class AIEngineHistory : EditorWindow
    {
        private VisualElement container;
        public VisualTreeAsset _visualTree;

        public AIEngineSettingsSO aiEngineSettingsSo;
        public HistorySO historySo;

        private VisualElement HistoryBlocks;
        private IntegerField showLast;
        private Button updateButton;
        private Button clearHistoryButton;

        /********************************************************************************************************************************/


        protected void OnEnable()
        {
            if (aiEngineSettingsSo == null)
                aiEngineSettingsSo = PGAssetUtility.LoadAsset<AIEngineSettingsSO>(Constants.AIEngineSettingsSO);

            if (historySo == null)
                historySo = PGAssetUtility.LoadAsset<HistorySO>(Constants.HistorySO);

            container = new VisualElement();
            _visualTree.CloneTree(container);
            rootVisualElement.Add(container);
            FindElements();

            SubscribeToRequestCallback();
        }

        private void FindElements()
        {
            HistoryBlocks = rootVisualElement.Q<VisualElement>(nameof(HistoryBlocks));
            showLast = rootVisualElement.Q<IntegerField>(nameof(showLast));
            updateButton = rootVisualElement.Q<Button>(nameof(updateButton));
            clearHistoryButton = rootVisualElement.Q<Button>(nameof(clearHistoryButton));
        }

        private void SubscribeToRequestCallback()
        {
            var aiEngine = PGEditorWindowUtility.GetWindow<AIEngine>();
            if (aiEngine == null) return;
            aiEngine.RequestDoneAction += RequestDoneCallback;
        }

        internal void RequestDoneCallback()
        {
            UpdateHistoryBlocks();
        }

        /********************************************************************************************************************************/

        protected void CreateGUI()
        {
            CreateButtons();
            UpdateHistoryBlocks();
        }

        private void CreateButtons()
        {
            showLast.PGClampValue();
            updateButton.clicked += UpdateHistoryBlocks;
            clearHistoryButton.clicked += ClearHistory;
        }

        /********************************************************************************************************************************/

        private void UpdateHistoryBlocks()
        {
            HistoryBlocks.Clear();
            var _showLast = 0;
            for (var i = historySo.historyClasses.Count - 1; i >= 0; i--)
            {
                var historyBlock = new VisualElement();
                historyBlock.AddToClassList(PGConstantsUSS.DrawTopLine);
                historyBlock.PGPadding(0, 0, 10, 10);

                var dateTime = new TextField();
                dateTime.tooltip = "Time of task execution and response time.";
                dateTime.isReadOnly = true;
                var task = new TextField();
                task.tooltip = "Task given by the user.";
                task.PGWrapText();
                var response = new TextField();
                response.tooltip = "Full response given by the AI.";
                response.PGWrapText();
                var errorMessage = new TextField();
                errorMessage.tooltip = "Error being spotted. \n" +
                                       "If you don't think the AI made a mistake but there is a problem with the conversion, " +
                                       "please join the discord and share your history. Thank you!";
                errorMessage.PGWrapText();
                errorMessage.PGTextInputColor(new Color32(60, 42, 42, 255));

                dateTime.value = historySo.historyClasses[i].dateTime + "\n" +
                                 "Response Time: "+ historySo.historyClasses[i].responseTime.ToString("0.00", CultureInfo.InvariantCulture) + "s\n" +
                                 "Tokens: "+historySo.historyClasses[i].tokensUsed;
                task.value = historySo.historyClasses[i].task;
                response.value = historySo.historyClasses[i].response;
                errorMessage.value = historySo.historyClasses[i].failText;


                historyBlock.Add(dateTime);
                historyBlock.Add(task);
                historyBlock.Add(response);
                if (historySo.historyClasses[i].failed) historyBlock.Add(errorMessage);
                HistoryBlocks.Add(historyBlock);

                _showLast++;
                if (_showLast >= showLast.value) break;
            }
        }

        private void ClearHistory()
        {
            historySo.historyClasses.Clear();
            EditorUtility.SetDirty(historySo);
            UpdateHistoryBlocks();
        }
    }
}