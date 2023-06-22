// ----------------------------------------------------
// AI Engine
// Copyright (c) Pampel Games e.K. All Rights Reserved.
// https://www.pampelgames.com
// ----------------------------------------------------

using System;
using System.Collections.Generic;
using System.Globalization;
using PampelGames.Shared.Editor;
using PampelGames.Shared.Utility;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UIElements;

namespace PampelGames.AIEngine.Editor
{
    public class AIEngine : EditorWindow
    {
        public Texture2D windowIcon;

        private VisualElement container;
        public VisualTreeAsset _visualTree;

        private SerializedObject serializedObject;

        private VisualElement documentation;
        private VisualElement globalSettings;

        public AIEngineSettingsSO aiEngineSettingsSo;
        public ChatGPTSettingsSO chatGPTSettingsSo;
        public HistorySO historySo;
        public AffixesSO affixesSo;

        private int undoGroupIndex = -1;

        private Enums.ConnectionStatus connectionStatus = Enums.ConnectionStatus.NotConnected;
        internal int tokensLastInt;
        private int tokensTotalInt;

        private VisualElement Input;
        internal TextField input;

        private Label infoText;
        private VisualElement notConnected;
        private VisualElement connected;
        private VisualElement connecting;
        private VisualElement reconnectIcon;
        private VisualElement historyIcon;

        private IntegerField tokensLast;
        private IntegerField tokensTotal;

        public Action RequestDoneAction = delegate { };
        private bool requestActive;
        private UnityWebRequest chatGPTRequest;
        private UnityWebRequest chatGPTRequestConnectionTest;

        public List<GameObject> prefabs = new();
        internal Label prefabsLabel;
        internal IntegerField prefabsAmount;
        internal VisualElement selectPrefabs;
        internal VisualElement clearPrefabs;
        internal VisualElement prefabsLocked;
        internal VisualElement prefabsUnlocked;

        public List<GameObject> references = new();
        public Enums.ReferencesBehaviour referencesBehaviour = Enums.ReferencesBehaviour.Unlocked;
        internal Label referencesLabel;
        internal IntegerField referencesAmount;
        internal VisualElement selectReferences;
        internal VisualElement clearReferences;
        internal VisualElement referencesLocked;
        internal VisualElement referencesUnlocked;

        private HistoryClass _historyClass;

        /********************************************************************************************************************************/
        protected void OnEnable()
        {
            serializedObject = new SerializedObject(this);

            if (aiEngineSettingsSo == null)
                aiEngineSettingsSo = PGAssetUtility.LoadAsset<AIEngineSettingsSO>(Constants.AIEngineSettingsSO);

            if (chatGPTSettingsSo == null)
                chatGPTSettingsSo = PGAssetUtility.LoadAsset<ChatGPTSettingsSO>(ChatGPTConstants.ChatGPTSettingsSO);

            if (affixesSo == null)
                affixesSo = PGAssetUtility.LoadAsset<AffixesSO>(Constants.AffixesSO);

            if (historySo == null)
                historySo = PGAssetUtility.LoadAsset<HistorySO>(Constants.HistorySO);

            container = new VisualElement();
            _visualTree.CloneTree(container);
            rootVisualElement.Add(container);
            FindElements();

            ResponseTimeVisibility();
            if (connectionStatus != Enums.ConnectionStatus.Connected && aiEngineSettingsSo.autoConnect) ConnectionTest();
        }

