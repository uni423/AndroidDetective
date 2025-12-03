using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using TMPro;

public class NetworkingManager : MonoBehaviour
{
    //public TextMeshProUGUI Result;
    public string IP = "localhost";
    public string Port = "8080";

    public string DefaultURL = "";

    private void Start()
    {
        DefaultURL = "http://" + IP + ":" + Port;
    }

    public void StartGame(string userId)
    {
        GameInfo gameInfo = new GameInfo();
        gameInfo.userId = userId;

        string postData = JsonUtility.ToJson(gameInfo);

        // Json Test

        List<Room> Map = new List<Room>();
        Map.Add(new Room
        {
            id = "roomLobby",
            name = "로비",
            description = "방 내부 설명",
            connection = new List<string>
                        {
                            "room_TypeA1",
                            "room_TypeB3"
                        }
        });

        Map.Add(new Room
        {
            id = "room_TypeA1",
            name = "1번 방",
            description = "방 내부 설명",
            connection = new List<string>
                        {
                            "roomLobby",
                        }
        });

        Map.Add(new Room
        {
            id = "room_TypeB3",
            name = "2번 방",
            description = "방 내부 설명",
            connection = new List<string>
                        {
                            "roomLobby",
                        }
        });

        string postData2 = JsonUtility.ToJson(Map);

        List<Clue> Clues = new List<Clue>();

        Clues.Add(new Clue
        {
            id = "clueLog2157",
            name = "단서명A",
            description = "단서 설명"
        });

        Clues.Add(new Clue
        {
            id = "clueLog4568",
            name = "단서명B",
            description = "단서 설명"
        });

        string postData3 = JsonUtility.ToJson(Clues);


        StartCoroutine(PostRequest(DefaultURL + "/chat/startGame", postData));
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
}

[System.Serializable]
public class GameInfo
{
    public string userId;
    public string mode;
}

public class Room
{
    public string id;
    public string name;
    public string description;
    public List<string> connection;
}

public class Clue
{
    public string id;
    public string name;
    public string description;
}