//----------------------------------------------
//            Realistic Car Controller
//
// Copyright © 2014 - 2022 BoneCracker Games
// http://www.bonecrackergames.com
// Buğra Özdoğanlar
//
//----------------------------------------------

using UnityEngine;
using UnityEngine.AI;
using System;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// AI Controller of RCC. It's not professional, but it does the job. Follows all waypoints, or follows/chases the target gameobject.
/// </summary>
[RequireComponent(typeof(RCC_CarControllerV3))]
[AddComponentMenu("BoneCracker Games/Realistic Car Controller/AI/RCC AI Car Controller")]
public class RCC_AICarController : MonoBehaviour
{

    internal RCC_CarControllerV3 carController;     // Main RCC of this vehicle.

    public RCC_AIWaypointsContainer waypointsContainer;                 // Waypoints Container.
    public int currentWaypointIndex = 0;                                            // Current index in Waypoint Container.
    public string targetTag = "Player";                                 // Search and chase Gameobjects with tags.

    // AI Type
    public NavigationMode navigationMode;
    public enum NavigationMode { FollowWaypoints, ChaseTarget, FollowTarget }

    // Raycast distances used for detecting obstacles at front of the AI vehicle.
    [Range(5f, 50f)] public float raycastLength = 3f;
    [Range(10f, 90f)] public float raycastAngle = 30f;
    public LayerMask obstacleLayers = -1;
    public GameObject obstacle;

    public bool useRaycasts = true;     //	Using forward and sideways raycasts to avoid obstacles.
    private float rayInput = 0f;                // Total ray input affected by raycast distances.
    private bool raycasting = false;        // Raycasts hits an obstacle now?
    private float resetTime = 0f;           // This timer was used for deciding go back or not, after crashing.
    private bool reversingNow = false;

    // Steer, Motor, And Brake inputs. Will feed RCC_CarController with these inputs.
    public float steerInput = 0f;
    public float throttleInput = 0f;
    public float brakeInput = 0f;
    public float handbrakeInput = 0f;

    // Limit speed.
    public bool limitSpeed = false;
    public float maximumSpeed = 100f;

    // NEW: Speed Multiplier. Increase this value to make the AI go faster.
    public float speedMultiplier = 5.0f;

    // Smoothed steering.
    public bool smoothedSteer = true;

    // Counts laps and how many waypoints were passed.
    public int lap = 0;
    public int stopLap = 10;
    public bool stopAfterLap = false;
    public int totalWaypointPassed = 0;
    public int nextWaypointPassDistance = 20;
    public bool ignoreWaypointNow = false;

    // Detector radius.
    public int chaseDistance = 200;
    public int startFollowDistance = 300;
    public int stopFollowDistance = 30;


    // Unity's Navigator.
    private NavMeshAgent navigator;

    // Detector with Sphere Collider. Used for finding target Gameobjects in chasing mode.
    private SphereCollider detector;
    public List<Transform> targetsInZone = new List<Transform>();
    public List<RCC_AIBrakeZone> brakeZones = new List<RCC_AIBrakeZone>();

    public Transform targetChase;       // Target Gameobject for chasing.
    public RCC_AIBrakeZone targetBrake;     //  Target brakezone.

    // Firing an event when each RCC AI vehicle spawned / enabled.
    public delegate void onRCCAISpawned(RCC_AICarController RCCAI);
    public static event onRCCAISpawned OnRCCAISpawned;

    // Firing an event when each RCC AI vehicle disabled / destroyed.
    public delegate void onRCCAIDestroyed(RCC_AICarController RCCAI);
    public static event onRCCAIDestroyed OnRCCAIDestroyed;

    public RaycastHit hit;
    public LayerMask layerMask;

    int[] anglesOfRaycasts = new int[5];
    float distanceToNextWaypoint;
    RCC_Waypoint currentWaypoint;
    Transform bestTarget;
    float closestDistanceSqr;
    Vector3 currentPosition;
    Vector3 directionToTarget;
    float dSqrToTarget;
    Vector3 pivotPos;
    bool casted;
    float navigatorInput;
    float distanceToTarget;
    static float lng;
    RCC_AIBrakeZone bestTargets;
    Vector3 currentPositions;
    Vector3 directionToTargets;
    float dSqrToTargets;

    public bool allowMovement = true;

