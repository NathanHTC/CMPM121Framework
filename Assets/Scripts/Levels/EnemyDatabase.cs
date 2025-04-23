using UnityEngine;
using System.Collections.Generic;
using Newtonsoft.Json;

public class EnemyDatabase
{
   public static Dictionary<string, EnemyData> Enemies;

   public static void LoadEnemies()
   {
      TextAsset enemyText = Resources.Load<TextAsset>("enemies");
      var list = JsonConvert.DeserializeObject<List<EnemyData>>(enemyText.text);
      Enemies = new Dictionary<string, EnemyData>();
      foreach (var enemy in list)
      {
         Enemies[enemy.name] = enemy;
      }
   }
}
