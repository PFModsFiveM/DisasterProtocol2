using UnityEngine;
using Unity.Netcode;

public class NetworkSetup : MonoBehaviour
{
    void Awake()
    {
        // Ensure NetworkManager exists
        if (NetworkManager.Singleton == null)
        {
            Debug.LogError("NetworkManager.Singleton is null!");
            return;
        }

        // Start as host if not already started
        if (!NetworkManager.Singleton.IsListening && !NetworkManager.Singleton.IsClient && !NetworkManager.Singleton.IsServer)
        {
            var result = NetworkManager.Singleton.StartHost();
            if (!result)
            {
                Debug.LogError("Failed to start host");
            }
            else
            {
                Debug.Log("Network host started successfully");
            }
        }
    }
}
