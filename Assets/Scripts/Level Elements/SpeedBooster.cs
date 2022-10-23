using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpeedBooster : MonoBehaviour
{
    // linked list node holding a reference to this booster
    private LinkedListNode<SpeedBooster> _asNode;
    // public getter for _asNode
    public LinkedListNode<SpeedBooster> asNode {get => _asNode;}

    [Tooltip(
        "How much the booster speeds up the player. A booster with a boost factor of 2 causes the speed of the player to double, 3 causes it to triple, and so on."
    )]
    [SerializeField][Range(1f + float.Epsilon, 10f)] public float boostFactor = 5f;
    
    Material boosterMaterial;
    Gradient emissionGradient = new Gradient();

    float xOffset;

    void Start()
    {
        _asNode = new LinkedListNode<SpeedBooster>(this);

        boosterMaterial = gameObject.GetComponent<MeshRenderer>().material;
        boosterMaterial.mainTextureScale = new Vector2(transform.localScale.x / 2f, transform.localScale.z / 2f);
        xOffset = -boosterMaterial.mainTextureScale.x / 2 - 0.5f;
    
        GradientColorKey colorKey1 = new GradientColorKey(new Color(1f, 0.8962696f, 0.5271153f, 1f), 0f);
        GradientColorKey colorKey2 = new GradientColorKey(new Color(1f, 0.05951124f, 0.09084173f, 1f), 1f);
        emissionGradient.colorKeys = new GradientColorKey[] {colorKey1, colorKey2};
    }

    void Update()
    {
        boosterMaterial.mainTextureOffset = new Vector2(xOffset, boosterMaterial.mainTextureOffset.y - boostFactor/2 * Time.deltaTime);
        
        boosterMaterial.SetColor("_EmissionColor", emissionGradient.Evaluate(2f * Mathf.Abs(((Time.time / 1.5f) % 1f) - 0.5f)));
    }
    
    void OnTriggerEnter(Collider other) // causes the ball to go faster when it touches the booster
    {
        PlayerController playerController = other.gameObject.GetComponent<PlayerController>();
        if (playerController != null) 
        {
            playerController.AddBooster(this.asNode);
        }
    }

    void OnTriggerExit(Collider other) // resets the ball to its regular speed when it leaves the booster
    {
        PlayerController playerController = other.gameObject.GetComponent<PlayerController>();
        if (playerController != null) 
        {
            playerController.RemoveBooster(this.asNode);
        }
    }
}
