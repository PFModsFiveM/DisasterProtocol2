using UnityEngine;
using TMPro;

public class CoinCounter : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI label;

    [SerializeField] private string displayFormat = "Coins: {0}/{1}";
    [SerializeField] private bool showScore = true;
    [SerializeField] private string scoreFormat = "  Score: {0}";

    private GameDataManager dataManager;

    private void Start()
    {
        if (label == null)
        {
            label = GetComponent<TextMeshProUGUI>();
        }

        dataManager = ResolveManager();
        if (dataManager == null)
        {
            Debug.LogWarning("CoinCounter: No GameDataManager found in scene.", this);
            if (label != null)
            {
                label.text = "Coins: manager missing";
            }
            return;
        }

        Refresh(force: true);
    }

    private void Update()
    {
        if (dataManager == null)
        {
            dataManager = ResolveManager();
            if (dataManager != null)
            {
                Refresh(force: true);
            }
            else if (label != null)
            {
                label.text = "Coins: manager missing";
            }
            return;
        }

        Refresh(force: true);
    }

    private void Refresh(bool force)
    {
        if (label == null)
        {
            label = GetComponent<TextMeshProUGUI>();
        }

        if (label == null)
        {
            return;
        }

        int collected = dataManager.GetCoinsCollected();
        int total = dataManager.GetTotalCoins();
        int score = dataManager.GetCurrentScore();

        string text = string.Format(displayFormat, collected, total);

        if (showScore)
        {
            text += string.Format(scoreFormat, score);
        }

        label.text = text;
    }

    private GameDataManager ResolveManager()
    {
        if (GameDataManager.Instance != null)
        {
            return GameDataManager.Instance;
        }

        return FindFirstObjectByType<GameDataManager>();
    }
}