        private void FindElements()
        {
            documentation = container.Q<VisualElement>(nameof(documentation));
            globalSettings = container.Q<VisualElement>(nameof(globalSettings));

            Input = container.Q<VisualElement>(nameof(Input));
            input = container.Q<TextField>(nameof(input));

            infoText = container.Q<Label>(nameof(infoText));
            notConnected = container.Q<VisualElement>(nameof(notConnected));
            connected = container.Q<VisualElement>(nameof(connected));
            connecting = container.Q<VisualElement>(nameof(connecting));
            reconnectIcon = container.Q<VisualElement>(nameof(reconnectIcon));
            historyIcon = container.Q<VisualElement>(nameof(historyIcon));

            prefabsLabel = container.Q<Label>(nameof(prefabsLabel));
            prefabsAmount = container.Q<IntegerField>(nameof(prefabsAmount));
            selectPrefabs = container.Q<VisualElement>(nameof(selectPrefabs));
            clearPrefabs = container.Q<VisualElement>(nameof(clearPrefabs));
            prefabsLocked = container.Q<VisualElement>(nameof(prefabsLocked));
            prefabsUnlocked = container.Q<VisualElement>(nameof(prefabsUnlocked));

            referencesLabel = container.Q<Label>(nameof(referencesLabel));
            referencesAmount = container.Q<IntegerField>(nameof(referencesAmount));
            selectReferences = container.Q<VisualElement>(nameof(selectReferences));
            clearReferences = container.Q<VisualElement>(nameof(clearReferences));
            referencesLocked = container.Q<VisualElement>(nameof(referencesLocked));
            referencesUnlocked = container.Q<VisualElement>(nameof(referencesUnlocked));

            tokensLast = container.Q<IntegerField>(nameof(tokensLast));
            tokensTotal = container.Q<IntegerField>(nameof(tokensTotal));
            if (!chatGPTSettingsSo.showTokenInfo)
            {
                tokensLast.style.display = DisplayStyle.None;
                tokensTotal.style.display = DisplayStyle.None;
            }
        }

        /********************************************************************************************************************************/

        protected void CreateGUI()
        {
            CreateHeader();
            CreateTextInput();
            CreateReferences();
            CreateInformation();
        }

        /********************************************************************************************************************************/

        private void CreateHeader()
        {
            documentation.PGSetupClickableIcon();
            documentation.tooltip = "Open the documentation page.";
            documentation.RegisterCallback<ClickEvent>(evt => Application.OpenURL(Constants.DocumentationURL));

            globalSettings.PGSetupClickableIcon();
            globalSettings.tooltip = "Open the settings window.";
            globalSettings.RegisterCallback<ClickEvent>(evt => { MenuItems.OpenAIEngineSettings(); });
        }

        /********************************************************************************************************************************/
        private void CreateTextInput()
        {
            ConnectionVisibility();

            reconnectIcon.PGSetupClickableIcon();
            reconnectIcon.tooltip = "Connect.";
            reconnectIcon.RegisterCallback<ClickEvent>(evt =>
            {
                input.Focus();
                input.value = "";
                ClearRequests();
                ConnectionTest();
            });


            historyIcon.PGSetupClickableIcon();
            historyIcon.tooltip = "Show history.";
            historyIcon.RegisterCallback<ClickEvent>(evt =>
            {
                MenuItems.OpenAIEngineHistory();
                InitializeAIEngineHistory();
            });


            input.tooltip = "Write a task and press the '" + aiEngineSettingsSo.confirmTextKeycode + "' key to confirm.\n" + "\n" +
                            "Use the '' or `` prefix to execute custom code.\n" + "\n" +
                            "You can also drag GameObjects onto this field to add them to the references (\"refs\").";
            input.PGWrapText();
            input.RegisterCallback<KeyDownEvent>(evt =>
            {
                if (evt.keyCode != aiEngineSettingsSo.confirmTextKeycode) return;
                var inputText = input.value;
                if (string.IsNullOrWhiteSpace(inputText)) return;
                inputText = inputText.TrimStart();
                inputText = inputText.TrimEnd();
                if (aiEngineSettingsSo.clearText)
                {
                    input.Focus();
                    input.value = "";
                }

                if (inputText.StartsWith("''") || inputText.StartsWith("``"))
                {
                    _historyClass = new HistoryClass();
                    _historyClass.startTime = Time.realtimeSinceStartup;
                    RequestCallback(inputText.Substring(2), 0);
                }
                else
                {
                    StartRequest(inputText);
                }
            });
        }

