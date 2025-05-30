﻿//----------------------------------------------
//            Realistic Car Controller
//
// Copyright © 2014 - 2022 BoneCracker Games
// http://www.bonecrackergames.com
// Buğra Özdoğanlar
//
//----------------------------------------------

using System.Collections.Generic;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

[CustomEditor(typeof(RCC_CarControllerV3))]
public class RCC_Editor : Editor
{

    RCC_CarControllerV3 carScript;

    Texture2D wheelIcon;
    Texture2D steerIcon;
    Texture2D suspensionIcon;
    Texture2D configIcon;
    Texture2D lightIcon;
    Texture2D soundIcon;
    Texture2D damageIcon;
    Texture2D stabilityIcon;

    bool WheelSettings;
    bool SteerSettings;
    bool SuspensionSettings;
    bool FrontAxle;
    bool RearAxle;
    bool Configurations;
    bool LightSettings;
    bool SoundSettings;
    bool DamageSettings;
    bool StabilitySettings;

    Color defBackgroundColor;

    [MenuItem("Tools/BoneCracker Games/Realistic Car Controller/Add Main Controller To Vehicle #r", false, -85)]
    static void CreateBehavior()
    {

        if (!Selection.activeGameObject.GetComponentInParent<RCC_CarControllerV3>())
        {

            bool isPrefab = PrefabUtility.IsAnyPrefabInstanceRoot(Selection.activeGameObject);

            if (isPrefab)
            {

                bool isModelPrefab = PrefabUtility.IsPartOfModelPrefab(Selection.activeGameObject);
                bool unpackPrefab = EditorUtility.DisplayDialog("Unpack Prefab", "This gameobject is connected to a " + (isModelPrefab ? "model" : "") + " prefab. Would you like to unpack the prefab completely? If you don't unpack it, you won't be able to move, reorder, or delete any children instance of the prefab.", "Unpack", "Don't Unpack");

                if (unpackPrefab)
                    PrefabUtility.UnpackPrefabInstance(Selection.activeGameObject, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);

            }

            bool fixPivot = EditorUtility.DisplayDialog("Fix Pivot Position Of The Vehicle", "Would you like to fix pivot position of the vehicle? If your vehicle has correct pivot position, select no.", "Fix", "No");

            if (fixPivot)
            {

                GameObject pivot = new GameObject(Selection.activeGameObject.name);
                pivot.transform.position = RCC_GetBounds.GetBoundsCenter(Selection.activeGameObject.transform);
                pivot.transform.rotation = Selection.activeGameObject.transform.rotation;

                if (pivot.GetComponent<RCC_CarControllerV3>() == null)
                    pivot.AddComponent<RCC_CarControllerV3>();

                pivot.GetComponent<Rigidbody>().mass = 1500f;
                pivot.GetComponent<Rigidbody>().drag = .01f;
                pivot.GetComponent<Rigidbody>().angularDrag = .5f;
                pivot.GetComponent<Rigidbody>().interpolation = RigidbodyInterpolation.Interpolate;

                Selection.activeGameObject.transform.SetParent(pivot.transform);
                Selection.activeGameObject = pivot;

            }
            else
            {

                GameObject selectedVehicle = Selection.activeGameObject;

                if (selectedVehicle.GetComponent<Rigidbody>() == null)
                    selectedVehicle.AddComponent<Rigidbody>();

                selectedVehicle.GetComponent<Rigidbody>().mass = 1500f;
                selectedVehicle.GetComponent<Rigidbody>().drag = .01f;
                selectedVehicle.GetComponent<Rigidbody>().angularDrag = .5f;
                selectedVehicle.GetComponent<Rigidbody>().interpolation = RigidbodyInterpolation.Interpolate;

                Selection.activeGameObject = selectedVehicle;

            }

            EditorUtility.DisplayDialog("RCC Initialized", "Drag and drop all your wheel models in to ''Wheel Models'' from hierarchy.", "Close");

        }
        else
        {

            EditorUtility.DisplayDialog("Your Gameobject Already Has Realistic Car ControllerV3", "Your Gameobject Already Has Realistic Car ControllerV3", "Close");

        }

    }

    [MenuItem("Tools/BoneCracker Games/Realistic Car Controller/Add Main Controller To Vehicle #r", true)]
    static bool CheckCreateBehavior()
    {

        if (Selection.gameObjects.Length > 1 || !Selection.activeTransform)
            return false;
        else
            return true;

    }

    void Awake()
    {

        wheelIcon = Resources.Load("Editor/WheelIcon", typeof(Texture2D)) as Texture2D;
        steerIcon = Resources.Load("Editor/SteerIcon", typeof(Texture2D)) as Texture2D;
        suspensionIcon = Resources.Load("Editor/SuspensionIcon", typeof(Texture2D)) as Texture2D;
        configIcon = Resources.Load("Editor/ConfigIcon", typeof(Texture2D)) as Texture2D;
        lightIcon = Resources.Load("Editor/LightIcon", typeof(Texture2D)) as Texture2D;
        soundIcon = Resources.Load("Editor/SoundIcon", typeof(Texture2D)) as Texture2D;
        damageIcon = Resources.Load("Editor/DamageIcon", typeof(Texture2D)) as Texture2D;
        stabilityIcon = Resources.Load("Editor/StabilityIcon", typeof(Texture2D)) as Texture2D;

    }

    public override void OnInspectorGUI()
    {

        carScript = (RCC_CarControllerV3)target;
        serializedObject.Update();
        defBackgroundColor = GUI.backgroundColor;

        EditorGUILayout.Space();
        EditorGUILayout.PropertyField(serializedObject.FindProperty("overrideBehavior"), new GUIContent("Override Behavior", "Vehicle won't be affected by selected behavior in RCC Settings."));
        EditorGUILayout.Space();

        if (carScript.overrideBehavior)
            EditorGUILayout.HelpBox("Vehicle won't be affected by selected behavior in RCC Settings.", MessageType.Info);

        EditorGUILayout.BeginHorizontal();

        Buttons();

        GUI.backgroundColor = defBackgroundColor;
        EditorGUILayout.EndHorizontal();

        if (WheelSettings)
            WheelSettingsTab();

        if (SteerSettings)
            SteeringTab();

        if (SuspensionSettings)
            SuspensionsTab();

        if (Configurations)
            ConfigurationTab();

        if (StabilitySettings)
            StabilityTab();

        if (LightSettings)
            LightingTab();

        if (SoundSettings)
            AudioTab();

        if (DamageSettings)
            DamageTab();

        if (!Application.isPlaying)
            CheckUp();

        serializedObject.ApplyModifiedProperties();

        if (GUI.changed && !EditorApplication.isPlaying)
        {

            if (RCC_Settings.Instance.setTagsAndLayers)
                SetLayerMask();

            if (carScript.autoGenerateEngineRPMCurve)
                carScript.ReCreateEngineTorqueCurve();

            EditorUtility.SetDirty(carScript);

        }

    }

    private void Buttons()
    {

        if (WheelSettings)
            GUI.backgroundColor = Color.gray;
        else GUI.backgroundColor = defBackgroundColor;

        if (GUILayout.Button(wheelIcon))
            WheelSettings = EnableCategory();

        if (SteerSettings)
            GUI.backgroundColor = Color.gray;
        else GUI.backgroundColor = defBackgroundColor;

        if (GUILayout.Button(steerIcon))
            SteerSettings = EnableCategory();

        if (SuspensionSettings)
            GUI.backgroundColor = Color.gray;
        else GUI.backgroundColor = defBackgroundColor;

        if (GUILayout.Button(suspensionIcon))
            SuspensionSettings = EnableCategory();

        if (Configurations)
            GUI.backgroundColor = Color.gray;
        else GUI.backgroundColor = defBackgroundColor;

        if (GUILayout.Button(configIcon))
            Configurations = EnableCategory();

        EditorGUILayout.EndHorizontal();
        EditorGUILayout.BeginHorizontal();

        if (StabilitySettings)
            GUI.backgroundColor = Color.gray;
        else GUI.backgroundColor = defBackgroundColor;

        if (GUILayout.Button(stabilityIcon))
            StabilitySettings = EnableCategory();

        if (LightSettings)
            GUI.backgroundColor = Color.gray;
        else GUI.backgroundColor = defBackgroundColor;

        if (GUILayout.Button(lightIcon))
            LightSettings = EnableCategory();

        if (SoundSettings)
            GUI.backgroundColor = Color.gray;
        else GUI.backgroundColor = defBackgroundColor;

        if (GUILayout.Button(soundIcon))
            SoundSettings = EnableCategory();

        if (DamageSettings)
            GUI.backgroundColor = Color.gray;
        else GUI.backgroundColor = defBackgroundColor;

        if (GUILayout.Button(damageIcon))
            DamageSettings = EnableCategory();

    }

