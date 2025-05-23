//----------------------------------------------
//            Realistic Car Controller
//
// Copyright © 2014 - 2022 BoneCracker Games
// http://www.bonecrackergames.com
// Buğra Özdoğanlar
//
//----------------------------------------------

using UnityEngine;
using UnityEngine.Audio;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;

[AddComponentMenu("BoneCracker Games/Realistic Car Controller/Main/RCC Realistic Car Controller V3")]
[RequireComponent(typeof(Rigidbody))]
/// <summary>
/// Main vehicle controller script that includes Wheels, Steering, Suspensions, Mechanic Configuration, Stability, Lights, Sounds, and Damage in AIO.
/// </summary>
public class RCC_CarControllerV3 : RCC_Core {

    public bool canControl = true;              // Enables / Disables controlling the vehicle. If enabled, vehicle can receive all inputs from the InputManager.
    public bool isGrounded = false;             // Is vehicle grounded completely now?
    public bool overrideBehavior = false;       //	Vehicle won't be affected by selected behavior in RCC Settings if override is selected.

    private static RCC_CarControllerV3 _instance;

    public static RCC_CarControllerV3 Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = GameObject.FindObjectOfType<RCC_CarControllerV3>();
            }
            return _instance;
        }
    }

    #region Wheels
    // Wheel models of the vehicle.
    public Transform FrontLeftWheelTransform;
    public Transform FrontRightWheelTransform;
    public Transform RearLeftWheelTransform;
    public Transform RearRightWheelTransform;
    public Transform[] ExtraRearWheelsTransform;        // Extra wheels in case your vehicle has extra wheels.

    // Wheel colliders of the vehicle.
    public RCC_WheelCollider FrontLeftWheelCollider;
    public RCC_WheelCollider FrontRightWheelCollider;
    public RCC_WheelCollider RearLeftWheelCollider;
    public RCC_WheelCollider RearRightWheelCollider;
    public RCC_WheelCollider[] ExtraRearWheelsCollider;     // Extra Wheels. In case of if your vehicle has extra wheels.

    private RCC_WheelCollider[] _allWheelColliders;
    public RCC_WheelCollider[] allWheelColliders {

        get {

            if (_allWheelColliders == null || _allWheelColliders.Length <= 0)
                _allWheelColliders = GetComponentsInChildren<RCC_WheelCollider>(true);

            return _allWheelColliders;

        }

    }       // All wheel colliders.
    public bool hasExtraWheels = false;

    [Obsolete("Deprecated and ineffective. Use overrideAllWheels.")] public bool overrideWheels = false;
    public bool overrideAllWheels = false;       //	Overriding individual wheel settings such as steer, power, brake, handbrake.
    public int poweredWheels = 0;       //	Total count of powered wheels. Used for dividing total power per each wheel.

    [System.Serializable]
    public class ConfigureVehicleSubsteps {

        public float speedThreshold = 10f;
        public int stepsBelowThreshold = 5;
        public int stepsAboveThreshold = 5;

    }

    public ConfigureVehicleSubsteps configureVehicleSubsteps = new ConfigureVehicleSubsteps();
    #endregion

    #region SteeringWheel
    // Steering wheel model.
    public Transform SteeringWheel;                                                     // Driver steering wheel model. In case of if your vehicle has individual steering wheel model in interior.
    private Quaternion orgSteeringWheelRot;                                         // Original rotation of steering wheel.
    public SteeringWheelRotateAround steeringWheelRotateAround;     // Current rotation of steering wheel.
    public enum SteeringWheelRotateAround { XAxis, YAxis, ZAxis }       //	Rotation axis of steering wheel.
    public float steeringWheelAngleMultiplier = 11f;                                    // Angle multiplier of steering wheel.
    #endregion

    #region Drivetrain Type
    // Drivetrain type of the vehicle.
    public WheelType wheelTypeChoise = WheelType.RWD;
    public enum WheelType { FWD, RWD, AWD, BIASED }
    #endregion

    #region AI
    public bool externalController = false;     // AI Controller.
    public float aiSpeedMultiplier = 100f;   // Multiplier for AI speed (set > 1 for faster acceleration)
    #endregion

    #region Steering
    public enum SteeringType { Curve, Simple, Constant }
    public SteeringType steeringType;
    public AnimationCurve steerAngleCurve = new AnimationCurve();   //	Steering angle limiter curve based on speed.
    public float steerAngle = 40f;                                                          // Maximum Steer Angle Of Your Vehicle.
    public float highspeedsteerAngle = 5f;                                          // Maximum Steer Angle At Highest Speed.
    public float highspeedsteerAngleAtspeed = 120f;                         // Highest Speed For Maximum Steer Angle.
    public float antiRollFrontHorizontal = 1000f;                                   // Anti Roll Horizontal Force For Preventing Flip Overs And Stability.
    public float antiRollRearHorizontal = 1000f;                                    // Anti Roll Horizontal Force For Preventing Flip Overs And Stability.
    public float antiRollVertical = 0f;                                                 // Anti Roll Vertical Force For Preventing Flip Overs And Stability. I know it doesn't exist, but it can improve gameplay if you have high COM vehicles like monster trucks.
    #endregion

    #region Configurations
    private Rigidbody _rigid;
    public Rigidbody rigid {

        get {

            if (!_rigid)
                _rigid = GetComponent<Rigidbody>();

            return _rigid;

        }

    }

    // Rigidbody.
    public Transform COM;                                                    // Center of mass.
    public float brakeTorque = 2000f;                                   // Maximum brake torque.,
    public float downForce = 25f;                                       // Applies downforce related with vehicle speed.
    public float speed = 0f;                                                    // Vehicle speed in km/h or mp/h.
    public float maxspeed = 6000f;                                       // Top speed.
    private float resetTime = 0f;                                           // Used for resetting the vehicle if upside down.

    #endregion

    #region Engine
    public AnimationCurve engineTorqueCurve = new AnimationCurve();     //	Engine torque curve based on RPM.
    public bool autoGenerateEngineRPMCurve = true;      // Auto create engine torque curve. If min/max engine rpm, engine torque, max engine torque at rpm, or top speed has been changed at runtime, it will generate new curve with them.
    public float maxEngineTorque = 300f;                        // Maximum engine torque at target RPM.
    public float maxEngineTorqueAtRPM = 5500f;          //	Maximum peek of the engine at this RPM.
    public float minEngineRPM = 1000f;                          // Minimum engine RPM.
    public float maxEngineRPM = 7000f;                          // Maximum engine RPM.
    public float engineRPM = 0f;                                        // Current engine RPM.
    public float engineRPMRaw = 0f;                             // Current raw engine RPM.
    [Range(.02f, .4f)] public float engineInertia = .15f;       // Engine inertia. Engine reacts faster on lower values.
    public bool useRevLimiter = true;                               // Rev limiter above maximum engine RPM. Cuts gas when RPM exceeds maximum engine RPM.
    public bool useExhaustFlame = true;                         // Exhaust blows flame when driver cuts gas at certain RPMs.
    public bool runEngineAtAwake { get { return RCC_Settings.Instance.runEngineAtAwake; } }         // Engine running at Awake?
    public bool engineRunning = false;                                                                      // Engine running now?

    //	Comparing old and new values to recreate engine torque curve.
    private float oldEngineTorque = 0f;             // Old engine torque used for recreating the engine curve.
    private float oldMaxTorqueAtRPM = 0f;           // Old max torque used for recreating the engine curve.
    private float oldMinEngineRPM = 0f;             // Old min RPM used for recreating the engine curve.
    private float oldMaxEngineRPM = 0f;             // Old max RPM used for recreating the engine curve.
    #endregion

    #region Steering Assistance
    public bool useSteeringLimiter = true;                              // Limits maximum steering angle when vehicle is sliding. It helps to keep the vehicle in control.
    public bool useCounterSteering = true;                              // Applies counter steering when vehicle is drifting. It helps to keep the vehicle in control.
    public bool useSteeringSensitivity = true;                          //	Steering sensitivity.
    [Range(0f, 1f)] public float counterSteeringFactor = .5f;                // Counter steering multiplier.
    [Range(.05f, 1f)] public float steeringSensitivityFactor = 1f;      // Steering sensitivity multiplier.
    private float orgSteerAngle = 0f;       // Original steer angle.
    public float oldSteeringInput = 0f;     //	Old steering input.
    public float steeringDifference = 0f;   //	Steering input difference.
    #endregion

    #region Fuel
    // Fuel.
    public bool useFuelConsumption = false;     // Enable / Disable Fuel Consumption.
    public float fuelTankCapacity = 62f;                // Fuel Tank Capacity.
    public float fuelTank = 62f;                            // Fuel Amount.
    public float fuelConsumptionRate = .1f;         // Fuel Consumption Rate.
    #endregion

    #region Heat
    // Engine heat.
    public bool useEngineHeat = false;                          // Enable / Disable engine heat.
    public float engineHeat = 15f;                                  // Engine heat.
    public float engineCoolingWaterThreshold = 90f;     // Engine cooling water engage point.
    public float engineHeatRate = 1f;                               // Engine heat multiplier.
    public float engineCoolRate = 1f;                               // Engine cool multiplier.
    #endregion

    #region Gears
    // Gears.
    [System.Serializable]
    public class Gear {

        public float maxRatio;
        public int maxSpeed;
        public int targetSpeedForNextGear;

        public void SetGear(float ratio, int speed, int targetSpeed) {

            maxRatio = ratio;
            maxSpeed = speed;
            targetSpeedForNextGear = targetSpeed;

        }

    }

    public Gear[] gears;                    // Gear class.
    public int totalGears = 6;          //	Total count of gears.
    public int currentGear = 0;     // Current gear of the vehicle.
    public bool NGear = false;          // N gear.

    public float finalRatio = 3.23f;                                                //	Final drive gear ratio. 
    [Range(0f, .5f)] public float gearShiftingDelay = .35f;             //	Gear shifting delay with time.
    [Range(.25f, 1)] public float gearShiftingThreshold = .75f;     //	Shifting gears at lower RPMs at higher values.
    [Range(.1f, .9f)] public float clutchInertia = .25f;                    //	Adjusting clutch faster at lower values. Higher values for smooth clutch.

    public float gearShiftUpRPM = 6500f;                //	Shifting up when engine RPM is high enough.
    public float gearShiftDownRPM = 3500f;      //	Shifting down when engine RPM is low enough.
    public bool changingGear = false;                   // Changing gear currently?

    public int direction = 1;                           // Reverse gear currently?
    internal bool canGoReverseNow = false;  //	If speed is low enough and player pushes the brake button, enable this bool to go reverse.
    public float launched = 0f;
    public bool autoReverse { get { if (!externalController) return RCC_Settings.Instance.autoReverse; else return true; } }                            // Enables / Disables auto reversing when player press brake button. Useful for if you are making parking style game.
    public bool automaticGear { get { if (!externalController) return RCC_Settings.Instance.useAutomaticGear; else return true; } }                // Enables / Disables automatic gear shifting.
    internal bool semiAutomaticGear = false;            // Enables / Disables semi-automatic gear shifting.
    public bool useAutomaticClutch { get { return RCC_Settings.Instance.useAutomaticClutch; } }
    #endregion

    #region Audio
    // How many audio sources we will use for simulating engine sounds?. Usually, all modern driving games have around six audio sources per vehicle.
    // Low RPM, Medium RPM, and High RPM. And their off versions. 
    public AudioType audioType;
    public enum AudioType { OneSource, TwoSource, ThreeSource, Off }

    // If you don't have their off versions, generate them.
    public bool autoCreateEngineOffSounds = true;

    // AudioSources and AudioClips.
    private AudioSource engineStartSound;
    public AudioClip engineStartClip;
    internal AudioSource engineSoundHigh;
    public AudioClip engineClipHigh;
    private AudioSource engineSoundMed;
    public AudioClip engineClipMed;
    private AudioSource engineSoundLow;
    public AudioClip engineClipLow;
    private AudioSource engineSoundIdle;
    public AudioClip engineClipIdle;
    private AudioSource gearShiftingSound;

    internal AudioSource engineSoundHighOff;
    public AudioClip engineClipHighOff;
    internal AudioSource engineSoundMedOff;
    public AudioClip engineClipMedOff;
    internal AudioSource engineSoundLowOff;
    public AudioClip engineClipLowOff;

    // Shared AudioSources and AudioClips.
    private AudioClip[] gearShiftingClips { get { return RCC_Settings.Instance.gearShiftingClips; } }
    private AudioSource crashSound;
    private AudioClip[] crashClips { get { return RCC_Settings.Instance.crashClips; } }
    private AudioSource reversingSound;
    private AudioClip reversingClip { get { return RCC_Settings.Instance.reversingClip; } }
    private AudioSource windSound;
    private AudioClip windClip { get { return RCC_Settings.Instance.windClip; } }
    private AudioSource brakeSound;
    private AudioClip brakeClip { get { return RCC_Settings.Instance.brakeClip; } }
    private AudioSource NOSSound;
    private AudioClip NOSClip { get { return RCC_Settings.Instance.NOSClip; } }
    private AudioSource turboSound;
    private AudioClip turboClip { get { return RCC_Settings.Instance.turboClip; } }
    private AudioSource blowSound;
    private AudioClip[] blowClip { get { return RCC_Settings.Instance.blowoutClip; } }

    // Min / Max sound pitches and volumes.
    [Range(0f, 1f)] public float minEngineSoundPitch = .75f;
    [Range(1f, 2f)] public float maxEngineSoundPitch = 1.75f;
    [Range(0f, 1f)] public float minEngineSoundVolume = .05f;
    [Range(0f, 1f)] public float maxEngineSoundVolume = .85f;
    [Range(0f, 1f)] public float idleEngineSoundVolume = .85f;

    // Positions of the created audio sources.
    public Vector3 engineSoundPosition = new Vector3(0f, 0f, 1.5f);
    public Vector3 gearSoundPosition = new Vector3(0f, -.5f, .5f);
    public Vector3 turboSoundPosition = new Vector3(0f, 0f, 1.5f);
    public Vector3 exhaustSoundPosition = new Vector3(0f, -.5f, 2f);
    public Vector3 windSoundPosition = new Vector3(0f, 0f, 2f);
    #endregion

    #region Inputs
    // Inputs. All values are clamped 0f - 1f. They will receive proper input values from RCC_InputManager class.
    public RCC_Inputs inputs;

    [HideInInspector] public float throttleInput = 0f;
    [HideInInspector] public float brakeInput = 0f;
    [HideInInspector] public float steerInput = 0f;
    [HideInInspector] public float counterSteerInput = 0f;
    [HideInInspector] public float clutchInput = 0f;
    [HideInInspector] public float handbrakeInput = 0f;
    [HideInInspector] public float boostInput = 0f;
    [HideInInspector] public float fuelInput = 0f;
    [HideInInspector] public bool cutGas = false;
    [HideInInspector] public bool permanentGas = false;
    #endregion

    #region Head Lights
    // Lights.
    public bool lowBeamHeadLightsOn = false;    // Low beam head lights.
    public bool highBeamHeadLightsOn = false;   // High beam head lights.
    #endregion

    #region Indicator Lights
    // For Indicators.
    public IndicatorsOn indicatorsOn;                       // Indicator system.
    public enum IndicatorsOn { Off, Right, Left, All }  //	Current indicator mode.
    public float indicatorTimer = 0f;                           // Used timer for indicator on / off sequence.
    #endregion

    #region Damage
    // Damage.
    public RCC_Damage damage;
    public bool useDamage = true;      // Use deformation on collisions.
    public bool useCollisionParticles = true;        //	Use particles on coliisions.
    public bool useCollisionAudio = true;       //	Play crash audio clips on collisions

    public GameObject contactSparkle { get { return RCC_Settings.Instance.contactParticles; } }     // Contact Particles for collisions. It must be Particle System.
    public GameObject scratchSparkle { get { return RCC_Settings.Instance.scratchParticles; } }     // Scratch Particles for collisions. It must be Particle System.

    private List<ParticleSystem> contactSparkeList = new List<ParticleSystem>();    // Array for Contact Particles.
    private List<ParticleSystem> scratchSparkeList = new List<ParticleSystem>();    // Array for Contact Particles.

    public int maximumContactSparkle = 5;                                                           //	Contact Particles will be ready to use for collisions in pool. 
    private GameObject allContactParticles;                                                         // Main particle gameobject for keep the hierarchy clean and organized.
    #endregion

    #region Helpers
    // Used for Angular and Linear Steering Helper.
    private float oldRotation;
    public Transform velocityDirection;
    public Transform steeringDirection;
    public float velocityAngle;
    private float angle;
    private float angularVelo;
    #endregion

    #region Driving Assistances
    // Driving Assistances.
    public bool ABS = true;
    public bool TCS = true;
    public bool ESP = true;
    public bool steeringHelper = true;
    public bool tractionHelper = true;
    public bool angularDragHelper = false;

    // Driving Assistance thresholds.
    [Range(.05f, .5f)] public float ABSThreshold = .35f;        //	ABS will be engaged at this threshold.
    [Range(.05f, 1f)] public float TCSStrength = .5f;
    [Range(.05f, .5f)] public float ESPThreshold = .5f;         //	ESP will be engaged at this threshold.
    [Range(.05f, 1f)] public float ESPStrength = .25f;
    [Range(0f, 1f)] public float steerHelperLinearVelStrength = .1f;
    [Range(0f, 1f)] public float steerHelperAngularVelStrength = .1f;
    [Range(0f, 1f)] public float tractionHelperStrength = .1f;
    [Range(0f, 1f)] public float angularDragHelperStrength = .1f;

    // Is Driving Assistance is in action now?
    public bool ABSAct = false;
    public bool TCSAct = false;
    public bool ESPAct = false;

    // ESP malfunction.
    public bool ESPBroken = false;

    // Used For ESP.
    public float frontSlip = 0f;
    public float rearSlip = 0f;

    // ESP Bools.
    public bool underSteering = false;
    public bool overSteering = false;
    #endregion

    #region Drift
    // Drift Variables.
    internal bool driftingNow = false;      // Currently drifting?
    internal float driftAngle = 0f;             // If we do, what's the drift angle?
    #endregion

    #region Turbo / NOS / Boost
    // Turbo and NOS.
    public float turboBoost = 0f;
    public float NoS = 100f;
    private float NoSConsumption = 25f;
    private float NoSRegenerateTime = 10f;

    public bool useNOS = false;
    public bool useTurbo = false;

    bool oppositeDirection;
    float sidewaysSlip;
    float maxSteerInput;
    float sign;
    float rearSidewaysSlip;
    int currentPoweredWheels;
    float wheelRPM;
    float velocity;
    float newEngineInertia;
    float lowRPM;
    float medRPM;
    float highRPM;
    float volumeLevel;
    float pitchLevel;
    bool appliedBrake;
    float travelFL;
    float travelFR;
    bool groundedFL;
    bool groundedFR;
    float antiRollForceFrontHorizontal;
    float travelRL;
    float travelRR;
    bool groundedRL;
    bool groundedRR;
    float antiRollForceRearHorizontal;
    float antiRollForceFrontVertical;
    float antiRollForceRearVertical;
    bool grounded;
    Vector3 v;
    int normalizer;
    float angle2;
    float turnadjust;
    Quaternion velRotation;
    Vector3 velocitys;

    #endregion

    #region Events
    /// <summary>
    /// On RCC player vehicle spawned.
    /// </summary>
    public delegate void onRCCPlayerSpawned(RCC_CarControllerV3 RCC);
    public static event onRCCPlayerSpawned OnRCCPlayerSpawned;

    /// <summary>
    /// On RCC player vehicle destroyed.
    /// </summary>
    public delegate void onRCCPlayerDestroyed(RCC_CarControllerV3 RCC);
    public static event onRCCPlayerDestroyed OnRCCPlayerDestroyed;

    /// <summary>
    /// On RCC player vehicle collision.
    /// </summary>
    public delegate void onRCCPlayerCollision(RCC_CarControllerV3 RCC, Collision collision);
    public static event onRCCPlayerCollision OnRCCPlayerCollision;
    #endregion

    public RCC_TruckTrailer attachedTrailer;

    void Awake() {

        //if (_instance != null)
        //{
        //    Destroy(gameObject);
        //}
        //else
        //{
        //    _instance = this;
        //}

        // Getting Rigidbody and settings.
        rigid.maxAngularVelocity = RCC_Settings.Instance.maxAngularVelocity;

        // Checks the important parameters. Normally, editor script limits them, but your old prefabs may still use out of range.
        gearShiftingThreshold = Mathf.Clamp(gearShiftingThreshold, .25f, 1f);

        // Checks the important parameters. Normally, editor script limits them, but your old prefabs may still use out of range.
        if (engineInertia > .4f)
            engineInertia = .15f;

        engineInertia = Mathf.Clamp(engineInertia, .02f, .4f);

        oldEngineTorque = maxEngineTorque;
        oldMaxTorqueAtRPM = maxEngineTorqueAtRPM;
        oldMinEngineRPM = minEngineRPM;
        oldMaxEngineRPM = maxEngineRPM;

        // You can configurate wheels for variable behaviors.
        GetComponentInChildren<WheelCollider>().ConfigureVehicleSubsteps(configureVehicleSubsteps.speedThreshold, configureVehicleSubsteps.stepsBelowThreshold, configureVehicleSubsteps.stepsAboveThreshold);

        // Assigning wheel models of the wheelcolliders.
        FrontLeftWheelCollider.wheelModel = FrontLeftWheelTransform;
        FrontRightWheelCollider.wheelModel = FrontRightWheelTransform;
        RearLeftWheelCollider.wheelModel = RearLeftWheelTransform;
        RearRightWheelCollider.wheelModel = RearRightWheelTransform;

        // If vehicle has extra rear wheels, assign them too.
        for (int i = 0; i < ExtraRearWheelsCollider.Length; i++)
            ExtraRearWheelsCollider[i].wheelModel = ExtraRearWheelsTransform[i];

        // Default Steer Angle. Using it for lerping current steer angle between default steer angle and high speed steer angle.
        orgSteerAngle = steerAngle;

        // Collecting all contact particles in same parent gameobject for clean hierarchy.
        allContactParticles = new GameObject("All Contact Particles");
        allContactParticles.transform.SetParent(transform, false);

        // Creating and initializing all audio sources.
        CreateAudios();

        // Checks the current selected behavior in RCC Settings. If any behavior selected, apply changes to the vehicle.
        if (!overrideBehavior)
            CheckBehavior();

        // And lastly, starting the engine.
        if (runEngineAtAwake || externalController) {

            engineRunning = true;
            fuelInput = 1f;

        }

        //	If steer angle curve is not initialized or has low keyframes, recreate it.
        if (steerAngleCurve == null)
            steerAngleCurve = new AnimationCurve(new Keyframe(0f, 40f), new Keyframe(120f, 7f), new Keyframe(200f, 5f));     //	Steering angle limiter curve based on speed.
        else if (steerAngleCurve.length < 1)
            steerAngleCurve = new AnimationCurve(new Keyframe(0f, 40f), new Keyframe(120f, 7f), new Keyframe(200f, 5f));     //	Steering angle limiter curve based on speed.

    }

    void OnEnable() {

        //	Make sure changing gear is set to false, because we're setting it in coroutine.
        changingGear = false;
        currentGear = 0;

        // Firing an event when each RCC car spawned / enabled. This event has been listening by RCC_MobileButtons.cs, RCC_DashboardInputs.cs.
        StartCoroutine(RCCPlayerSpawned());

        // Listening an event when main behavior changed.
        RCC_SceneManager.OnBehaviorChanged += CheckBehavior;

        // Listening input events on RCC_InputManager.
        RCC_InputManager.OnStartStopEngine += RCC_InputManager_OnStartStopEngine;
        RCC_InputManager.OnLowBeamHeadlights += RCC_InputManager_OnLowBeamHeadlights;
        RCC_InputManager.OnHighBeamHeadlights += RCC_InputManager_OnHighBeamHeadlights;
        RCC_InputManager.OnIndicatorLeft += RCC_InputManager_OnIndicatorLeft;
        RCC_InputManager.OnIndicatorRight += RCC_InputManager_OnIndicatorRight;
        RCC_InputManager.OnIndicatorHazard += RCC_InputManager_OnIndicatorHazard;
        RCC_InputManager.OnGearShiftUp += RCC_InputManager_OnGearShiftUp;
        RCC_InputManager.OnGearShiftDown += RCC_InputManager_OnGearShiftDown;
        RCC_InputManager.OnNGear += RCC_InputManager_OnNGear;
        RCC_InputManager.OnTrailerDetach += RCC_InputManager_OnTrailerDetach;

    }

    /// <summary>
    /// Firing an event when each RCC car spawned / enabled. This event has been listening by RCC_MobileButtons.cs, RCC_DashboardInputs.cs.
    /// </summary>
    /// <returns>The player spawned.</returns>
    private IEnumerator RCCPlayerSpawned() {

        yield return new WaitForEndOfFrame();

        // Firing an event when each RCC car spawned / enabled. This event has been listening by RCC_SceneManager.
        if (!externalController) {

            if (OnRCCPlayerSpawned != null)
                OnRCCPlayerSpawned(this);

        }

    }

    /// <summary>
    /// Creates all wheelcolliders. Only editor script calls this when you click "Create WheelColliders" button.
    /// </summary>
    public void CreateWheelColliders() {

        CreateWheelColliders(this);

    }

    /// <summary>
    /// Creates all audio sources and assigns corresponding audio clips with proper names and keeping hierarchy more clean.
    /// </summary>
    private void CreateAudios() {

        switch (audioType) {

            case AudioType.OneSource:

                engineSoundHigh = NewAudioSource(RCC_Settings.Instance.audioMixer, gameObject, engineSoundPosition, "Engine Sound High AudioSource", 5, 50, 0, engineClipHigh, true, true, false);

                if (autoCreateEngineOffSounds) {

                    engineSoundHighOff = NewAudioSource(RCC_Settings.Instance.audioMixer, gameObject, engineSoundPosition, "Engine Sound High Off AudioSource", 5, 50, 0, engineClipHigh, true, true, false);

                    NewLowPassFilter(engineSoundHighOff, 3000f);

                } else {

                    engineSoundHighOff = NewAudioSource(RCC_Settings.Instance.audioMixer, gameObject, engineSoundPosition, "Engine Sound High Off AudioSource", 5, 50, 0, engineClipHighOff, true, true, false);

                }

                break;

            case AudioType.TwoSource:

                engineSoundHigh = NewAudioSource(RCC_Settings.Instance.audioMixer, gameObject, engineSoundPosition, "Engine Sound High AudioSource", 5, 50, 0, engineClipHigh, true, true, false);
                engineSoundLow = NewAudioSource(RCC_Settings.Instance.audioMixer, gameObject, engineSoundPosition, "Engine Sound Low AudioSource", 5, 25, 0, engineClipLow, true, true, false);

                if (autoCreateEngineOffSounds) {

                    engineSoundHighOff = NewAudioSource(RCC_Settings.Instance.audioMixer, gameObject, engineSoundPosition, "Engine Sound High Off AudioSource", 5, 50, 0, engineClipHigh, true, true, false);
                    engineSoundLowOff = NewAudioSource(RCC_Settings.Instance.audioMixer, gameObject, engineSoundPosition, "Engine Sound Low Off AudioSource", 5, 25, 0, engineClipLow, true, true, false);

                    NewLowPassFilter(engineSoundHighOff, 3000f);
                    NewLowPassFilter(engineSoundLowOff, 3000f);

                } else {

                    engineSoundHighOff = NewAudioSource(RCC_Settings.Instance.audioMixer, gameObject, engineSoundPosition, "Engine Sound High Off AudioSource", 5, 50, 0, engineClipHighOff, true, true, false);
                    engineSoundLowOff = NewAudioSource(RCC_Settings.Instance.audioMixer, gameObject, engineSoundPosition, "Engine Sound Low Off AudioSource", 5, 25, 0, engineClipLowOff, true, true, false);

                }

                break;

            case AudioType.ThreeSource:

                engineSoundHigh = NewAudioSource(RCC_Settings.Instance.audioMixer, gameObject, engineSoundPosition, "Engine Sound High AudioSource", 5, 50, 0, engineClipHigh, true, true, false);
                engineSoundMed = NewAudioSource(RCC_Settings.Instance.audioMixer, gameObject, engineSoundPosition, "Engine Sound Medium AudioSource", 5, 50, 0, engineClipMed, true, true, false);
                engineSoundLow = NewAudioSource(RCC_Settings.Instance.audioMixer, gameObject, engineSoundPosition, "Engine Sound Low AudioSource", 5, 25, 0, engineClipLow, true, true, false);

                if (autoCreateEngineOffSounds) {

                    engineSoundHighOff = NewAudioSource(RCC_Settings.Instance.audioMixer, gameObject, engineSoundPosition, "Engine Sound High Off AudioSource", 5, 50, 0, engineClipHigh, true, true, false);
                    engineSoundMedOff = NewAudioSource(RCC_Settings.Instance.audioMixer, gameObject, engineSoundPosition, "Engine Sound Medium Off AudioSource", 5, 50, 0, engineClipMed, true, true, false);
                    engineSoundLowOff = NewAudioSource(RCC_Settings.Instance.audioMixer, gameObject, engineSoundPosition, "Engine Sound Low Off AudioSource", 5, 25, 0, engineClipLow, true, true, false);

                    if (engineSoundHighOff)
                        NewLowPassFilter(engineSoundHighOff, 3000f);
                    if (engineSoundMedOff)
                        NewLowPassFilter(engineSoundMedOff, 3000f);
                    if (engineSoundLowOff)
                        NewLowPassFilter(engineSoundLowOff, 3000f);

                } else {

                    engineSoundHighOff = NewAudioSource(RCC_Settings.Instance.audioMixer, gameObject, engineSoundPosition, "Engine Sound High Off AudioSource", 5, 50, 0, engineClipHighOff, true, true, false);
                    engineSoundMedOff = NewAudioSource(RCC_Settings.Instance.audioMixer, gameObject, engineSoundPosition, "Engine Sound Medium Off AudioSource", 5, 50, 0, engineClipMedOff, true, true, false);
                    engineSoundLowOff = NewAudioSource(RCC_Settings.Instance.audioMixer, gameObject, engineSoundPosition, "Engine Sound Low Off AudioSource", 5, 25, 0, engineClipLowOff, true, true, false);

                }

                break;

        }

        engineSoundIdle = NewAudioSource(RCC_Settings.Instance.audioMixer, gameObject, engineSoundPosition, "Engine Sound Idle AudioSource", 5, 25, 0, engineClipIdle, true, true, false);
        reversingSound = NewAudioSource(RCC_Settings.Instance.audioMixer, gameObject, gearSoundPosition, "Reverse Sound AudioSource", 10, 50, 0, reversingClip, true, false, false);
        windSound = NewAudioSource(RCC_Settings.Instance.audioMixer, gameObject, windSoundPosition, "Wind Sound AudioSource", 1, 10, 0, windClip, true, true, false);
        brakeSound = NewAudioSource(RCC_Settings.Instance.audioMixer, gameObject, "Brake Sound AudioSource", 1, 10, 0, brakeClip, true, true, false);

        if (useNOS)
            NOSSound = NewAudioSource(RCC_Settings.Instance.audioMixer, gameObject, exhaustSoundPosition, "NOS Sound AudioSource", 5, 10, .5f, NOSClip, true, false, false);

        if (useNOS || useTurbo)
            blowSound = NewAudioSource(RCC_Settings.Instance.audioMixer, gameObject, exhaustSoundPosition, "NOS Blow", 1f, 10f, .5f, null, false, false, false);

        if (useTurbo) {

            turboSound = NewAudioSource(RCC_Settings.Instance.audioMixer, gameObject, turboSoundPosition, "Turbo Sound AudioSource", .1f, .5f, 0f, turboClip, true, true, false);
            NewHighPassFilter(turboSound, 10000f, 10);

        }

    }

    /// <summary>
    /// Overrides the behavior.
    /// </summary>
    private void CheckBehavior() {

        //	If override is enabled, return.
        if (overrideBehavior)
            return;

        //	If selected behavior is none, return.
        if (RCC_Settings.Instance.selectedBehaviorType == null)
            return;

        // If any behavior is selected in RCC Settings, override changes.
        SetBehavior(this);

    }

    //private void OnTriggerEnter(Collider other)
    //{
    //    if (other.tag == "ParkPos")
    //    {
    //        other.gameObject.SetActive(false);
    //        GameManager.Instance.gameObject.GetComponent<OilTankerTimeLines>().enabled = true;
    //        gameObject.transform.parent.gameObject.SetActive(false);
    //    }
    //}

    /// <summary>
    /// Creates the engine curve.
    /// </summary>
    public void ReCreateEngineTorqueCurve() {

        engineTorqueCurve = new AnimationCurve();
        engineTorqueCurve.AddKey(minEngineRPM, maxEngineTorque / 2f);                                                               //	First index of the curve.
        engineTorqueCurve.AddKey(maxEngineTorqueAtRPM, maxEngineTorque);        //	Second index of the curve at max.
        engineTorqueCurve.AddKey(maxEngineRPM, maxEngineTorque / 1.5f);         // Last index of the curve at maximum RPM.

        oldEngineTorque = maxEngineTorque;
        oldMaxTorqueAtRPM = maxEngineTorqueAtRPM;
        oldMinEngineRPM = minEngineRPM;
        oldMaxEngineRPM = maxEngineRPM;

    }

    /// <summary>
    /// Inits the gears.
    /// </summary>
    public void InitGears() {

        gears = new Gear[totalGears];

        float[] gearRatio = new float[gears.Length];
        int[] maxSpeedForGear = new int[gears.Length];
        int[] targetSpeedForGear = new int[gears.Length];

        if (gears.Length == 1)
            gearRatio = new float[] { 1.0f };

        if (gears.Length == 2)
            gearRatio = new float[] { 2.0f, 1.0f };

        if (gears.Length == 3)
            gearRatio = new float[] { 2.0f, 1.5f, 1.0f };

        if (gears.Length == 4)
            gearRatio = new float[] { 2.86f, 1.62f, 1.0f, .72f };

        if (gears.Length == 5)
            gearRatio = new float[] { 4.23f, 2.52f, 1.66f, 1.22f, 1.0f, };

        if (gears.Length == 6)
            gearRatio = new float[] { 4.35f, 2.5f, 1.66f, 1.23f, 1.0f, .85f };

        if (gears.Length == 7)
            gearRatio = new float[] { 4.5f, 2.5f, 1.66f, 1.23f, 1.0f, .9f, .8f };

        if (gears.Length == 8)
            gearRatio = new float[] { 4.6f, 2.5f, 1.86f, 1.43f, 1.23f, 1.05f, .9f, .72f };

        for (int i = 0; i < gears.Length; i++) {

            maxSpeedForGear[i] = (int)((maxspeed / gears.Length) * (i + 1));
            targetSpeedForGear[i] = (int)(Mathf.Lerp(0, maxspeed * Mathf.Lerp(0f, 1f, gearShiftingThreshold), ((float)(i + 1) / (float)(gears.Length))));

        }

        for (int i = 0; i < gears.Length; i++) {

            gears[i] = new Gear();
            gears[i].SetGear(gearRatio[i], maxSpeedForGear[i], targetSpeedForGear[i]);

        }

    }

    /// <summary>
    /// Kills or start engine.
    /// </summary>
    public void KillOrStartEngine() {

        if (engineRunning)
            KillEngine();
        else
            StartEngine();

    }

    /// <summary>
    /// Starts the engine.
    /// </summary>
    public void StartEngine() {

        if (!engineRunning)
            StartCoroutine(StartEngineDelayed());

    }

    /// <summary>
    /// Starts the engine.
    /// </summary>
    /// <param name="instantStart">If set to <c>true</c> instant start.</param>
    public void StartEngine(bool instantStart) {

        if (instantStart) {

            fuelInput = 1f;
            engineRunning = true;

        } else {

            StartCoroutine(StartEngineDelayed());

        }

    }

    /// <summary>
    /// Starts the engine delayed.
    /// </summary>
    /// <returns>The engine delayed.</returns>
    public IEnumerator StartEngineDelayed() {

        if (!engineRunning) {

            engineStartSound = NewAudioSource(RCC_Settings.Instance.audioMixer, gameObject, engineSoundPosition, "Engine Start AudioSource", 1, 10, 1, engineStartClip, false, true, true);

            if (engineStartSound.isPlaying)
                engineStartSound.Play();

            yield return new WaitForSeconds(1f);

            engineRunning = true;
            fuelInput = 1f;

        }

        yield return new WaitForSeconds(1f);

    }

    /// <summary>
    /// Kills the engine.
    /// </summary>
    public void KillEngine() {

        fuelInput = 0f;
        engineRunning = false;

    }

    /// <summary>
    /// Other visuals.
    /// </summary>
    private void OtherVisuals() {

        //Driver SteeringWheel Transform.
        if (SteeringWheel) {

            if (orgSteeringWheelRot.eulerAngles == Vector3.zero)
                orgSteeringWheelRot = SteeringWheel.transform.localRotation;

            switch (steeringWheelRotateAround) {

                case SteeringWheelRotateAround.XAxis:
                    SteeringWheel.transform.localRotation = orgSteeringWheelRot * Quaternion.AngleAxis(((steerInput * steerAngle) * -steeringWheelAngleMultiplier), Vector3.right);
                    break;

                case SteeringWheelRotateAround.YAxis:
                    SteeringWheel.transform.localRotation = orgSteeringWheelRot * Quaternion.AngleAxis(((steerInput * steerAngle) * -steeringWheelAngleMultiplier), Vector3.up);
                    break;

                case SteeringWheelRotateAround.ZAxis:
                    SteeringWheel.transform.localRotation = orgSteeringWheelRot * Quaternion.AngleAxis(((steerInput * steerAngle) * -steeringWheelAngleMultiplier), Vector3.forward);
                    break;

            }

        }

    }

    void Update() {

        Inputs();

        //Reversing Bool.
        if (brakeInput > .9f && transform.InverseTransformDirection(rigid.velocity).z < 1f && canGoReverseNow && automaticGear && !semiAutomaticGear && !changingGear && direction != -1 && !RCC_InputManager.logitechHShifterUsed)
            StartCoroutine(ChangeGear(-1));
        else if (throttleInput < .1f && transform.InverseTransformDirection(rigid.velocity).z > -1f && direction == -1 && !changingGear && automaticGear && !semiAutomaticGear && !RCC_InputManager.logitechHShifterUsed)
            StartCoroutine(ChangeGear(0));

        Audio();
        ResetCar();

        if (useDamage) {

            damage.UpdateRepair();
            damage.UpdateDamage();

        }

        OtherVisuals();

        indicatorTimer += Time.deltaTime;

        if (throttleInput >= .1f)
            launched += throttleInput * Time.deltaTime;
        else
            launched -= Time.deltaTime;

        launched = Mathf.Clamp01(launched);

        rearSidewaysSlip = RearLeftWheelCollider.wheelSlipAmountSideways + RearRightWheelCollider.wheelSlipAmountSideways;
        rearSidewaysSlip /= 2f;

        if (Mathf.Abs(rearSidewaysSlip) > .25f)
            driftingNow = true;
        else
            driftingNow = false;

        if (driftingNow && speed > 10f)
            driftAngle = rearSidewaysSlip * .75f;
        else
            driftAngle = 0f;

    }

    private void Inputs() {

        if (canControl) {

            if (!externalController) {

                inputs = RCC_InputManager.GetInputs();

                if (!automaticGear || semiAutomaticGear) {
                    if (!changingGear && !cutGas)
                        throttleInput = inputs.throttleInput;
                    else
                        throttleInput = 0f;
                } else {
                    if (!changingGear && !cutGas)
                        throttleInput = (direction == 1 ? Mathf.Clamp01(inputs.throttleInput) : Mathf.Clamp01(inputs.brakeInput));
                    else
                        throttleInput = 0f;
                }

                if (!automaticGear || semiAutomaticGear) {
                    brakeInput = Mathf.Clamp01(inputs.brakeInput);
                } else {
                    if (!cutGas)
                        brakeInput = (direction == 1 ? Mathf.Clamp01(inputs.brakeInput) : Mathf.Clamp01(inputs.throttleInput));
                    else
                        brakeInput = 0f;
                }

                if (useSteeringSensitivity) {

                    oppositeDirection = Mathf.Sign(inputs.steerInput) != Mathf.Sign(steerInput) ? true : false;
                    steerInput = Mathf.MoveTowards(steerInput, inputs.steerInput + counterSteerInput, (Time.deltaTime * steeringSensitivityFactor * Mathf.Lerp(10f, 5f, steerAngle / orgSteerAngle)) * (oppositeDirection ? 1f : 1f));

                } else {
                    steerInput = inputs.steerInput + counterSteerInput;
                }

                SteeringAssistance();

                boostInput = inputs.boostInput;
                handbrakeInput = inputs.handbrakeInput;

                if (RCC_InputManager.logitechHShifterUsed) {

                    currentGear = inputs.gearInput;

                    if (currentGear == -1) {

                        currentGear = 0;
                        direction = -1;

                    } else {

                        direction = 1;

                    }

                    if (currentGear == -2) {

                        currentGear = 0;
                        NGear = true;

                    } else {

                        NGear = false;

                    }

                }

                if (!useAutomaticClutch) {

                    if (!NGear)
                        clutchInput = inputs.clutchInput;
                    else
                        clutchInput = 1f;

                }

            }

        } else if (!externalController) {

            throttleInput = 0f;
            brakeInput = 0f;
            steerInput = 0f;
            boostInput = 0f;
            handbrakeInput = 1f;

        }

        if (fuelInput <= 0f) {

            throttleInput = 0f;
            engineRunning = false;

        }

        if (changingGear || cutGas)
            throttleInput = 0f;

        if (!useNOS || NoS < 5 || throttleInput < .75f)
            boostInput = 0f;

        throttleInput = Mathf.Clamp01(throttleInput);
        brakeInput = Mathf.Clamp01(brakeInput);
        steerInput = Mathf.Clamp(steerInput, -1f, 1f);
        boostInput = Mathf.Clamp01(boostInput);
        handbrakeInput = Mathf.Clamp01(handbrakeInput);

        if (RCC_InputManager.logitechSteeringUsed) {

            steeringType = SteeringType.Constant;
            useSteeringLimiter = false;
            useSteeringSensitivity = false;
            useCounterSteering = false;

        }

    }

    private void SteeringAssistance() {

        if (speed < 5f)
            return;

        sidewaysSlip = 0f;        //	Total sideways slip of all wheels.

        foreach (RCC_WheelCollider w in allWheelColliders)
            sidewaysSlip += w.wheelSlipAmountSideways;

        sidewaysSlip /= allWheelColliders.Length;

        if (useSteeringLimiter) {

            maxSteerInput = Mathf.Clamp(1f - Mathf.Abs(sidewaysSlip), -1f, 1f);      //	Subtract total average sideways slip from max steer input (1f).;
            sign = -Mathf.Sign(sidewaysSlip);      //	Is sideways slip is left or right?

            //	If slip is high enough, apply counter input.
            if (maxSteerInput > 0f)
                steerInput = Mathf.Clamp(steerInput, -maxSteerInput, maxSteerInput);
            else
                steerInput = Mathf.Clamp(steerInput, sign * maxSteerInput, sign * maxSteerInput);

        }

        if (useCounterSteering)
            counterSteerInput = counterSteeringFactor * driftAngle;

    }

    void FixedUpdate() {

        if ((autoGenerateEngineRPMCurve) && oldEngineTorque != maxEngineTorque || oldMaxTorqueAtRPM != maxEngineTorqueAtRPM || minEngineRPM != oldMinEngineRPM || maxEngineRPM != oldMaxEngineRPM)
            ReCreateEngineTorqueCurve();

        if (gears == null || gears.Length == 0) {

            print("Gear can not be 0! Recreating gears...");
            InitGears();

        }

        currentPoweredWheels = 0;

        for (int i = 0; i < allWheelColliders.Length; i++) {

            if (allWheelColliders[i].canPower)
                currentPoweredWheels++;

        }

        poweredWheels = currentPoweredWheels;

        Engine();
        EngineSounds();
        Wheels();

        if (canControl) {

            GearBox();
            Clutch();

        }

        AntiRollBars();
        CheckGrounded();
        RevLimiter();
        Turbo();
        NOS();

        if (useFuelConsumption)
            Fuel();

        if (useEngineHeat)
            EngineHeat();

        if (steeringHelper)
            SteerHelper();

        if (tractionHelper)
            TractionHelper();

        if (angularDragHelper)
            AngularDragHelper();

        if (ESP)
            ESPCheck(FrontLeftWheelCollider.wheelCollider.steerAngle);

        if (RCC_Settings.Instance.selectedBehaviorType != null && RCC_Settings.Instance.selectedBehaviorType.applyRelativeTorque) {

            // If current selected behavior has apply relative torque enabled, and wheel is grounded, apply it.
            if (isGrounded)
                rigid.AddRelativeTorque(Vector3.up * (((steerInput * throttleInput) * direction)) * Mathf.Lerp(1.5f, .5f, speed / 200f), ForceMode.Acceleration);

        }

        // Setting centre of mass.
        rigid.centerOfMass = transform.InverseTransformPoint(COM.transform.position);

        // Applying downforce.
        rigid.AddForceAtPosition(-transform.up * (Mathf.Clamp(transform.InverseTransformDirection(rigid.velocity).z, 0f, 300f) * downForce), COM.transform.position, ForceMode.Force);

    }

    /// <summary>
    /// Engine.
    /// </summary>
    private void Engine() {

        //Speed.
        speed = rigid.velocity.magnitude * 3.6f;

        switch (steeringType) {

            case SteeringType.Curve:
                steerAngle = steerAngleCurve.Evaluate(speed);
                break;

            case SteeringType.Simple:
                steerAngle = Mathf.Lerp(orgSteerAngle, highspeedsteerAngle, (speed / highspeedsteerAngleAtspeed));
                break;

            case SteeringType.Constant:
                steerAngle = orgSteerAngle;
                break;

        }

        wheelRPM = 0;

        for (int i = 0; i < allWheelColliders.Length; i++) {

            if (allWheelColliders[i].canPower)
                wheelRPM += Mathf.Abs(allWheelColliders[i].wheelCollider.rpm);

        }

        velocity = 0f;
        newEngineInertia = engineInertia + (clutchInput / 5f * engineInertia);
        newEngineInertia *= Mathf.Lerp(1f, Mathf.Clamp(clutchInput, .75f, 1f), engineRPM / maxEngineRPM);

        engineRPMRaw = Mathf.SmoothDamp(engineRPMRaw, (Mathf.Lerp(minEngineRPM, maxEngineRPM + 500f, (clutchInput * throttleInput)) +
            ((wheelRPM / Mathf.Clamp(poweredWheels, 1, Mathf.Infinity)) *
            finalRatio * (gears[currentGear].maxRatio)) * (1f - clutchInput)) *
            fuelInput, ref velocity, newEngineInertia * .75f);

        engineRPMRaw = Mathf.Clamp(engineRPMRaw, 0f, maxEngineRPM + 500f);

        engineRPM = Mathf.Lerp(engineRPM, engineRPMRaw, Time.fixedDeltaTime * Mathf.Clamp(1f - clutchInput, .25f, 1f) * 25f);

        //Auto Reverse Bool.
        if (autoReverse) {

            canGoReverseNow = true;

        } else {

            if (brakeInput < .5f && speed < 5)
                canGoReverseNow = true;
            else if (brakeInput > 0 && transform.InverseTransformDirection(rigid.velocity).z > 1f)
                canGoReverseNow = false;

        }

    }

    /// <summary>
    /// Audio.
    /// </summary>
    private void Audio() {

        windSound.volume = Mathf.Lerp(0f, RCC_Settings.Instance.maxWindSoundVolume, speed / 300f);
        windSound.pitch = UnityEngine.Random.Range(.9f, 1f);

        if (direction == 1)
            brakeSound.volume = Mathf.Lerp(0f, RCC_Settings.Instance.maxBrakeSoundVolume, Mathf.Clamp01((FrontLeftWheelCollider.wheelCollider.brakeTorque + FrontRightWheelCollider.wheelCollider.brakeTorque) / (brakeTorque * 2f)) * Mathf.Lerp(0f, 1f, FrontLeftWheelCollider.wheelCollider.rpm / 50f));
        else
            brakeSound.volume = 0f;

    }

    /// <summary>
    /// Checks the ESP.
    /// </summary>
    /// <param name="steering">Steering.</param>
    private void ESPCheck(float steering) {

        if (ESPBroken) {

            frontSlip = 0f;
            rearSlip = 0f;
            underSteering = false;
            overSteering = false;
            ESPAct = false;
            return;

        }

        frontSlip = FrontLeftWheelCollider.wheelSlipAmountSideways + FrontRightWheelCollider.wheelSlipAmountSideways;
        rearSlip = RearLeftWheelCollider.wheelSlipAmountSideways + RearRightWheelCollider.wheelSlipAmountSideways;

        if (Mathf.Abs(frontSlip) >= ESPThreshold)
            underSteering = true;
        else
            underSteering = false;

        if (Mathf.Abs(rearSlip) >= ESPThreshold)
            overSteering = true;
        else
            overSteering = false;

        if (overSteering || underSteering)
            ESPAct = true;
        else
            ESPAct = false;

    }

    /// <summary>
    /// Engine sounds.
    /// </summary>
    private void EngineSounds() {

        lowRPM = 0f;
        medRPM = 0f;
        highRPM = 0f;

        if (engineRPM < ((maxEngineRPM) / 2f))
            lowRPM = Mathf.Lerp(0f, 1f, engineRPM / ((maxEngineRPM) / 2f));
        else
            lowRPM = Mathf.Lerp(1f, .25f, engineRPM / maxEngineRPM);

        if (engineRPM < ((maxEngineRPM) / 2f))
            medRPM = Mathf.Lerp(-.5f, 1f, engineRPM / ((maxEngineRPM) / 2f));
        else
            medRPM = Mathf.Lerp(1f, .5f, engineRPM / maxEngineRPM);

        highRPM = Mathf.Lerp(-1f, 1f, engineRPM / maxEngineRPM);

        lowRPM = Mathf.Clamp01(lowRPM) * maxEngineSoundVolume;
        medRPM = Mathf.Clamp01(medRPM) * maxEngineSoundVolume;
        highRPM = Mathf.Clamp01(highRPM) * maxEngineSoundVolume;

        volumeLevel = Mathf.Clamp(throttleInput, 0f, 1f);
        pitchLevel = Mathf.Lerp(minEngineSoundPitch, maxEngineSoundPitch, engineRPM / maxEngineRPM) * (engineRunning ? 1f : 0f);

        switch (audioType) {

            case RCC_CarControllerV3.AudioType.OneSource:

                engineSoundHigh.volume = volumeLevel * maxEngineSoundVolume;
                engineSoundHigh.pitch = pitchLevel;

                engineSoundHighOff.volume = (1f - volumeLevel) * maxEngineSoundVolume;
                engineSoundHighOff.pitch = pitchLevel;

                if (engineSoundIdle) {

                    engineSoundIdle.volume = Mathf.Lerp(engineRunning ? idleEngineSoundVolume : 0f, 0f, engineRPM / maxEngineRPM);
                    engineSoundIdle.pitch = pitchLevel;

                }

                if (!engineSoundHigh.isPlaying)
                    engineSoundHigh.Play();
                if (!engineSoundIdle.isPlaying)
                    engineSoundIdle.Play();

                break;

            case RCC_CarControllerV3.AudioType.TwoSource:

                engineSoundHigh.volume = highRPM * volumeLevel;
                engineSoundHigh.pitch = pitchLevel;
                engineSoundLow.volume = lowRPM * volumeLevel;
                engineSoundLow.pitch = pitchLevel;

                engineSoundHighOff.volume = highRPM * (1f - volumeLevel);
                engineSoundHighOff.pitch = pitchLevel;
                engineSoundLowOff.volume = lowRPM * (1f - volumeLevel);
                engineSoundLowOff.pitch = pitchLevel;

                if (engineSoundIdle) {

                    engineSoundIdle.volume = Mathf.Lerp(engineRunning ? idleEngineSoundVolume : 0f, 0f, engineRPM / maxEngineRPM);
                    engineSoundIdle.pitch = pitchLevel;

                }

                if (!engineSoundLow.isPlaying)
                    engineSoundLow.Play();
                if (!engineSoundHigh.isPlaying)
                    engineSoundHigh.Play();
                if (!engineSoundIdle.isPlaying)
                    engineSoundIdle.Play();

                break;

            case RCC_CarControllerV3.AudioType.ThreeSource:

                engineSoundHigh.volume = highRPM * volumeLevel;
                engineSoundHigh.pitch = pitchLevel;
                engineSoundMed.volume = medRPM * volumeLevel;
                engineSoundMed.pitch = pitchLevel;
                engineSoundLow.volume = lowRPM * volumeLevel;
                engineSoundLow.pitch = pitchLevel;

                engineSoundHighOff.volume = highRPM * (1f - volumeLevel);
                engineSoundHighOff.pitch = pitchLevel;
                engineSoundMedOff.volume = medRPM * (1f - volumeLevel);
                engineSoundMedOff.pitch = pitchLevel;
                engineSoundLowOff.volume = lowRPM * (1f - volumeLevel);
                engineSoundLowOff.pitch = pitchLevel;

                if (engineSoundIdle) {

                    engineSoundIdle.volume = Mathf.Lerp(engineRunning ? idleEngineSoundVolume : 0f, 0f, engineRPM / maxEngineRPM);
                    engineSoundIdle.pitch = pitchLevel;

                }

                if (engineSoundLow.gameObject.activeSelf && !engineSoundLow.isPlaying)
                    engineSoundLow.Play();
                if (!engineSoundMed.gameObject.activeSelf && engineSoundMed.isPlaying)
                    engineSoundMed.Play();
                if (!engineSoundHigh.gameObject.activeSelf && engineSoundHigh.isPlaying)
                    engineSoundHigh.Play();
                if (!engineSoundIdle.gameObject.activeSelf && engineSoundIdle.isPlaying)
                    engineSoundIdle.Play();

                break;

        }

    }

    private void Wheels() {

        for (int i = 0; i < allWheelColliders.Length; i++) {
            if (allWheelColliders[i].canPower)
            {
                // Calculate the base motor torque.
                float motorTorque = (direction * allWheelColliders[i].powerMultiplier * (1f - clutchInput) * throttleInput *
                                     (1f + boostInput) * (engineTorqueCurve.Evaluate(engineRPM) * gears[currentGear].maxRatio * finalRatio))
                                    / Mathf.Clamp(poweredWheels, 1, Mathf.Infinity);
                // Apply the AI speed multiplier.
                motorTorque *= aiSpeedMultiplier;

                // Feed the modified torque to the wheel collider.
                allWheelColliders[i].ApplyMotorTorque(motorTorque);
            }

            if (allWheelColliders[i].canSteer)
                allWheelColliders[i].ApplySteering(steerInput * allWheelColliders[i].steeringMultiplier, steerAngle);

            appliedBrake = false;

            if (!appliedBrake && handbrakeInput > .5f) {

                appliedBrake = true;

                if (allWheelColliders[i].canHandbrake)
                    allWheelColliders[i].ApplyBrakeTorque((brakeTorque * handbrakeInput) * allWheelColliders[i].handbrakeMultiplier);

            }

            if (!appliedBrake && brakeInput >= .05f) {

                appliedBrake = true;

                if (allWheelColliders[i].canBrake)
                    allWheelColliders[i].ApplyBrakeTorque((brakeInput * brakeTorque) * allWheelColliders[i].brakingMultiplier);
                
            }

            if (ESPAct)
                appliedBrake = true;

            if (!appliedBrake)
                allWheelColliders[i].ApplyBrakeTorque(0f);

            //	Checking all wheels. If one of them is not powered, reset.
            if (!allWheelColliders[i].canPower)
                allWheelColliders[i].ApplyMotorTorque(0f);
            if (!allWheelColliders[i].canBrake)
                allWheelColliders[i].ApplyBrakeTorque(0f);
            if (!allWheelColliders[i].canSteer)
                allWheelColliders[i].ApplySteering(0f, 0f);

        }

    }

    /// <summary>
    /// Antiroll bars.
    /// </summary>
    private void AntiRollBars() {

        #region Horizontal

        travelFL = 1f;
        travelFR = 1f;

        groundedFL = FrontLeftWheelCollider.isGrounded;

        if (groundedFL)
            travelFL = (-FrontLeftWheelCollider.transform.InverseTransformPoint(FrontLeftWheelCollider.wheelHit.point).y - FrontLeftWheelCollider.wheelCollider.radius) / FrontLeftWheelCollider.wheelCollider.suspensionDistance;

        groundedFR = FrontRightWheelCollider.isGrounded;

        if (groundedFR)
            travelFR = (-FrontRightWheelCollider.transform.InverseTransformPoint(FrontRightWheelCollider.wheelHit.point).y - FrontRightWheelCollider.wheelCollider.radius) / FrontRightWheelCollider.wheelCollider.suspensionDistance;

        antiRollForceFrontHorizontal = (travelFL - travelFR) * antiRollFrontHorizontal;

        if (FrontLeftWheelCollider.isActiveAndEnabled && FrontRightWheelCollider.isActiveAndEnabled) {

            if (groundedFL)
                rigid.AddForceAtPosition(FrontLeftWheelCollider.transform.up * -antiRollForceFrontHorizontal, FrontLeftWheelCollider.transform.position);
            if (groundedFR)
                rigid.AddForceAtPosition(FrontRightWheelCollider.transform.up * antiRollForceFrontHorizontal, FrontRightWheelCollider.transform.position);

        }

        travelRL = 1f;
        travelRR = 1f;

        groundedRL = RearLeftWheelCollider.isGrounded;

        if (groundedRL)
            travelRL = (-RearLeftWheelCollider.transform.InverseTransformPoint(RearLeftWheelCollider.wheelHit.point).y - RearLeftWheelCollider.wheelCollider.radius) / RearLeftWheelCollider.wheelCollider.suspensionDistance;

        groundedRR = RearRightWheelCollider.isGrounded;

        if (groundedRR)
            travelRR = (-RearRightWheelCollider.transform.InverseTransformPoint(RearRightWheelCollider.wheelHit.point).y - RearRightWheelCollider.wheelCollider.radius) / RearRightWheelCollider.wheelCollider.suspensionDistance;

        antiRollForceRearHorizontal = (travelRL - travelRR) * antiRollRearHorizontal;

        if (RearLeftWheelCollider.isActiveAndEnabled && RearRightWheelCollider.isActiveAndEnabled) {

            if (groundedRL)
                rigid.AddForceAtPosition(RearLeftWheelCollider.transform.up * -antiRollForceRearHorizontal, RearLeftWheelCollider.transform.position);
            if (groundedRR)
                rigid.AddForceAtPosition(RearRightWheelCollider.transform.up * antiRollForceRearHorizontal, RearRightWheelCollider.transform.position);

        }

        #endregion

        #region Vertical

        antiRollForceFrontVertical = (travelFL - travelRL) * antiRollVertical;

        if (FrontLeftWheelCollider.isActiveAndEnabled && RearLeftWheelCollider.isActiveAndEnabled) {

            if (groundedFL)
                rigid.AddForceAtPosition(FrontLeftWheelCollider.transform.up * -antiRollForceFrontVertical, FrontLeftWheelCollider.transform.position);
            if (groundedRL)
                rigid.AddForceAtPosition(RearLeftWheelCollider.transform.up * antiRollForceFrontVertical, RearLeftWheelCollider.transform.position);

        }

        antiRollForceRearVertical = (travelFR - travelRR) * antiRollVertical;

        if (FrontRightWheelCollider.isActiveAndEnabled && RearRightWheelCollider.isActiveAndEnabled) {

            if (groundedFR)
                rigid.AddForceAtPosition(FrontRightWheelCollider.transform.up * -antiRollForceRearVertical, FrontRightWheelCollider.transform.position);
            if (groundedRR)
                rigid.AddForceAtPosition(RearRightWheelCollider.transform.up * antiRollForceRearVertical, RearRightWheelCollider.transform.position);

        }

        #endregion

    }

    public void CheckGrounded() {

        grounded = false;

        for (int i = 0; i < allWheelColliders.Length; i++) {

            if (allWheelColliders[i].wheelCollider.isGrounded)
                grounded = true;

        }

        isGrounded = grounded;

    }

    /// <summary>
    /// Steering helper.
    /// </summary>
    private void SteerHelper() {

        if (!isGrounded)
            return;

        if (!steeringDirection || !velocityDirection) {

            if (!steeringDirection) {

                GameObject steeringDirectionGO = new GameObject("Steering Direction");
                steeringDirectionGO.transform.SetParent(transform, false);
                steeringDirection = steeringDirectionGO.transform;
                steeringDirectionGO.transform.localPosition.Set(1f, 2f, 0f);/* = new Vector3(1f, 2f, 0f);*/
                steeringDirectionGO.transform.localScale.Set(.1f, .1f, 3f);/* = new Vector3(.1f, .1f, 3f);*/

            }

            if (!velocityDirection) {

                GameObject velocityDirectionGO = new GameObject("Velocity Direction");
                velocityDirectionGO.transform.SetParent(transform, false);
                velocityDirection = velocityDirectionGO.transform;
                velocityDirectionGO.transform.localPosition.Set(-1f, 2f, 0f);/* = new Vector3(-1f, 2f, 0f);*/
                velocityDirectionGO.transform.localScale.Set(.1f, .1f, 3f);/* = new Vector3(.1f, .1f, 3f);*/

            }

            return;

        }

        for (int i = 0; i < allWheelColliders.Length; i++) {

            if (allWheelColliders[i].wheelHit.point == Vector3.zero)
                return;

        }

        v = rigid.angularVelocity;
        velocityAngle = (v.y * Mathf.Clamp(transform.InverseTransformDirection(rigid.velocity).z, -1f, 1f)) * Mathf.Rad2Deg;
        velocityDirection.localRotation = Quaternion.Lerp(velocityDirection.localRotation, Quaternion.AngleAxis(Mathf.Clamp(velocityAngle / 3f, -45f, 45f), Vector3.up), Time.fixedDeltaTime * 20f);
        steeringDirection.localRotation = Quaternion.Euler(0f, FrontLeftWheelCollider.wheelCollider.steerAngle, 0f);

        normalizer = 1;

        if (steeringDirection.localRotation.y > velocityDirection.localRotation.y)
            normalizer = 1;
        else
            normalizer = -1;

        angle2 = Quaternion.Angle(velocityDirection.localRotation, steeringDirection.localRotation) * (normalizer);

        rigid.AddRelativeTorque(Vector3.up * ((angle2 * (Mathf.Clamp(transform.InverseTransformDirection(rigid.velocity).z, -10f, 10f) / 1000f)) * steerHelperAngularVelStrength), ForceMode.VelocityChange);

        if (Mathf.Abs(oldRotation - transform.eulerAngles.y) < 10f) {

            turnadjust = (transform.eulerAngles.y - oldRotation) * (steerHelperLinearVelStrength / 2f);
            velRotation = Quaternion.AngleAxis(turnadjust, Vector3.up);
            rigid.velocity = (velRotation * rigid.velocity);

        }

        oldRotation = transform.eulerAngles.y;

    }

    /// <summary>
    /// Traction helper.
    /// </summary>
    private void TractionHelper() {

        if (!isGrounded)
            return;

        velocitys = rigid.velocity;
        velocitys -= transform.up * Vector3.Dot(velocitys, transform.up);
        velocitys.Normalize();

        angle = -Mathf.Asin(Vector3.Dot(Vector3.Cross(transform.forward, velocitys), transform.up));

        angularVelo = rigid.angularVelocity.y;

        if (angle * FrontLeftWheelCollider.wheelCollider.steerAngle < 0)
            FrontLeftWheelCollider.tractionHelpedSidewaysStiffness = (1f - Mathf.Clamp01(tractionHelperStrength * Mathf.Abs(angularVelo)));
        else
            FrontLeftWheelCollider.tractionHelpedSidewaysStiffness = 1f;

        if (angle * FrontRightWheelCollider.wheelCollider.steerAngle < 0)
            FrontRightWheelCollider.tractionHelpedSidewaysStiffness = (1f - Mathf.Clamp01(tractionHelperStrength * Mathf.Abs(angularVelo)));
        else
            FrontRightWheelCollider.tractionHelpedSidewaysStiffness = 1f;

    }

    /// <summary>
    /// Angular drag helper.
    /// </summary>
    private void AngularDragHelper() {

        rigid.angularDrag = Mathf.Lerp(0f, 10f, (speed * angularDragHelperStrength) / 1000f);

    }

    /// <summary>
    /// Clutch.
    /// </summary>
    private void Clutch() {

        if (!useAutomaticClutch)
            return;

        wheelRPM = 0;

        for (int i = 0; i < allWheelColliders.Length; i++) {

            if (allWheelColliders[i].canPower)
                wheelRPM += Mathf.Abs(allWheelColliders[i].wheelCollider.rpm);

        }

        if (currentGear == 0) {

            if (launched >= .25f)
                clutchInput = Mathf.Lerp(clutchInput, (Mathf.Lerp(1f, (Mathf.Lerp(clutchInertia, 0f, (wheelRPM / Mathf.Clamp(poweredWheels, 1, Mathf.Infinity)) / gears[0].targetSpeedForNextGear)), Mathf.Abs(throttleInput))), Time.fixedDeltaTime * 20f);
            else
                clutchInput = Mathf.Lerp(clutchInput, 1f / speed, Time.fixedDeltaTime * 20f);

        } else {

            if (changingGear)
                clutchInput = Mathf.Lerp(clutchInput, 1, Time.fixedDeltaTime * 20f);
            else
                clutchInput = Mathf.Lerp(clutchInput, 0, Time.fixedDeltaTime * 20f);

        }

        if (cutGas || handbrakeInput >= .1f)
            clutchInput = 1f;

        if (NGear)
            clutchInput = 1f;

        clutchInput = Mathf.Clamp01(clutchInput);

    }

    /// <summary>
    /// Gearbox.
    /// </summary>
    private void GearBox() {

        if (automaticGear && !RCC_InputManager.logitechHShifterUsed) {

            if (currentGear < gears.Length - 1 && !changingGear) {

                if (direction == 1 && speed >= gears[currentGear].targetSpeedForNextGear && engineRPM >= gearShiftUpRPM) {

                    if (!semiAutomaticGear)
                        StartCoroutine(ChangeGear(currentGear + 1));
                    else if (semiAutomaticGear && direction != -1)
                        StartCoroutine(ChangeGear(currentGear + 1));

                }

            }

            if (currentGear > 0) {

                if (!changingGear) {

                    if (direction != -1 && speed < gears[currentGear - 1].targetSpeedForNextGear && engineRPM <= gearShiftDownRPM)
                        StartCoroutine(ChangeGear(currentGear - 1));

                }

            }

        }

        if (direction == -1) {

            if (!reversingSound.isPlaying)
                reversingSound.Play();

            reversingSound.volume = Mathf.Lerp(0f, 1f, speed / gears[0].maxSpeed);
            reversingSound.pitch = reversingSound.volume;

        } else {

            if (reversingSound.isPlaying)
                reversingSound.Stop();

            reversingSound.volume = 0f;
            reversingSound.pitch = 0f;

        }

    }

    /// <summary>
    /// Changes the gear.
    /// </summary>
    /// <returns>The gear.</returns>
    /// <param name="gear">Gear.</param>
    public IEnumerator ChangeGear(int gear) {

        changingGear = true;

        if (RCC_Settings.Instance.useTelemetry)
            print("Shifted to: " + (gear).ToString());

        if (gearShiftingClips.Length > 0) {

            gearShiftingSound = NewAudioSource(RCC_Settings.Instance.audioMixer, gameObject, gearSoundPosition, "Gear Shifting AudioSource", 1f, 5f, RCC_Settings.Instance.maxGearShiftingSoundVolume, gearShiftingClips[UnityEngine.Random.Range(0, gearShiftingClips.Length)], false, true, true);

            if (!gearShiftingSound.isPlaying)
                gearShiftingSound.Play();

        }

        yield return new WaitForSeconds(gearShiftingDelay);

        if (gear == -1) {

            currentGear = 0;

            if (!NGear)
                direction = -1;
            else
                direction = 0;

        } else {

            currentGear = gear;

            if (!NGear)
                direction = 1;
            else
                direction = 0;

        }

        changingGear = false;

    }

    /// <summary>
    /// Gears the shift up.
    /// </summary>
    public void GearShiftUp() {

        if (currentGear < gears.Length - 1 && !changingGear) {

            if (direction != -1)
                StartCoroutine(ChangeGear(currentGear + 1));
            else
                StartCoroutine(ChangeGear(0));

        }

    }

    /// <summary>
    /// Gears the shift to.
    /// </summary>
    public void GearShiftTo(int gear) {

        if (gear < -1 || gear >= gears.Length)
            return;

        if (gear == currentGear)
            return;

        StartCoroutine(ChangeGear(gear));

    }

    /// <summary>
    /// Gears the shift down.
    /// </summary>
    public void GearShiftDown() {

        if (currentGear >= 0)
            StartCoroutine(ChangeGear(currentGear - 1));

    }

    /// <summary>
    /// Rev limiter.
    /// </summary>
    private void RevLimiter() {

        if ((useRevLimiter && engineRPM >= maxEngineRPM))
            cutGas = true;
        else if (engineRPM < (maxEngineRPM * .975f))
            cutGas = false;

    }

    /// <summary>
    /// NOS.
    /// </summary>
    private void NOS() {

        if (!useNOS)
            return;

        if (!NOSSound)
            NOSSound = NewAudioSource(RCC_Settings.Instance.audioMixer, gameObject, exhaustSoundPosition, "NOS Sound AudioSource", 5f, 10f, .5f, NOSClip, true, false, false);

        if (!blowSound)
            blowSound = NewAudioSource(RCC_Settings.Instance.audioMixer, gameObject, exhaustSoundPosition, "NOS Blow", 1f, 10f, .5f, null, false, false, false);

        if (boostInput >= .8f && throttleInput >= .8f && NoS > 5) {

            NoS -= NoSConsumption * Time.fixedDeltaTime;
            NoSRegenerateTime = 0f;

            if (!NOSSound.isPlaying)
                NOSSound.Play();

        } else {

            if (NoS < 100 && NoSRegenerateTime > 3)
                NoS += (NoSConsumption / 1.5f) * Time.fixedDeltaTime;

            NoSRegenerateTime += Time.fixedDeltaTime;

            if (NOSSound.isPlaying) {

                NOSSound.Stop();
                blowSound.clip = blowClip[UnityEngine.Random.Range(0, blowClip.Length)];
                blowSound.Play();

            }

        }

    }

    /// <summary>
    /// Turbo.
    /// </summary>
    private void Turbo() {

        if (!useTurbo)
            return;

        if (!turboSound) {

            turboSound = NewAudioSource(RCC_Settings.Instance.audioMixer, gameObject, turboSoundPosition, "Turbo Sound AudioSource", .1f, .5f, 0, turboClip, true, true, false);
            NewHighPassFilter(turboSound, 10000f, 10);

        }

        turboBoost = Mathf.Lerp(turboBoost, Mathf.Clamp(Mathf.Pow(throttleInput, 10) * 30f + Mathf.Pow(engineRPM / maxEngineRPM, 10) * 30f, 0f, 30f), Time.fixedDeltaTime * 10f);

        if (turboBoost >= 25f) {

            if (turboBoost < (turboSound.volume * 30f)) {

                if (!blowSound.isPlaying) {

                    blowSound.clip = RCC_Settings.Instance.blowoutClip[UnityEngine.Random.Range(0, RCC_Settings.Instance.blowoutClip.Length)];
                    blowSound.Play();

                }

            }

        }

        turboSound.volume = Mathf.Lerp(turboSound.volume, turboBoost / 30f, Time.fixedDeltaTime * 5f);
        turboSound.pitch = Mathf.Lerp(Mathf.Clamp(turboSound.pitch, 2f, 3f), (turboBoost / 30f) * 2f, Time.fixedDeltaTime * 5f);

    }

    /// <summary>
    /// Fuel.
    /// </summary>
    private void Fuel() {

        fuelTank -= ((engineRPM / 10000f) * fuelConsumptionRate) * Time.fixedDeltaTime;
        fuelTank = Mathf.Clamp(fuelTank, 0f, fuelTankCapacity);

        if (fuelTank <= 0f)
            fuelInput = 0f;

    }

    /// <summary>
    /// Engine heat.
    /// </summary>
    private void EngineHeat() {

        engineHeat += ((engineRPM / 10000f) * engineHeatRate) * Time.fixedDeltaTime;

        if (engineHeat > engineCoolingWaterThreshold)
            engineHeat -= engineCoolRate * Time.fixedDeltaTime;

        engineHeat -= (engineCoolRate / 10f) * Time.fixedDeltaTime;

        engineHeat = Mathf.Clamp(engineHeat, 15f, 120f);

    }

    /// <summary>
    /// Resets the car.
    /// </summary>
    private void ResetCar() {

        if (speed < 5 && !rigid.isKinematic) {

            if (!RCC_Settings.Instance.autoReset)
                return;

            if (transform.eulerAngles.z < 300 && transform.eulerAngles.z > 60) {

                resetTime += Time.deltaTime;

                if (resetTime > 3) {

                    transform.rotation = Quaternion.Euler(0f, transform.eulerAngles.y, 0f);
                    transform.position = new Vector3(transform.position.x, transform.position.y + 3, transform.position.z);
                    resetTime = 0f;

                }

            }

        }

    }

    /// <summary>
    /// Raises the collision enter event.
    /// </summary>
    /// <param name="collision">Collision.</param>
    void OnCollisionEnter(Collision collision) {

        if (collision.contactCount < 1)
            return;

        if (collision.relativeVelocity.magnitude < 5)
            return;

        if (OnRCCPlayerCollision != null && this == RCC_SceneManager.Instance.activePlayerVehicle)
            OnRCCPlayerCollision(this, collision);

        if (((1 << collision.gameObject.layer) & damage.damageFilter) != 0) {

            if (useDamage) {

                if (!damage.carController)
                    damage.Initialize(this);

                damage.OnCollision(collision);

            }

            if (useCollisionAudio) {

                if (crashClips.Length > 0) {

                    if (collision.GetContact(0).thisCollider.gameObject.transform != transform.parent) {

                        crashSound = NewAudioSource(RCC_Settings.Instance.audioMixer, gameObject, "Crash Sound AudioSource", 5f, 20f, RCC_Settings.Instance.maxCrashSoundVolume * (collision.impulse.magnitude / 10000f), crashClips[UnityEngine.Random.Range(0, crashClips.Length)], false, true, true);

                        if (!crashSound.isPlaying)
                            crashSound.Play();

                    }

                }

            }

            if (useCollisionParticles) {

                // Particle System used for collision effects. Creating it at start. We will use this when we collide something.
                if (contactSparkle && contactSparkeList.Count < 1) {

                    for (int i = 0; i < maximumContactSparkle; i++) {

                        GameObject sparks = Instantiate(contactSparkle, transform.position, Quaternion.identity);
                        sparks.transform.SetParent(allContactParticles.transform);
                        contactSparkeList.Add(sparks.GetComponent<ParticleSystem>());
                        ParticleSystem.EmissionModule em = sparks.GetComponent<ParticleSystem>().emission;
                        em.enabled = false;

                    }

                }

                for (int i = 0; i < contactSparkeList.Count; i++) {

                    if (!contactSparkeList[i].isPlaying) {

                        contactSparkeList[i].transform.position = collision.GetContact(0).point;
                        ParticleSystem.EmissionModule em = contactSparkeList[i].emission;
                        em.rateOverTimeMultiplier = collision.impulse.magnitude / 500f;
                        em.enabled = true;
                        contactSparkeList[i].Play();
                        break;

                    }

                }

            }

        }

    }

    private void OnCollisionStay(Collision collision) {

        if (collision.contactCount < 1 || collision.relativeVelocity.magnitude < 2f) {

            if (scratchSparkeList != null) {

                for (int i = 0; i < scratchSparkeList.Count; i++) {

                    ParticleSystem.EmissionModule em = scratchSparkeList[i].emission;
                    em.enabled = false;

                }

            }

            return;

        }

        if (OnRCCPlayerCollision != null && this == RCC_SceneManager.Instance.activePlayerVehicle)
            OnRCCPlayerCollision(this, collision);

        if (((1 << collision.gameObject.layer) & damage.damageFilter) != 0) {

            if (useCollisionParticles) {

                // Particle System used for collision effects. Creating it at start. We will use this when we collide something.
                if (scratchSparkle && scratchSparkeList.Count < 1) {

                    for (int i = 0; i < maximumContactSparkle; i++) {

                        GameObject sparks = Instantiate(scratchSparkle, transform.position, Quaternion.identity);
                        sparks.transform.SetParent(allContactParticles.transform);
                        scratchSparkeList.Add(sparks.GetComponent<ParticleSystem>());
                        ParticleSystem.EmissionModule em = sparks.GetComponent<ParticleSystem>().emission;
                        em.enabled = false;

                    }

                }

                ContactPoint[] contacts = new ContactPoint[collision.contactCount];
                collision.GetContacts(contacts);

                int ind = -1;

                foreach (ContactPoint cp in contacts) {

                    ind++;

                    if (ind < scratchSparkeList.Count && !scratchSparkeList[ind].isPlaying) {

                        scratchSparkeList[ind].transform.position = cp.point;
                        ParticleSystem.EmissionModule em = scratchSparkeList[ind].emission;
                        em.enabled = true;
                        em.rateOverTimeMultiplier = collision.relativeVelocity.magnitude / 1f;
                        scratchSparkeList[ind].Play();

                    }

                }

            }

        }

    }

    private void OnCollisionExit(Collision collision) {

        for (int i = 0; i < scratchSparkeList.Count; i++) {

            ParticleSystem.EmissionModule em = scratchSparkeList[i].emission;
            em.enabled = true;
            scratchSparkeList[i].Stop();

        }

    }

    /// <summary>
    /// Raises the draw gizmos event.
    /// </summary>
    void OnDrawGizmos() {
#if UNITY_EDITOR
        if (Application.isPlaying) {

            WheelHit hit;

            for (int i = 0; i < allWheelColliders.Length; i++) {

                allWheelColliders[i].wheelCollider.GetGroundHit(out hit);

                Matrix4x4 temp = Gizmos.matrix;
                Gizmos.matrix = Matrix4x4.TRS(allWheelColliders[i].transform.position, Quaternion.AngleAxis(-90, Vector3.right), Vector3.one);
                Gizmos.color = new Color((hit.force / rigid.mass) / 5f, (-hit.force / rigid.mass) / 5f, 0f);
                Gizmos.DrawFrustum(Vector3.zero, 2f, hit.force / rigid.mass, .1f, 1f);
                Gizmos.matrix = temp;

            }

        }
#endif
    }

    /// <summary>
    /// Previews the smoke particle.
    /// </summary>
    /// <param name="state">If set to <c>true</c> state.</param>
    public void PreviewSmokeParticle(bool state) {

        canControl = state;
        permanentGas = state;
        rigid.isKinematic = state;

    }

    /// <summary>
    /// Detachs the trailer.
    /// </summary>
    public void DetachTrailer() {

        if (!attachedTrailer)
            return;

        attachedTrailer.DetachTrailer();

    }

    /// <summary>
    /// Raises the destroy event.
    /// </summary>
    void OnDestroy() {

        if (OnRCCPlayerDestroyed != null)
            OnRCCPlayerDestroyed(this);

        if (canControl) {

            RCC_Camera rccCam = GetComponentInChildren<RCC_Camera>();

            if (rccCam)
                gameObject.GetComponentInChildren<RCC_Camera>().transform.SetParent(null);

        }

    }

    /// <summary>
    /// Sets the can control.
    /// </summary>
    /// <param name="state">If set to <c>true</c> state.</param>
    public void SetCanControl(bool state) {

        canControl = state;

    }

    /// <summary>
    /// Sets the external controller.
    /// </summary>
    /// <param name="state"></param>
    public void SetExternalControl(bool state) {

        externalController = state;

    }

    /// <summary>
    /// Sets the engine state.
    /// </summary>
    /// <param name="state">If set to <c>true</c> state.</param>
    public void SetEngine(bool state) {

        if (state)
            StartEngine();
        else
            KillEngine();

    }

    public void Repair() {

        damage.repairNow = true;

    }

    private void RCC_InputManager_OnTrailerDetach() {

        if (!canControl || externalController)
            return;

        DetachTrailer();

    }

    private void RCC_InputManager_OnGearShiftDown() {

        if (!canControl || externalController)
            return;

        GearShiftDown();

    }

    private void RCC_InputManager_OnGearShiftUp() {

        if (!canControl || externalController)
            return;

        GearShiftUp();

    }

    private void RCC_InputManager_OnNGear(bool state) {

        if (!canControl || externalController)
            return;

        NGear = state;

    }

    private void RCC_InputManager_OnIndicatorHazard() {

        if (!canControl || externalController)
            return;

        if (indicatorsOn != IndicatorsOn.All)
            indicatorsOn = IndicatorsOn.All;
        else
            indicatorsOn = IndicatorsOn.Off;

    }

    private void RCC_InputManager_OnIndicatorRight() {

        if (!canControl || externalController)
            return;

        if (indicatorsOn != IndicatorsOn.Right)
            indicatorsOn = IndicatorsOn.Right;
        else
            indicatorsOn = IndicatorsOn.Off;

    }

    private void RCC_InputManager_OnIndicatorLeft() {

        if (!canControl || externalController)
            return;

        if (indicatorsOn != IndicatorsOn.Left)
            indicatorsOn = IndicatorsOn.Left;
        else
            indicatorsOn = IndicatorsOn.Off;

    }

    private void RCC_InputManager_OnHighBeamHeadlights() {

        if (!canControl || externalController)
            return;

        highBeamHeadLightsOn = !highBeamHeadLightsOn;

    }

    private void RCC_InputManager_OnLowBeamHeadlights() {

        if (!canControl || externalController)
            return;

        lowBeamHeadLightsOn = !lowBeamHeadLightsOn;

    }

    private void RCC_InputManager_OnStartStopEngine() {

        if (!canControl || externalController)
            return;

        KillOrStartEngine();

    }

    void OnDisable() {

        RCC_SceneManager.OnBehaviorChanged -= CheckBehavior;

        // Listening input events.
        RCC_InputManager.OnStartStopEngine -= RCC_InputManager_OnStartStopEngine;
        RCC_InputManager.OnLowBeamHeadlights -= RCC_InputManager_OnLowBeamHeadlights;
        RCC_InputManager.OnHighBeamHeadlights -= RCC_InputManager_OnHighBeamHeadlights;
        RCC_InputManager.OnIndicatorLeft -= RCC_InputManager_OnIndicatorLeft;
        RCC_InputManager.OnIndicatorRight -= RCC_InputManager_OnIndicatorRight;
        RCC_InputManager.OnIndicatorHazard -= RCC_InputManager_OnIndicatorHazard;
        RCC_InputManager.OnGearShiftUp -= RCC_InputManager_OnGearShiftUp;
        RCC_InputManager.OnGearShiftDown -= RCC_InputManager_OnGearShiftDown;
        RCC_InputManager.OnNGear -= RCC_InputManager_OnNGear;
        RCC_InputManager.OnTrailerDetach -= RCC_InputManager_OnTrailerDetach;

    }

}