    void Awake()
    {
        // Getting main controller and enabling external controller.
        carController = GetComponent<RCC_CarControllerV3>();
        carController.externalController = true;

        // If Waypoints Container is not selected in Inspector Panel, find it on scene.
        if (!waypointsContainer)
            waypointsContainer = FindObjectOfType(typeof(RCC_AIWaypointsContainer)) as RCC_AIWaypointsContainer;

        // Creating our Navigator and setting properties.
        GameObject navigatorObject = new GameObject("Navigator");
        navigatorObject.transform.SetParent(transform, false);
        navigator = navigatorObject.AddComponent<NavMeshAgent>();
        navigator.radius = 1;
        navigator.speed = 1;
        navigator.angularSpeed = 100000f;
        navigator.acceleration = 100000f;
        navigator.height = 1;
        navigator.avoidancePriority = 0;

        // Creating our Detector and setting properties. Used for getting nearest target gameobjects.
        GameObject detectorGO = new GameObject("Detector");
        detectorGO.transform.SetParent(transform, false);
        detectorGO.layer = LayerMask.NameToLayer("Ignore Raycast");
        detector = detectorGO.gameObject.AddComponent<SphereCollider>();
        detector.isTrigger = true;
        detector.radius = 10f;
    }

    void OnEnable()
    {

        carController.externalController = true;

        // Calling this event when AI vehicle spawned.
        if (OnRCCAISpawned != null)
            OnRCCAISpawned(this);
    }

    void Update()
    {

        // If not controllable, no need to go further.
        if (!carController.canControl)
            return;

        if (!limitSpeed)
            maximumSpeed = carController.maxspeed;

        // Assigning navigator's position to front wheels of the vehicle.
        navigator.transform.localPosition = Vector3.zero;
        navigator.transform.localPosition += Vector3.forward * carController.FrontLeftWheelCollider.transform.localPosition.z;

        CheckTargets();
        CheckBrakeZones();
    }

    void FixedUpdate()
    {

        // If not controllable, no need to go further.
        if (!carController.canControl)
            return;

        if (useRaycasts)
            FixedRaycasts();            // Recalculates steerInput if one of raycasts detects an object front of AI vehicle.

        Navigation();             // Calculates steerInput based on navigator.
        CheckReset();           // Was used for deciding go back or not after crashing.
        FeedRCC();              // Feeds inputs of the RCC.
    }

