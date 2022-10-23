using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class SignSprite : MonoBehaviour
{
    Transform cameraTransform; 
    private float distance;

    private Color color;
    private float opacity;
    private float yPos;
    Image sprite;
    static float baseOpacity = 0.8f;

    // Start is called before the first frame update
    void Start()
    {
        sprite = gameObject.GetComponent<Image>();
        cameraTransform = CameraController.instance.transform;
    }

    // Update is called once per frame
    void Update()
    {
        distance = (transform.position - cameraTransform.position).magnitude;
        if (distance < 5)
        {
            opacity = baseOpacity - (5 - distance)/2.5f;
        }
        else if (distance > 50)
        {
            opacity = baseOpacity - (distance - 50)/20;
        }
        else
        {
            opacity = baseOpacity;
        }

        color = new Color(1, 1, 1, opacity);
        sprite.color = color;
    }
}
