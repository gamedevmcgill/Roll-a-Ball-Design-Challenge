using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cannon : MonoBehaviour
{
    [Tooltip(
        "Which object the canon fires. These objects should always have a Rigidbody attached."
    )]
    [SerializeField] GameObject projectile;
    // the last projectile instance fires
    GameObject latestProjectile;
    [Tooltip(
        "How much force the projectile is launched with."
    )]
    [SerializeField] float force = 100f;
    [Tooltip(
        "How far from the cannon the projectile is initially created. Greater values mean further forward."
    )]
    [SerializeField] float offset = 0f;

    [Tooltip(
        "How many seconds pass between shots."
    )]
    [SerializeField] float reloadTime = 1.5f;
    [Tooltip(
        "Offsets the time before the first shot. A canon with a Start Phase of 1 fires 1 second before one with a Start Phase of 0. This can be used to stagger shots from canon while still keeping them synchonized."
    )]
    [SerializeField] float startPhase = 0f;
    
    float time = 0;

    void Start()
    {
        time = startPhase;
    }

    void FixedUpdate()
    {
        time += Time.deltaTime;
        if (time >= reloadTime){
            latestProjectile = GameObject.Instantiate(projectile, transform.position + transform.up * offset, new Quaternion());
            if (latestProjectile.GetComponent<Rigidbody>() == null) latestProjectile.AddComponent<Rigidbody>();
            if (latestProjectile.GetComponent<Cannonball>() == null) latestProjectile.AddComponent<Cannonball>();
            latestProjectile.GetComponent<Rigidbody>().AddForce(transform.up * force, ForceMode.Impulse);
            time -= reloadTime;
        }
    }
}
