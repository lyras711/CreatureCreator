// ----------------------------------------------------
// AI Engine
// Copyright (c) Pampel Games e.K. All Rights Reserved.
// https://www.pampelgames.com
// ----------------------------------------------------

using System;
using System.Text;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Networking;

namespace PampelGames.AIEngine.Editor
{
    public static class ChatGPTRequest
    {
        public static void Request(UnityWebRequest request, string promptString, ChatGPTSettingsSO chatGPTSettingsSo, Action<string, int> Callback,
            float temperature, Action CallbackFail)
        {
            var url = chatGPTSettingsSo.apiURL;
            var apiKey = chatGPTSettingsSo.apiKey;

            request.url = url;
            request.method = "POST";

            var requestParams = JsonConvert.SerializeObject(new ChatGPTRequestJSON
            {
                model = chatGPTSettingsSo.apiModel,
                messages = new ChatGPTPromptMessageJSON[]
                {
                    new()
                    {
                        role = "user",
                        content = promptString
                    }
                },
                temperature = temperature,
                max_tokens = chatGPTSettingsSo.apiMaxTokens
            });

            var bodyRaw = Encoding.UTF8.GetBytes(requestParams);

            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.disposeDownloadHandlerOnDispose = true;
            request.disposeUploadHandlerOnDispose = true;
            request.disposeCertificateHandlerOnDispose = true;

            request.SetRequestHeader("Content-Type", "application/json");

            // required to authenticate against OpenAI
            request.SetRequestHeader("Authorization", $"Bearer {apiKey}");
            request.SetRequestHeader("OpenAI-Organization", chatGPTSettingsSo.apiOrganization);

            request.SendWebRequest().completed += operation =>
            {
                ProcessRequest(request, out var messageConverted, out var total_tokens);
                if (!CheckValidRequest(request)) CallbackFail.Invoke();
                else Callback(messageConverted, total_tokens);
            };
        }

        /********************************************************************************************************************************/

        private static bool CheckValidRequest(UnityWebRequest request)
        {
            if (request.result == UnityWebRequest.Result.ConnectionError)
            {
                Debug.Log(request.downloadHandler.text);
                Debug.Log(request.error);
                return false;
            }

            if (request.result == UnityWebRequest.Result.DataProcessingError)
            {
                Debug.Log(request.downloadHandler.text);
                Debug.Log(request.error);
                return false;
            }

            if (request.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.Log(request.downloadHandler.text);
                Debug.Log(request.error);
                return false;
            }

            return true;
        }

        private static void ProcessRequest(UnityWebRequest request, out string responseString, out int totalTokens)
        {
            var responseJson = request.downloadHandler.text;

            var chatCompletion = JsonUtility.FromJson<ChatGPTResponseJSON>(responseJson);
            if (chatCompletion == null || chatCompletion.usage == null || chatCompletion.choices == null)
            {
                responseString = "";
                totalTokens = 0;
                return;
            }

            // Extract usage values as integers
            var promptTokens = chatCompletion.usage.prompt_tokens;
            var completionTokens = chatCompletion.usage.completion_tokens;
            totalTokens = chatCompletion.usage.total_tokens;

            // Extract message as string
            responseString = chatCompletion.choices[0].message.content;
        }
    }
}