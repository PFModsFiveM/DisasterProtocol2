using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class Scene3_Script1 : MonoBehaviour
{

    Animator anim;
    CharacterController characterController;

    public float moveSpeed = 5.0f;
    public float gravity = -9.81f;
    public float turnSpeed = 12.0f;

    public Vector2 velocity = Vector2.zero;
    public bool shouldMove;

    float verticalVelocity;
    // Start is called before the first frame update
    void Start()
    {

        anim = GetComponent<Animator>();
        characterController = GetComponent<CharacterController>();

        // Root motion can overwrite CharacterController movement.
        if (anim != null)
        {
            anim.applyRootMotion = false;
        }
        
    }

    // Update is called once per frame
    void Update()
    {
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");

        if (Mathf.Approximately(horizontal, 0f) && Mathf.Approximately(vertical, 0f))
        {
            if (Input.GetKey(KeyCode.A)) horizontal -= 1f;
            if (Input.GetKey(KeyCode.D)) horizontal += 1f;
            if (Input.GetKey(KeyCode.S)) vertical -= 1f;
            if (Input.GetKey(KeyCode.W)) vertical += 1f;
        }

        velocity.x = horizontal;
        velocity.y = vertical;

        Vector3 moveDir = new Vector3(horizontal, 0f, vertical);
        moveDir = Vector3.ClampMagnitude(moveDir, 1f);

        if (characterController.isGrounded && verticalVelocity < 0f)
        {
            verticalVelocity = -1f;
        }

        verticalVelocity += gravity * Time.deltaTime;

        Vector3 finalMove = (moveDir * moveSpeed) + (Vector3.up * verticalVelocity);
        characterController.Move(finalMove * Time.deltaTime);

        if (moveDir.sqrMagnitude > 0.001f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(moveDir, Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, turnSpeed * Time.deltaTime);
        }

        shouldMove = velocity.sqrMagnitude > Mathf.Epsilon;

        if (anim != null)
        {
            anim.SetBool("move", shouldMove);
            anim.SetFloat("velx", velocity.x);
            anim.SetFloat("vely", velocity.y);
        }
    }
}
