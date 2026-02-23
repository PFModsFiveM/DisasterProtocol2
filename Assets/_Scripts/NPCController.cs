using UnityEngine;
using UnityEngine.AI;

public class NPCController : MonoBehaviour
{
    NavMeshAgent agent;
    Animator anim;
    public Transform target;
    public Transform[] waypoints;
    int waypointIndex = 0;

    public float visionRange = 15f;
    public float fovAngle = 90f;
    public bool canSee = false;

    public float hearRange = 10f;
    public bool canHear = false;

    public float closeRange = 3f;
    public bool isClose = false;

    public NPCState state = NPCState.Patrol;

    public float fleeDist = 20f;
    public float healthThresh = 30f;
    public float health = 100f;

    Vector3 fleeDest;

    public enum NPCState
    {
        Idle,
        Patrol,
        Seek,
        Flee
    }

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        anim = GetComponent<Animator>();
        
        if (waypoints.Length > 0)
            state = NPCState.Patrol;
        else
            state = NPCState.Idle;
    }

    void Update()
    {
        CheckPerception();
        DecideState();
        DoAction();
        UpdateAnimation();
    }

    void CheckPerception()
    {
        if (target == null)
        {
            canSee = false;
            canHear = false;
            isClose = false;
            return;
        }

        float dist = Vector3.Distance(transform.position, target.position);

        canSee = CheckVis(dist);
        canHear = dist <= hearRange;
        isClose = dist <= closeRange;
    }

    bool CheckVis(float dist)
    {
        if (dist > visionRange) return false;

        Vector3 dir = (target.position - transform.position).normalized;
        float angle = Vector3.Angle(transform.forward, dir);

        if (angle > fovAngle) return false;

        RaycastHit hit;
        if (Physics.Raycast(transform.position + Vector3.up, dir, out hit, visionRange))
        {
            return hit.transform == target || hit.transform.IsChildOf(target);
        }

        return false;
    }

    void DecideState()
    {
        if (health < healthThresh)
        {
            state = NPCState.Flee;
            return;
        }

        if (canSee && isClose)
        {
            state = NPCState.Seek;
            return;
        }

        if (canSee || canHear)
        {
            state = NPCState.Seek;
            return;
        }

        if (waypoints.Length > 0)
        {
            state = NPCState.Patrol;
            return;
        }

        state = NPCState.Idle;
    }

    void DoAction()
    {
        switch (state)
        {
            case NPCState.Idle:
                DoIdle();
                break;
            case NPCState.Patrol:
                DoPatrol();
                break;
            case NPCState.Seek:
                DoSeek();
                break;
            case NPCState.Flee:
                DoFlee();
                break;
        }
    }

    void DoIdle()
    {
        agent.ResetPath();
    }

    void DoPatrol()
    {
        if (waypoints.Length == 0) return;

        Transform wp = waypoints[waypointIndex];
        agent.SetDestination(wp.position);

        if (agent.remainingDistance <= agent.stoppingDistance + 0.5f)
        {
            waypointIndex = (waypointIndex + 1) % waypoints.Length;
        }
    }

    void DoSeek()
    {
        if (target == null) return;
        agent.SetDestination(target.position);
    }

    void DoFlee()
    {
        if (target == null)
        {
            DoIdle();
            return;
        }

        Vector3 runDir = (transform.position - target.position).normalized;
        fleeDest = transform.position + runDir * fleeDist;

        NavMeshHit hit;
        if (NavMesh.SamplePosition(fleeDest, out hit, fleeDist, NavMesh.AllAreas))
        {
            agent.SetDestination(hit.position);
        }
    }

    void UpdateAnimation()
    {
        if (anim != null)
        {
            float speed = agent.velocity.magnitude;
            anim.SetFloat("Speed", speed);
        }
    }

    void OnCollisionEnter(Collision col)
    {
        if (col.gameObject.CompareTag("Player"))
        {
            Debug.Log("Got player");
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, visionRange);

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, hearRange);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, closeRange);
    }
}
