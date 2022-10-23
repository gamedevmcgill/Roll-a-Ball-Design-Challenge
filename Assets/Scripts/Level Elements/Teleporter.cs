using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif


[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(Light))]
public class Teleporter : MonoBehaviour
{
    [Tooltip(
        "The teleporter this teleporter sends the player to."
    )]
    [SerializeField] Teleporter linkedTeleporter;

    bool active = true;

    Light glow;
    Material teleporterMaterial;
    // The color of the teleoporter (this only appears when the game is being played).
    [SerializeField, HideInInspector] Color color;

    // Start is called before the first frame update
    void Start()
    {
        teleporterMaterial = gameObject.GetComponent<MeshRenderer>().material;
        teleporterMaterial.color = color;

        glow = gameObject.GetComponent<Light>();
        glow.color = color;
    }

    // Update is called once per frame
    void UpdateColor()
    {
        if (Application.isPlaying) 
        {
            teleporterMaterial.color = color;
            glow.color = color;
        }
    }

    void OnTriggerEnter(Collider other) // causes the ball to go faster when it touches the booster
    {
        if (active && linkedTeleporter != null)
        {
            PlayerController playerController = other.gameObject.GetComponent<PlayerController>();
            if (playerController != null) 
            {
                Vector3 relativePosition = playerController.gameObject.transform.position - gameObject.transform.position;
                playerController.gameObject.transform.position = linkedTeleporter.gameObject.transform.position + new Vector3(0f, relativePosition.y, 0f);
                // playerController.gameObject.transform.position = linkedTeleporter.gameObject.transform.position + new Vector3(-relativePosition.x, relativePosition.y, -relativePosition.z);
                linkedTeleporter.active = false;
            }
        }
    }

    private void OnTriggerExit(Collider other) 
    {
        PlayerController playerController = other.gameObject.GetComponent<PlayerController>();
        if (playerController != null) active = true;
    }

    /*
    private void OnValidate() 
    {
        if (linkedTeleporter != null)
        {
            linkedTeleporter.color = color;
            linkedTeleporter.linkedTeleporter = this;
            
            if (Application.isPlaying && teleporterMaterial != null && linkedTeleporter.teleporterMaterial != null) 
            {
                teleporterMaterial.color = color;
                linkedTeleporter.teleporterMaterial.color = color;
            }
        }
    }
    */

    void OnDrawGizmos()
    {
        if (linkedTeleporter != null)
        {
            Gizmos.color = color;
            Gizmos.DrawLine(transform.position, linkedTeleporter.transform.position);
        }
    }

    #if UNITY_EDITOR
    [CustomEditor(typeof(Teleporter))]
    public class TeleporterEditor : Editor
    {
        static GUIContent colorContent = new GUIContent
        (
            text: "Color",
            tooltip: "The color of the teleporter (this only appears when the game is being played). Linked teleporters are always the same color."
        );


        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            Teleporter teleporter = (Teleporter) target;

            teleporter.color = EditorGUILayout.ColorField(colorContent, teleporter.color);
            if (GUI.changed)
            {
                teleporter.UpdateColor();
                if (teleporter.linkedTeleporter != null)
                {
                    if (teleporter.linkedTeleporter.color != teleporter.color)
                    {
                        teleporter.linkedTeleporter.color = teleporter.color;
                        EditorUtility.SetDirty(teleporter.linkedTeleporter);
                        teleporter.linkedTeleporter.UpdateColor();
                    }
                    if (teleporter.linkedTeleporter != teleporter) teleporter.linkedTeleporter = teleporter;
                }
            }
            EditorUtility.SetDirty(target);
        }
    }
    #endif
}
