using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using TMPro;

public class NetworkingManager : MonoBehaviour
{
    public TextMeshProUGUI Result;

    private void Start()
    {
        GameInfo gameInfo = new GameInfo();
        gameInfo.scenarioId = "lyj";

        string postData = JsonUtility.ToJson(gameInfo);

        StartCoroutine(PostRequest("http://localhost:8080/chat/startGame", postData));
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
                Result.text = webRequest.downloadHandler.text;
                //Debug.Log(webRequest.downloadHandler.text);
            }
        }
    }
}

[System.Serializable]
public class GameInfo
{
    public string scenarioId;
    public string playerInput;
    public string npcId;
    public List<string> knownClues = new List<string>();
}