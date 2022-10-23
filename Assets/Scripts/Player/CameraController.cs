using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

#if UNITY_EDITOR
using UnityEditor;
#endif

[RequireComponent(typeof(Camera))]
public class CameraController : MonoBehaviour
{
    [Tooltip(
        @"Determines how much control the player has over the camera.
        
Fixed: No camera control.

Horizontal Control: The player can move the mouse to turn the camera left and right.

Full Control: The player can move the camera in any direction with the mouse."
    )]
    [SerializeField] CameraMode cameraMode = CameraMode.FullControl;

    private static CameraController _instance = null;
    public static CameraController instance
    {
        get 
        {
            if (_instance == null) _instance = (CameraController) FindObjectOfType<CameraController>();
            return _instance;
        }
        private set
        {
            if (_instance != null && _instance != value) 
            {
                Destroy(value.gameObject);
            }
            else _instance = value;
        }
    }

    [Tooltip(
        "Offsets the position the camera follows."
    )]
    [SerializeField] private Vector3 offset = new Vector3(0f, 1f, 0f);
    [Tooltip(
        "The distance the camera follows the player from."
    )]
    [SerializeField] float cameraDistance = 10f;
    // how far the camera should actually be (less than cameraRadius if there is an obstruction)
    private float distance; 
    // used to store information about how far the camera should follow from to not collide with obstructions
    private RaycastHit raycastHit;
    
    [Tooltip(
        "The initial rotation of the camera when the game starts."
    )]
    [SerializeField] Vector2 startRotation = new Vector2(0f, 0f);
    // the direction the camera is facing
    Vector3 _direction;
    // public getter for _direction
    public Vector3 direction {get => _direction;}
    // holds the change in camera angle
    private Vector2 deltaAngle;
    // holds the camera's rotation
    private Quaternion rotation;
    private Vector3 eulerRotation;
    [Tooltip(
        "How mouse movement should be translated into camera movement. Higher sensitivity means faster camera movement."
    )]
    [SerializeField] float sensitivity = 0.7f;

    // Start is called before the first frame update
    void OnEnable()
    {
        instance = this;

        rotation.SetLookRotation(Quaternion.Euler(startRotation) * Vector3.forward, Vector3.up);
        _direction = (rotation * Vector3.forward);

        if (Physics.SphereCast(PlayerController.player.transform.position + offset, 0.1f, -_direction, out raycastHit, cameraDistance, ~(1 << 2), QueryTriggerInteraction.Ignore)) // ignores layer 2 AKA Ignore Raycast
        {
            distance = raycastHit.distance - 1f;
        }
        else distance = cameraDistance;

        transform.position = PlayerController.player.transform.position + offset + cameraDistance * -_direction;
        transform.rotation = rotation;

        eulerRotation = rotation.eulerAngles;
    }
 
    // Update is called once per frame
    void LateUpdate()
    {
        if (Physics.SphereCast(PlayerController.player.transform.position + offset, 0.1f, -_direction, out raycastHit, cameraDistance, ~(1 << 2), QueryTriggerInteraction.Ignore)) // ignores layer 2 AKA Ignore Raycast
        {
            distance = raycastHit.distance - 1f;
        }
        else distance = cameraDistance;
        

        transform.position = PlayerController.player.transform.position + offset + distance * -_direction;
        transform.rotation = rotation;
    }

    void OnLook(InputValue mouseValue) // gets mouse position inputs
    {
        if (cameraMode == CameraMode.Fixed) return;

        deltaAngle = mouseValue.Get<Vector2>() * sensitivity;
        
        eulerRotation.x = eulerRotation.x - deltaAngle.y;

        while (eulerRotation.x < 0f) eulerRotation.x = eulerRotation.x + 360f;
        while (eulerRotation.x > 360f) eulerRotation.x -= 360f;
        if (eulerRotation.x > 89f && eulerRotation.x < 180f) eulerRotation.x = 89f;
        else if (eulerRotation.x < 271f && eulerRotation.x > 180f) eulerRotation.x = 271f;

        eulerRotation.y = eulerRotation.y + deltaAngle.x;

        rotation = Quaternion.Euler(eulerRotation);
        _direction = rotation * Vector3.forward;
    }

    void OnValidate() 
    {
        startRotation.x = Mathf.Clamp(startRotation.x, -89f, 89f);
        while (startRotation.y >= 360f) startRotation.y -= 360f;
        while (startRotation.y < 0f) startRotation.y += 360f;
    }

    private enum CameraMode : byte
    {
        Fixed,
        HorizontalControl,
        FullControl
    }



    #if UNITY_EDITOR
    [CustomEditor(typeof(CameraController))]
    public class CameraControllerEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            if (GameObject.FindObjectsOfType<CameraController>().Length > 1)
                EditorGUILayout.HelpBox("WARNING: There is more than one instance of the CameraController component in this scene. While playing, all but one will be destroyed. To avoid unexpected behaviour, ensure there is only one CameraController in the scene.", MessageType.Warning);

            DrawDefaultInspector();

            CameraController cameraController = (CameraController) target;

            if (GUILayout.Button("Reset Camera Transform"))
            {
                cameraController.rotation.SetLookRotation(Quaternion.Euler(cameraController.startRotation.x, cameraController.startRotation.y, 0f) * Vector3.forward, Vector3.up);
                cameraController._direction = Quaternion.Euler(cameraController.startRotation.x, cameraController.startRotation.y, 0f) * Vector3.forward;

                cameraController.transform.position = PlayerController.player.transform.position + cameraController.offset + cameraController.cameraDistance * -cameraController._direction;
                cameraController.transform.rotation = cameraController.rotation;
                cameraController.eulerRotation = cameraController.rotation.eulerAngles;
            }

            EditorUtility.SetDirty(target);
        }
    }
    #endif
}