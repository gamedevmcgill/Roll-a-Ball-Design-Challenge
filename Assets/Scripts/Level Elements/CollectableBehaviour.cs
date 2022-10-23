using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollectableBehaviour : MonoBehaviour
{
    [Tooltip(
        "How many points the player gains after picking up the collectable."
    )]
    public int value = 1;

    void OnTriggerEnter(Collider other) // causes the collectable to be collected when the player touches it
    {
        if (other.gameObject.CompareTag("Player")) 
        {
            PlayerController playerController = other.gameObject.GetComponent<PlayerController>();
            playerController.UpdateScore(value);

            gameObject.SetActive(false);
        } 
    }
}