    private void WheelSettingsTab()
    {

        EditorGUILayout.Space();
        GUI.color = Color.cyan;
        EditorGUILayout.HelpBox("Wheel Settings", MessageType.None);
        GUI.color = defBackgroundColor;
        EditorGUILayout.Space();

        EditorGUILayout.Space();
        EditorGUILayout.PropertyField(serializedObject.FindProperty("FrontLeftWheelTransform"), new GUIContent("Front Left Wheel Model", "Select front left wheel of your vehicle."), false);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("FrontRightWheelTransform"), new GUIContent("Front Right Wheel Model", "Select front right wheel of your vehicle."), false);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("RearLeftWheelTransform"), new GUIContent("Rear Left Wheel Model", "Select rear left wheel of your vehicle."), false);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("RearRightWheelTransform"), new GUIContent("Rear Right Wheel Model", "Select rear right wheel of your vehicle."), false);
        EditorGUILayout.Space();

        GUI.color = Color.green;

        if (GUILayout.Button("Create Wheel Colliders"))
        {

            WheelCollider[] wheelColliders = carScript.gameObject.GetComponentsInChildren<WheelCollider>();

            if (wheelColliders.Length >= 1)
            {

                EditorUtility.DisplayDialog("Vehicle has Wheel Colliders already!", "Vehicle has Wheel Colliders already! Delete all of them to create new Wheel Colliders.", "Close");
                return;

            }
            else
            {

                carScript.CreateWheelColliders();

            }

            bool createCenter = EditorUtility.DisplayDialog("Create WheelColliders", "Do you want to create wheelcollider at the center of the wheel, or with suspension distance?", "Center", "With Suspension Distance");

            if (createCenter)
            {

                RCC_WheelCollider[] wheels = carScript.GetComponentsInChildren<RCC_WheelCollider>();

                foreach (RCC_WheelCollider wc in wheels)
                    wc.transform.position += carScript.transform.up * (wc.wheelCollider.suspensionDistance / 2f);

            }

        }

        GUI.color = defBackgroundColor;

        if (carScript.FrontLeftWheelTransform == null || carScript.FrontRightWheelTransform == null || carScript.RearLeftWheelTransform == null || carScript.RearRightWheelTransform == null)
            EditorGUILayout.HelpBox("Select all of your Wheel Models before creating Wheel Colliders", MessageType.Error);

        EditorGUILayout.Space();

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.PropertyField(serializedObject.FindProperty("FrontLeftWheelCollider"), new GUIContent("Front Left WheelCollider", "WheelColliders are generated when you click ''Create WheelColliders'' button. But if you want to create your WheelCollider yourself, select corresponding WheelCollider for each wheel after you created."), false);

        if (carScript.FrontLeftWheelCollider && GUILayout.Button("Edit", GUILayout.Width(50f)))
            Selection.activeGameObject = carScript.FrontLeftWheelCollider.gameObject;

        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.PropertyField(serializedObject.FindProperty("FrontRightWheelCollider"), new GUIContent("Front Right WheelCollider", "WheelColliders are generated when you click ''Create WheelColliders'' button. But if you want to create your WheelCollider yourself, select corresponding WheelCollider for each wheel after you created."), false);

        if (carScript.FrontRightWheelCollider && GUILayout.Button("Edit", GUILayout.Width(50f)))
            Selection.activeGameObject = carScript.FrontRightWheelCollider.gameObject;

        EditorGUILayout.EndHorizontal();
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.PropertyField(serializedObject.FindProperty("RearLeftWheelCollider"), new GUIContent("Rear Left WheelCollider", "WheelColliders are generated when you click ''Create WheelColliders'' button. But if you want to create your WheelCollider yourself, select corresponding WheelCollider for each wheel after you created."), false);

        if (carScript.RearLeftWheelCollider && GUILayout.Button("Edit", GUILayout.Width(50f)))
            Selection.activeGameObject = carScript.RearLeftWheelCollider.gameObject;

        EditorGUILayout.EndHorizontal();
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.PropertyField(serializedObject.FindProperty("RearRightWheelCollider"), new GUIContent("Rear Right WheelCollider", "WheelColliders are generated when you click ''Create WheelColliders'' button. But if you want to create your WheelCollider yourself, select corresponding WheelCollider for each wheel after you created."), false);

        if (carScript.RearRightWheelCollider && GUILayout.Button("Edit", GUILayout.Width(50f)))
            Selection.activeGameObject = carScript.RearRightWheelCollider.gameObject;

        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space();
        EditorGUILayout.PropertyField(serializedObject.FindProperty("hasExtraWheels"), new GUIContent("Extra Wheels", "Extra Wheels."), false);
        EditorGUILayout.Space();

        if (carScript.hasExtraWheels)
        {

            EditorGUI.indentLevel++;

            EditorGUILayout.PropertyField(serializedObject.FindProperty("ExtraRearWheelsTransform"), new GUIContent("Extra Rear Wheel Models", "In case of if your vehicle has extra wheels."), true);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("ExtraRearWheelsCollider"), new GUIContent("Extra Rear Wheel Colliders", "In case of if your vehicle has extra wheels."), true);
            EditorGUILayout.Space();

            if (carScript.ExtraRearWheelsCollider != null)
            {

                for (int i = 0; i < carScript.ExtraRearWheelsCollider.Length; i++)
                {

                    EditorGUILayout.BeginHorizontal();

                    if (carScript.ExtraRearWheelsCollider[i] != null && GUILayout.Button("Edit" + carScript.ExtraRearWheelsCollider[i].transform.name))
                        Selection.activeGameObject = carScript.ExtraRearWheelsCollider[i].gameObject;

                    EditorGUILayout.EndHorizontal();

                }

            }

            EditorGUI.indentLevel--;

        }

        EditorGUILayout.HelpBox("Each wheel can be customized above by clicking their edit buttons. Such as power, steer, brake, and handbrake. Drivetrain type will be ineffective while enabled.", MessageType.Info);
        carScript.overrideAllWheels = EditorGUILayout.BeginToggleGroup("Override All Wheels", carScript.overrideAllWheels);
        EditorGUI.indentLevel++;

        EditorGUILayout.Space();

        RCC_WheelCollider[] allWheels = carScript.GetComponentsInChildren<RCC_WheelCollider>();

        if (allWheels != null && allWheels.Length >= 1)
        {

            for (int i = 0; i < allWheels.Length; i++)
            {

                EditorGUILayout.LabelField(allWheels[i].name);

                EditorGUILayout.BeginHorizontal();

                float org = EditorGUIUtility.labelWidth;
                EditorGUIUtility.labelWidth = 75f;

                allWheels[i].canPower = EditorGUILayout.ToggleLeft("Power", allWheels[i].canPower);
                allWheels[i].canSteer = EditorGUILayout.ToggleLeft("Steer", allWheels[i].canSteer);
                allWheels[i].canBrake = EditorGUILayout.ToggleLeft("Brake", allWheels[i].canBrake);
                allWheels[i].canHandbrake = EditorGUILayout.ToggleLeft("EBrake", allWheels[i].canHandbrake);

                EditorGUILayout.EndHorizontal();
                EditorGUILayout.BeginHorizontal();

                allWheels[i].powerMultiplier = EditorGUILayout.Slider("", allWheels[i].powerMultiplier, -1f, 1f);
                allWheels[i].steeringMultiplier = EditorGUILayout.Slider("", allWheels[i].steeringMultiplier, -1f, 1f);
                allWheels[i].brakingMultiplier = EditorGUILayout.Slider("", allWheels[i].brakingMultiplier, -1f, 1f);
                allWheels[i].handbrakeMultiplier = EditorGUILayout.Slider("", allWheels[i].handbrakeMultiplier, -1f, 1f);

                EditorGUILayout.EndHorizontal();

                EditorGUIUtility.labelWidth = org;

            }

        }

        if (!carScript.overrideAllWheels)
        {

            for (int i = 0; i < allWheels.Length; i++)
            {

                if (carScript.FrontLeftWheelCollider && carScript.FrontLeftWheelCollider == allWheels[i])
                {

                    if (carScript.wheelTypeChoise == RCC_CarControllerV3.WheelType.RWD)
                        allWheels[i].canPower = false;
                    else
                        allWheels[i].canPower = true;


                    allWheels[i].canBrake = true;
                    allWheels[i].canSteer = true;
                    allWheels[i].canHandbrake = false;

                }

                if (carScript.FrontRightWheelCollider && carScript.FrontRightWheelCollider == allWheels[i])
                {

                    if (carScript.wheelTypeChoise == RCC_CarControllerV3.WheelType.RWD)
                        allWheels[i].canPower = false;
                    else
                        allWheels[i].canPower = true;

                    allWheels[i].canBrake = true;
                    allWheels[i].canSteer = true;
                    allWheels[i].canHandbrake = false;

                }

                if (carScript.RearLeftWheelCollider && carScript.RearLeftWheelCollider == allWheels[i])
                {

                    if (carScript.wheelTypeChoise == RCC_CarControllerV3.WheelType.FWD)
                        allWheels[i].canPower = false;
                    else
                        allWheels[i].canPower = true;

                    allWheels[i].canBrake = true;
                    allWheels[i].canSteer = false;
                    allWheels[i].canHandbrake = true;

                }

                if (carScript.RearRightWheelCollider && carScript.RearRightWheelCollider == allWheels[i])
                {

                    if (carScript.wheelTypeChoise == RCC_CarControllerV3.WheelType.FWD)
                        allWheels[i].canPower = false;
                    else
                        allWheels[i].canPower = true;

                    allWheels[i].canBrake = true;
                    allWheels[i].canSteer = false;
                    allWheels[i].canHandbrake = true;

                }

            }

        }

        EditorGUI.indentLevel--;
        EditorGUILayout.EndToggleGroup();

        EditorGUILayout.Space();

        if (!Application.isPlaying)
        {

            if (carScript.FrontLeftWheelCollider && carScript.FrontLeftWheelTransform)
                carScript.FrontLeftWheelCollider.wheelModel = carScript.FrontLeftWheelTransform;

            if (carScript.FrontRightWheelCollider && carScript.FrontRightWheelTransform)
                carScript.FrontRightWheelCollider.wheelModel = carScript.FrontRightWheelTransform;

            if (carScript.RearLeftWheelCollider && carScript.RearLeftWheelTransform)
                carScript.RearLeftWheelCollider.wheelModel = carScript.RearLeftWheelTransform;

            if (carScript.RearRightWheelCollider && carScript.RearRightWheelTransform)
                carScript.RearRightWheelCollider.wheelModel = carScript.RearRightWheelTransform;

        }

        EditorGUILayout.Space();

        EditorGUILayout.Space();
        EditorGUILayout.PropertyField(serializedObject.FindProperty("SteeringWheel"), new GUIContent("Interior Steering Wheel Model", "In case of if your vehicle has individual steering wheel model in interior."), false);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("steeringWheelRotateAround"), new GUIContent("Steering Wheel Rotate Around Axis", "Rotate the steering wheel around this axis. Useful if your steering wheel has wrong axis."), false);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("steeringWheelAngleMultiplier"), new GUIContent("Steering Wheel Angle Multiplier", "Steering wheel angle multiplier."), false);
        EditorGUILayout.Space();

    }

    private void SteeringTab()
    {

        EditorGUILayout.Space();
        GUI.color = Color.cyan;
        EditorGUILayout.HelpBox("Steer Settings", MessageType.None);
        GUI.color = defBackgroundColor;
        EditorGUILayout.Space();

        EditorGUILayout.Space();

        EditorGUILayout.PropertyField(serializedObject.FindProperty("steeringType"), new GUIContent("Steering Type", "Steering Type. Curve based or simple point based."), false);

        EditorGUI.indentLevel++;

        switch (carScript.steeringType)
        {

            case RCC_CarControllerV3.SteeringType.Curve:

                EditorGUILayout.PropertyField(serializedObject.FindProperty("steerAngleCurve"), new GUIContent("Steer Angle Curve", "Steer Angle Curve based on speed. Maximum steer angle will be adjusted related to speed."), false);
                break;

            case RCC_CarControllerV3.SteeringType.Simple:

                EditorGUILayout.PropertyField(serializedObject.FindProperty("steerAngle"), new GUIContent("Maximum Steer Angle", "Maximum steer angle for your vehicle."), false);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("highspeedsteerAngle"), new GUIContent("Maximum Steer Angle At ''X'' Speed", "Maximum steer angle at highest speed."), false);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("highspeedsteerAngleAtspeed"), new GUIContent("''X'' Speed", "Steer Angle At Highest Speed."), false);
                break;

            case RCC_CarControllerV3.SteeringType.Constant:

                EditorGUILayout.PropertyField(serializedObject.FindProperty("steerAngle"), new GUIContent("Maximum Steer Angle", "Maximum steer angle for your vehicle."), false);
                break;

        }

        EditorGUI.indentLevel--;
        EditorGUILayout.Space();

        EditorGUILayout.PropertyField(serializedObject.FindProperty("antiRollFrontHorizontal"), new GUIContent("Anti Roll Front Horizontal", "Anti Roll Force for prevents flip overs."));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("antiRollRearHorizontal"), new GUIContent("Anti Roll Rear Horizontal", "Anti Roll Force for prevents flip overs."));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("antiRollVertical"), new GUIContent("Anti Roll Forward", "Anti Roll Force for preventing flip overs."));
        EditorGUILayout.Space();
        EditorGUILayout.PropertyField(serializedObject.FindProperty("useSteeringLimiter"), new GUIContent("Use Steering Limiter", "Limits steering while sliding to avoid losing the control."));
        EditorGUILayout.Space();
        EditorGUILayout.PropertyField(serializedObject.FindProperty("useCounterSteering"), new GUIContent("Use Counter Steering", "Counter steers to opposite direction while sliding. Useful for drifting."));

        EditorGUI.indentLevel++;

        if (carScript.useCounterSteering)
            EditorGUILayout.PropertyField(serializedObject.FindProperty("counterSteeringFactor"), new GUIContent("Counter Steering Factor", "Counter Steering multiplier."));

        EditorGUI.indentLevel--;

        EditorGUILayout.PropertyField(serializedObject.FindProperty("useSteeringSensitivity"), new GUIContent("Use Steering Sensitivity", "Applies Steering sensitivity."));

        EditorGUI.indentLevel++;

        if (carScript.useSteeringSensitivity)
            EditorGUILayout.PropertyField(serializedObject.FindProperty("steeringSensitivityFactor"), new GUIContent("Steering Sensitivity", "Steering sensitivity multiplier."));

        EditorGUI.indentLevel--;

    }

    private void SuspensionsTab()
    {

        EditorGUILayout.Space();
        GUI.color = Color.cyan;
        EditorGUILayout.HelpBox("Suspension Settings", MessageType.None);
        GUI.color = defBackgroundColor;
        EditorGUILayout.Space();

        if (!carScript.FrontLeftWheelCollider || !carScript.FrontRightWheelCollider || !carScript.RearLeftWheelCollider || !carScript.RearRightWheelCollider)
        {
            EditorGUILayout.HelpBox("Vehicle Missing Wheel Colliders. Be Sure You Have Created Wheel Colliders Before Adjusting Suspensions", MessageType.Error);
            return;
        }

        if (Selection.gameObjects.Length > 1)
        {
            EditorGUILayout.HelpBox("Multiple Editing Suspensions Is Not Allowed", MessageType.Error);
            return;
        }

        JointSpring frontSspring = carScript.FrontLeftWheelCollider.wheelCollider.suspensionSpring;
        JointSpring rearSpring = carScript.RearLeftWheelCollider.wheelCollider.suspensionSpring;

        GUILayout.BeginHorizontal();

        if (FrontAxle)
            GUI.backgroundColor = Color.gray;
        else
            GUI.backgroundColor = defBackgroundColor;

        if (GUILayout.Button("Front Axle"))
        {
            FrontAxle = true;
            RearAxle = false;
        }

        if (RearAxle)
            GUI.backgroundColor = Color.gray;
        else
            GUI.backgroundColor = defBackgroundColor;

        if (GUILayout.Button("Rear Axle"))
        {
            FrontAxle = false;
            RearAxle = true;
        }

        GUI.backgroundColor = defBackgroundColor;

        GUILayout.EndHorizontal();

        if (FrontAxle)
        {

            EditorGUILayout.Space();

            carScript.FrontLeftWheelCollider.wheelCollider.suspensionDistance = carScript.FrontRightWheelCollider.wheelCollider.suspensionDistance = EditorGUILayout.FloatField("Front Suspensions Distance", carScript.FrontLeftWheelCollider.wheelCollider.suspensionDistance);
            carScript.FrontLeftWheelCollider.wheelCollider.forceAppPointDistance = carScript.FrontRightWheelCollider.wheelCollider.forceAppPointDistance = EditorGUILayout.FloatField("Front Force App Distance", carScript.FrontLeftWheelCollider.wheelCollider.forceAppPointDistance);

            if (carScript.FrontLeftWheelCollider && carScript.FrontRightWheelCollider)
                carScript.FrontLeftWheelCollider.camber = carScript.FrontRightWheelCollider.camber = EditorGUILayout.FloatField("Front Camber Angle", carScript.FrontLeftWheelCollider.camber);

            EditorGUILayout.Space();

            frontSspring.spring = EditorGUILayout.FloatField("Front Suspensions Spring", frontSspring.spring);
            frontSspring.damper = EditorGUILayout.FloatField("Front Suspensions Damping", frontSspring.damper);
            frontSspring.targetPosition = EditorGUILayout.FloatField("Front Suspensions Target Position", frontSspring.targetPosition);

            EditorGUILayout.Space();

        }

        if (RearAxle)
        {

            EditorGUILayout.Space();

            carScript.RearLeftWheelCollider.wheelCollider.suspensionDistance = carScript.RearRightWheelCollider.wheelCollider.suspensionDistance = EditorGUILayout.FloatField("Rear Suspensions Distance", carScript.RearLeftWheelCollider.wheelCollider.suspensionDistance);
            carScript.RearLeftWheelCollider.wheelCollider.forceAppPointDistance = carScript.RearRightWheelCollider.wheelCollider.forceAppPointDistance = EditorGUILayout.FloatField("Rear Force App Distance", carScript.RearLeftWheelCollider.wheelCollider.forceAppPointDistance);

            if (carScript.RearLeftWheelCollider && carScript.RearRightWheelCollider)
            {

                carScript.RearLeftWheelCollider.camber = carScript.RearRightWheelCollider.camber = EditorGUILayout.FloatField("Rear Camber Angle", carScript.RearLeftWheelCollider.camber);

                if (carScript.ExtraRearWheelsCollider != null && carScript.ExtraRearWheelsCollider.Length > 0)
                {

                    foreach (RCC_WheelCollider wc in carScript.ExtraRearWheelsCollider)
                        wc.camber = carScript.RearLeftWheelCollider.camber;

                }

            }

            EditorGUILayout.Space();

            rearSpring.spring = EditorGUILayout.FloatField("Rear Suspensions Spring", rearSpring.spring);
            rearSpring.damper = EditorGUILayout.FloatField("Rear Suspensions Damping", rearSpring.damper);
            rearSpring.targetPosition = EditorGUILayout.FloatField("Rear Suspensions Target Position", rearSpring.targetPosition);

            EditorGUILayout.Space();
        }

        carScript.FrontLeftWheelCollider.wheelCollider.suspensionSpring = frontSspring;
        carScript.FrontRightWheelCollider.wheelCollider.suspensionSpring = frontSspring;
        carScript.RearLeftWheelCollider.wheelCollider.suspensionSpring = rearSpring;
        carScript.RearRightWheelCollider.wheelCollider.suspensionSpring = rearSpring;

        EditorGUILayout.Space();

    }

    private void ConfigurationTab()
    {

        EditorGUILayout.Space();
        GUI.color = Color.cyan;
        EditorGUILayout.HelpBox("Configurations", MessageType.None);
        GUI.color = defBackgroundColor;
        EditorGUILayout.Space();

        EditorGUILayout.Space();
        EditorGUILayout.PropertyField(serializedObject.FindProperty("canControl"), new GUIContent("Can Be Controllable Now", "Enables/Disables controlling the vehicle."));
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Engine Is Running Now", carScript.engineRunning.ToString());
        EditorGUILayout.Space();

        EditorGUILayout.PropertyField(serializedObject.FindProperty("wheelTypeChoise"));

        EditorGUILayout.Space();
        EditorGUILayout.PropertyField(serializedObject.FindProperty("configureVehicleSubsteps"), new GUIContent("Configure Vehicle Substeps", "Configures Vehicle Substeps."), true);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("COM"), new GUIContent("Center Of Mass (''COM'')", "Center of Mass of the vehicle. Usually, COM is below around front driver seat."));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("totalGears"), new GUIContent("Total Gears", "Total Gears"), false);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("gears"), new GUIContent("Gears", "Gears"), true);

        EditorGUILayout.BeginHorizontal();

        if (GUILayout.Button("1 Gear Preset"))
        {

            carScript.totalGears = 1;
            carScript.InitGears();

        }

        if (GUILayout.Button("2 Gears Preset"))
        {

            carScript.totalGears = 2;
            carScript.InitGears();

        }

        if (GUILayout.Button("3 Gears Preset"))
        {

            carScript.totalGears = 3;
            carScript.InitGears();

        }

        if (GUILayout.Button("4 Gears Preset"))
        {

            carScript.totalGears = 4;
            carScript.InitGears();

        }

        EditorGUILayout.EndHorizontal();
        EditorGUILayout.BeginHorizontal();

        if (GUILayout.Button("5 Gears Preset"))
        {

            carScript.totalGears = 5;
            carScript.InitGears();

        }

        if (GUILayout.Button("6 Gears Preset"))
        {

            carScript.totalGears = 6;
            carScript.InitGears();

        }

        if (GUILayout.Button("7 Gears Preset"))
        {

            carScript.totalGears = 7;
            carScript.InitGears();

        }

        if (GUILayout.Button("8 Gears Preset"))
        {

            carScript.totalGears = 8;
            carScript.InitGears();

        }

        EditorGUILayout.EndHorizontal();

        EditorGUILayout.PropertyField(serializedObject.FindProperty("autoGenerateEngineRPMCurve"), new GUIContent("Auto Generate Engine Torque Curve", "If min/max engine rpm, engine torque, max engine torque at rpm, or top speed has been changed at runtime, it will generate new curve with them."), false);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("engineTorqueCurve"), new GUIContent("Engine Torque Curve", "Based on engine rpm."), false);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("gearShiftingDelay"), new GUIContent("Gear Shifting Delay", "Gear Shifting Delay"), false);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("gearShiftingThreshold"), new GUIContent("Gear Shifting Threshold", "Gear Shifting Threshold"), false);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("gearShiftUpRPM"), new GUIContent("Gear Shift Up RPM", "Gear Shifting Up RPM"), false);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("gearShiftDownRPM"), new GUIContent("Gear Shift Down RPM", "Gear Shifting Down RPM"), false);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("finalRatio"), new GUIContent("Final Drive Ratio"), false);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("clutchInertia"), new GUIContent("Clutch Inertia", "Clutch Inertia. Lazy clutching on higher values."), false);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("currentGear"), new GUIContent("Current Gear"), false);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("maxEngineTorque"), new GUIContent("Maximum Engine Torque"), false);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("maxEngineTorqueAtRPM"), new GUIContent("Maximum Engine Torque At RPM"), false);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("brakeTorque"), new GUIContent("Maximum Brake Torque"), false);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("maxspeed"), new GUIContent("Maximum Speed"), false);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("downForce"), new GUIContent("DownForce"), false);

        EditorGUILayout.PropertyField(serializedObject.FindProperty("minEngineRPM"), new GUIContent("Lowest Engine RPM"), false);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("maxEngineRPM"), new GUIContent("Highest Engine RPM"), false);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("engineInertia"), new GUIContent("Engine Inertia", "Fast on lower values."), false);

        EditorGUILayout.Space();
        EditorGUILayout.PropertyField(serializedObject.FindProperty("useRevLimiter"), new GUIContent("Rev Limiter"), false);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("useNOS"), new GUIContent("Use NOS"), false);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("useTurbo"), new GUIContent("Use Turbo"), false);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("useExhaustFlame"), new GUIContent("Use Exhaust Flame"), false);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("useEngineHeat"), new GUIContent("Use Engine Heat"), false);

        if (carScript.useEngineHeat)
        {

            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(serializedObject.FindProperty("engineHeatRate"), new GUIContent("Engine Heat Rate"), false);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("engineCoolRate"), new GUIContent("Engine Cool Rate"), false);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("engineCoolingWaterThreshold"), new GUIContent("Engine Cooling Open Threshold"), false);
            EditorGUI.indentLevel--;

        }

        EditorGUILayout.PropertyField(serializedObject.FindProperty("useFuelConsumption"), new GUIContent("Use Fuel Consumption"), false);

        if (carScript.useFuelConsumption)
        {

            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(serializedObject.FindProperty("fuelTankCapacity"), new GUIContent("Fuel Tank Capacity"), false);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("fuelTank"), new GUIContent("Fuel Tank Amount"), false);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("fuelConsumptionRate"), new GUIContent("Fuel Consumption Rate"), false);
            EditorGUI.indentLevel--;

        }

        EditorGUILayout.Space();

    }

    private void StabilityTab()
    {

        EditorGUILayout.Space();
        GUI.color = Color.cyan;
        EditorGUILayout.HelpBox("Stability System Settings", MessageType.None);
        GUI.color = defBackgroundColor;
        EditorGUILayout.Space();

        EditorGUILayout.PropertyField(serializedObject.FindProperty("ABS"), new GUIContent("ABS"), false);

        if (carScript.ABS)
            EditorGUILayout.PropertyField(serializedObject.FindProperty("ABSThreshold"), new GUIContent("ABS Threshold"), false);

        EditorGUILayout.Space();
        EditorGUILayout.PropertyField(serializedObject.FindProperty("TCS"), new GUIContent("TCS"), false);

        if (carScript.TCS)
            EditorGUILayout.PropertyField(serializedObject.FindProperty("TCSStrength"), new GUIContent("TCS Strength"), false);

        EditorGUILayout.Space();
        EditorGUILayout.PropertyField(serializedObject.FindProperty("ESP"), new GUIContent("ESP"), false);

        if (carScript.ESP)
        {
            EditorGUILayout.PropertyField(serializedObject.FindProperty("ESPThreshold"), new GUIContent("ESP Threshold"), false);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("ESPStrength"), new GUIContent("ESP Strength"), false);
        }

        EditorGUILayout.Space();
        EditorGUILayout.PropertyField(serializedObject.FindProperty("steeringHelper"), new GUIContent("Steering Helper"), false);

        if (carScript.steeringHelper)
        {
            EditorGUILayout.PropertyField(serializedObject.FindProperty("steerHelperLinearVelStrength"), new GUIContent("Steering Helper Linear Velocity Strength"), false);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("steerHelperAngularVelStrength"), new GUIContent("Steering Helper Angular Velocity Strength"), false);
        }

        EditorGUILayout.Space();
        EditorGUILayout.PropertyField(serializedObject.FindProperty("tractionHelper"), new GUIContent("Traction Helper"), false);

        if (carScript.tractionHelper)
            EditorGUILayout.PropertyField(serializedObject.FindProperty("tractionHelperStrength"), new GUIContent("Traction Helper Strength"), false);

        EditorGUILayout.Space();
        EditorGUILayout.PropertyField(serializedObject.FindProperty("angularDragHelper"), new GUIContent("Angular Drag Helper"), false);

        if (carScript.angularDragHelper)
            EditorGUILayout.PropertyField(serializedObject.FindProperty("angularDragHelperStrength"), new GUIContent("Angular Drag Helper Strength"), false);

        EditorGUILayout.Space();

    }

    private void LightingTab()
    {

        EditorGUILayout.Space();
        GUI.color = Color.cyan;
        EditorGUILayout.HelpBox("Light Settings", MessageType.None);
        GUI.color = defBackgroundColor;
        EditorGUILayout.Space();

        EditorGUILayout.PropertyField(serializedObject.FindProperty("lowBeamHeadLightsOn"), new GUIContent("Head Lights On"));
        EditorGUILayout.Space();

        RCC_Light[] lights = carScript.GetComponentsInChildren<RCC_Light>();

        EditorGUILayout.LabelField("Head Lights", EditorStyles.boldLabel);
        EditorGUILayout.Space();
        EditorGUI.indentLevel++;

        for (int i = 0; i < lights.Length; i++)
        {

            EditorGUILayout.BeginHorizontal();

            if (lights[i].lightType == RCC_Light.LightType.HeadLight)
            {

                EditorGUILayout.ObjectField("Head Light", lights[i].GetComponent<Light>(), typeof(Light), true);

                if (GUILayout.Button("Edit", GUILayout.Width(50f)))
                    Selection.activeGameObject = lights[i].gameObject;

                GUI.color = Color.red;

                if (GUILayout.Button("X", GUILayout.Width(25f)))
                    DestroyImmediate(lights[i].gameObject);

                GUI.color = defBackgroundColor;

            }

            EditorGUILayout.EndHorizontal();

        }

        EditorGUILayout.Space();
        EditorGUI.indentLevel--;
        EditorGUILayout.LabelField("Brake Lights", EditorStyles.boldLabel);
        EditorGUILayout.Space();
        EditorGUI.indentLevel++;

        for (int i = 0; i < lights.Length; i++)
        {

            EditorGUILayout.BeginHorizontal();

            if (lights[i].lightType == RCC_Light.LightType.BrakeLight)
            {

                EditorGUILayout.ObjectField("Brake Light", lights[i].GetComponent<Light>(), typeof(Light), true);

                if (GUILayout.Button("Edit", GUILayout.Width(50f)))
                    Selection.activeGameObject = lights[i].gameObject;

                GUI.color = Color.red;

                if (GUILayout.Button("X", GUILayout.Width(25f)))
                    DestroyImmediate(lights[i].gameObject);

                GUI.color = defBackgroundColor;

            }

            EditorGUILayout.EndHorizontal();

        }

        EditorGUILayout.Space();
        EditorGUI.indentLevel--;
        EditorGUILayout.LabelField("Reverse Lights", EditorStyles.boldLabel);
        EditorGUILayout.Space();
        EditorGUI.indentLevel++;

        for (int i = 0; i < lights.Length; i++)
        {

            EditorGUILayout.BeginHorizontal();

            if (lights[i].lightType == RCC_Light.LightType.ReverseLight)
            {

                EditorGUILayout.ObjectField("Reverse Light", lights[i].GetComponent<Light>(), typeof(Light), true);

                if (GUILayout.Button("Edit", GUILayout.Width(50f)))
                    Selection.activeGameObject = lights[i].gameObject;

                GUI.color = Color.red;

                if (GUILayout.Button("X", GUILayout.Width(25f)))
                    DestroyImmediate(lights[i].gameObject);

                GUI.color = defBackgroundColor;

            }
            EditorGUILayout.EndHorizontal();

        }

        EditorGUILayout.Space();
        EditorGUI.indentLevel--;
        EditorGUILayout.LabelField("Indicator Lights", EditorStyles.boldLabel);
        EditorGUILayout.Space();
        EditorGUI.indentLevel++;

        for (int i = 0; i < lights.Length; i++)
        {

            EditorGUILayout.BeginHorizontal();

            if (lights[i].lightType == RCC_Light.LightType.Indicator)
            {

                EditorGUILayout.ObjectField("Indicator Light", lights[i].GetComponent<Light>(), typeof(Light), true);

                if (GUILayout.Button("Edit", GUILayout.Width(50f)))
                    Selection.activeGameObject = lights[i].gameObject;

                GUI.color = Color.red;

                if (GUILayout.Button("X", GUILayout.Width(25f)))
                    DestroyImmediate(lights[i].gameObject);

                GUI.color = defBackgroundColor;

            }

            EditorGUILayout.EndHorizontal();

        }

        EditorGUI.indentLevel--;

        EditorGUILayout.Space();

        EditorGUILayout.BeginHorizontal();

        if (GUILayout.Button("+ Head Light"))
            RCC_EditorWindows.CreateHeadLight();
        if (GUILayout.Button("+ Brake Light"))
            RCC_EditorWindows.CreateBrakeLight();
        if (GUILayout.Button("+ Reverse Light"))
            RCC_EditorWindows.CreateReverseLight();
        if (GUILayout.Button("+ Indicator Light"))
            RCC_EditorWindows.CreateIndicatorLight();

        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space();

    }

    private void AudioTab()
    {

        EditorGUILayout.Space();
        GUI.color = Color.cyan;
        EditorGUILayout.HelpBox("Sound Settings", MessageType.None);
        GUI.color = defBackgroundColor;

        EditorGUILayout.Space();
        EditorGUILayout.PropertyField(serializedObject.FindProperty("audioType"), new GUIContent("Audio Type"), false);
        EditorGUILayout.Space();

        switch (carScript.audioType)
        {

            case RCC_CarControllerV3.AudioType.Off:

                break;

            case RCC_CarControllerV3.AudioType.OneSource:

                EditorGUILayout.PropertyField(serializedObject.FindProperty("engineClipHigh"), new GUIContent("Engine Sound"), false);

                if (!carScript.autoCreateEngineOffSounds)
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("engineClipHighOff"), new GUIContent("Engine Sound Off"), false);

                break;

            case RCC_CarControllerV3.AudioType.TwoSource:

                EditorGUILayout.PropertyField(serializedObject.FindProperty("engineClipLow"), new GUIContent("Engine Sound Low RPM"), false);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("engineClipHigh"), new GUIContent("Engine Sound High RPM"), false);

                if (!carScript.autoCreateEngineOffSounds)
                {

                    EditorGUILayout.PropertyField(serializedObject.FindProperty("engineClipLowOff"), new GUIContent("Engine Sound Low Off RPM"), false);
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("engineClipHighOff"), new GUIContent("Engine Sound High Off RPM"), false);

                }

                break;

            case RCC_CarControllerV3.AudioType.ThreeSource:

                EditorGUILayout.PropertyField(serializedObject.FindProperty("engineClipLow"), new GUIContent("Engine Sound Low RPM"), false);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("engineClipMed"), new GUIContent("Engine Sound Medium RPM"), false);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("engineClipHigh"), new GUIContent("Engine Sound High RPM"), false);

                if (!carScript.autoCreateEngineOffSounds)
                {

                    EditorGUILayout.PropertyField(serializedObject.FindProperty("engineClipLowOff"), new GUIContent("Engine Sound Low Off RPM"), false);
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("engineClipMedOff"), new GUIContent("Engine Sound Medium Off RPM"), false);
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("engineClipHighOff"), new GUIContent("Engine Sound High Off RPM"), false);

                }

                break;

        }

        if (carScript.audioType != RCC_CarControllerV3.AudioType.Off)
        {

            EditorGUILayout.PropertyField(serializedObject.FindProperty("autoCreateEngineOffSounds"), new GUIContent("Auto Create Engine Off Sounds"), false);
            EditorGUILayout.Space();
            EditorGUILayout.PropertyField(serializedObject.FindProperty("engineClipIdle"), new GUIContent("Engine Sound Idle RPM"), false);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("engineStartClip"), new GUIContent("Engine Starting Sound", "Optional"), false);
            EditorGUILayout.Space();

            EditorGUILayout.Space();
            EditorGUILayout.PropertyField(serializedObject.FindProperty("idleEngineSoundVolume"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("minEngineSoundPitch"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("maxEngineSoundPitch"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("minEngineSoundVolume"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("maxEngineSoundVolume"));
            EditorGUILayout.Space();

        }

        EditorGUILayout.PropertyField(serializedObject.FindProperty("engineSoundPosition"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("gearSoundPosition"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("turboSoundPosition"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("exhaustSoundPosition"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("windSoundPosition"));

    }

    private void DamageTab()
    {

        EditorGUILayout.Space();
        GUI.color = Color.cyan;
        EditorGUILayout.HelpBox("Damage Settings", MessageType.None);
        GUI.color = defBackgroundColor;
        EditorGUILayout.Space();

        EditorGUILayout.Space();
        EditorGUILayout.PropertyField(serializedObject.FindProperty("useDamage"), new GUIContent("Use Damage"), false);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("useCollisionParticles"), new GUIContent("Use Collision Particles"), false);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("useCollisionAudio"), new GUIContent("Use Collision Audio"), false);
        EditorGUILayout.Space();

        if (carScript.useDamage)
        {

            carScript.damage.automaticInstallation = EditorGUILayout.Toggle("Auto Install", carScript.damage.automaticInstallation);
            EditorGUILayout.HelpBox("Auto Install: All meshes, lights, parts, and wheels will be collected automatically at runtime. If you want to select specific objects, disable ''Auto Install'' and select specific objects. If you want to remove only few objects, you can use buttom buttons to get all.", MessageType.Info);

            LayerMask lmask = carScript.damage.damageFilter;
            LayerMask tempMask = EditorGUILayout.MaskField("Damage Filter", InternalEditorUtility.LayerMaskToConcatenatedLayersMask(lmask), InternalEditorUtility.layers);
            carScript.damage.damageFilter = InternalEditorUtility.ConcatenatedLayersMaskToLayerMask(tempMask);

            EditorGUILayout.BeginHorizontal();

            float org = EditorGUIUtility.labelWidth;
            EditorGUIUtility.labelWidth = 75f;

            carScript.damage.meshDeformation = EditorGUILayout.Toggle("Mesh", carScript.damage.meshDeformation, GUILayout.Width(200));
            GUILayout.FlexibleSpace();
            carScript.damage.wheelDamage = EditorGUILayout.Toggle("Wheel", carScript.damage.wheelDamage, GUILayout.Width(200));

            EditorGUILayout.EndHorizontal();
            EditorGUILayout.BeginHorizontal();

            carScript.damage.lightDamage = EditorGUILayout.Toggle("Light", carScript.damage.lightDamage, GUILayout.Width(200));
            GUILayout.FlexibleSpace();
            carScript.damage.partDamage = EditorGUILayout.Toggle("Part", carScript.damage.partDamage, GUILayout.Width(200));

            EditorGUIUtility.labelWidth = org;

            EditorGUILayout.EndHorizontal();

            if (carScript.damage.meshDeformation)
            {

                EditorGUILayout.Space();
                GUILayout.Label("Mesh Deformation", EditorStyles.boldLabel);
                EditorGUILayout.Space();
                EditorGUI.indentLevel++;

                carScript.damage.deformationMode = (RCC_Damage.DeformationMode)EditorGUILayout.EnumPopup("Deformation Mode", carScript.damage.deformationMode);
                carScript.damage.damageResolution = EditorGUILayout.IntSlider("Resolution", carScript.damage.damageResolution, 1, 100);
                carScript.damage.randomizeVertices = EditorGUILayout.FloatField("Randomize Vertices", carScript.damage.randomizeVertices);
                carScript.damage.damageRadius = EditorGUILayout.FloatField("Radius", carScript.damage.damageRadius);
                carScript.damage.damageMultiplier = EditorGUILayout.FloatField("Multiplier", carScript.damage.damageMultiplier);
                carScript.damage.maximumDamage = EditorGUILayout.FloatField("Maximum Deformation", carScript.damage.maximumDamage);
                EditorGUILayout.Space();
                carScript.damage.recalculateBounds = EditorGUILayout.Toggle("Recalculate Bounds", carScript.damage.recalculateBounds);
                carScript.damage.recalculateNormals = EditorGUILayout.Toggle("Recalculate Normals", carScript.damage.recalculateNormals);

                EditorGUI.indentLevel--;

            }

            if (carScript.damage.wheelDamage)
            {

                EditorGUILayout.Space();
                GUILayout.Label("Wheel Deformation", EditorStyles.boldLabel);
                EditorGUILayout.Space();
                EditorGUI.indentLevel++;

                carScript.damage.wheelDamageRadius = EditorGUILayout.FloatField("Radius", carScript.damage.wheelDamageRadius);
                carScript.damage.wheelDamageMultiplier = EditorGUILayout.FloatField("Multiplier", carScript.damage.wheelDamageMultiplier);
                carScript.damage.wheelDetachment = EditorGUILayout.Toggle("Wheel Detachment", carScript.damage.wheelDetachment);

                EditorGUILayout.Space();

                EditorGUI.indentLevel--;

            }

            if (carScript.damage.lightDamage)
            {

                EditorGUILayout.Space();
                GUILayout.Label("Light Deformation", EditorStyles.boldLabel);
                EditorGUILayout.Space();
                EditorGUI.indentLevel++;

                carScript.damage.lightDamageRadius = EditorGUILayout.FloatField("Radius", carScript.damage.lightDamageRadius);
                carScript.damage.lightDamageMultiplier = EditorGUILayout.FloatField("Multiplier", carScript.damage.lightDamageMultiplier);

                EditorGUILayout.Space();

                EditorGUI.indentLevel--;

            }

            EditorGUILayout.Space();

            if (!carScript.damage.automaticInstallation)
            {

                EditorGUILayout.BeginVertical(GUI.skin.box);
                EditorGUILayout.LabelField("Mesh Filters", EditorStyles.boldLabel);
                EditorGUILayout.Space();
                EditorGUI.indentLevel++;

                if (carScript.damage.meshFilters != null)
                {

                    for (int i = 0; i < carScript.damage.meshFilters.Length; i++)
                    {

                        EditorGUILayout.BeginHorizontal();

                        EditorGUILayout.ObjectField(carScript.damage.meshFilters[i], typeof(MeshFilter), false);

                        if (carScript.damage.meshFilters[i].sharedMesh == null)
                        {

                            GUI.color = Color.red;
                            EditorGUILayout.HelpBox("Mesh is null!", MessageType.None);

                        }

                        if (carScript.damage.meshFilters[i].GetComponent<MeshRenderer>() == null)
                        {

                            GUI.color = Color.red;
                            EditorGUILayout.HelpBox("No renderer found!", MessageType.None);

                        }

                        bool fixedRotation = 1 - Mathf.Abs(Quaternion.Dot(carScript.damage.meshFilters[i].transform.rotation, carScript.transform.rotation)) < .01f;

                        if (!fixedRotation)
                        {

                            GUI.color = Color.red;
                            EditorGUILayout.HelpBox("Axis is wrong!", MessageType.None);

                            if (GUILayout.Button("Fix Axis"))
                            {

                                RCC_FixAxisWindow fw = EditorWindow.GetWindow<RCC_FixAxisWindow>(true);
                                fw.target = carScript.damage.meshFilters[i];
                                SceneView.lastActiveSceneView.Frame(new Bounds(carScript.damage.meshFilters[i].transform.position, Vector3.one), false);
                                Selection.activeGameObject = carScript.damage.meshFilters[i].gameObject;

                            }

                        }

                        GUI.color = defBackgroundColor;
                        GUI.color = Color.red;

                        if (GUILayout.Button("X", GUILayout.Width(25f)))
                        {

                            List<MeshFilter> meshes = new List<MeshFilter>();

                            for (int k = 0; k < carScript.damage.meshFilters.Length; k++)
                                meshes.Add(carScript.damage.meshFilters[k]);

                            meshes.RemoveAt(i);

                            carScript.damage.meshFilters = meshes.ToArray();

                        }

                        GUI.color = defBackgroundColor;
                        EditorGUILayout.EndHorizontal();

                    }

                }

                EditorGUI.indentLevel--;
                EditorGUILayout.EndVertical();
                //
                EditorGUILayout.BeginVertical(GUI.skin.box);
                EditorGUILayout.LabelField("Wheels", EditorStyles.boldLabel);
                EditorGUILayout.Space();
                EditorGUI.indentLevel++;

                if (carScript.damage.wheels != null)
                {

                    for (int i = 0; i < carScript.damage.wheels.Length; i++)
                    {

                        EditorGUILayout.BeginHorizontal();
                        EditorGUILayout.ObjectField(carScript.damage.wheels[i], typeof(RCC_WheelCollider), false);
                        GUI.color = Color.red;

                        if (GUILayout.Button("X", GUILayout.Width(25f)))
                        {

                            List<RCC_WheelCollider> wheels = new List<RCC_WheelCollider>();

                            for (int k = 0; k < carScript.damage.wheels.Length; k++)
                                wheels.Add(carScript.damage.wheels[k]);

                            wheels.RemoveAt(i);

                            carScript.damage.wheels = wheels.ToArray();

                        }

                        GUI.color = defBackgroundColor;
                        EditorGUILayout.EndHorizontal();

                    }

                }

                EditorGUI.indentLevel--;
                EditorGUILayout.EndVertical();
                //
                EditorGUILayout.BeginVertical(GUI.skin.box);
                EditorGUILayout.LabelField("Lights", EditorStyles.boldLabel);
                EditorGUILayout.Space();
                EditorGUI.indentLevel++;

                if (carScript.damage.lights != null)
                {

                    for (int i = 0; i < carScript.damage.lights.Length; i++)
                    {

                        EditorGUILayout.BeginHorizontal();
                        EditorGUILayout.ObjectField(carScript.damage.lights[i], typeof(RCC_Light), false);
                        GUI.color = Color.red;

                        if (GUILayout.Button("X", GUILayout.Width(25f)))
                        {

                            List<RCC_Light> lights = new List<RCC_Light>();

                            for (int k = 0; k < carScript.damage.lights.Length; k++)
                                lights.Add(carScript.damage.lights[k]);

                            lights.RemoveAt(i);

                            carScript.damage.lights = lights.ToArray();

                        }

                        GUI.color = defBackgroundColor;
                        EditorGUILayout.EndHorizontal();

                    }

                }

                EditorGUI.indentLevel--;
                EditorGUILayout.EndVertical();
                //
                EditorGUILayout.BeginVertical(GUI.skin.box);
                EditorGUILayout.LabelField("Parts", EditorStyles.boldLabel);
                EditorGUILayout.Space();
                EditorGUI.indentLevel++;

                if (carScript.damage.detachableParts != null)
                {

                    for (int i = 0; i < carScript.damage.detachableParts.Length; i++)
                    {

                        EditorGUILayout.BeginHorizontal();
                        EditorGUILayout.ObjectField(carScript.damage.detachableParts[i], typeof(RCC_DetachablePart), false);
                        GUI.color = Color.red;

                        if (GUILayout.Button("X", GUILayout.Width(25f)))
                        {

                            List<RCC_DetachablePart> parts = new List<RCC_DetachablePart>();

                            for (int k = 0; k < carScript.damage.detachableParts.Length; k++)
                                parts.Add(carScript.damage.detachableParts[k]);

                            parts.RemoveAt(i);

                            carScript.damage.detachableParts = parts.ToArray();

                        }

                        GUI.color = defBackgroundColor;
                        EditorGUILayout.EndHorizontal();

                    }

                }

                EditorGUI.indentLevel--;
                EditorGUILayout.EndVertical();

                ///////////////////////

                EditorGUILayout.Space();

                EditorGUILayout.BeginHorizontal();

                if (GUILayout.Button("Get Meshes"))
                    GetMeshes();

                if (GUILayout.Button("Get Lights"))
                    GetLights();

                if (GUILayout.Button("Get Parts"))
                    GetParts();

                if (GUILayout.Button("Get Wheels"))
                    GetWheels();

                EditorGUILayout.EndHorizontal();

                if (GUILayout.Button("Clean Empty Elements"))
                    CleanEmptyElements();

            }

            if (carScript.damage.repaired)
            {

                GUILayout.Button("Repaired");

            }
            else
            {

                GUI.color = Color.green;

                if (GUILayout.Button("Repair Now"))
                    carScript.damage.repairNow = true;

                GUI.color = defBackgroundColor;

            }

        }

        EditorGUILayout.Space();

    }

    private void CheckUp()
    {

        if (!PrefabUtility.IsPartOfPrefabAsset(carScript.gameObject))
        {

            if (!carScript.COM)
            {

                GameObject COM = new GameObject("COM");
                COM.transform.parent = carScript.transform;
                COM.transform.localPosition = Vector3.zero;
                COM.transform.localRotation = Quaternion.identity;
                COM.transform.localScale = Vector3.one;
                carScript.COM = COM.transform;

            }

        }

        if (carScript.GetComponent<RCC_AICarController>())
        {

            EditorGUILayout.Space();
            EditorGUILayout.HelpBox("This Vehicle Is Controlling By AI. Therefore, All Player Controllers Are Disabled For This Vehicle.", MessageType.Info);
            EditorGUILayout.Space();

            if (GUILayout.Button("Remove AI Controller From Vehicle"))
                DestroyImmediate(carScript.GetComponent<RCC_AICarController>());

        }

        EditorGUILayout.Space();
        GUI.color = Color.cyan;
        EditorGUILayout.HelpBox("System Overall Check", MessageType.None);
        GUI.color = defBackgroundColor;
        EditorGUILayout.Space();

        EditorGUILayout.BeginHorizontal();

        if (carScript.FrontLeftWheelCollider == null || carScript.FrontRightWheelCollider == null || carScript.RearLeftWheelCollider == null || carScript.RearRightWheelCollider == null)
            EditorGUILayout.HelpBox("Wheel Colliders = NOT OK", MessageType.Error);

        if (carScript.FrontLeftWheelTransform == null || carScript.FrontRightWheelTransform == null || carScript.RearLeftWheelTransform == null || carScript.RearRightWheelTransform == null)
            EditorGUILayout.HelpBox("Wheel Models = NOT OK", MessageType.Error);

        if (carScript.COM == null)
            EditorGUILayout.HelpBox("COM = NOT OK", MessageType.Error);

        int totalCountsOfWheelColliders = carScript.GetComponentsInChildren<WheelCollider>().Length;

        if (totalCountsOfWheelColliders < 1)
            EditorGUILayout.HelpBox("Your vehicle MUST have four wheel colliders at least.", MessageType.Error);

        EditorGUILayout.EndHorizontal();

        if (carScript.steerAngleCurve == null)
            carScript.steerAngleCurve = new AnimationCurve(new Keyframe(0f, 40f), new Keyframe(120f, 7f), new Keyframe(200f, 5f));     //	Steering angle limiter curve based on speed.
        else if (carScript.steerAngleCurve.length < 2)
            carScript.steerAngleCurve = new AnimationCurve(new Keyframe(0f, 40f), new Keyframe(120f, 7f, -.1f, -.1f), new Keyframe(200f, 5f));     //	Steering angle limiter curve based on speed.

        if (carScript.COM)
        {

            if (Mathf.Approximately(carScript.COM.transform.localPosition.y, 0f))
                EditorGUILayout.HelpBox("You haven't changed COM position of the vehicle yet. Keep in that your mind, COM is most extremely important for realistic behavior.", MessageType.Warning);

        }
        else
        {

            EditorGUILayout.HelpBox("You haven't created COM of the vehicle yet. Just hit ''Create Necessary Gameobject Groups'' under ''Wheel'' tab for creating this too.", MessageType.Error);

        }

        if (carScript.GetComponent<RCC_AICarController>() && !GameObject.FindObjectOfType<RCC_AIWaypointsContainer>())
            EditorGUILayout.HelpBox("Scene doesn't have RCC_AIWaypointsContainer. You can create it from Tool --> BCG --> RCC --> AI.", MessageType.Error);

        if (FindObjectOfType<RCC_SceneManager>() == null)
        {

            GameObject sceneManager = new GameObject("_RCCSceneManager");
            sceneManager.AddComponent<RCC_SceneManager>();

        }

        if (carScript.gears == null || carScript.gears.Length == 0)
        {

            carScript.totalGears = 6;
            carScript.InitGears();

        }

        Collider[] colliders = RCC_CheckUp.GetColliders(carScript.gameObject);

        if (colliders.Length >= 1)
        {

            for (int i = 0; i < colliders.Length; i++)
            {

                if (!colliders[i].enabled)
                    EditorGUILayout.ObjectField("This collider is not enabled", colliders[i], typeof(Collider), true);

                if (colliders[i].isTrigger)
                    EditorGUILayout.ObjectField("This collider is trigger enabled", colliders[i], typeof(Collider), true);

                MeshCollider meshCol = null;

                if (colliders[i].GetType() == typeof(MeshCollider))
                    meshCol = (MeshCollider)colliders[i];

                if (meshCol && !meshCol.convex)
                    EditorGUILayout.ObjectField("This mesh collider is not convex enabled", colliders[i], typeof(MeshCollider), true);

            }

        }

        bool haveMeshCollider = RCC_CheckUp.HaveCollider(carScript.gameObject);

        if (!haveMeshCollider)
            EditorGUILayout.HelpBox("Your vehicle MUST have any type of body collider.", MessageType.Error);

        Rigidbody[] rigids = RCC_CheckUp.GetRigids(carScript.gameObject);

        if (rigids.Length >= 1)
        {

            EditorGUILayout.HelpBox("Additional rigidbodies found.", MessageType.Info);

            foreach (Rigidbody item in rigids)
                EditorGUILayout.ObjectField("Rigidbody", item, typeof(Rigidbody), true);

        }

        SphereCollider[] sphereColliders = RCC_CheckUp.GetSphereColliders(carScript.gameObject);

        if (sphereColliders.Length >= 1)
        {

            EditorGUILayout.HelpBox("Sphere colliders found. Be sure they are not attached to the wheels.", MessageType.Warning);

            foreach (SphereCollider item in sphereColliders)
            {

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.HelpBox("Sphere Collider", MessageType.None);
                EditorGUILayout.ObjectField("", item, typeof(SphereCollider), true);
                EditorGUILayout.EndHorizontal();

            }

        }

        EditorGUILayout.Space();

        WheelCollider[] wheelColliders = RCC_CheckUp.GetWheelColliders(carScript.gameObject);

        if (wheelColliders.Length >= 1)
        {

            EditorGUILayout.HelpBox("Some of the wheelcolliders have 0 radius. Be sure your wheel transforms are not empty gameobjects. Otherwise, bounds of the wheel model can't be calculated and set to 0 in this case.", MessageType.Warning);

            foreach (WheelCollider item in wheelColliders)
            {

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.HelpBox("WheelCollider has 0 radius.", MessageType.None);
                EditorGUILayout.ObjectField("", item, typeof(WheelCollider), true);
                EditorGUILayout.EndHorizontal();

                if (item.suspensionDistance <= 0.01f)
                {

                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.HelpBox("WheelCollider has almost 0 suspension distance.", MessageType.None);
                    EditorGUILayout.ObjectField("", item, typeof(WheelCollider), true);
                    EditorGUILayout.EndHorizontal();

                }

            }

        }

        string[] errorMessages = RCC_CheckUp.IncorrectConfiguration(carScript);

        if (errorMessages.Length >= 1)
        {

            foreach (string error in errorMessages)
            {

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.HelpBox(error, MessageType.Error);
                EditorGUILayout.EndHorizontal();

            }

        }

        EditorGUILayout.Space();

        if (carScript.damage != null && carScript.damage.meshFilters != null)
        {

            bool haveWrongAxes = RCC_CheckUp.HaveWrongAxis(carScript.gameObject, carScript.damage.meshFilters);

            if (haveWrongAxes)
                EditorGUILayout.HelpBox("Meshes with wrong axes found. Check deformable meshes in the damage category.", MessageType.Warning);

        }

        EditorGUILayout.Space();

        if (carScript.overrideAllWheels)
        {

            bool[] os = RCC_CheckUp.HaveWrongOverride(carScript);

            EditorGUILayout.BeginHorizontal();

            if (!os[0])
                EditorGUILayout.HelpBox("No power wheel found!", MessageType.Error);

            if (!os[1])
                EditorGUILayout.HelpBox("No steering wheel found!", MessageType.Error);

            if (!os[2])
                EditorGUILayout.HelpBox("No brake wheel found!", MessageType.Error);

            if (!os[3])
                EditorGUILayout.HelpBox("No handbrake wheel found!", MessageType.Error);

            EditorGUILayout.EndHorizontal();

        }

    }

    private void GetMeshes()
    {

        MeshFilter[] allMeshFilters = carScript.gameObject.GetComponentsInChildren<MeshFilter>(true);
        List<MeshFilter> properMeshFilters = new List<MeshFilter>();

        // Model import must be readable. If it's not readable, inform the developer. We don't wanna deform wheel meshes. Exclude any meshes belongts to the wheels.
        foreach (MeshFilter mf in allMeshFilters)
        {

            if (mf.sharedMesh != null)
            {

                if (!mf.sharedMesh.isReadable)
                    Debug.LogError("Not deformable mesh detected. Mesh of the " + mf.transform.name + " isReadable is false; Read/Write must be enabled in import settings for this model!");
                else if (!mf.transform.IsChildOf(carScript.FrontLeftWheelTransform) && !mf.transform.IsChildOf(carScript.FrontRightWheelTransform) && !mf.transform.IsChildOf(carScript.RearLeftWheelTransform) && !mf.transform.IsChildOf(carScript.RearRightWheelTransform))
                    properMeshFilters.Add(mf);

            }

        }

        allMeshFilters = properMeshFilters.ToArray();
        carScript.damage.GetMeshes(allMeshFilters);

    }

    private void GetLights()
    {

        RCC_Light[] allLights = carScript.gameObject.GetComponentsInChildren<RCC_Light>(true);
        carScript.damage.GetLights(allLights);

    }

    private void GetParts()
    {

        RCC_DetachablePart[] allParts = carScript.gameObject.GetComponentsInChildren<RCC_DetachablePart>(true);
        carScript.damage.GetParts(allParts);

    }

    private void GetWheels()
    {

        RCC_WheelCollider[] allWheels = carScript.gameObject.GetComponentsInChildren<RCC_WheelCollider>(true);
        carScript.damage.GetWheels(allWheels);

    }

    private void CleanEmptyElements()
    {

        List<MeshFilter> meshFilterList = new List<MeshFilter>();

        for (int i = 0; i < carScript.damage.meshFilters.Length; i++)
        {

            if (carScript.damage.meshFilters[i] != null)
                meshFilterList.Add(carScript.damage.meshFilters[i]);

        }

        carScript.damage.meshFilters = meshFilterList.ToArray();

        List<RCC_Light> lightList = new List<RCC_Light>();

        for (int i = 0; i < carScript.damage.lights.Length; i++)
        {

            if (carScript.damage.lights[i] != null)
                lightList.Add(carScript.damage.lights[i]);

        }

        carScript.damage.lights = lightList.ToArray();

        List<RCC_DetachablePart> partList = new List<RCC_DetachablePart>();

        for (int i = 0; i < carScript.damage.detachableParts.Length; i++)
        {

            if (carScript.damage.detachableParts[i] != null)
                partList.Add(carScript.damage.detachableParts[i]);

        }

        carScript.damage.detachableParts = partList.ToArray();

        List<RCC_WheelCollider> wheelsList = new List<RCC_WheelCollider>();

        for (int i = 0; i < carScript.damage.wheels.Length; i++)
        {

            if (carScript.damage.wheels[i] != null)
                wheelsList.Add(carScript.damage.wheels[i]);

        }

        carScript.damage.wheels = wheelsList.ToArray();

    }

    bool EnableCategory()
    {

        WheelSettings = false;
        SteerSettings = false;
        SuspensionSettings = false;
        FrontAxle = false;
        RearAxle = false;
        Configurations = false;
        StabilitySettings = false;
        LightSettings = false;
        SoundSettings = false;
        DamageSettings = false;

        return true;

    }

    void SetLayerMask()
    {

        if (string.IsNullOrEmpty(RCC_Settings.Instance.RCCLayer))
        {

            Debug.LogError("RCC Layer is missing in RCC Settings. Go to Tools --> BoneCracker Games --> RCC --> Edit Settings, and set the layer of RCC.");
            return;

        }

        if (string.IsNullOrEmpty(RCC_Settings.Instance.RCCTag))
        {

            Debug.LogError("RCC Tag is missing in RCC Settings. Go to Tools --> BoneCracker Games --> RCC --> Edit Settings, and set the tag of RCC.");
            return;

        }

        Transform[] allTransforms = carScript.GetComponentsInChildren<Transform>(true);

        foreach (Transform t in allTransforms)
        {

            int layerInt = LayerMask.NameToLayer(RCC_Settings.Instance.RCCLayer);

            if (layerInt >= 0 && layerInt <= 31)
            {

                if (!t.GetComponent<RCC_Light>())
                {

                    t.gameObject.layer = LayerMask.NameToLayer(RCC_Settings.Instance.RCCLayer);

                    if (!carScript.GetComponent<RCC_AICarController>())
                    {

                        if (RCC_Settings.Instance.tagAllChildrenGameobjects)
                            t.gameObject.transform.tag = RCC_Settings.Instance.RCCTag;

                        else
                            carScript.transform.gameObject.tag = RCC_Settings.Instance.RCCTag;

                    }
                    else
                    {

                        t.gameObject.transform.tag = "Untagged";

                    }

                    if (t.GetComponent<RCC_WheelCollider>())
                        t.gameObject.layer = LayerMask.NameToLayer(RCC_Settings.Instance.WheelColliderLayer);

                    if (t.GetComponent<RCC_DetachablePart>())
                        t.gameObject.layer = LayerMask.NameToLayer(RCC_Settings.Instance.DetachablePartLayer);

                }

            }
            else
            {

                Debug.LogError("RCC layers selected in RCC Settings doesn't exist on your Tags & Layers. Go to Edit --> Project Settings --> Tags & Layers, and create a new layer named ''" + RCC_Settings.Instance.RCCLayer + " " + RCC_Settings.Instance.WheelColliderLayer + " " + RCC_Settings.Instance.DetachablePartLayer + "''.");
                Debug.LogError("From now on, ''Setting Tags and Layers'' disabled in RCCSettings! You can enable this when you created this layer.");

                foreach (Transform tr in allTransforms)
                    tr.gameObject.layer = LayerMask.NameToLayer("Default");

                RCC_Settings.Instance.setTagsAndLayers = false;
                return;

            }

        }

    }

}
