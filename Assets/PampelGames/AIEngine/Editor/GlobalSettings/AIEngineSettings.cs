// ----------------------------------------------------
// AI Engine
// Copyright (c) Pampel Games e.K. All Rights Reserved.
// https://www.pampelgames.com
// ----------------------------------------------------

using PampelGames.Shared.Editor;
using PampelGames.Shared.Utility;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace PampelGames.AIEngine.Editor
{
    public class AIEngineSettings : EditorWindow
    {
        private SerializedObject globalSettingsSerializedObject;
        public AIEngineSettingsSO aiEngineSettingsSo;
        
        private SerializedObject chatGPTSettingsSerializedObject;
        public ChatGPTSettingsSO chatGPTSettingsSo;

        private VisualElement chatGPTWrapper;
        private VisualElement settingsWrapper;
        private VisualElement detailsWrapper;
        
        private Button resetChatGPTSettingsButton;
        private Button resetSettingsButton;
        
        // ChatGPT
        private SerializedProperty chatGPTAPIKeySerializedProperty;
        private TextField chatGPTAPIKey;
        private VisualElement ChatGPTAPIModelWrapper;
        private SerializedProperty chatGPTAPIModelSerializedProperty;
        private TextField chatGPTAPIModel;
        private Button chatGPTAPIModelLink;
        private SerializedProperty chatGPTAPIOrganizationSerializedProperty;
        private TextField chatGPTAPIOrganization;
        private SerializedProperty chatGPTAPIURLSerializedProperty;
        private TextField chatGPTAPIURL;
        private SerializedProperty chatGPTMaxTokensSerializedProperty;
        private IntegerField chatGPTMaxTokens;
        private SerializedProperty chatGPTShowTokenInfoSerializedProperty;
        private Toggle chatGPTShowTokenInfo;
        
        
        // Settings
        private SerializedProperty autoConnectSerializedProperty;
        private Toggle autoConnect;
        private VisualElement SaveHistoryWrapper;
        private SerializedProperty saveHistorySerializedProperty;
        private Toggle saveHistory;
        private SerializedProperty saveHistoryAmountSerializedProperty;
        private IntegerField saveHistoryAmount;

        // Details
        private SerializedProperty logMessagesSerializedProperty;
        private Toggle logMessages;
        private SerializedProperty logErrorsSerializedProperty;
        private Toggle logErrors;
        private SerializedProperty logConvertedLinesSerializedProperty;
        private Toggle logConvertedLines;
        private SerializedProperty logLineDeterminationSerializedProperty;
        private Toggle logLineDetermination;
        
        private SerializedProperty confirmTextKeycodeSerializedProperty;
        private EnumField confirmTextKeycode;
        private SerializedProperty clearTextSerializedProperty;
        private Toggle clearText;
        
        

        /********************************************************************************************************************************/
        private void OnEnable()
        {
            if (aiEngineSettingsSo == null)
                aiEngineSettingsSo = PGAssetUtility.LoadAsset<AIEngineSettingsSO>(Constants.AIEngineSettingsSO);
            
            if (chatGPTSettingsSo == null)
                chatGPTSettingsSo = PGAssetUtility.LoadAsset<ChatGPTSettingsSO>(ChatGPTConstants.ChatGPTSettingsSO);
            
            globalSettingsSerializedObject ??= new SerializedObject(aiEngineSettingsSo);
            chatGPTSettingsSerializedObject ??= new SerializedObject(chatGPTSettingsSo);


            CreateEditorWindow();
            CreateChatGPTWrapper();
            CreateSettingsWrapper();
            CreateDevelopersWrapper();
            CreateResetButtons();
            
            BindElements();

        }

        private void CreateEditorWindow()
        {
            string[] elementNames = new string[3];
            elementNames[0] = "ChatGPT";
            elementNames[1] = "Settings";
            elementNames[2] = "Details";

            PGEditorWindowUtility.CreateEditorWindow("AI Engine - Settings", elementNames, out var _parentElement, out var _elementsArray);
            
            chatGPTWrapper = _elementsArray[0];
            settingsWrapper = _elementsArray[1];
            detailsWrapper = _elementsArray[2];

            var scrollView = new ScrollView();
            scrollView.Add(_parentElement);
            rootVisualElement.Add(scrollView);
        }

        private void CreateChatGPTWrapper()
        {
            chatGPTAPIKey = new TextField("API Key");
            chatGPTAPIKey.tooltip = "Your API key to access the OpenAI. You can get it by signing up on the OpenAI website.";
            chatGPTAPIKey.PGWrapText();
            chatGPTAPIKey.style.height = 34f;

            ChatGPTAPIModelWrapper = new VisualElement();
            ChatGPTAPIModelWrapper.style.flexDirection = FlexDirection.Row;
            chatGPTAPIModel = new TextField("API Model");
            chatGPTAPIModel.tooltip = "Choose from one of the models that OpenAI offers.";
            chatGPTAPIModel.style.flexGrow = 1f;
            chatGPTAPIModelLink = new Button();
            chatGPTAPIModelLink.text = "?";
            chatGPTAPIModelLink.tooltip = "Go to the OpenAI models overview page.";
            ChatGPTAPIModelWrapper.Add(chatGPTAPIModel);
            ChatGPTAPIModelWrapper.Add(chatGPTAPIModelLink);

            chatGPTAPIOrganization = new TextField("API Organization");
            chatGPTAPIOrganization.tooltip = "Organization that is used for the API requests (optional).";
            
            chatGPTAPIURL = new TextField("API URL");
            chatGPTAPIURL.tooltip = "OpenAI API URL address.";

            chatGPTMaxTokens = new IntegerField("Max Tokens");
            chatGPTMaxTokens.tooltip = "The maximum number of tokens used per request. Serves mainly as a safety precaution.";

            chatGPTShowTokenInfo = new Toggle("Display used Tokens");
            chatGPTShowTokenInfo.tooltip = "Show information about used tokens.\n" +
                                           "For this setting to apply, reopen the AI Engine window.";
            
            chatGPTWrapper.Add(chatGPTAPIKey);
            chatGPTWrapper.Add(chatGPTAPIOrganization);
            chatGPTWrapper.Add(ChatGPTAPIModelWrapper);
            chatGPTWrapper.Add(chatGPTAPIURL);
            chatGPTWrapper.Add(chatGPTMaxTokens);
            chatGPTWrapper.Add(chatGPTShowTokenInfo);
        }

        private void CreateSettingsWrapper()
        {
            autoConnect = new Toggle("Auto Connect");
            autoConnect.tooltip = "Automatically try to connect to the public API when AI Engine opens.";
            confirmTextKeycode = new EnumField("Confirm Prompt Key");
            confirmTextKeycode.tooltip = "Key to confirm the text input.";
            clearText = new Toggle("Clear Textfield");
            clearText.tooltip = "Remove the text after it was send.";
            
            SaveHistoryWrapper = new VisualElement();
            SaveHistoryWrapper.style.flexDirection = FlexDirection.Row;
            saveHistory = new Toggle("Save History");
            saveHistory.tooltip = "Save history of tasks, responses and executed methods.";
            saveHistoryAmount = new IntegerField();
            saveHistoryAmount.tooltip = "Max storage amount before they are overwritten.";
            saveHistoryAmount.style.flexGrow = 1f;
            SaveHistoryWrapper.Add(saveHistory);
            SaveHistoryWrapper.Add(saveHistoryAmount);

            settingsWrapper.Add(autoConnect);
            settingsWrapper.Add(SaveHistoryWrapper);
            settingsWrapper.Add(confirmTextKeycode);
            settingsWrapper.Add(clearText);
        }
        
        private void CreateDevelopersWrapper()
        {
            logMessages = new Toggle("Log Messages");
            logMessages.tooltip = "Send log messages to the console.";
            logErrors = new Toggle("Log Errors");
            logErrors.tooltip = "Send error messages to the console.";
            logConvertedLines = new Toggle("Log Converted Lines");
            logConvertedLines.tooltip = "Send converted lines to the console before they are determined.";
            logLineDetermination = new Toggle("Log Line Determination");
            logLineDetermination.tooltip =
                "(Advanced)\n" +"\n" +
                "Send details of script lines that are being converted.\n" + "\n" +
                "LINE defines the line of the script.\n" +
                "LEVEL defines the nested inner bracket. 0 is the most upper level, 1 is within the first nested brackets, and so on.";
            
            detailsWrapper.Add(logMessages);
            detailsWrapper.Add(logErrors);
            detailsWrapper.Add(logConvertedLines);
            detailsWrapper.Add(logLineDetermination);
        }

        private void CreateResetButtons()
        {
            resetChatGPTSettingsButton = PGEditorWindowUtility.CreateResetButton();
            resetChatGPTSettingsButton.clicked += ResetChatGPTSettingsClicked;
            
            resetSettingsButton = PGEditorWindowUtility.CreateResetButton();
            resetSettingsButton.clicked += ResetSettingsClicked;

            chatGPTWrapper.Add(resetChatGPTSettingsButton);
            detailsWrapper.Add(resetSettingsButton);
        }
        
        private void ResetChatGPTSettingsClicked()
        {
            if (EditorUtility.DisplayDialog("Reset ChatGPT Settings", "Reset the ChatGPT settings to their default values?\n" +
                                                                      "Key and organization remain.", "Ok", "Cancel"))
            {
                chatGPTSettingsSo.ResetValues();
                EditorUtility.SetDirty(chatGPTSettingsSo);
            }
        }
        private void ResetSettingsClicked()
        {
            if (EditorUtility.DisplayDialog("Reset Settings", "Reset settings to their default values?", "Ok", "Cancel"))
            {
                aiEngineSettingsSo.ResetSettings();
                EditorUtility.SetDirty(aiEngineSettingsSo);
            }
        }
        
        private void BindElements()
        {

            chatGPTAPIKeySerializedProperty = chatGPTSettingsSerializedObject.FindProperty(nameof(chatGPTSettingsSo.apiKey));
            chatGPTAPIKey.BindProperty(chatGPTAPIKeySerializedProperty);
            chatGPTAPIModelSerializedProperty = chatGPTSettingsSerializedObject.FindProperty(nameof(chatGPTSettingsSo.apiModel));
            chatGPTAPIModel.BindProperty(chatGPTAPIModelSerializedProperty);
            chatGPTAPIOrganizationSerializedProperty = chatGPTSettingsSerializedObject.FindProperty(nameof(chatGPTSettingsSo.apiOrganization));
            chatGPTAPIOrganization.BindProperty(chatGPTAPIOrganizationSerializedProperty);
            chatGPTAPIURLSerializedProperty = chatGPTSettingsSerializedObject.FindProperty(nameof(chatGPTSettingsSo.apiURL));
            chatGPTAPIURL.BindProperty(chatGPTAPIURLSerializedProperty);
            chatGPTMaxTokensSerializedProperty = chatGPTSettingsSerializedObject.FindProperty(nameof(chatGPTSettingsSo.apiMaxTokens));
            chatGPTMaxTokens.BindProperty(chatGPTMaxTokensSerializedProperty);
            chatGPTShowTokenInfoSerializedProperty = chatGPTSettingsSerializedObject.FindProperty(nameof(chatGPTSettingsSo.showTokenInfo));
            chatGPTShowTokenInfo.BindProperty(chatGPTShowTokenInfoSerializedProperty);
            
            autoConnectSerializedProperty = globalSettingsSerializedObject.FindProperty(nameof(AIEngineSettingsSO.autoConnect));
            autoConnect.BindProperty(autoConnectSerializedProperty);
            confirmTextKeycodeSerializedProperty = globalSettingsSerializedObject.FindProperty(nameof(AIEngineSettingsSO.confirmTextKeycode));
            confirmTextKeycode.BindProperty(confirmTextKeycodeSerializedProperty);
            saveHistorySerializedProperty = globalSettingsSerializedObject.FindProperty(nameof(AIEngineSettingsSO.storeHistory));
            saveHistory.BindProperty(saveHistorySerializedProperty);
            saveHistoryAmountSerializedProperty = globalSettingsSerializedObject.FindProperty(nameof(AIEngineSettingsSO.storeHistoryAmount));
            saveHistoryAmount.BindProperty(saveHistoryAmountSerializedProperty);
            clearTextSerializedProperty = globalSettingsSerializedObject.FindProperty(nameof(AIEngineSettingsSO.clearText));
            clearText.BindProperty(clearTextSerializedProperty);
            
            logMessagesSerializedProperty = globalSettingsSerializedObject.FindProperty(nameof(AIEngineSettingsSO.logMessages));
            logMessages.BindProperty(logMessagesSerializedProperty);
            logErrorsSerializedProperty = globalSettingsSerializedObject.FindProperty(nameof(AIEngineSettingsSO.logErrors));
            logErrors.BindProperty(logErrorsSerializedProperty);
            logConvertedLinesSerializedProperty = globalSettingsSerializedObject.FindProperty(nameof(AIEngineSettingsSO.logConvertedLines));
            logConvertedLines.BindProperty(logConvertedLinesSerializedProperty);
            logLineDeterminationSerializedProperty = globalSettingsSerializedObject.FindProperty(nameof(AIEngineSettingsSO.logLineDetermination));
            logLineDetermination.BindProperty(logLineDeterminationSerializedProperty);
        }
        
        
        /********************************************************************************************************************************/
        /********************************************************************************************************************************/
        
        public void CreateGUI()
        {
            CreateChatGPTSettings();
            
            StoreHistoryVisibility();
            saveHistory.RegisterValueChangedCallback(evt =>
            {
                StoreHistoryVisibility();
            });

        }
        
        private void StoreHistoryVisibility()
        {
            if (aiEngineSettingsSo.storeHistory)
                saveHistoryAmount.style.display = DisplayStyle.Flex;
            else
                saveHistoryAmount.style.display = DisplayStyle.None;
        }
        private void CreateChatGPTSettings()
        {
            chatGPTAPIModelLink.clicked += () =>
            {
                Application.OpenURL(ChatGPTConstants.ChatGPTModelsLink);
            };
            chatGPTMaxTokens.PGClampValue(0);
        }
        
    }
}