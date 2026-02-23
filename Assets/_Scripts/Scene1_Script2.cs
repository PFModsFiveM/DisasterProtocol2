using UnityEngine;
using System.Collections;

public class Scene1_Script2 : MonoBehaviour
{
	// Update is called once per frame
	void FixedUpdate ()
	{
		// Test the velocity of the collider and only move it when it is slowing down
		Vector3 ballSpeed = GetComponent<Rigidbody> ().linearVelocity;
		if (ballSpeed.magnitude < 4.5) {
			// Take code from lecture 6 for random direction switching
			int myDirection = Random.Range (0, 4);
			Vector3 myDirectionVector = new Vector3 ();
			switch (myDirection) {
			case 0:
				myDirectionVector = Vector3.forward;
				break; // Now break the switch
			case 1:
				myDirectionVector = Vector3.back;
				break;
			case 2:
				myDirectionVector = Vector3.left;
				break;
			default:
				myDirectionVector = Vector3.right;
				break;
			}
			// Add force to the sphere to move it at random, use velocityChange
			GetComponent<Rigidbody> ().AddForce (myDirectionVector * 5, ForceMode.VelocityChange);
		}
	}
}
