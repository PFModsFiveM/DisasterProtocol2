using UnityEngine;

// Attach to coin objects for a classic arcade spin.
public class CoinSpin : MonoBehaviour
{
    [Tooltip("Degrees per second.")]
    [SerializeField] private float spinSpeed = 180f;

    [Tooltip("Spin around local X axis for a sideways coin spin.")]
    [SerializeField] private bool useSidewaysSpin = true;

    [Tooltip("Optional bob amount in world Y.")]
    [SerializeField] private float bobAmount = 0f;

    [SerializeField] private float bobSpeed = 2f;

    private Vector3 startPosition;

    private void Awake()
    {
        startPosition = transform.position;
    }

    private void Update()
    {
        Vector3 axis = useSidewaysSpin ? Vector3.right : Vector3.up;
        transform.Rotate(axis, spinSpeed * Time.deltaTime, Space.Self);

        if (bobAmount > 0f)
        {
            float y = Mathf.Sin(Time.time * bobSpeed) * bobAmount;
            transform.position = new Vector3(startPosition.x, startPosition.y + y, startPosition.z);
        }
    }
}
