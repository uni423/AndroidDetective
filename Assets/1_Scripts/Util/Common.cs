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
}

#endregion [ JSON Create Class ]