using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float walkSpeed = 4f;
    [SerializeField] private float runSpeed = 6f;
    [SerializeField] private float jumpHeight = 1.5f;
    [SerializeField] private float gravity = -20f;
    [SerializeField] private float rotationSpeed = 12f;

    [Header("References")]
    [SerializeField] private Transform cameraTransform;
    [SerializeField] private Animator animator;

    private CharacterController controller;
    private float verticalVelocity;
    private static readonly int SpeedHash =
        Animator.StringToHash("Speed");

    private void Awake()
    {
        controller = GetComponent<CharacterController>();

        if (cameraTransform == null && Camera.main != null)
        {
            cameraTransform = Camera.main.transform;
        }

        if (animator == null)
        {
            animator = GetComponentInChildren<Animator>();
        }
    }

    private void Update()
    {
        float dt = Time.deltaTime;

        float x = Input.GetAxisRaw("Horizontal");
        float z = Input.GetAxisRaw("Vertical");

        Vector3 forward = cameraTransform != null
            ? cameraTransform.forward
            : Vector3.forward;

        Vector3 right = cameraTransform != null
            ? cameraTransform.right
            : Vector3.right;

        forward.y = 0f;
        right.y = 0f;

        forward.Normalize();
        right.Normalize();

        Vector3 move =
            Vector3.ClampMagnitude(forward * z + right * x, 1f);

        if (move.sqrMagnitude > 0.001f)
        {
            Quaternion desiredRotation =
                Quaternion.LookRotation(move, Vector3.up);

            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                desiredRotation,
                rotationSpeed * dt
            );
        }

        bool running = Input.GetKey(KeyCode.LeftShift);

        float speed = running ? runSpeed : walkSpeed;

        bool grounded = controller.isGrounded;

        if (grounded && verticalVelocity < 0f)
        {
            verticalVelocity = -2f;
        }

        if (grounded && Input.GetKeyDown(KeyCode.Space))
        {
            verticalVelocity =
                Mathf.Sqrt(jumpHeight * -2f * gravity);
        }

        verticalVelocity += gravity * dt;

        Vector3 velocity =
            move * speed + Vector3.up * verticalVelocity;

        controller.Move(velocity * dt);

        // Update animation based on movement input.
        float animationSpeed = move.magnitude;

        if (animator != null)
        {
            float targetSpeed = animationSpeed <= 0.01f
                ? 0f
                : (running ? 2f : 1f);

            animator.SetFloat(
                SpeedHash,
                targetSpeed,
                0.1f,
                dt
            );
        }
    }
}
