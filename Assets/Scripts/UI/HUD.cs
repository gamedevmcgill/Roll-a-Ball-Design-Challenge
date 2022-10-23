using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class HUD : MonoBehaviour
{
    private static HUD _instance;
    public static HUD instance
    {
        get 
        {
            if (_instance == null) _instance = (HUD) FindObjectOfType<HUD>();
            return _instance;
        }
        private set
        {
            if (_instance != null && _instance != value) Destroy(value.gameObject);
            else _instance = value;
        }
    }

    bool _enableScoreText = true;
    bool enableScoreText
    {
        get => _enableScoreText;
        set
        {
            _enableScoreText = value;
            scoreText?.gameObject.SetActive(value);
        }
    }

    [SerializeField, HideInInspector] TextMeshProUGUI scoreText;
    bool _enableSpeedometer = true;
    bool enableSpeedometer
    {
        get => _enableSpeedometer;
        set
        {
            _enableSpeedometer = value;
            speedometer?.gameObject.SetActive(value);
        }
    }
    [SerializeField, HideInInspector] TextMeshProUGUI speedometer;

    bool _enableDarkness = true;
    bool enableDarkness
    {
        get => _enableDarkness;
        set
        {
            _enableDarkness = value;
            darkness?.gameObject.SetActive(value);
        }
    }
    [SerializeField, HideInInspector] Image darkness;


    // Start is called before the first frame update
    void OnEnable()
    {
        instance = this;

        Cursor.visible = false; // hides the cursor when the game starts
        
        if (enableScoreText) scoreText.text = "Score: 0";

        if (enableSpeedometer) speedometer.text = "0 km/h";
    }

    void Start() 
    {
        PlayerController.ScoreUpdated += OnScoreUpdated;    
    }

    // Update is called once per frame
    void Update()
    {
        if (enableSpeedometer)
            speedometer.text = speedometer.text = Mathf.Round(PlayerController.instance.rigidbody.velocity.magnitude * 3.6f).ToString() + " km/h";

        if (enableDarkness)
            darkness.color = new Color(0f, 0f, 0f, 1.25f * (float) PlayerController.instance.dying / PlayerController.instance.DEATH_FRAMES); // fades to black
    }

    void OnScoreUpdated(int points, bool silent)
    {
       if (enableScoreText) scoreText.text = "Score: " + PlayerController.instance.score.ToString();
    }



    #if UNITY_EDITOR
    [CustomEditor(typeof(HUD))]
    public class HUDEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            if (GameObject.FindObjectsOfType<HUD>().Length > 1)
                EditorGUILayout.HelpBox("WARNING: There is more than one instance of the HUD component in this scene. While playing, all but one will be destroyed. To avoid unexpected behaviour, ensure there is only one HUD in the scene.", MessageType.Warning);

            DrawDefaultInspector();

            HUD targetHUD = (HUD) target;

            targetHUD.enableScoreText = EditorGUILayout.Toggle ("Enable Score Text", targetHUD.enableScoreText);
            GUI.enabled = targetHUD.enableScoreText;
            targetHUD.scoreText = (TextMeshProUGUI) EditorGUILayout.ObjectField("    Score Text", targetHUD.scoreText, typeof(TextMeshProUGUI), true);
            GUI.enabled = true;

            targetHUD.enableSpeedometer = EditorGUILayout.Toggle ("Enable Speedometer", targetHUD.enableSpeedometer);
            GUI.enabled = targetHUD.enableSpeedometer;
            targetHUD.speedometer = (TextMeshProUGUI) EditorGUILayout.ObjectField("    Speedometer", targetHUD.speedometer, typeof(TextMeshProUGUI), true);
            GUI.enabled = true;
            
            targetHUD.enableDarkness = EditorGUILayout.Toggle ("Enable Darkness", targetHUD.enableDarkness);
            GUI.enabled = targetHUD.enableDarkness;
            targetHUD.darkness = (Image) EditorGUILayout.ObjectField("    Darkness", targetHUD.darkness, typeof(Image), true);
            GUI.enabled = true;

            EditorUtility.SetDirty(target);
        }
    }
    #endif
}
