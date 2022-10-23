using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LaunchPad : MonoBehaviour
{
    [Tooltip(
        "The force imparted on the player when it touches the launchpad."
    )]
    [SerializeField] Vector3 force = new Vector3(0f, 10f, 0f);
    Vector3 forceDir;

    float phase;
    Vector2 baseTextureScale;
    Material launchPadMaterial;
    Gradient emissionGradient = new Gradient();

    void Start()
    {
        forceDir = force.normalized;

        launchPadMaterial = gameObject.GetComponent<MeshRenderer>().material;
        baseTextureScale = new Vector2(gameObject.transform.localScale.x / 4, gameObject.transform.localScale.z / 4);
        launchPadMaterial.mainTextureScale = baseTextureScale;
        launchPadMaterial.mainTextureOffset = -baseTextureScale;

        GradientColorKey colorKey1 = new GradientColorKey(new Color(0.06124607f, 0.1412633f, 1f, 1f), 0f);
        GradientColorKey colorKey2 = new GradientColorKey(new Color(0.799103f, 1f, 1f, 1f), 1f);
        emissionGradient.colorKeys = new GradientColorKey[] {colorKey1, colorKey2};
    }

    void Update()
    {
        phase = Mathf.Abs(Mathf.Sin(Time.time));
        launchPadMaterial.SetColor("_EmissionColor", emissionGradient.Evaluate(phase));
        launchPadMaterial.mainTextureScale = Vector2.Lerp(baseTextureScale, baseTextureScale / (1 + forceDir.y), phase);
        launchPadMaterial.mainTextureOffset = Vector2.Lerp(Vector2.zero, new Vector2(forceDir.x, forceDir.z), phase) - launchPadMaterial.mainTextureScale/2;
    }
    
    void OnTriggerEnter(Collider other) // causes the ball to go faster when it touches the booster
    {
        PlayerController playerController = other.gameObject.GetComponent<PlayerController>();
        if (playerController != null) 
        {
            playerController.rigidbody.AddForce(gameObject.transform.rotation * force, ForceMode.Impulse);
        }
    }

    private void OnValidate() 
    {
        if (force.x > 1000f) force.x = 1000f;
        if (force.x < -1000f) force.x = -1000f;
        if (force.y > 1000f) force.y = 1000f;
        if (force.y < -1000f) force.y = -1000f;
        if (force.z > 1000f) force.z = 1000f; 
        if (force.z < -1000f) force.z = -1000f;

        forceDir = force.normalized;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawLine(transform.position, transform.position + 5 * (transform.rotation * forceDir));
    }
}
