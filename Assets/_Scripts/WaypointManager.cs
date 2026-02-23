using UnityEngine;

public class WaypointManager : MonoBehaviour
{
    public Color color = Color.cyan;
    public float size = 0.5f;
    public bool drawPath = true;

    Transform[] points;

    void Start()
    {
        GetPoints();
    }

    void GetPoints()
    {
        points = new Transform[transform.childCount];
        for (int i = 0; i < transform.childCount; i++)
        {
            points[i] = transform.GetChild(i);
        }
    }

    public Transform[] GetWaypoints()
    {
        GetPoints();
        return points;
    }

    void OnDrawGizmos()
    {
        if (!drawPath) return;

        GetPoints();
        if (points == null || points.Length == 0) return;

        Gizmos.color = color;

        for (int i = 0; i < points.Length; i++)
        {
            if (points[i] == null) continue;

            Gizmos.DrawSphere(points[i].position, size);

            if (drawPath)
            {
                int next = (i + 1) % points.Length;
                if (points[next] != null)
                {
                    Gizmos.DrawLine(points[i].position, points[next].position);
                }
            }
        }
    }
}