        private void InitializeAIEngineHistory()
        {
            var aiEngineHistory = PGEditorWindowUtility.GetWindow<AIEngineHistory>();
            if (aiEngineHistory != null) RequestDoneAction += aiEngineHistory.RequestDoneCallback;
        }

        /********************************************************************************************************************************/

        private void CreateReferences()
        {
            References.CreateReferences(this);
        }

        /********************************************************************************************************************************/

        private void CreateInformation()
        {
            infoText.text = "";
            tokensLast.tooltip = "Tokens used for the last prompt.";
            tokensTotal.tooltip = "Tokens used for this session.";
            SetTokenValues(0);
        }

        private void SetTokenValues(int addLastTokens)
        {
            tokensLastInt = addLastTokens;
            tokensTotalInt += tokensLastInt;
            tokensLast.value = tokensLastInt;
            tokensTotal.value = tokensTotalInt;
        }

        /********************************************************************************************************************************/
        /* Requests **********************************************************************************************************************/

        private void StartRequest(string promptRaw)
        {
            if (!CheckAPIKey()) return;
            if (chatGPTRequest != null && !chatGPTRequest.isDone) return;
            _historyClass = new HistoryClass();
            _historyClass.startTime = Time.realtimeSinceStartup;
            _historyClass.task = promptRaw;
            requestActive = true;
            _historyClass.testRequest = false;
            ResponseTimeVisibility();
            References.ClearEmptyReferences(this);
            References.UpdateReferencesTooltip(this);
            chatGPTRequest = new UnityWebRequest();
            var promptString = ConversionPrompt.GeneratePrompt(promptRaw, affixesSo, prefabs, references);
            ChatGPTRequest.Request(chatGPTRequest, promptString, chatGPTSettingsSo, RequestCallback, 0, ConnectionFailCallback);
        }

        private void RequestCallback(string responseString, int totalTokens)
        {
            SetTokenValues(totalTokens);
            _historyClass.response = responseString;
            _historyClass.tokensUsed = tokensLastInt;
            UpdateHistorySO(true);
            if (!ConversionResponse.CreateLinesString(this, responseString, prefabs, references, out var linesString, out var namespaces,
                    out var parentClass)) return;
            var executionClass = new ExecutionClass();
            executionClass.Initialize(this, linesString, prefabs, references, namespaces, parentClass);
            CodeMethods.CreateCodeMethods(executionClass);
            var objectList = Execution.ExecuteTasks(executionClass);
            AddRuntimeReferences(objectList);
            RemoveDefaultPrimitive(executionClass);
            FinalizeResponse();
        }

        /********************************************************************************************************************************/

        private void ConnectionTest()
        {
            if (!CheckAPIKey()) return;
            if (chatGPTRequestConnectionTest != null && !chatGPTRequestConnectionTest.isDone) return;
            _historyClass = new HistoryClass();
            _historyClass.startTime = Time.realtimeSinceStartup;
            connectionStatus = Enums.ConnectionStatus.Connecting;
            ConnectionVisibility();
            requestActive = true;
            _historyClass.testRequest = true;
            ResponseTimeVisibility();
            var prompt = affixesSo.connectionTest;
            chatGPTRequestConnectionTest = new UnityWebRequest();
            ChatGPTRequest.Request(chatGPTRequestConnectionTest, prompt, chatGPTSettingsSo, ConnectionTestCallback, 1.5f, ConnectionFailCallback);
        }

        private void ConnectionTestCallback(string response, int totalTokens)
        {
            SetTokenValues(totalTokens);
            connectionStatus = Enums.ConnectionStatus.Connected;
            input.value = response;
            FinalizeResponse();
        }

        /********************************************************************************************************************************/

        private bool CheckAPIKey()
        {
            if (!string.IsNullOrWhiteSpace(chatGPTSettingsSo.apiKey)) return true;
            connectionStatus = Enums.ConnectionStatus.NotConnected;
            input.value = "No API Key specified.";
            ConnectionVisibility();
            return false;
        }

        /********************************************************************************************************************************/
        private void ConnectionFailCallback()
        {
            DebugHandler.SendDebugError(this, "Connection failed.");
            connectionStatus = Enums.ConnectionStatus.NotConnected;
            FinalizeResponse();
        }

