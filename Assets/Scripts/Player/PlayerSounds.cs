using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(PlayerController))]
public class PlayerSounds : MonoBehaviour
{
    [Tooltip(
        "The sound that plays when the ball hits something."
    )]
    [SerializeField] AudioSource bounceSound;
    [Tooltip(
        "The sound that plays when the ball is moving quickly."
    )]
    [SerializeField] AudioSource windSound;
    [Tooltip(
        "The sound that plays when the player's score changes."
    )]
    [SerializeField] AudioSource chime;
    float chimePitch = 0.9f;
    float chimeTime = 0;

    new Rigidbody rigidbody;

    // Start is called before the first frame update
    void Start()
    {
        PlayerController.ScoreUpdated += OnScoreUpdated;

        rigidbody = gameObject.GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        windSound.volume = Mathf.Clamp((PlayerController.instance.rigidbody.velocity.magnitude / 450f) - 0.025f, 0f, 1f);
    }

    void OnScoreUpdated(int points, bool silent)
    {
        if (!silent)
        {
            if (Time.time - chimeTime  > 1)
            {
                chimePitch = 0.9f;
            }
            chimeTime = Time.time;
            chime.pitch += 0.05f * points;
            chime.Play();
            if (chimePitch > 2) chimePitch = 2;
        }
    }

    public void OnCollisionEnter(Collision collisionInfo) // used for playing the bounce sound;
    {
        bounceSound.pitch = rigidbody.velocity.magnitude/10f + 0.5f;
        bounceSound.PlayOneShot(bounceSound.clip, Mathf.Clamp(collisionInfo.impulse.magnitude/15 - 0.1f, 0f, 3f));
    }
}
