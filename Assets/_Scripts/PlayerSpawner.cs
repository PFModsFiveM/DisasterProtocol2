using UnityEngine;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;

public class PlayerSpawner : MonoBehaviour
{
    [SerializeField] private Transform[] spawnPoints;

    void Awake()
    {
        EnsureNetworkManager();

        if (spawnPoints == null || spawnPoints.Length == 0)
        {
            CreateDefaultSpawnPoints();
        }

        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
        }
    }

    private void EnsureNetworkManager()
    {
        if (NetworkManager.Singleton != null)
            return;

        GameObject networkManagerObj = GameObject.Find("NetworkManager");
        if (networkManagerObj == null)
        {
            networkManagerObj = new GameObject("NetworkManager");
        }

        NetworkManager networkManager = networkManagerObj.GetComponent<NetworkManager>();
        if (networkManager == null)
        {
            networkManager = networkManagerObj.AddComponent<NetworkManager>();
        }

        if (networkManagerObj.GetComponent<UnityTransport>() == null)
        {
            networkManagerObj.AddComponent<UnityTransport>();
        }

        if (networkManagerObj.GetComponent<NetworkSetup>() == null)
        {
            networkManagerObj.AddComponent<NetworkSetup>();
        }
    }

    private void CreateDefaultSpawnPoints()
    {
        spawnPoints = new Transform[4];

        for (int i = 0; i < 4; i++)
        {
            GameObject spawnPoint = new GameObject($"SpawnPoint_{i}");
            spawnPoint.transform.parent = transform;
            spawnPoint.transform.position = new Vector3(i * 5f, 0f, 0f);
            spawnPoints[i] = spawnPoint.transform;
        }

        Debug.Log("Created default spawn points");
    }

    private void OnClientConnected(ulong clientId)
    {
        if (NetworkManager.Singleton == null || !NetworkManager.Singleton.IsServer)
            return;

        Transform spawnPoint = spawnPoints.Length > 0
            ? spawnPoints[Random.Range(0, spawnPoints.Length)]
            : transform;

        GameObject playerInstance = CreatePlayerObject();
        playerInstance.transform.position = spawnPoint.position + Vector3.up * 1.1f;
        playerInstance.transform.rotation = spawnPoint.rotation;

        NetworkObject networkObject = playerInstance.GetComponent<NetworkObject>();
        if (networkObject != null)
        {
            networkObject.SpawnAsPlayerObject(clientId);
            Debug.Log($"Spawned player for client {clientId} at {spawnPoint.position}");
        }
        else
        {
            Debug.LogError("NetworkObject component missing on player!");
        }
    }

    private GameObject CreatePlayerObject()
    {
        GameObject player = new GameObject("NetworkPlayer");
        player.tag = "Player";

        CharacterController controller = player.AddComponent<CharacterController>();
        controller.height = 2f;
        controller.radius = 0.5f;
        controller.center = new Vector3(0, 1f, 0);

        player.AddComponent<Animator>();

        PlayerMovement movement = player.AddComponent<PlayerMovement>();
        movement.maximumSpeed = 5f;
        movement.runSpeedMultiplier = 1.6f;
        movement.rotationSpeed = 10f;
        movement.jumpSpeed = 5f;
        movement.jumpButtonGracePeriod = 0.2f;
        movement.mouseSensitivity = 2f;
        movement.minPitch = -45f;
        movement.maxPitch = 75f;

        player.AddComponent<StateMachine>();
        player.AddComponent<NetworkObject>();
        player.AddComponent<ClientAuthoritativeNetworkTransform>();
        player.AddComponent<ClientAuthoritativeNetworkAnimator>();
        player.AddComponent<PlayerNetworkController>();

        GameObject capsule = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        capsule.transform.parent = player.transform;
        capsule.transform.localPosition = new Vector3(0, 1f, 0);
        capsule.transform.localScale = new Vector3(1f, 2f, 1f);

        // Remove the extra collider so it doesn't fight the player controller.
        Collider capsuleCollider = capsule.GetComponent<Collider>();
        if (capsuleCollider != null)
        {
            Object.Destroy(capsuleCollider);
        }

        Renderer renderer = capsule.GetComponent<Renderer>();
        if (renderer != null)
        {
            renderer.material.color = Random.ColorHSV(0f, 1f, 1f, 1f, 0.5f, 1f);
        }

        return player;
    }

    void OnDestroy()
    {
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
        }
    }
}
