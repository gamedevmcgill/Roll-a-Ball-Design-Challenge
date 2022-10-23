using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FaceCamera : MonoBehaviour
{

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        transform.LookAt(CameraController.instance.transform);
        transform.eulerAngles = new Vector3(0, 180 + transform.eulerAngles[1], 0);
    }
}
