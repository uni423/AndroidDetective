using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using TMPro;
using System;

public class NetworkingManager : MonoBehaviour
{
    public string IP = "localhost";
    public string Port = "8080";

    public string DefaultURL = "";

    public string qrPageUrl = "/phone/qr?";
    public string talkNPCUrl = "/chat/talk";
    public string galleryGetClueUrl = "/gallery/get-clue";
    public string chatCheckAnswer = "/chat/checkAnswer";

    public void Init()
    {
        DefaultURL = "http://" + IP + ":" + Port;
    }

    public string StartGame(string userId, List<MapRoom> map, List<ClueData> clues,
                      Action<ScenarioResponse> onSuccess = null, Action<string> onError = null)
    {
        GameInfo gameInfo = new GameInfo();
        gameInfo.userId = userId;
        gameInfo.mode = "SETUP";
        gameInfo.map = map;
        gameInfo.clues = clues;

        string postData = JsonUtility.ToJson(gameInfo);
        Debug.Log(postData);
        StartCoroutine(PostStartGameRequest(DefaultURL + "/chat/startGame", postData, onSuccess, onError));
        return "";
    }

    public void PopupPhoneQRWeb(string userId)
    {
        string url = DefaultURL + qrPageUrl + userId;
        Application.OpenURL(url);   // 외부 브라우저에서 바로 열기
    }

    public void SendGetClueInfo(string userId, ClueMeta clue)
    {
        GetClueInfo getClue = new GetClueInfo();
        getClue.userId = userId;
        getClue.clueImgId = clue.clueId;
        getClue.clueName = clue.clueName;

        string postData = JsonUtility.ToJson(getClue);
        Debug.Log(postData);
        StartCoroutine(PostRequest(DefaultURL + galleryGetClueUrl, postData));
    }

    IEnumerator PostRequest(string url, string postData)
    {
        using (UnityWebRequest webRequest = new UnityWebRequest(url, "POST"))
        {
            byte[] jsonToSend = new System.Text.UTF8Encoding().GetBytes(postData);
            webRequest.uploadHandler = new UploadHandlerRaw(jsonToSend);
            webRequest.downloadHandler = new DownloadHandlerBuffer();
            webRequest.SetRequestHeader("Content-Type", "application/json");

            yield return webRequest.SendWebRequest();

#if UNITY_2020_1_OR_NEWER
            if (webRequest.result == UnityWebRequest.Result.ConnectionError ||
                webRequest.result == UnityWebRequest.Result.ProtocolError)
#else
            if (webRequest.isNetworkError || webRequest.isHttpError)
#endif
            {
                Debug.LogError(webRequest.error);
            }
            else
            {
                Debug.Log("Form upload complete!");
                //Result.text = webRequest.downloadHandler.text;
                //Debug.Log(webRequest.downloadHandler.text);
            }
        }
    }

    IEnumerator PostStartGameRequest(string url, string postData, Action<ScenarioResponse> onSuccess, Action<string> onError)
    {
        using (UnityWebRequest webRequest = new UnityWebRequest(url, "POST"))
        {
            byte[] jsonToSend = new System.Text.UTF8Encoding().GetBytes(postData);
            webRequest.uploadHandler = new UploadHandlerRaw(jsonToSend);
            webRequest.downloadHandler = new DownloadHandlerBuffer();
            webRequest.SetRequestHeader("Content-Type", "application/json");

            Debug.Log($"[NetworkingManager] POST {url} : {postData}");

            yield return webRequest.SendWebRequest();

#if UNITY_2020_1_OR_NEWER
            bool isError = webRequest.result == UnityWebRequest.Result.ConnectionError ||
                           webRequest.result == UnityWebRequest.Result.ProtocolError;
#else
        bool isError = webRequest.isNetworkError || webRequest.isHttpError;
#endif

            if (isError)
            {
                Debug.LogError($"[NetworkingManager] Error: {webRequest.error}");
                onError?.Invoke(webRequest.error);
                yield break;
            }

            string responseText = webRequest.downloadHandler.text;
            Debug.Log($"[NetworkingManager] StartGame Response Raw: {responseText}");

            ScenarioResponse scenario = null;
            try
            {
                scenario = JsonUtility.FromJson<ScenarioResponse>(responseText);
            }
            catch (Exception e)
            {
                Debug.LogError($"[NetworkingManager] JSON 파싱 실패: {e.Message}");
                onError?.Invoke("JSON parse error: " + e.Message);
                yield break;
            }

            // 파싱 성공 시 저장 + 콜백 호출
            onSuccess?.Invoke(scenario);
        }
    }

}

[System.Serializable]
public class GameInfo
{
    public string userId;
    public string mode;
    public List<MapRoom> map;
    public List<ClueData> clues;
}

[System.Serializable]
public class GetClueInfo
{
    public string userId;
    public string clueImgId;
    public string clueName;
}