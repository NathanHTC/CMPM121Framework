using UnityEngine;
using Newtonsoft.Json;
using System.Collections.Generic;

public class LevelDatabase
{
   public static List<LevelData> Levels;

   public static void LoadLevels()
   {
      TextAsset levelText = Resources.Load<TextAsset>("levels");
      Levels = JsonConvert.DeserializeObject<List<LevelData>>(levelText.text);
   }

   public static LevelData GetLevelByName(string name)
   {
      return Levels.Find(level => level.name == name);
   }
}