        internal void ScriptExecutionError(string message)
        {
            if (_historyClass.failed) return;
            _historyClass.failed = true;
            _historyClass.failText = message;
            FinalizeResponse();
        }

        /********************************************************************************************************************************/

        private void AddRuntimeReferences(List<GameObject> objectList)
        {
            References.AddReferences(this, objectList, referencesBehaviour, false);
        }

        private void FinalizeResponse()
        {
            if (!_historyClass.testRequest) undoGroupIndex = Undo.SaveCurrentState(references);
            requestActive = false;
            _historyClass.endTime = Time.realtimeSinceStartup;
            _historyClass.responseTime = _historyClass.endTime - _historyClass.startTime;
            _historyClass.dateTime = DateTime.Now.ToString(CultureInfo.InvariantCulture);
            RemoveMonoBehaviourObject();
            ConnectionVisibility();
            ResponseTimeVisibility();
            InitializeAIEngineHistory();
            References.ClearEmptyReferences(this);
            References.UpdateReferencesTooltip(this);
            if (!_historyClass.testRequest) UpdateHistorySO(false);
            if (!_historyClass.testRequest) RequestDoneAction();
            if (_historyClass.failed) infoText.text = Constants.InfoSomethingWrong;
        }

        /********************************************************************************************************************************/
        /********************************************************************************************************************************/

        private void RemoveMonoBehaviourObject()
        {
            var monoBehaviourObject = GameObject.Find(Constants.MonoBehaviourObject);
            if (monoBehaviourObject == null) return;
            var childCount = monoBehaviourObject.transform.childCount;
            for (var i = childCount - 1; i >= 0; i--)
            {
                var childTransform = monoBehaviourObject.transform.GetChild(i);
                childTransform.SetParent(null);
                childTransform.SetAsLastSibling();
            }

            DestroyImmediate(monoBehaviourObject);
        }

        private void RemoveDefaultPrimitive(ExecutionClass executionClass)
        {
            var defaultPrimitive = executionClass.defaultPrimitive;
            DestroyImmediate(defaultPrimitive);
        }

        private void UpdateHistorySO(bool createNew)
        {
            if (createNew)
            {
                if (!aiEngineSettingsSo.storeHistory) return;
                if (historySo.historyClasses.Count >= aiEngineSettingsSo.storeHistoryAmount && historySo.historyClasses.Count > 0)
                    historySo.historyClasses.RemoveAt(0);
                historySo.historyClasses.Add(new HistoryClass());
            }

            historySo.historyClasses[^1] = _historyClass;
            EditorUtility.SetDirty(historySo);
        }

        private void ConnectionVisibility()
        {
            connected.style.display = DisplayStyle.None;
            connecting.style.display = DisplayStyle.None;
            notConnected.style.display = DisplayStyle.None;

            if (connectionStatus == Enums.ConnectionStatus.Connected)
            {
                connected.style.display = DisplayStyle.Flex;
            }
            else
            {
                infoText.text = "";
                if (connectionStatus == Enums.ConnectionStatus.Connecting)
                {
                    infoText.text = Constants.InfoPleaseWait;
                    connecting.style.display = DisplayStyle.Flex;
                }
                else if (connectionStatus == Enums.ConnectionStatus.NotConnected)
                {
                    notConnected.style.display = DisplayStyle.Flex;
                }
            }
        }

        private void ResponseTimeVisibility()
        {
            if (requestActive)
                infoText.text = Constants.InfoPleaseWait;
            else
                infoText.text = "";
        }


        /********************************************************************************************************************************/

        private void OnDestroy()
        {
            ClearRequests();
        }

        private void ClearRequests()
        {
            if (chatGPTRequest != null && !chatGPTRequest.isDone) chatGPTRequest.Abort();
            if (chatGPTRequestConnectionTest != null && !chatGPTRequestConnectionTest.isDone) chatGPTRequestConnectionTest.Abort();
            requestActive = false;
        }
    }
}