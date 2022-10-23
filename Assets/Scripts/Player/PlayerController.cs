using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

#if UNITY_EDITOR
using UnityEditor;
#endif



[RequireComponent(typeof(Rigidbody))]
public class PlayerController : MonoBehaviour
{
    // implements singleton pattern
    // there is always one statically accessible instance
    // if there is already a non-null instance assigned, attempting to assign a new object to the instance destroys that object instead
    // singleton pattern should be enforced at play time by atttempting to assign each object to the instance during OnEnable()
    private static PlayerController _instance = null;
    public static PlayerController instance
    {
        get 
        {
            if (_instance == null) _instance = (PlayerController) FindObjectOfType<PlayerController>();
            return _instance;
        }
        private set
        {
            if (_instance != null && _instance != value) Destroy(value);
            else _instance = value;
        }
    }

    // alias for instance.gameObject
    public static GameObject player 
    {
        get => instance.gameObject;
    }



    // Player Movement

    // used to hold the attached rigidbody
    private Rigidbody _rigidbody;
    // public accessor for _rigidbody
    new public Rigidbody rigidbody {get => _rigidbody;}
    
    // used to store the player's directional input
    private Vector2 inputVector;
    // stores the actual movement input by the player (with adjusted magnitude)
    private Vector2 movement;
    // movement in the x and z axis relative to the camera's look direction
    private Vector2 xMovement;
    private Vector2 zMovement;

    // stores information about the speherecast which checks if the player is grounded
    RaycastHit groundInfo;
    // backing field for isGrounded
    bool _isGrounded;
    // stores the last time it was checked whether the player is grounded
    float _groundedCheckTime = 0f;
    // returns true if the player is grounded
    // only checks after each FixedUpdate in order to save on spherecasts
    bool isGrounded 
    {
        get
        {
            if (Time.fixedTime != _groundedCheckTime)
            {
                _isGrounded = Physics.SphereCast(gameObject.transform.position, 0.4f, Vector3.down, out groundInfo, 0.2f, int.MaxValue);
                _groundedCheckTime = Time.fixedTime;
            }
            return _isGrounded;
        }
    }

    [Tooltip(
        "The player's max speed, before any modifiers such as speed boosters. "
    )]
    [SerializeField] [Range(1f, 100f)] float baseSpeed = 20f;
    [Tooltip(
        "The player's acceleration. Setting this significantly lower than speed makes the player gives movement more weight, and setting it higher than speed makes it very responsive."
    )]
    [SerializeField] [Range(1f, 200f)] float baseAcceleration = 20f;
    // acceleration after modifiers, such as speed boosters
    float acceleration;
    [Tooltip(
        "How much control the player has in the air. A value lower than 1 results in less air control than ground control, and a value greater than 1 results in greater air control. A value of 0 gives the player no control in the air."
    )]
    [SerializeField] [Range(0f, 2f)] float airControlMultiplier = 1f;
    // used to calculate a rudimentary version of drag
    // drag is only applied along the x-z plane, and is linear rather than quadratic
    // this is unrealistic, but it makes the player's movement more predictable, and makes some calculations easier
    float dragCoefficient;
    // stores velocity in the x-z plane
    Vector3 horizontalVelocity = Vector3.zero;
    
    [Tooltip(
        @"Controls which directions the player can move in, relative to the position of the camera.

Back And Forth: The player can only move away from and towards the camera

Side To Side: The player can only move left and right vis a vis the camera

Rook Movement: The player can move forwards, backwards, left, and right, but not diagonally

Omnidirectional: The player can move in any direction"
    )]
    [SerializeField] MovementMode movementMode = MovementMode.Omnidirectional;

