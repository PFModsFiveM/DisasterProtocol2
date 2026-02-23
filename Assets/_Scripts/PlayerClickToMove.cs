using UnityEngine;
using UnityEngine.AI;

public class PlayerClickToMove : MonoBehaviour
{
    NavMeshAgent agent;
    Animator anim;
    public Camera cam;
    public LayerMask groundMask;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        anim = GetComponent<Animator>();
        
        if (cam == null)
            cam = Camera.main;
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = cam.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit))
            {
                agent.SetDestination(hit.point);
            }
        }

        if (anim != null)
        {
            float speed = agent.velocity.magnitude;
            anim.SetFloat("Speed", speed);
        }
    }
}
