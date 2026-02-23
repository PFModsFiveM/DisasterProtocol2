using UnityEngine;
using System.Collections;

public class Scene2_Script1 : MonoBehaviour
{
	// Reference to NavMeshAgent component
	UnityEngine.AI.NavMeshAgent navAgent;
	Vector3 destination;

	// Decision Tree control booleans 
	public bool isVisible;
	public bool isAudible;
	public bool isClose;

	// Also to the game object we will follow
	public Transform targetObject;

	// Waypoints for Patrol functionality
	int nextIndex;
	public GameObject[] waypoints;

	void Start ()
	{
		// Get the component reference
		navAgent = GetComponent<UnityEngine.AI.NavMeshAgent> ();

		// Patrol Scene
		destination = NextWaypoint(Vector3.zero);
	}

	// This is where we can create our Decision Tree
	void Update ()
	{
		// Check for visibility and proximity
		if (isVisible && isClose) {
			// If Randomball is visible and is close, then SEEK
			seekFunction ();
		} else if (isVisible && !isClose) {
			// If Randomball is visible and not close, then PATROL
			patrolFunction ();
		} else if (!isVisible && !isAudible) {
			// If Randomball is not visible and not audable, then PATROL
			IdleFunction ();
		} else if(!isVisible && isAudible ){
			// If Randomball is visible and not close, then PATROL
			patrolFunction ();
		}
		navAgent.SetDestination (destination);
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
	// Seek out RandomBall
	void seekFunction(){
		// Seek out the Enemy
		destination = targetObject.position;
	}
	// Patrol Array of cubes (added to script)
	void patrolFunction(){
		// If within 2.5, then move onto next waypoint in array
		if (Vector3.Distance (transform.position, destination) < 2.5) {
			destination = NextWaypoint (destination);
		}
	}
	// Idle at (0,0,0)
	void IdleFunction(){
		// Idle at 0
		destination = Vector3.zero;
	}

	// Function that loops through waypoints for the Patrol fucntionality
	public Vector3 NextWaypoint (Vector3 currentPosition)
	{
		Debug.Log (currentPosition);
		if (currentPosition != Vector3.zero) {
			// Find array index of given waypoint
			for (int i = 0; i < waypoints.Length; i++) {
				// Once found calculate next one
				if (currentPosition == waypoints [i].transform.position) {
					// Modulus operator helps to avoid to go out of bounds
					// And resets to 0 the index count once we reach the end of the array
					nextIndex = (i + 1) % waypoints.Length;
				}
			}
		} else {
			// Default is first index in array 
			nextIndex = 0;
		}
		return waypoints [nextIndex].transform.position	;
	}
}
