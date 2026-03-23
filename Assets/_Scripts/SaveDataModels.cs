using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public struct Vector3Data
{
    public float x;
    public float y;
    public float z;

    public Vector3Data(Vector3 value)
    {
        x = value.x;
        y = value.y;
        z = value.z;
    }

    public Vector3 ToVector3()
    {
        return new Vector3(x, y, z);
    }
}

[Serializable]
public struct QuaternionData
{
    public float x;
    public float y;
    public float z;
    public float w;

    public QuaternionData(Quaternion value)
    {
        x = value.x;
        y = value.y;
        z = value.z;
        w = value.w;
    }

    public Quaternion ToQuaternion()
    {
        return new Quaternion(x, y, z, w);
    }
}

[Serializable]
public struct TransformData
{
    public string objectName;
    public Vector3Data position;
    public QuaternionData rotation;
    public Vector3Data scale;
}

[Serializable]
public struct GameplayData
{
    public string sceneName;
    public string savedAtUtc;
    public Vector3Data playerPosition;
    public float playerHealth;
    public Vector3Data npcPosition;
    public string npcState;
    public int currentScore;
    public int coinsCollected;
    public int totalCoinsInScene;
    public float elapsedTime;
}

[Serializable]
public struct StatisticalData
{
    public string savedAtUtc;
    public int highScore;
    public int previousScore;
    public int sessionCount;
    public float bestTime;
}

[Serializable]
public struct EnvironmentData
{
    public string sceneName;
    public string savedAtUtc;
    public List<TransformData> sceneObjects;
}
