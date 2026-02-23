using UnityEngine;
using System.Collections;

public class Scene3_Script2: MonoBehaviour
{

	// Class that deals with moving the Teddy Character using the Navmesh Agent and Root Motion. Instead of taking values from the Keyboard, the NavMesh Agent is used to set the animator parameters. In the Start Function, we set the agents updatePosition property to false. This
	// means that the position of the NavMesgAgent component is decoupled from the position of the character, the 2 positions can move independently. In each videoframe, the agent calculates its next position
	// but does not automatically move the character there. Instead we ask the agent what its next position would be and compare that to our current position to create a worldDeltaPosition which is how
	// the AI agent thinks the character should move in 3D. We then take the Dot Products to obtain the components in the forward directions and sideways direction and call that groundDeltaPosition. We divide that by
	// the time duration of the frame to get the velocity the character should move and if that is greater than a small number, and we have not yet arrived at the destination, we set the move parameter to true and set the
	// velx and vely animator parameters. Its important that we decouple the agents position from the characters position so that the character movement is driven by the animation clip and his feet dont slide on the ground and
	// The movement looks natural. There is an issue though that the characters position and the agent components position are not in lock step and will drift apart by a small amount in each video frame. To correct for this,
	// we have an event handler that is called when the animator moves to tweak its position to match the agents position.

	// Reference to NavMeshAgent component
	UnityEngine.AI.NavMeshAgent navAgent;
	Vector3 destination;

	//Controlling Navmesh and Animation
	Vector3 worldDeltaPosition;
	Vector2 groundDeltaPosition;
	Vector2 velocity = Vector2.zero;

	// Animator Object
	Animator anim;

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
		// Get Animator
		anim = GetComponent<Animator>();
		// Get NavAgent
		navAgent = GetComponent<UnityEngine.AI.NavMeshAgent> ();

		// Stop agent navigating automatically by setting the agents updatePosition property to false.
		// This means that the position of the NavMesgAgent component is decoupled from the position of
		// the character, i.e the 2 positions can move independently. In each videoframe, the agent
		// calculates its next position but does not automatically move the character there.
		navAgent.updatePosition = false;

		// Patrol Scene
		destination = NextWaypoint(Vector3.zero);
	}

	// This is where we can create our Decision Tree. This stays as normal
	// as we still have to calculate what the destination is as this is
	// used by the NavMesh Agent to calculate the next move 
	void Update ()
	{

        // Check for visibility and proximity
        if (isVisible && isClose)
        {
            // If Randomball is visible and is close, then SEEK
            seekFunction();
        }
        else if (isVisible && !isClose)
        {
            // If Randomball is visible and not close, then PATROL
            patrolFunction();
        }
        else if (!isVisible && !isAudible)
        {
            // If Randomball is not visible and not audable, then PATROL
            IdleFunction();
        }
        else if (!isVisible && isAudible)
        {
            // If Randomball is visible and not close, then PATROL
            patrolFunction();
        }

		// Set the final destination to whatever has been determined by
		// decision tree functions above. The NavMesh Agent now calculates
		// the next move but does not move becauase updatePosition is false
		navAgent.SetDestination(destination);

		// Now we determine what the NavMesh Agents next move would be but dont move.
		// Instead we ask the agent what its next position would be and compare that
		// to our current position to create a worldDeltaPosition which is how
		// the AI agent thinks the character should move in 3D.
		worldDeltaPosition = navAgent.nextPosition - transform.position;
		// We then take the Dot Products to obtain the components in the forward directions
		// and sideways direction and call that groundDeltaPosition.
		groundDeltaPosition.x = Vector3.Dot(transform.right, worldDeltaPosition);
		groundDeltaPosition.y = Vector3.Dot(transform.forward, worldDeltaPosition);
		// We divide that by the time duration of the frame to get the velocity the character should move
		// and if that is greater than a small number, i.e 1 to the power -5 = 0.00001 (C# Ternary Operator Usage)
		velocity = (Time.deltaTime > 1e-5f) ? groundDeltaPosition / Time.deltaTime : velocity = Vector2.zero;
		// And we have not yet arrived at the destination....,
		bool shouldMove = velocity.magnitude > 0.025f && navAgent.remainingDistance > navAgent.radius;
		// We set the move parameter to true and set the velx and vely animator parameters. 
		anim.SetBool("move", shouldMove);
		anim.SetFloat("velx", velocity.x);
		anim.SetFloat("vely", velocity.y);
	}

	// It is important that we decouple the agents position from the characters position so that the
	// character movement is driven by the animation clip and his feet dont slide on the ground and
	// the movement looks natural. There is an issue though that the characters position and the agent
	// components position are not in lock step and will drift apart by a small amount in each video
	// frame. To correct for this, we have an event handler that is called when the animator moves to
	// tweak its position to match the agents position.
	void OnAnimatorMove() {

		transform.position = navAgent.nextPosition;
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
		if (targetObject)
        {
			destination = targetObject.position;
        }
        else
        {
			IdleFunction();
		}
		
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
