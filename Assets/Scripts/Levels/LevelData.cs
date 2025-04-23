using System.Collections.Generic;

[System.Serializable]
public class LevelData
{
   public string name;
   public int waves;
   public List<SpawnInfo> spawns;
}

[System.Serializable]
public class SpawnInfo
{
   public string enemy;
   public string count;
   public List<int> sequence;
   public string delay;
   public string location;
   public string hp;
   public string speed;
   public string damage;
}
