using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeadBob : MonoBehaviour
{
    public GameObject player;

    [Range(0, 1f)]
    public float idleAmplitude;

    [Range(0, 30)]
    public float idleFrequency;

    [Range(0, 1f)]
    public float walkAmplitude;

    [Range(0, 30)]
    public float walkFrequency;

    [Range(0, 2f)]
    public float sprintAmplitude;

    [Range(0, 30)]
    public float sprintFrequency;

    [Range(0, 1f)]
    public float crouchAmplitude;

    [Range(0, 30)]
    public float crouchFrequency;
    public float stablizedOffset;

    PlayerMovement playerMovement;
    Transform cameraHolder;
    List<Transform> cameras = new List<Transform>();
    Vector3 startPosition;
    float amplitude;
    float frequency;

    void Start()
    {
        cameraHolder = transform;
        foreach (Transform camera in cameraHolder)
        {
            cameras.Add(camera);
        }

        startPosition = cameras[0].localPosition;

        playerMovement = player.GetComponent<PlayerMovement>();
    }

    void Update()
    {
        PlayerMovement.MovementState movementState = playerMovement.GetMovementState();

        if (playerMovement.GetMoveVelocity().magnitude <= 0.3f)
        {
            amplitude = idleAmplitude;
            frequency = idleFrequency;
        }
        else
        {
            // Change amplitude and frequency depending on movement state
            switch (movementState)
            {
                case PlayerMovement.MovementState.SPRINT:
                    amplitude = sprintAmplitude;
                    frequency = sprintFrequency;
                    break;
                case PlayerMovement.MovementState.CROUCH:
                    amplitude = crouchAmplitude;
                    frequency = crouchFrequency;
                    break;
                case PlayerMovement.MovementState.WALK:
                default:
                    amplitude = walkAmplitude;
                    frequency = walkFrequency;
                    break;
            }
        }

        foreach (Transform camera in cameras)
        {
            PlayMotion(camera);
            ResetPosition(camera);
        }
    }

    void PlayMotion(Transform camera)
    {
        // Head bob only if is not in air
        if (playerMovement.movementState == PlayerMovement.MovementState.AIR)
            return;

        // Offset camera position by head bob
        camera.localPosition += FootStepMotion() * Time.deltaTime;
    }

    Vector3 FootStepMotion()
    {
        Vector3 pos = Vector3.zero;

        // Oscillate using sine and cosine curves
        pos.y += Mathf.Sin(Time.time * frequency) * amplitude;
        pos.x += Mathf.Cos(Time.time * frequency / 2) * amplitude / 2;

        return pos;
    }

    void ResetPosition(Transform camera)
    {
        if (camera.localPosition == startPosition)
            return;

        // Slowly move back to start position
        camera.localPosition = Vector3.Lerp(
            camera.localPosition,
            startPosition,
            1 * Time.deltaTime
        );
    }

    Vector3 StablizedTarget(Transform camera)
    {
        // Focuses at a position in front
        Vector3 pos = new Vector3(
            player.transform.position.x,
            player.transform.position.y + cameraHolder.localPosition.y,
            player.transform.position.z
        );
        pos += camera.forward * stablizedOffset;

        return pos;
    }
}
