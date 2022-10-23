using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(MeshFilter))]
public class MovingPlatform : MonoBehaviour
{
    new Rigidbody rigidbody;

    [Tooltip(
        "How smooth the platform's movement should be. 0 is completely linear, and 1 is completely smooth."
    )]
    [SerializeField] [Range(0f, 1f)] float smoothing = 1;

    Vector3 startPosition;
    Vector3 endPosition = Vector3.zero;
    [Tooltip(
        "How far the end of the movement should be from the beginning. For example, a platform at (1, 2, 3) with a movement value of (4, 5, 6) would have a final position of (1+4, 2+5, 3+6) = (5, 7, 9)."
    )]
    [SerializeField] Vector3 movement = Vector3.zero;

    Quaternion startRotationQuaternion;
    [Tooltip(
        "The final rotation of the platform. When it reaches the end of its movement, it will have this rotation."
    )]
    [SerializeField] Vector3 endRotation = Vector3.zero;
    Quaternion endRotationQuaternion;

    float counter = 0;
    [Tooltip(
        "Time in seconds it takes the platform to reach the end of its movement, after which it will return to its original position."
    )]
    [SerializeField] float moveDuration = 1;
    float moveDurationFrames;
    float phase = 0f;

    void Start()
    {
        rigidbody = gameObject.GetComponent<Rigidbody>();

        startPosition = gameObject.transform.position;
        endPosition = startPosition + movement;

        moveDurationFrames = moveDuration / Time.fixedDeltaTime;

        startRotationQuaternion = transform.rotation;
        endRotationQuaternion = Quaternion.Euler(endRotation);
    }

    

    void FixedUpdate()
    {
        counter += 1;
        phase = 1f - ((1f - smoothing) * (2f * Mathf.Abs(((counter / moveDurationFrames) % 1f) - 0.5f)) + smoothing * (0.5f * Mathf.Cos(2f * Mathf.PI * counter / moveDurationFrames) + 0.5f));

        rigidbody.MovePosition(Vector3.Lerp(startPosition, endPosition, phase));
        rigidbody.MoveRotation(Quaternion.Slerp(startRotationQuaternion, endRotationQuaternion, phase));
    }

    // recalculates certain values when serialized variables are modified
    private void OnValidate() 
    {
        endPosition = startPosition + movement;

        moveDurationFrames = moveDuration / Time.fixedDeltaTime;

        while (endRotation.x >= 360) endRotation.x -= 360;
        while (endRotation.x < 0) endRotation.x += 360;
        while (endRotation.y >= 360) endRotation.y -= 360;
        while (endRotation.y < 0) endRotation.y += 360;
        while (endRotation.z >= 360) endRotation.z -= 360;
        while (endRotation.z < 0) endRotation.z += 360;
        endRotationQuaternion = Quaternion.Euler(endRotation);
    }

    void OnDrawGizmos()
    {
        Vector3 drawStartPosition = Application.isPlaying? startPosition: transform.position;
        Quaternion drawStartRotation = Application.isPlaying? startRotationQuaternion: transform.rotation;
        Gizmos.color = new Color(0f, 1f, 0f, 0.4f);
        Gizmos.DrawLine(drawStartPosition, drawStartPosition + movement);
        Gizmos.DrawMesh(gameObject.GetComponent<MeshFilter>().sharedMesh, drawStartPosition, drawStartRotation, transform.lossyScale);
        Gizmos.DrawMesh(gameObject.GetComponent<MeshFilter>().sharedMesh, drawStartPosition + movement, endRotationQuaternion, transform.lossyScale);
    }
}
