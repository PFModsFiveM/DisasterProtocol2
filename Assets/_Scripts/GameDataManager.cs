using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameDataManager : MonoBehaviour
{
    public static GameDataManager Instance { get; private set; }

    [Header("Tracked Scene References")]
    [SerializeField] private Transform playerTransform;
    [SerializeField] private Transform npcTransform;
    [SerializeField] private NPCController npcController;
    [SerializeField] private Transform[] environmentObjects;

    [Header("Scriptable Object Storage")]
    [SerializeField] private GameProfileAsset gameProfile;

    [Header("Runtime Gameplay")]
    [SerializeField] private int currentScore;
    [SerializeField] private int coinsCollected;
    [SerializeField] private int totalCoinsInScene;
    [SerializeField] private float playerHealth = 100f;
    [SerializeField] private float elapsedTime;
    [SerializeField] private bool loadFromDiskOnStart = true;

    private string gameplayPath;
    private string statisticsPath;
    private string environmentPath;

    private GameplayData gameplayData;
    private StatisticalData statisticalData;
    private EnvironmentData environmentData;

    private bool hasGameplayData;
    private bool hasEnvironmentData;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning("Duplicate GameDataManager found. Destroying duplicate instance.", this);
            Destroy(gameObject);
            return;
        }

        Instance = this;

        gameplayPath = Path.Combine(Application.persistentDataPath, "gameplay_data.json");
        statisticsPath = Path.Combine(Application.persistentDataPath, "statistical_data.json");
        environmentPath = Path.Combine(Application.persistentDataPath, "environment_data.json");

        if (gameProfile != null)
        {
            currentScore = gameProfile.defaultStartScore;
            playerHealth = gameProfile.defaultPlayerHealth;
        }

        if (loadFromDiskOnStart)
        {
            LoadAllData();
        }
    }

    private void Update()
    {
        elapsedTime += Time.deltaTime;
    }

    public void AddScore(int amount)
    {
        currentScore += amount;
    }

    public void AddCoin(int worth = 1)
    {
        coinsCollected += 1;
        currentScore += worth;
    }

    public void RegisterCoinInScene()
    {
        totalCoinsInScene += 1;
    }

    public int GetCoinsCollected() => coinsCollected;
    public int GetTotalCoins() => totalCoinsInScene;
    public int GetCurrentScore() => currentScore;

    public void SetPlayerHealth(float value)
    {
        playerHealth = Mathf.Max(0f, value);
    }

    public void SaveAllData()
    {
        SaveGameplayData();
        SaveStatisticalData();
        SaveEnvironmentData();
        SaveToScriptableAsset();
    }

    public void LoadAllData()
    {
        LoadGameplayData();
        LoadStatisticalData();
        LoadEnvironmentData();
        ApplyLoadedData();
        SaveToScriptableAsset();
    }

    private void SaveGameplayData()
    {
        gameplayData = new GameplayData
        {
            sceneName = SceneManager.GetActiveScene().name,
            savedAtUtc = DateTime.UtcNow.ToString("o"),
            playerPosition = playerTransform != null ? new Vector3Data(playerTransform.position) : default,
            playerHealth = playerHealth,
            npcPosition = npcTransform != null ? new Vector3Data(npcTransform.position) : default,
            npcState = npcController != null ? npcController.state.ToString() : "Unknown",
            currentScore = currentScore,
            coinsCollected = coinsCollected,
            totalCoinsInScene = totalCoinsInScene,
            elapsedTime = elapsedTime
        };

        WriteJson(gameplayPath, gameplayData);
    }

    private void SaveStatisticalData()
    {
        statisticalData.previousScore = currentScore;
        statisticalData.highScore = Mathf.Max(statisticalData.highScore, currentScore);

        if (statisticalData.sessionCount <= 0)
        {
            statisticalData.sessionCount = 1;
        }

        bool betterTime = statisticalData.bestTime <= 0f || elapsedTime < statisticalData.bestTime;
        if (betterTime)
        {
            statisticalData.bestTime = elapsedTime;
        }

        statisticalData.savedAtUtc = DateTime.UtcNow.ToString("o");

        WriteJson(statisticsPath, statisticalData);
    }

    private void SaveEnvironmentData()
    {
        environmentData = new EnvironmentData
        {
            sceneName = SceneManager.GetActiveScene().name,
            savedAtUtc = DateTime.UtcNow.ToString("o"),
            sceneObjects = new List<TransformData>()
        };

        if (environmentObjects != null)
        {
            for (int i = 0; i < environmentObjects.Length; i++)
            {
                Transform item = environmentObjects[i];
                if (item == null)
                {
                    continue;
                }

                environmentData.sceneObjects.Add(new TransformData
                {
                    objectName = item.name,
                    position = new Vector3Data(item.position),
                    rotation = new QuaternionData(item.rotation),
                    scale = new Vector3Data(item.localScale)
                });
            }
        }

        WriteJson(environmentPath, environmentData);
    }

    private void LoadGameplayData()
    {
        hasGameplayData = false;

        if (!File.Exists(gameplayPath))
        {
            return;
        }

        string json = File.ReadAllText(gameplayPath);
        gameplayData = JsonUtility.FromJson<GameplayData>(json);

        if (string.IsNullOrWhiteSpace(gameplayData.sceneName))
        {
            return;
        }

        string activeScene = SceneManager.GetActiveScene().name;
        if (!string.Equals(gameplayData.sceneName, activeScene, StringComparison.Ordinal))
        {
            // Ignore positional data from other scenes.
            return;
        }

        hasGameplayData = true;
        currentScore = gameplayData.currentScore;
        coinsCollected = gameplayData.coinsCollected;
        playerHealth = gameplayData.playerHealth;
        elapsedTime = gameplayData.elapsedTime;
    }

    private void LoadStatisticalData()
    {
        if (!File.Exists(statisticsPath))
        {
            statisticalData = new StatisticalData
            {
                highScore = gameProfile != null ? gameProfile.highScore : 0,
                previousScore = gameProfile != null ? gameProfile.previousScore : 0,
                sessionCount = 0,
                bestTime = 0f
            };
            return;
        }

        string json = File.ReadAllText(statisticsPath);
        statisticalData = JsonUtility.FromJson<StatisticalData>(json);
        statisticalData.sessionCount += 1;
    }

    private void LoadEnvironmentData()
    {
        hasEnvironmentData = false;

        if (!File.Exists(environmentPath))
        {
            return;
        }

        string json = File.ReadAllText(environmentPath);
        environmentData = JsonUtility.FromJson<EnvironmentData>(json);

        if (string.IsNullOrWhiteSpace(environmentData.sceneName))
        {
            return;
        }

        string activeScene = SceneManager.GetActiveScene().name;
        if (!string.Equals(environmentData.sceneName, activeScene, StringComparison.Ordinal))
        {
            return;
        }

        hasEnvironmentData = true;
    }

    private void ApplyLoadedData()
    {
        if (hasGameplayData)
        {
            if (playerTransform != null)
            {
                Vector3 loadedPlayerPos = gameplayData.playerPosition.ToVector3();
                if (IsValidVector(loadedPlayerPos))
                {
                    playerTransform.position = loadedPlayerPos;
                }
            }

            if (npcTransform != null)
            {
                Vector3 loadedNpcPos = gameplayData.npcPosition.ToVector3();
                if (IsValidVector(loadedNpcPos))
                {
                    npcTransform.position = loadedNpcPos;
                }
            }
        }

        if (!hasEnvironmentData || environmentObjects == null || environmentData.sceneObjects == null)
        {
            return;
        }

        for (int i = 0; i < environmentData.sceneObjects.Count; i++)
        {
            TransformData data = environmentData.sceneObjects[i];
            Transform obj = FindEnvironmentObject(data.objectName);
            if (obj == null)
            {
                continue;
            }

            obj.position = data.position.ToVector3();
            obj.rotation = data.rotation.ToQuaternion();
            obj.localScale = data.scale.ToVector3();
        }
    }

    private bool IsValidVector(Vector3 value)
    {
        return !(float.IsNaN(value.x) || float.IsNaN(value.y) || float.IsNaN(value.z)
            || float.IsInfinity(value.x) || float.IsInfinity(value.y) || float.IsInfinity(value.z));
    }

    private Transform FindEnvironmentObject(string objectName)
    {
        for (int i = 0; i < environmentObjects.Length; i++)
        {
            Transform item = environmentObjects[i];
            if (item != null && item.name == objectName)
            {
                return item;
            }
        }

        return null;
    }

    private void WriteJson<T>(string filePath, T data)
    {
        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(filePath, json);
    }

    private void SaveToScriptableAsset()
    {
        if (gameProfile == null)
        {
            return;
        }

        gameProfile.highScore = statisticalData.highScore;
        gameProfile.previousScore = statisticalData.previousScore;
        gameProfile.lastGameplayJson = JsonUtility.ToJson(gameplayData, true);
        gameProfile.lastStatisticsJson = JsonUtility.ToJson(statisticalData, true);
        gameProfile.lastEnvironmentJson = JsonUtility.ToJson(environmentData, true);

#if UNITY_EDITOR
        UnityEditor.EditorUtility.SetDirty(gameProfile);
        UnityEditor.AssetDatabase.SaveAssets();
#endif
    }

    private void OnApplicationPause(bool pauseStatus)
    {
        if (pauseStatus)
        {
            SaveAllData();
        }
    }

    private void OnApplicationFocus(bool hasFocus)
    {
        if (!hasFocus)
        {
            SaveAllData();
        }
    }

    private void OnApplicationQuit()
    {
        SaveAllData();
    }
}
