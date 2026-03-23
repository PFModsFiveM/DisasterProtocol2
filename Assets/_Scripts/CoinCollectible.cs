using UnityEngine;

// Attach to every coin GameObject.
// Requirements in Unity:
//   - Coin must have a Collider with "Is Trigger" enabled.
//   - Player must have the tag "Player".
public class CoinCollectible : MonoBehaviour
{
    [Tooltip("Score value awarded when this coin is collected.")]
    public int worth = 1;

    [Tooltip("Optional visual effect spawned on collection.")]
    public GameObject collectEffect;

    [Tooltip("Optional collection sound.")]
    public AudioClip collectSound;

    [Tooltip("When enabled, collector must have this tag.")]
    public bool requirePlayerTag = true;

    public string playerTag = "Player";

    [Range(0f, 1f)]
    public float collectVolume = 1f;

    [Tooltip("Delay before destroy so SFX/FX can start cleanly.")]
    public float destroyDelay = 0.05f;

    [Tooltip("Optional override for what gets destroyed on pickup. Leave empty to destroy this coin object only.")]
    public GameObject destroyTarget;

    private bool collected = false;
    private GameDataManager dataManager;

    private void Start()
    {
        // Register this coin with the manager so totalCoinsInScene is tracked.
        dataManager = ResolveManager();
        if (dataManager != null)
        {
            dataManager.RegisterCoinInScene();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        TryCollect(other.gameObject);
    }

    private void OnTriggerStay(Collider other)
    {
        // Helps in cases where trigger enter is missed due to controller timing.
        TryCollect(other.gameObject);
    }

    private void OnCollisionEnter(Collision other)
    {
        TryCollect(other.gameObject);
    }

    private void TryCollect(GameObject collector)
    {
        if (collected || collector == null)
        {
            return;
        }

        bool validCollector = IsValidCollector(collector);
        if (!validCollector)
        {
            return;
        }

        collected = true;

        DisableCoinColliders();
        HideCoinRenderers();

        if (dataManager != null)
        {
            dataManager.AddCoin(worth);
        }
        else
        {
            dataManager = ResolveManager();
            if (dataManager != null)
            {
                dataManager.AddCoin(worth);
            }
            else
            {
                Debug.LogWarning("CoinCollectible: GameDataManager not found, coin count will not update.", this);
            }
        }

        if (collectEffect != null)
        {
            // Guard against assigning the coin prefab itself as effect, which makes it appear uncollected.
            if (collectEffect.GetComponent<CoinCollectible>() != null)
            {
                Debug.LogWarning("CoinCollectible: Collect Effect points to a coin prefab/script. Clear this field or use a VFX prefab.", this);
            }
            else
            {
                Instantiate(collectEffect, transform.position, Quaternion.identity);
            }
        }

        if (collectSound != null)
        {
            AudioSource.PlayClipAtPoint(collectSound, transform.position, collectVolume);
        }

        GameObject target = destroyTarget != null ? destroyTarget : gameObject;
        Destroy(target, destroyDelay);
    }

    private bool IsValidCollector(GameObject collector)
    {
        Transform root = collector.transform.root;

        if (requirePlayerTag)
        {
            if (collector.CompareTag(playerTag) || root.CompareTag(playerTag))
            {
                return true;
            }
        }

        if (collector.GetComponentInParent<CharacterController>() != null)
        {
            return true;
        }

        if (collector.GetComponentInParent<PlayerMovement>() != null)
        {
            return true;
        }

        return false;
    }

    private GameDataManager ResolveManager()
    {
        if (GameDataManager.Instance != null)
        {
            return GameDataManager.Instance;
        }

        return FindFirstObjectByType<GameDataManager>();
    }

    private void DisableCoinColliders()
    {
        Collider[] colliders = GetComponentsInChildren<Collider>(true);
        for (int i = 0; i < colliders.Length; i++)
        {
            colliders[i].enabled = false;
        }
    }

    private void HideCoinRenderers()
    {
        Renderer[] renderers = GetComponentsInChildren<Renderer>(true);
        for (int i = 0; i < renderers.Length; i++)
        {
            renderers[i].enabled = false;
        }
    }
}