    void Navigation()
    {
        // Navigator Input is multiplied by 1.5f for fast reactions.
        navigatorInput = Mathf.Clamp(transform.InverseTransformDirection(navigator.desiredVelocity).x * 1.5f, -1f, 1f);
        switch (navigationMode)
        {

            case NavigationMode.FollowWaypoints:

                detector.radius = 100f;

                // If our scene doesn't have a Waypoint Container, stop and return with error.
                if (!waypointsContainer)
                {
                    Debug.LogError("Waypoints Container Couldn't Found!");
                    Stop();
                    return;
                }

                // If our scene has Waypoints Container and it doesn't have any waypoints, stop and return with error.
                if (waypointsContainer && waypointsContainer.waypoints.Count < 1)
                {
                    Debug.LogError("Waypoints Container Doesn't Have Any Waypoints!");
                    Stop();
                    return;
                }

                //	If stop after lap is enabled, stop at target lap.
                if (stopAfterLap && lap >= stopLap)
                {
                    Stop();
                    return;
                }

                if (!allowMovement)
                {
                    Stop();
                    return;
                }
                else if (hit.transform != null)
                {
                    if (hit.collider.CompareTag("TrafficAI") || hit.collider.CompareTag("Player"))
                    {
                        Stop();
                        return;
                    }
                    else
                    {
                        Start();
                    }
                }
                else
                {
                    Start();
                }

                // Next waypoint and its position.
                currentWaypoint = waypointsContainer.waypoints[currentWaypointIndex];

                // Checks for the distance to next waypoint. If it is less than written value, then pass to next waypoint.
                distanceToNextWaypoint = Vector3.Distance(transform.position, currentWaypoint.transform.position);

                // Setting destination of the Navigator. 
                if (!navigator.hasPath)
                {
                    if (navigator)
                        navigator.SetDestination(waypointsContainer.waypoints[currentWaypointIndex].transform.position);
                }

                if (distanceToNextWaypoint != 0 && distanceToNextWaypoint < nextWaypointPassDistance)
                {
                    currentWaypointIndex++;
                    totalWaypointPassed++;

                    // If all waypoints were passed, sets the current waypoint to first waypoint and increase lap.
                    if (currentWaypointIndex >= waypointsContainer.waypoints.Count)
                    {
                        currentWaypointIndex = 0;
                        lap++;
                    }

                    // Setting destination of the Navigator. 
                    if (navigator.isOnNavMesh)
                        if (navigator)
                            navigator.SetDestination(waypointsContainer.waypoints[currentWaypointIndex].transform.position);
                }

                if (!reversingNow)
                {
                    // MODIFIED: Multiply target speed by speedMultiplier and scale maximumSpeed accordingly.
                    throttleInput = (distanceToNextWaypoint < (nextWaypointPassDistance * (carController.speed / 30f))) ?
                        Mathf.Clamp01((currentWaypoint.targetSpeed * speedMultiplier) - carController.speed) : 1f;
                    throttleInput *= Mathf.Clamp01(Mathf.Lerp(10f, 0f, carController.speed / (maximumSpeed * speedMultiplier)));
                    brakeInput = (distanceToNextWaypoint < (nextWaypointPassDistance * (carController.speed / 30f))) ?
                        Mathf.Clamp01(carController.speed - (currentWaypoint.targetSpeed * speedMultiplier)) : 0f;

                    handbrakeInput = 0f;

                    if (carController.speed > 30f)
                    {
                        throttleInput -= Mathf.Abs(navigatorInput) / 3f;
                        brakeInput += Mathf.Abs(navigatorInput) / 3f;
                    }
                }
                break;

            case NavigationMode.ChaseTarget:

                detector.radius = chaseDistance;

                // If our scene doesn't have a Waypoints Container, return with error.
                if (!targetChase)
                {
                    Stop();
                    return;
                }

                // Setting destination of the Navigator. 
                if (navigator.isOnNavMesh)
                    navigator.SetDestination(targetChase.position);

                if (!reversingNow)
                {
                    throttleInput = 1f;
                    // MODIFIED: Scale maximumSpeed with speedMultiplier here as well.
                    throttleInput *= Mathf.Clamp01(Mathf.Lerp(10f, 0f, carController.speed / (maximumSpeed * speedMultiplier)));
                    brakeInput = 0f;
                    handbrakeInput = 0f;

                    if (carController.speed > 30f)
                    {
                        throttleInput -= Mathf.Abs(navigatorInput) / 3f;
                        brakeInput += Mathf.Abs(navigatorInput) / 3f;
                    }
                }
                break;

            case NavigationMode.FollowTarget:

                detector.radius = startFollowDistance;

                // If our scene doesn't have a Waypoints Container, return with error.
                if (!targetChase)
                {
                    Stop();
                    return;
                }

                // Setting destination of the Navigator. 
                if (navigator.isOnNavMesh)
                    navigator.SetDestination(targetChase.position);

                // Checks for the distance to target. 
                distanceToTarget = GetPathLength(navigator.path);

                if (!reversingNow)
                {
                    throttleInput = distanceToTarget < (stopFollowDistance * Mathf.Lerp(1f, 5f, carController.speed / 50f)) ?
                        Mathf.Lerp(-5f, 1f, distanceToTarget / (stopFollowDistance / 1f)) : 1f;
                    throttleInput *= Mathf.Clamp01(Mathf.Lerp(10f, 0f, carController.speed / (maximumSpeed * speedMultiplier)));
                    brakeInput = distanceToTarget < (stopFollowDistance * Mathf.Lerp(1f, 5f, carController.speed / 50f)) ?
                        Mathf.Lerp(5f, 0f, distanceToTarget / (stopFollowDistance / 1f)) : 0f;
                    handbrakeInput = 0f;

                    if (carController.speed > 30f)
                    {
                        throttleInput -= Mathf.Abs(navigatorInput) / 3f;
                        brakeInput += Mathf.Abs(navigatorInput) / 3f;
                    }

                    if (throttleInput < .05f)
                        throttleInput = 0f;
                    if (brakeInput < .05f)
                        brakeInput = 0f;
                }
                break;
        }

        if (targetBrake)
        {
            if (Vector3.Distance(transform.position, targetBrake.transform.position) < targetBrake.distance && carController.speed > targetBrake.targetSpeed)
            {
                throttleInput = 0f;
                brakeInput = 1f;
            }
        }

        // Steer Input.
        steerInput = (ignoreWaypointNow ? rayInput : navigatorInput + rayInput);
        steerInput = Mathf.Clamp(steerInput, -1f, 1f) * carController.direction;
        throttleInput = Mathf.Clamp01(throttleInput);
        brakeInput = Mathf.Clamp01(brakeInput);
        handbrakeInput = Mathf.Clamp01(handbrakeInput);

        if (reversingNow)
        {
            throttleInput = 0f;
            brakeInput = 1f;
            handbrakeInput = 0f;
        }
        else
        {
            if (carController.speed < 5f && brakeInput >= .5f)
            {
                brakeInput = 0f;
                handbrakeInput = 1f;
            }
        }
    }

