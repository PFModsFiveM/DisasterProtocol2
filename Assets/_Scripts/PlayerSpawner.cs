using UnityEngine;
using Unity.Netcode;
using Unity.Netcode.Components;
using Unity.Netcode.Transports.UTP;
#if UNITY_EDITOR
using UnityEditor;
#endif

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

    void Start()
    {
        if (NetworkManager.Singleton != null && NetworkManager.Singleton.IsServer)
        {
            SpawnNpcs();
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
            spawnPoint.transform.position = new Vector3(i * 5f - 7.5f, 5f, i * 5f - 7.5f);
            spawnPoints[i] = spawnPoint.transform;
        }

        Debug.Log("Created default spawn points at origin area");
    }

    public int npcCount = 2;
    public bool spawnNpcs = true;
    private bool npcsSpawned;

    private void OnClientConnected(ulong clientId)
    {
        Debug.Log($"PlayerSpawner: OnClientConnected({clientId}) called. Server={NetworkManager.Singleton != null && NetworkManager.Singleton.IsServer}");

        if (NetworkManager.Singleton == null || !NetworkManager.Singleton.IsServer)
            return;

        Transform spawnPoint = spawnPoints.Length > 0
            ? spawnPoints[Random.Range(0, spawnPoints.Length)]
            : transform;

        GameObject playerInstance = CreatePlayerObject();
        playerInstance.transform.position = spawnPoint.position + Vector3.up * 0.5f;
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

        if (spawnNpcs && clientId == NetworkManager.Singleton.LocalClientId)
        {
            SpawnNpcs();
        }
    }

    private void SpawnNpcs()
    {
        if (npcsSpawned || !spawnNpcs || npcCount <= 0 || NetworkManager.Singleton == null || !NetworkManager.Singleton.IsServer)
            return;

        npcsSpawned = true;

        for (int i = 0; i < npcCount; i++)
        {
            Transform spawnPoint = spawnPoints.Length > 0
                ? spawnPoints[i % spawnPoints.Length]
                : transform;

            GameObject npc = CreateNpcObject(i);
            npc.transform.position = spawnPoint.position + Vector3.up * 1.1f + Vector3.right * (i * 2f + 2f);

            NetworkObject networkObject = npc.GetComponent<NetworkObject>();
            if (networkObject != null)
            {
                networkObject.Spawn();
            }
        }
    }

    private GameObject CreateNpcObject(int index)
    {
        GameObject npc = new GameObject($"NetworkNPC_{index}");
        npc.tag = "NPC";

        npc.AddComponent<NetworkObject>();
        npc.AddComponent<NetworkTransform>();
        npc.AddComponent<NetworkNpcMover>();

        GameObject body = GameObject.CreatePrimitive(PrimitiveType.Cube);
        body.transform.parent = npc.transform;
        body.transform.localPosition = new Vector3(0f, 0.5f, 0f);
        body.transform.localScale = new Vector3(1f, 1f, 1f);

        Collider cubeCollider = body.GetComponent<Collider>();
        if (cubeCollider != null)
        {
            Object.Destroy(cubeCollider);
        }

        Renderer renderer = body.GetComponent<Renderer>();
        if (renderer != null)
        {
            renderer.material.color = Color.red;
        }

        return npc;
    }

    private GameObject CreatePlayerObject()
    {
        GameObject player = new GameObject("NetworkPlayer");
        player.tag = "Player";

        Rigidbody rb = player.AddComponent<Rigidbody>();
        rb.isKinematic = true;
        rb.useGravity = false;

        CharacterController characterController = player.AddComponent<CharacterController>();
        characterController.height = 2f;
        characterController.radius = 0.5f;
        characterController.center = new Vector3(0, 1f, 0);

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

        var animator = player.GetComponent<Animator>();
        if (animator != null)
        {
#if UNITY_EDITOR
            RuntimeAnimatorController runtimeController = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>("Assets/Character/Player_Animator.controller");
            if (runtimeController != null)
            {
                animator.runtimeAnimatorController = runtimeController;
            }
#endif
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
