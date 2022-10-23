using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(Light))]
public class Checkpoint : MonoBehaviour
{
    static Checkpoint activeCheckpoint;
    new private Light light;

    Material glowMaterial = null;

    static Color INACTIVE_COLOR = new Color(0f, 0.255435f, 1f, 1f);
    static Color INACTIVE_EMISSION = new Color(0.05087609f, 0.5743476f, 1f, 1f);
    static Color ACTIVE_COLOR = new Color(0.2588235f, 0.8392157f, 0.4425263f, 1f);
    static Color ACTIVE_EMISSION = new Color(0.304651f, 0.735849f, 0f, 1f);
    
    // Start is called before the first frame update
    void Start()
    {
        light = gameObject.GetComponent<Light>();

        MeshRenderer meshRenderer = gameObject.GetComponent<MeshRenderer>();
        if (meshRenderer.materials.Length > 1) glowMaterial = meshRenderer.materials[1];
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnTriggerEnter(Collider other) // Sets this checkpoint as the ball's respawn point, and changes the look of the checkpoint to reflect that
    {
        if (other.gameObject == PlayerController.player.gameObject) 
        {
            PlayerController playerController = other.gameObject.GetComponent<PlayerController>();
            playerController.respawnPoint = gameObject.transform.position + new Vector3(0f, 0.5f, 0f);
            if (activeCheckpoint != null) {activeCheckpoint.Deactivate();}
            activeCheckpoint = this;

            light.color = new Color(0.9971439f, 1f, 0.7686275f);
            if (glowMaterial != null) 
            {
                glowMaterial.color = ACTIVE_COLOR;
                glowMaterial.SetColor("_EmissionColor", ACTIVE_EMISSION);
            }
        }
    }

    void Deactivate()
    {
        light.color = new Color(0.7688679f, 0.9704443f, 1f);
        if (glowMaterial != null) 
        {
            glowMaterial.color = INACTIVE_COLOR;
            glowMaterial.SetColor("_EmissionColor", INACTIVE_EMISSION);
        }
    }
}
