using System.Collections;
using System.Collections.Generic;

using UnityEngine;

public class ThirdPersonCamera : MonoBehaviour
{
    [SerializeField] private Transform target;

    [Header("Camera Position")]
    [SerializeField] private Vector3 targetOffset =
        new Vector3(0f, 1.4f, 0f);

    [SerializeField] private float distance = 6f;
    [SerializeField] private float minDistance = 2.5f;
    [SerializeField] private float maxDistance = 10f;
    [SerializeField] private float zoomSpeed = 3f;

    [Header("Mouse Control")]
    [SerializeField] private float sensitivity = 3f;
    [SerializeField] private float initialPitch = 15f;
    [SerializeField] private float minPitch = -15f;
    [SerializeField] private float maxPitch = 65f;

    [Header("Smoothing")]
    [SerializeField] private float smoothSpeed = 12f;

    [Header("Camera Collision")]
    [SerializeField] private LayerMask collisionMask = ~0;
    [SerializeField] private float collisionRadius = 0.25f;
    [SerializeField] private float collisionPadding = 0.15f;

    private float yaw;
    private float pitch;
    private bool cursorLocked;

    // Reusable buffer avoids allocating a new hit array every frame.
    private readonly RaycastHit[] collisionHits =
        new RaycastHit[16];

    private void Start()
    {
        if (target == null)
        {
            Debug.LogError(
                "ThirdPersonCamera: Assign the Player target.",
                this
            );

            enabled = false;
            return;
        }

        yaw = target.eulerAngles.y;
        pitch = initialPitch;

        SetCursorLocked(true);
        UpdateCameraPosition(true);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            SetCursorLocked(!cursorLocked);
        }
        else if (!cursorLocked &&
                 Input.GetMouseButtonDown(0))
        {
            SetCursorLocked(true);
        }

        if (cursorLocked)
        {
            yaw += Input.GetAxis("Mouse X") * sensitivity;

            pitch -= Input.GetAxis("Mouse Y") * sensitivity;
            pitch = Mathf.Clamp(pitch, minPitch, maxPitch);
        }

        distance -=
            Input.GetAxis("Mouse ScrollWheel") * zoomSpeed;

        distance = Mathf.Clamp(
            distance,
            minDistance,
            maxDistance
        );
    }

    private void LateUpdate()
    {
        if (target != null)
        {
            UpdateCameraPosition(false);
        }
    }

    private void UpdateCameraPosition(bool snap)
    {
        Vector3 pivot = target.position + targetOffset;
        Quaternion rotation = Quaternion.Euler(pitch, yaw, 0f);

        Vector3 direction = rotation * Vector3.back;
        float safeDistance = distance;
        bool obstructed = false;

        int hitCount = Physics.SphereCastNonAlloc(
            pivot,
            collisionRadius,
            direction,
            collisionHits,
            distance,
            collisionMask,
            QueryTriggerInteraction.Ignore
        );

        for (int i = 0; i < hitCount; i++)
        {
            RaycastHit hit = collisionHits[i];

            if (hit.collider == null)
                continue;

            Transform hitTransform = hit.collider.transform;

            // Ignore the player and any colliders under the player.
            if (hitTransform == target ||
                hitTransform.IsChildOf(target))
            {
                continue;
            }

            if (hit.distance < safeDistance)
            {
                safeDistance = Mathf.Max(
                    0.05f,
                    hit.distance - collisionPadding
                );

                obstructed = true;
            }
        }

        Vector3 desiredPosition =
            pivot + direction * safeDistance;

        // Move immediately around an obstruction to avoid clipping.
        if (snap || obstructed)
        {
            transform.SetPositionAndRotation(
                desiredPosition,
                rotation
            );

            return;
        }

        float blend =
            1f - Mathf.Exp(-smoothSpeed * Time.deltaTime);

        transform.position = Vector3.Lerp(
            transform.position,
            desiredPosition,
            blend
        );

        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            rotation,
            blend
        );
    }

    private void SetCursorLocked(bool locked)
    {
        cursorLocked = locked;

        Cursor.lockState = locked
            ? CursorLockMode.Locked
            : CursorLockMode.None;

        Cursor.visible = !locked;
    }

    private void OnDisable()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
}
