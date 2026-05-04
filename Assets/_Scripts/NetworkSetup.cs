using System;
using UnityEngine;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;

public class NetworkSetup : MonoBehaviour
{
    public string connectAddress = "127.0.0.1";
    public ushort connectPort = 7777;

    void Start()
    {
        if (NetworkManager.Singleton == null)
            return;

        if (NetworkManager.Singleton.IsListening || NetworkManager.Singleton.IsClient || NetworkManager.Singleton.IsServer)
            return;

        if (IsParrelSyncClone())
        {
            var transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
            if (transport != null)
            {
                transport.ConnectionData.Address = connectAddress;
                transport.ConnectionData.Port = connectPort;
            }

            NetworkManager.Singleton.StartClient();
            Debug.Log("Starting Netcode client (Parrel Sync clone detected)");
            return;
        }

        NetworkManager.Singleton.StartHost();
        Debug.Log("Starting Netcode host");
    }

    private bool IsParrelSyncClone()
    {
        string[] typeNames = new[]
        {
            "ParrelSync.ClonesManager, ParrelSync",
            "ParrelSync.CloneManager, ParrelSync",
            "ParrelSync.ClonesManager, com.veriorpies.parrelsync",
            "ParrelSync.CloneManager, com.veriorpies.parrelsync"
        };

        foreach (var typeName in typeNames)
        {
            var cloneManagerType = Type.GetType(typeName);
            if (cloneManagerType == null)
                continue;

            var method = cloneManagerType.GetMethod("IsClone");
            if (method == null)
                continue;

            bool isClone = (bool)method.Invoke(null, null);
            Debug.Log($"Parrel Sync detection using {typeName}: {isClone}");
            return isClone;
        }

        bool fallbackClone = IsParrelSyncCloneByFile();
        Debug.Log($"Parrel Sync file-based fallback detection: {fallbackClone}");
        return fallbackClone;
    }

    private bool IsParrelSyncCloneByFile()
    {
        string projectPath = Application.dataPath.Replace("/Assets", "").Replace("\\Assets", "");
        string cloneFilePath = System.IO.Path.Combine(projectPath, ".clone");
        bool exists = System.IO.File.Exists(cloneFilePath);
        if (exists)
        {
            Debug.Log($"Detected Parrel Sync clone by .clone file at {cloneFilePath}");
        }
        else
        {
            Debug.Log($"No .clone file found at {cloneFilePath}");
        }
        return exists;
    }
}

