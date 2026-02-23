using UnityEngine;
using System.Collections;

public class Scene1_Script1 : MonoBehaviour
{
	// Reference to NavMeshAgent component
	UnityEngine.AI.NavMeshAgent navAgent;
	Vector3 destination;

	// Also to the game object we will follow
	public Transform targetObject;

	void Start ()
	{
		// Get the component reference
		navAgent = GetComponent<UnityEngine.AI.NavMeshAgent> ();
	}

	void Update ()
	{
		// Get the position of the target and move the agent to it
		if (targetObject != null) {
			destination = targetObject.position;
		} else {
			destination = Vector3.zero;
		}

		// if (navAgent.isOnNavMesh)
		// {
    	// 	navAgent.SetDestination(destination);
		// }

		navAgent.SetDestination(destination);
	}

	void OnCollisionEnter (Collision col)
	{
		if (col.gameObject.name == "PlayerBall") {
			// Stop the chase by destroying the yellow ball
			Destroy (col.gameObject);
		} else if (col.gameObject.name == "Cube") {
			// Change the colour of the object
			col.gameObject.GetComponent<Renderer> ().material.color = Color.green;
		}
	}
}
