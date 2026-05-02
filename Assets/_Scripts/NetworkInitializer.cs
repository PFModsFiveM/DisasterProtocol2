using UnityEngine;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;

public class NetworkInitializer : MonoBehaviour
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void InitializeNetwork()
    {
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
            Debug.Log("NetworkSetup automatically added to NetworkManager");
        }
    }
}