using UnityEngine;

[CreateAssetMenu(fileName = "GameProfile", menuName = "DisasterProtocol/Game Profile", order = 1)]
public class GameProfileAsset : ScriptableObject
{
    [Header("Profile")]
    public string profileName = "Default Profile";

    [Header("Default Runtime Values")]
    public int defaultStartScore = 0;
    public float defaultPlayerHealth = 100f;

    [Header("Stored Statistics")]
    public int highScore;
    public int previousScore;

    [Header("Latest JSON Snapshots")]
    [TextArea(2, 10)]
    public string lastGameplayJson;

    [TextArea(2, 10)]
    public string lastStatisticsJson;

    [TextArea(2, 10)]
    public string lastEnvironmentJson;
}
