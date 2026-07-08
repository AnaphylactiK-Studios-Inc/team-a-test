using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class PlayerMovement : MonoBehaviour
{
    [SerializeField] float maxSpeed = 6f;
    public CharacterController controller;
    public float gravity = -9.81f;
    public float jumpHeight = 3f;
    public Transform groundCheck;
    public float groundDistance = 0.4f;
    public LayerMask groundMask;
    public float alternateTime = 2f;
    public float stopTime = 0.5f;

    Vector3 velocity;
    bool isGrounded;
    public bool moveLocked;
    float slow = 1;
    private Vector3 lastPosition;
    public float stepDistance = 2f;
    private float x;
    private float z;
    private float distanceMoved = 0f;

    [Header("Footsteps")]
    [SerializeField] private AudioSource footstepSource;
    [SerializeField] private AudioClip[] footstepClips;
    private int _lastFootstepIndex = -1;

    private Coroutine movementLoopCoroutine;

    void Start()
    {
        x = 0;
        z = 0;
        lastPosition = transform.position;
        movementLoopCoroutine = StartCoroutine(MovementLoopRoutine());
    }

    // Update is called once per frame
    void Update()
    {
        if (moveLocked)
        {
            if (!isGrounded)
            {
                velocity.y += gravity * Time.unscaledDeltaTime;
                controller.Move(velocity * Time.unscaledDeltaTime);
            }
            return;
        }

        isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);

        // Reset downward velocity when grounded
        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }

        // float x = Input.GetAxisRaw("Horizontal");
        // float z = Input.GetAxisRaw("Vertical");

        Vector3 inputDir = new Vector3(x, 0, z).normalized;

        // Instant constant horizontal speed
        Vector3 horizontalMove = transform.TransformDirection(inputDir) * maxSpeed * slow;

        velocity.y += gravity * Time.unscaledDeltaTime;

        Vector3 finalMove = horizontalMove + Vector3.up * velocity.y;
        controller.Move(finalMove * Time.unscaledDeltaTime);

        if (isGrounded && inputDir.sqrMagnitude > 0f)
        {
            distanceMoved += Vector3.Distance(transform.position, lastPosition);
            if (distanceMoved >= stepDistance)
            {
                distanceMoved = 0f;
                PlayFootstep();
            }
        }

        lastPosition = transform.position;
    }

    private void PlayFootstep()
    {
        if (footstepSource == null || footstepClips == null || footstepClips.Length == 0) return;

        int index;
        if (footstepClips.Length == 1)
        {
            index = 0;
        }
        else
        {
            do { index = UnityEngine.Random.Range(0, footstepClips.Length); }
            while (index == _lastFootstepIndex);
        }

        _lastFootstepIndex = index;
        footstepSource.PlayOneShot(footstepClips[index]);
    }

    public void changeSpeed(float speed)
    {
        slow = speed;
    }

    IEnumerator MovementLoopRoutine()
    {
        yield return null;
        bool left = true;
        while (true)
        {
            left = !left;

            if (left)
            {
                x = -1;
                z = 0;
            }
            else
            {
                x = 1;
                z = 0;
            }
            yield return new WaitForSeconds(alternateTime);

            x = 0;
            z = 0;

            yield return new WaitForSeconds(stopTime);
        }
    }
}