    private void CheckReset()
    {
        if (navigationMode == NavigationMode.FollowTarget && GetPathLength(navigator.path) < stopFollowDistance)
        {
            reversingNow = false;
            resetTime = 0;
            return;
        }

        // If unable to move forward, puts the gear to R.
        if (carController.speed <= 25 && transform.InverseTransformDirection(carController.rigid.velocity).z < 1f && allowMovement && handbrakeInput != 1)
            resetTime += Time.deltaTime;

        if (resetTime >= 2)
            reversingNow = true;

        if (resetTime >= 4 || carController.speed >= 25)
        {
            reversingNow = false;
            resetTime = 0;
        }
    }

    private void FixedRaycasts()
    {
        anglesOfRaycasts[0] = 0;
        anglesOfRaycasts[1] = Mathf.FloorToInt(raycastAngle / 3f);
        anglesOfRaycasts[2] = Mathf.FloorToInt(raycastAngle / 1f);
        anglesOfRaycasts[3] = -Mathf.FloorToInt(raycastAngle / 1f);
        anglesOfRaycasts[4] = -Mathf.FloorToInt(raycastAngle / 3f);

        // Ray pivot position.
        pivotPos = transform.position;
        pivotPos += transform.forward * carController.FrontLeftWheelCollider.transform.localPosition.z;

        rayInput = 0f;
        casted = false;

        for (int i = 0; i < anglesOfRaycasts.Length; i++)
        {
            if (Physics.Raycast(pivotPos, Quaternion.AngleAxis(anglesOfRaycasts[i], transform.up) * transform.forward, out hit, raycastLength, obstacleLayers) &&
                !hit.collider.isTrigger && hit.transform.root != transform)
            {

                switch (navigationMode)
                {
                    case NavigationMode.FollowWaypoints:
                        casted = true;
                        if (i != 0)
                            rayInput -= Mathf.Lerp(Mathf.Sign(anglesOfRaycasts[i]), 0f, (hit.distance / raycastLength));
                        break;
                    case NavigationMode.ChaseTarget:
                        if (targetChase && hit.transform != targetChase && !hit.transform.IsChildOf(targetChase))
                        {
                            casted = true;
                            if (i != 0)
                                rayInput -= Mathf.Lerp(Mathf.Sign(anglesOfRaycasts[i]), 0f, (hit.distance / raycastLength));
                        }
                        break;
                    case NavigationMode.FollowTarget:
                        casted = true;
                        if (i != 0)
                            rayInput -= Mathf.Lerp(Mathf.Sign(anglesOfRaycasts[i]), 0f, (hit.distance / raycastLength));
                        break;
                }

                if (casted)
                    obstacle = hit.transform.gameObject;
                else
                    obstacle = null;
            }
        }

        raycasting = casted;
        rayInput = Mathf.Clamp(rayInput, -1f, 1f);

        if (raycasting && Mathf.Abs(rayInput) > .5f)
            ignoreWaypointNow = true;
        else
            ignoreWaypointNow = false;
    }

    private void FeedRCC()
    {
        // Feeding gasInput of the RCC.
        if (!carController.changingGear && !carController.cutGas)
            carController.throttleInput = (carController.direction == 1 ? Mathf.Clamp01(throttleInput) : Mathf.Clamp01(brakeInput));
        else
            carController.throttleInput = 0f;

        if (!carController.changingGear && !carController.cutGas)
            carController.brakeInput = (carController.direction == 1 ? Mathf.Clamp01(brakeInput) : Mathf.Clamp01(throttleInput));
        else
            carController.brakeInput = 0f;

        // Feeding steerInput of the RCC.
        if (smoothedSteer)
            carController.steerInput = Mathf.Lerp(carController.steerInput, steerInput, Time.deltaTime * 20f);
        else
            carController.steerInput = steerInput;

        carController.handbrakeInput = handbrakeInput;
    }

