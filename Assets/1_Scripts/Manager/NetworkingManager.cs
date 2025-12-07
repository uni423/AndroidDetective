using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using TMPro;

public class NetworkingManager : MonoBehaviour
{
    public string IP = "localhost";
    public string Port = "8080";

    public string DefaultURL = "";

    public string qrPageUrl = "phone/qr?";

    public void Init()
    {
        DefaultURL = "http://" + IP + ":" + Port;
    }

    public string StartGame(string userId, string map, string clues)
    {
        GameInfo gameInfo = new GameInfo();
        gameInfo.userId = userId;
        gameInfo.mode = "SETUP";
        gameInfo.map = map;
        gameInfo.clues = clues;

        string postData = JsonUtility.ToJson(gameInfo);

        StartCoroutine(PostRequest(DefaultURL + "/chat/startGame", postData));
        return "";
    }

    public void PopupPhoneQRWeb(string userId)
    {
        StartCoroutine(PostRequest(DefaultURL + qrPageUrl + userId));
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

    IEnumerator PostRequest(string url)
    {
        using (UnityWebRequest webRequest = new UnityWebRequest(url, "POST"))
        {
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
            }
        }
    }
}

[System.Serializable]
public class GameInfo
{
    public string userId;
    public string mode;
    public string map;
    public string clues;
}