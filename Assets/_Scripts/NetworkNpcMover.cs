using Unity.Netcode;
using UnityEngine;

public class NetworkNpcMover : NetworkBehaviour
{
    public float radius = 3f;
    public float speed = 0.8f;

    private Vector3 startPosition;

    public override void OnNetworkSpawn()
    {
        if (!IsServer)
            return;

        startPosition = transform.position;
    }

    void Update()
    {
        if (!IsServer)
            return;

        float angle = Time.time * speed * 50f;
        Vector3 offset = new Vector3(Mathf.Sin(angle), 0f, Mathf.Cos(angle)) * radius;
        transform.position = startPosition + offset;
    }
}