    private void Stop()
    {
        throttleInput = 0f;
        brakeInput = 0f;
        steerInput = 0f;
        handbrakeInput = 1f;
    }

    private void Start()
    {
        throttleInput = 0f;
        brakeInput = 0f;
        steerInput = 0f;
        handbrakeInput = 0f;
    }

    private void CheckTargets()
    {
        // Removing unnecessary targets in list.
        for (int i = 0; i < targetsInZone.Count; i++)
        {
            if (targetsInZone[i] == null)
                targetsInZone.RemoveAt(i);
            if (!targetsInZone[i].gameObject.activeInHierarchy)
                targetsInZone.RemoveAt(i);
            else
            {
                if (Vector3.Distance(transform.position, targetsInZone[i].transform.position) > (detector.radius * 1.25f))
                    targetsInZone.RemoveAt(i);
            }
        }
        // If there is a target, get closest enemy.
        if (targetsInZone.Count > 0)
            targetChase = GetClosestEnemy(targetsInZone.ToArray());
        else
            targetChase = null;
    }

    private void CheckBrakeZones()
    {
        // Removing unnecessary targets in list.
        for (int i = 0; i < brakeZones.Count; i++)
        {
            if (brakeZones[i] == null)
                brakeZones.RemoveAt(i);
            if (!brakeZones[i].gameObject.activeInHierarchy)
                brakeZones.RemoveAt(i);
            else
            {
                if (Vector3.Distance(transform.position, brakeZones[i].transform.position) > (detector.radius * 1.25f))
                    brakeZones.RemoveAt(i);
            }
        }
        // If there is a target, get closest enemy.
        if (brakeZones.Count > 0)
            targetBrake = GetClosestBrakeZone(brakeZones.ToArray());
        else
            targetBrake = null;
    }

    void OnTriggerEnter(Collider col)
    {
        if (col.transform.root.CompareTag(targetTag))
        {
            if (!targetsInZone.Contains(col.transform.root))
                targetsInZone.Add(col.transform.root);
        }
        if (col.GetComponent<RCC_AIBrakeZone>())
        {
            if (!brakeZones.Contains(col.GetComponent<RCC_AIBrakeZone>()))
                brakeZones.Add(col.GetComponent<RCC_AIBrakeZone>());
        }
    }

    private Transform GetClosestEnemy(Transform[] enemies)
    {
        bestTarget = null;
        closestDistanceSqr = Mathf.Infinity;
        currentPosition = transform.position;
        foreach (Transform potentialTarget in enemies)
        {
            directionToTarget = potentialTarget.position - currentPosition;
            dSqrToTarget = directionToTarget.sqrMagnitude;
            if (dSqrToTarget < closestDistanceSqr)
            {
                closestDistanceSqr = dSqrToTarget;
                bestTarget = potentialTarget;
            }
        }
        return bestTarget;
    }

    private RCC_AIBrakeZone GetClosestBrakeZone(RCC_AIBrakeZone[] enemies)
    {
        bestTargets = null;
        closestDistanceSqr = Mathf.Infinity;
        currentPositions = transform.position;
        foreach (RCC_AIBrakeZone potentialTarget in enemies)
        {
            directionToTarget = potentialTarget.transform.position - currentPositions;
            dSqrToTargets = directionToTarget.sqrMagnitude;
            if (dSqrToTargets < closestDistanceSqr)
            {
                closestDistanceSqr = dSqrToTargets;
                bestTargets = potentialTarget;
            }
        }
        return bestTargets;
    }

    private static bool GetPath(NavMeshPath path, Vector3 fromPos, Vector3 toPos, int passableMask)
    {
        path.ClearCorners();
        if (NavMesh.CalculatePath(fromPos, toPos, passableMask, path) == false)
            return false;
        return true;
    }

    private static float GetPathLength(NavMeshPath path)
    {
        lng = 0.0f;
        if ((path.status != NavMeshPathStatus.PathInvalid) && (path.corners.Length > 1))
        {
            for (int i = 1; i < path.corners.Length; ++i)
                lng += Vector3.Distance(path.corners[i - 1], path.corners[i]);
        }
        return lng;
    }

    void OnDisable()
    {
        carController.externalController = false;
        // Calling this event when AI vehicle is destroyed.
        if (OnRCCAIDestroyed != null)
            OnRCCAIDestroyed(this);
    }
}
