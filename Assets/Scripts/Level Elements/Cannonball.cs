using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cannonball : MonoBehaviour
{
    float age = 0;
    [Tooltip(
        "The amount of time, in seconds, that the cannonball persists for."
    )]
    [SerializeField] int lifetime = 5;
    [Tooltip(
        "The sound that plays when the cannonball hits something."
    )]
    [SerializeField] AudioSource bounceSound;

    // Start is called before the first frame update
    void Start()
    {
        age = 0;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        age += Time.deltaTime;
        if (age >= lifetime){
            transform.localScale *= 0.99f; // the ball shrinks slowly instead of popping out of existence; weird but less jarring than some alternatives
            if (transform.localScale.magnitude <= 0.01f){
                GameObject.Destroy(gameObject);
            }
        }
    }

    void OnCollisionEnter(Collision collisionInfo){
        bounceSound?.Play();
    }
}
