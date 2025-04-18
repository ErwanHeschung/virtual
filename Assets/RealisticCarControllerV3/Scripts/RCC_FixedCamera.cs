﻿//----------------------------------------------
//            Realistic Car Controller
//
// Copyright © 2014 - 2022 BoneCracker Games
// http://www.bonecrackergames.com
// Buğra Özdoğanlar
//
//----------------------------------------------

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Fixed camera system for RCC Camera. It simply parents the RCC Camera, and calculates target position, rotation, FOV, etc...
/// </summary>
[AddComponentMenu("BoneCracker Games/Realistic Car Controller/Camera/RCC Fixed Camera")]
public class RCC_FixedCamera : RCC_Singleton<RCC_FixedCamera> {

    private Vector3 targetPosition;
    public float maxDistance = 50f;
    private float distance;

    public float minimumFOV = 20f;
    public float maximumFOV = 60f;
    public bool canTrackNow = false;
    Transform target;
    float speed;
    float randomizedAngle;
    RaycastHit hit;

    void LateUpdate() {

        if (!canTrackNow)
            return;

        // If current camera is null, return.
        if (!RCC_SceneManager.Instance.activePlayerCamera)
            return;
        if (RCC_SceneManager.Instance.activePlayerCamera.cameraTarget.playerVehicle)
            target = RCC_SceneManager.Instance.activePlayerCamera.cameraTarget.playerVehicle.transform;
        speed = RCC_SceneManager.Instance.activePlayerCamera.cameraTarget.speed;

        if (target == null)
            return;

        distance = Vector3.Distance(transform.position, target.position);

        RCC_SceneManager.Instance.activePlayerCamera.targetFieldOfView = Mathf.Lerp(distance > maxDistance / 10f ? maximumFOV : 70f, minimumFOV, (distance * 1.5f) / maxDistance);

        targetPosition = target.transform.position;
        targetPosition += target.transform.rotation * Vector3.forward * (speed * .05f);

        transform.Translate((-target.forward * speed) / 50f * Time.deltaTime);

        transform.LookAt(targetPosition);

        if (distance > maxDistance)
            ChangePosition();

    }

    public void ChangePosition() {

        if (!canTrackNow)
            return;

        if (!RCC_SceneManager.Instance.activePlayerCamera)
            return;

        target = null;

        if (RCC_SceneManager.Instance.activePlayerCamera.cameraTarget.playerVehicle)
            target = RCC_SceneManager.Instance.activePlayerCamera.cameraTarget.playerVehicle.transform;

        if (target == null)
            return;

        randomizedAngle = Random.Range(-15f, 15f);

        if (Physics.Raycast(target.position, Quaternion.AngleAxis(randomizedAngle, target.up) * target.forward, out hit, maxDistance) && !hit.transform.IsChildOf(target) && !hit.collider.isTrigger) {
            
            transform.position = hit.point;
            transform.LookAt(target.position + new Vector3(0f, Mathf.Clamp(randomizedAngle, .5f, 5f), 0f));
            transform.position += transform.rotation * Vector3.forward * 5f;

        } else {

            transform.position = target.position + new Vector3(0f, Mathf.Clamp(randomizedAngle, 0f, 5f), 0f);
            transform.position += Quaternion.AngleAxis(randomizedAngle, target.up) * target.forward * (maxDistance * .9f);

        }
    }

}
