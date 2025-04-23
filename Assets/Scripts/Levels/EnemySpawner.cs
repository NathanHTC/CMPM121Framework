using UnityEngine;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.IO;
using System.Collections.Generic;
using UnityEngine.UI;
using System.Collections;
using System.Linq;
using TMPro;

public class EnemySpawner : MonoBehaviour
{
    public Image level_selector;
    public GameObject button;
    public GameObject enemy;
    public GameObject gameOverPanel;
    public TMP_Text gameOverMessage;
    public Button returnButton;
    public SpawnPoint[] SpawnPoints;
    private LevelData currentLevel;
    private int currentWave = 1;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        EnemyDatabase.LoadEnemies();
        LevelDatabase.LoadLevels();

        float yPos = 130;
        foreach (var level in LevelDatabase.Levels)
        {
            GameObject selector = Instantiate(button, level_selector.transform);
            selector.transform.localPosition = new Vector3(0, yPos);
            selector.GetComponent<MenuSelectorController>().spawner = this;
            selector.GetComponent<MenuSelectorController>().SetLevel(level.name);
            yPos -= 50; // space buttons vertically
        }
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void StartLevel(string levelname)
    {
        level_selector.gameObject.SetActive(false);
        // this is not nice: we should not have to be required to tell the player directly that the level is starting
        GameManager.Instance.player.GetComponent<PlayerController>().StartLevel();
        currentLevel = LevelDatabase.GetLevelByName(levelname);
        currentWave = 1;
        StartCoroutine(SpawnWave());
    }

    public void NextWave()
    {
        StartCoroutine(SpawnWave());
    }


    IEnumerator SpawnWave()
    {
        GameManager.Instance.state = GameManager.GameState.COUNTDOWN;
        GameManager.Instance.countdown = 3;
        for (int i = 3; i > 0; i--)
        {
            yield return new WaitForSeconds(1);
            GameManager.Instance.countdown--;
        }

        GameManager.Instance.state = GameManager.GameState.INWAVE;

        foreach (var spawn in currentLevel.spawns)
        {
            CoroutineManager.Instance.Run(SpawnEnemyGroup(spawn));
        }

        yield return new WaitWhile(() => GameManager.Instance.enemy_count > 0);
        GameManager.Instance.state = GameManager.GameState.WAVEEND;

        // Simple placeholder UI
        Debug.Log($"Wave {currentWave} completed. Click to continue.");
    }

    IEnumerator SpawnZombie()
    {
        SpawnPoint spawn_point = SpawnPoints[Random.Range(0, SpawnPoints.Length)];
        Vector2 offset = Random.insideUnitCircle * 1.8f;

        Vector3 initial_position = spawn_point.transform.position + new Vector3(offset.x, offset.y, 0);
        GameObject new_enemy = Instantiate(enemy, initial_position, Quaternion.identity);

        new_enemy.GetComponent<SpriteRenderer>().sprite = GameManager.Instance.enemySpriteManager.Get(0);
        EnemyController en = new_enemy.GetComponent<EnemyController>();
        en.hp = new Hittable(50, Hittable.Team.MONSTERS, new_enemy);
        en.speed = 10;
        GameManager.Instance.AddEnemy(new_enemy);
        yield return new WaitForSeconds(0.5f);
    }
    IEnumerator SpawnEnemyGroup(SpawnInfo spawn)
    {
        var enemyData = EnemyDatabase.Enemies[spawn.enemy];
        var vars = new Dictionary<string, int> {
        {"base", enemyData.hp},
        {"wave", currentWave}
    };

        int total = RPNEvaluator.Evaluate(spawn.count ?? "1", vars);
        int hp = RPNEvaluator.Evaluate(spawn.hp ?? "base", vars);
        int speed = RPNEvaluator.Evaluate(spawn.speed ?? "base", new() { { "base", enemyData.speed }, { "wave", currentWave } });
        int damage = RPNEvaluator.Evaluate(spawn.damage ?? "base", new() { { "base", enemyData.damage }, { "wave", currentWave } });

        List<int> sequence = spawn.sequence ?? new List<int> { 1 };
        int delay = int.Parse(spawn.delay ?? "2");

        int spawned = 0;
        int seqIndex = 0;

        while (spawned < total)
        {
            int batchCount = Mathf.Min(sequence[seqIndex % sequence.Count], total - spawned);
            for (int i = 0; i < batchCount; i++)
            {
                SpawnEnemy(spawn.location, enemyData.sprite, hp, speed, damage);
                spawned++;
            }
            seqIndex++;
            yield return new WaitForSeconds(delay);
        }
    }
    void SpawnEnemy(string location, int spriteIndex, int hp, int speed, int damage)
    {
        SpawnPoint[] points = SpawnPoints;
        if (!string.IsNullOrEmpty(location) && location != "random")
        {
            string kind = location.Replace("random ", "").ToUpper();
            points = System.Array.FindAll(SpawnPoints, sp => sp.kind.ToString() == kind);
        }

        var spawnPoint = points[Random.Range(0, points.Length)];
        Vector2 offset = Random.insideUnitCircle * 1.8f;
        Vector3 pos = spawnPoint.transform.position + new Vector3(offset.x, offset.y, 0);

        GameObject new_enemy = Instantiate(enemy, pos, Quaternion.identity);
        new_enemy.GetComponent<SpriteRenderer>().sprite = GameManager.Instance.enemySpriteManager.Get(spriteIndex);

        EnemyController ec = new_enemy.GetComponent<EnemyController>();
        ec.hp = new Hittable(hp, Hittable.Team.MONSTERS, new_enemy);
        ec.speed = speed;

        GameManager.Instance.AddEnemy(new_enemy);
    }
    public void ShowGameOver(string message)
    {
        gameOverPanel.SetActive(true);
        gameOverMessage.text = message;

        returnButton.onClick.RemoveAllListeners();
        returnButton.onClick.AddListener(() =>
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene("Main");
        });
    }

}
