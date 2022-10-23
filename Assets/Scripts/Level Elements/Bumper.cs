using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider))]
[RequireComponent(typeof(AudioSource))]
public class Bumper : MonoBehaviour
{
    [Tooltip(
        "How much force the bumper imparts on the player."
    )]
    [SerializeField] float forceAmount = 20f;

    AudioSource bounceSound;
    Animator animator;

    bool playingAnimation = false;
    float ANIMATION_LENGTH = 0.2f;
    float animationTimer;
    Vector3 initialScale;

    // Start is called before the first frame update
    void Start()
    {
        bounceSound = gameObject.GetComponent<AudioSource>();
        
        initialScale = gameObject.transform.localScale;
    }

    // Update is called once per frame
    void Update()
    {
        if (playingAnimation == true)
        {
            animationTimer -= Time.deltaTime;

            if (animationTimer > 0) gameObject.transform.localScale = (1 + 0.05f * Mathf.Sin(2f * Mathf.PI * animationTimer / (ANIMATION_LENGTH))) * initialScale;
            else
            {
                transform.localScale = initialScale;
                playingAnimation = false;
            }
        }
    }



    private void OnCollisionEnter(Collision collision) 
    {
        PlayerController playerController = collision.gameObject.GetComponent<PlayerController>();
        if (playerController != null) 
        {
            Vector3 force = forceAmount * - collision.contacts[0].normal;
            force.y = Mathf.Clamp(force.y, 5f, 5f);
            playerController.rigidbody.AddForce(force, ForceMode.Impulse);
            bounceSound.Play();

            playingAnimation = true;
            animationTimer = ANIMATION_LENGTH;
        }
    }
}