    // If checked, allows the player to jump by hitting the space bar or left clicking.
    [SerializeField , HideInInspector] bool allowJumping = false;
    // How much force the player jumps with. Higher values allow the player to reach greater heights when jumping.
    [SerializeField , HideInInspector] float jumpForce = 5f;
    // How many times the player can jump. 
    // At 1, the player can only jump once and only on the ground. At 2, the player can jump once on the ground and once in the air. At n jumps, the player can jump once on the ground and n-1 times in the air
    [SerializeField , HideInInspector] int jumpCount = 1;
    // how many times the player has jumped since touching the ground
    int jumpsSpent = 0;

    // Determines whether or not the player can hit control or right click to brake
    // Never: The player cannot brake
    // Grounded Only: The player can only brake on the ground
    // Any Time: The player can brake both on the ground and in the air
    [SerializeField , HideInInspector] BrakeMode allowBraking = BrakeMode.Never;
    // true if the player is inputting brake, false otherwise
    bool isBraking = false;
    // How much braking slows the player down. The greater the number, the more the player slows down.
    [SerializeField , HideInInspector] float brakeAmount = 0.05f;

    // list of all speed boosters the player is touching
    private LinkedList<SpeedBooster> _speedBoosters = new LinkedList<SpeedBooster>();

    // angular drag applied on the y axis to stop the ball from spinning in place
    const float SPIN_DRAG = 0.5f;
    // coefficient by which the balls rotation slows in the air
    const float AIR_ANGULAR_DRAG = 0.025f;

    // camera direction
    Vector2 facing;



    // Score

    // the player's score
    private int _score = 0;
    // getter for the score
    public int score {get => _score;}
    // delegate signature for OnScoreUpdated
    public delegate void ScoreDelegate(int points, bool silent);
    // event called when the player's score changes
    public static event ScoreDelegate ScoreUpdated;

    // death and respawning

    // respawn point on fall into void
    [HideInInspector] public Vector3 respawnPoint;
    [Tooltip (
        "y level at which the ball \"dies\" and has to be respawned. The ball will not die if it is under this height for less than a second. -50 by default."
    )]
    [SerializeField]
    int deathHeight = -50;
    // how many frames the ball can be below death height "dying"
    public readonly int DEATH_FRAMES = 50; 
    // how many frames have elapsed at or below DeathHeight
    private int _dying = 0; 
    // public getter for _dying
    public int dying {get => _dying;}



    // Start is called before the first frame update
    void OnEnable()
    {
        // enforces singleton pattern
        instance = this;

        // sets respawn point at player's start point with a tiny offset to avoid clipping on respawn
        respawnPoint = gameObject.transform.position + new Vector3(0f, 0.1f, 0f);

        // assigns acceleration and calculates drag
        acceleration = baseAcceleration;
        dragCoefficient = baseAcceleration / baseSpeed;

        // gets the player's rigidbody
        _rigidbody = gameObject.GetComponent<Rigidbody>();

        // resets the rigidbody's settings
        _rigidbody.mass = 1f;
        rigidbody.drag = 0f;
        rigidbody.angularDrag = 0f;
        rigidbody.isKinematic = false;
        _rigidbody.maxAngularVelocity = float.PositiveInfinity;

        // updates score to make sure everything that uses score is initialized
        UpdateScore(0, true);
    }

