using System.Collections.Generic;

public enum Unit_Type
{
    Rabbit_Normal,
    Rabbit_Baby,
    Rabbit_Strong,
    Rabbit_Evolve,
    Rabbit_BulkUp,
}

public enum GameStep
{
    Loading, Menu, Playing, End, Continue, Pause,
}

public enum InGameStep
{
    QRConnectWait,
    CreateGameLoading, 
    StartGame,
    Playing, 
}

public enum AttackType
{
    Normal, 

}

#region [ JSON Create Class ]

[System.Serializable]
public class MapExport
{
    public List<MapRoom> map;
}

[System.Serializable]
public class MapRoom
{
    public string id;
    public string name;
    public string description;
    public List<MapRoomConnection> connection;
}

[System.Serializable]
public class MapRoomConnection
{
    public string id;
    public string name;
    public string description;
}

[System.Serializable]
public class ScenarioResponse
{
    public string title;
    public string background;
    public Suspect[] suspects;
    public PublicView publicView;
    // map, clues 는 현재 null 이라서 일단 생략 (JSON에 있어도 자동으로 무시됨)
}

[System.Serializable]
public class Suspect
{
    public string id;
    public string name;
    public int age;
    public string job;
    public string role;
    public Relationship[] relationships;
    public Alibi alibi;
}

[System.Serializable]
public class Relationship
{
    public string with;
    public string type;
    public string note;
}

[System.Serializable]
public class Alibi
{
    public string summary;
    public string detail;
}

[System.Serializable]
public class PublicView
{
    public string overview;
    public SuspectSummary[] suspectSummaries;
}

[System.Serializable]
public class SuspectSummary
{
    public string npcId;
    public string text;
}

#endregion [ JSON Create Class ]