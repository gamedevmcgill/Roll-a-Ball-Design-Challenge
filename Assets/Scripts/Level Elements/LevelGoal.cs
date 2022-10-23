using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

#if UNITY_EDITOR
using UnityEditor;
#endif


[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(Collider))]
[RequireComponent(typeof(Light))]
public class LevelGoal : MonoBehaviour
{
    [Tooltip(
        "Which level the goal brings you to."
    )]
    #if UNITY_EDITOR
    [SerializeField] SceneAsset targetScene;
    #endif
    [SerializeField, HideInInspector] string sceneName;
    // If checked, the goal is inactive so long as the player has not reached the Score Threshold.
    [SerializeField, HideInInspector] bool requireScore;
    // How many points the player must have to proceed to the next level.
    [SerializeField, HideInInspector] int scoreThreshold;
    bool active = true;

    static Color activeColor = new Color(0f, 1f, 0.3471055f, 1f);
    static Color inactiveColor = new Color (1f, 0.312f, 0.404f, 1f);

    Material goalMaterial;
    new Light light;
    
    // Start is called before the first frame update
    void Start()
    {
        goalMaterial = gameObject.GetComponent<MeshRenderer>().material;
        light = gameObject.GetComponent<Light>();

        if (requireScore == true)
        {
            PlayerController.ScoreUpdated += OnScoreUpdated;

            active = false;
            goalMaterial.color = inactiveColor;
            light.color = inactiveColor;
        }
        else
        {
            active = true;
            goalMaterial.color = activeColor;
            light.color = activeColor;
        }
    }

    void OnScoreUpdated(int points, bool silent)
    {
        if (!active && PlayerController.instance.score >= scoreThreshold)
        {
            active = true;
            goalMaterial.color = activeColor;
            light.color = activeColor;
        }
    }

    void OnTriggerEnter(Collider other) 
    {
        if (active)
        {
            if (other.gameObject == PlayerController.player) SceneManager.LoadScene(sceneName, LoadSceneMode.Single);
        }
    }

    private void OnValidate() 
    {
        if (scoreThreshold < 1) scoreThreshold = 1;   
        #if UNITY_EDITOR
        sceneName = targetScene.name;
        #endif
    }



    #if UNITY_EDITOR
    [CustomEditor(typeof(LevelGoal)), CanEditMultipleObjects]
    public class LevelGoalEditor : Editor
    {
        static GUIContent requireScoreContent = new GUIContent
        (
            text: "Require Score",
            tooltip: "If checked, the goal is inactive so long as the player has not reached the Score Threshold."
        );
        static GUIContent scoreThresholdContent = new GUIContent
        (
           text: "    Score Threshold",
           tooltip: "How many points the player must have to proceed to the next level."
        );

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            LevelGoal levelGoal = (LevelGoal) target;

            levelGoal.requireScore = EditorGUILayout.Toggle(requireScoreContent, levelGoal.requireScore);

            GUI.enabled = levelGoal.requireScore;
            levelGoal.scoreThreshold = EditorGUILayout.IntField(scoreThresholdContent, levelGoal.scoreThreshold);
            GUI.enabled = true;

            if (GUI.changed) levelGoal.OnValidate();
            EditorUtility.SetDirty(target);
        }
    }
    #endif
}