    void FixedUpdate()
    {
        // adds force in the direction of the player's input
        facing = (new Vector2(CameraController.instance.direction[0], CameraController.instance.direction[2])).normalized;
        xMovement = inputVector[0] * -Vector2.Perpendicular(facing);
        zMovement = inputVector[1] * facing;

        horizontalVelocity.x = rigidbody.velocity.x;
        horizontalVelocity.z = rigidbody.velocity.z;

        if (isGrounded) 
        {
            // resets jumping
            jumpsSpent = 0;

            // moves according to input
            movement = (xMovement + zMovement).normalized * acceleration;
            _rigidbody.AddForce(movement[0], 0, movement[1], ForceMode.Acceleration);

            _rigidbody.AddForce(-horizontalVelocity * dragCoefficient, ForceMode.Acceleration);

            _rigidbody.AddTorque(0f, -AIR_ANGULAR_DRAG * _rigidbody.angularVelocity.y, 0f);

            // slows the ball if the player is braking (and braking is enabled)
            if (isBraking && allowBraking != BrakeMode.Never)
            {
                _rigidbody.velocity = new Vector3(horizontalVelocity.x * (1f - brakeAmount), _rigidbody.velocity.y, horizontalVelocity.z * (1f - brakeAmount));
            }
        }
        else 
        {
            // moves according to input
            movement = (xMovement + zMovement).normalized * acceleration * airControlMultiplier;
            _rigidbody.AddForce(movement[0], 0, movement[1], ForceMode.Acceleration);
            _rigidbody.AddForce(-horizontalVelocity * dragCoefficient * airControlMultiplier, ForceMode.Acceleration);
            _rigidbody.AddTorque(-AIR_ANGULAR_DRAG * _rigidbody.angularVelocity);
        
            // slows the ball if the player is braking (and air braking is enabled)
            if (isBraking && allowBraking == BrakeMode.AnyTime)
            {
                _rigidbody.velocity = new Vector3(horizontalVelocity.x * (1f - brakeAmount), _rigidbody.velocity.y, horizontalVelocity.z * (1f - brakeAmount));
            } 
        }

        // checks if the ball has gone too low and needs to respawn
        if (_rigidbody.position[1] <= deathHeight)
        {
            _dying++;

            if (_dying >= DEATH_FRAMES)
            {
                Respawn(respawnPoint);
            }
        }
        // cancels respawn if the player leaves the danger zone
        else if (_dying > 0) 
        {
            _dying--;
        }
    }

    // gets movement inputs
    void OnMove(InputValue movementValue) 
    {
        inputVector = Vector2.ClampMagnitude(movementValue.Get<Vector2>(), 1f);
        // depending on the movement mode, restricts input
        switch (movementMode)
        {
            case (MovementMode.BackAndForth):
                inputVector.x = 0f;
                break;

            case (MovementMode.SideToSide):
                inputVector.y = 0f;
                break;

            case (MovementMode.RookMovement):
                if (Mathf.Abs(inputVector.x) > Mathf.Abs(inputVector.y)) inputVector.y = 0;
                else inputVector.x = 0;
                break;

            default:
                break;
        }
    }

    // called when the player starts or stops inputting braking
    void OnBrake(InputValue braking)
    {
        isBraking = (braking.Get<float>() == 1);
    }

    // called when the player inputs a jump
    void OnJump()
    {
        if (!allowJumping) return;
        // jumping should always be possible from the ground
        if (isGrounded)
        {
            _rigidbody.AddForce(0f, jumpForce, 0f, ForceMode.Impulse);
            jumpsSpent += 1;
        }
        else
        {
            // if the character is in the air, at least one jump should be spent
            if (jumpsSpent == 0) jumpsSpent = 1;
            if (jumpsSpent < jumpCount)
            {
                _rigidbody.velocity = new Vector3(_rigidbody.velocity.x, 0f, _rigidbody.velocity.z);
                _rigidbody.AddForce(0f, jumpForce, 0f, ForceMode.Impulse);
                jumpsSpent += 1;
            }
        }
    }

    // add a booster when it starts touching the player
    public void AddBooster(LinkedListNode<SpeedBooster> boosterAsNode)
    {
        _speedBoosters.AddLast(boosterAsNode);
        if (acceleration < baseAcceleration * boosterAsNode.Value.boostFactor) acceleration = baseAcceleration * boosterAsNode.Value.boostFactor;
    }

    // remove a booster when it is not touching the player
    public void RemoveBooster(LinkedListNode<SpeedBooster> boosterAsNode)
    {
        _speedBoosters.Remove(boosterAsNode);
        acceleration = baseAcceleration;
        foreach(SpeedBooster booster in _speedBoosters) if (acceleration < baseAcceleration * booster.boostFactor) acceleration = baseAcceleration * booster.boostFactor;
    }

