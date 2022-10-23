using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

[RequireComponent(typeof(TextMeshProUGUI))]
public class SignText : MonoBehaviour
{
    Transform cameraTransform; 
    private float distance;

    private Color textColor;
    private float opacity;
    private float yPos;
    TextMeshProUGUI text;
    static float baseOpacity = 0.8f;
    // Start is called before the first frame update
    void Start()
    {
        text = gameObject.GetComponent<TextMeshProUGUI>();
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

        textColor = new Color(1, 1, 1, opacity);
        text.color = textColor;
    }
}