    //adds points to score
    public void UpdateScore(int points, bool silent = false)
    {   
        _score += points;
        
        // invokes any methods which are listening for changes in score
        ScoreUpdated?.Invoke(points, silent);
    }

    // respawns the player if they fall to their doom
    public void Respawn(Vector3 respawnLocation) 
    {
        _dying = 0;
        transform.position = respawnLocation;
        _rigidbody.velocity *= 0;
        _rigidbody.angularVelocity *= 0;
    }

    void OnValidate() 
    {
        acceleration = baseAcceleration;
        dragCoefficient = baseAcceleration / baseSpeed;
    }



    private enum MovementMode : byte
    {
        BackAndForth,
        SideToSide,
        RookMovement,
        Omnidirectional
    }

    private enum BrakeMode : byte
    {
        Never,
        GroundedOnly,
        AnyTime
    }



    #if UNITY_EDITOR
    [CustomEditor(typeof(PlayerController))]
    public class PlayerControllerEditor : Editor
    {
        static string[] brakeModeLabels = new string[] {"Never", "Grounded Only", "Any Time"};

        static GUIContent allowJumpingContent = new GUIContent
        (
            text: "Allow Jumping",
            tooltip: "If checked, allows the player to jump by hitting the space bar or left clicking."
        );
        static GUIContent jumpForceContent = new GUIContent
        (
           text: "    Jump Force",
           tooltip: "How much force the player jumps with. Higher values allow the player to reach greater heights when jumping."
        );
        static GUIContent jumpCountContent = new GUIContent
        (
            text: "    Jump Count",
            tooltip: @"How many times the player can jump. At 1, the player can only jump once and only on the ground. 
At 2, the player can jump once on the ground and once in the air. At n jumps, the player can jump once on the ground and n-1 times in the air"    
        );
        static GUIContent allowBrakingContent = new GUIContent
        (
            text: "Allow Braking",
            tooltip: @"Determines whether or not the player can hit control or right click to brake

Never: The player cannot brake

Grounded Only: The player can only brake on the ground

Any Time: The player can brake both on the ground and in the air"
        );
        static GUIContent brakeAmountContent = new GUIContent
        (
            text: "    Brake Amount",
            tooltip: "How much braking slows the player down. The greater the number, the more the player slows down."
        );


        public override void OnInspectorGUI()
        {
            if (GameObject.FindObjectsOfType<PlayerController>().Length > 1)
                EditorGUILayout.HelpBox("WARNING: There is more than one instance of the PlayerController component in this scene. While playing, all but one will be destroyed. To avoid unexpected behaviour, ensure there is only one PlayerController in the scene.", MessageType.Warning);

            DrawDefaultInspector();

            PlayerController playerController = (PlayerController) target;

            playerController.allowJumping = EditorGUILayout.Toggle(allowJumpingContent, playerController.allowJumping);
            
            GUI.enabled = playerController.allowJumping;
            playerController.jumpForce = EditorGUILayout.Slider(jumpForceContent, playerController.jumpForce, 1f, 10f);
            playerController.jumpCount = EditorGUILayout.IntSlider(jumpCountContent, playerController.jumpCount, 1, 5);
            GUI.enabled = true;


            playerController.allowBraking = (BrakeMode) EditorGUILayout.Popup(allowBrakingContent, (byte) playerController.allowBraking, brakeModeLabels);

            GUI.enabled = playerController.allowBraking != BrakeMode.Never;
            playerController.brakeAmount = EditorGUILayout.Slider(brakeAmountContent, playerController.brakeAmount, float.Epsilon, 0.1f);
            GUI.enabled = true;

            if (GUI.changed) playerController.OnValidate();
            EditorUtility.SetDirty(target);
        }
    }
    #endif
}

