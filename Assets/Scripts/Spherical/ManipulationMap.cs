using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Leap.Unity.Interaction;
using System;
using Leap.Unity;

[RequireComponent(typeof(InteractionBehaviour))]
public class ManipulationMap : MonoBehaviour
{
    private InteractionBehaviour leap;
    private MapGraspedPoseHandler graspedPoseHandler;

    // Time users have to hold the hand against the proxy to start manipulation mode
    public const float ManipulationActivationTimeout = 0.5f;

    private float hoverTime = 0f;

    // When this much time has passed without interaction, go back to hover mode
    public const float InactiveTimeout = 5.0f;
    private float inactiveTime;


    void Start()
    {
        graspedPoseHandler = new MapGraspedPoseHandler(leap);
        leap.graspedPoseHandler = graspedPoseHandler;
    }

    private void Update()
    {
        //HandleActiveMode();
    }

    // private void HandleActiveMode()
    // {
    //     if (graspedPoseHandler.InteractingHands == 0)
    //     {
    //         inactiveTime += Time.deltaTime;
    //         if (inactiveTime > InactiveTimeout)
    //         {
    //             ChangeMode(ManipulationMode.Hover);
    //             graspedPoseHandler.SetEnabled(false);
    //         }
    //         else
    //         {
    //             ChangeMode(ManipulationMode.Active);
    //         }
    //     }
    //     else if (graspedPoseHandler.InteractingHands == 1)
    //     {
    //         inactiveTime = 0f;
    //         // Don't allow going from Translating into anchoring
    //         if (Mode == ManipulationMode.Translating) hasHoverHand = false;
    //         // Have slight delay on anchoring start as not to trigger it on rotation/scaling
    //         if (hasHoverHand && hoverTime > 0.2f)
    //         {
    //             anchorHand = leap.graspingController.intHand;
    //             anchoringStart = anchorHand.isGraspingObject ? anchorHand.GetGraspPoint() : anchorHand.position;
    //             anchoringStopTimer = 0f;
    //             lineRenderer.enabled = true;
    //             ChangeMode(ManipulationMode.Anchoring);
    //             Player.Instance?.ShowAnchoringTarget();
    //             graspedPoseHandler.SetEnabled(false);
    //         }
    //         else
    //         {
    //             ChangeMode(ManipulationMode.Translating);
    //         }
    //     }
    //     else if (graspedPoseHandler.InteractingHands == 2)
    //     {
    //         inactiveTime = 0f;
    //         if (proxyNode.Radius < MINIMIZE_THRESHOLD)
    //         {
    //             graspedPoseHandler.SetEnabled(false);
    //             proxyNode.Minimize();
    //             ChangeMode(ManipulationMode.Minimized);
    //         }
    //         else
    //         {
    //             ChangeMode(ManipulationMode.ScalingAndRotating);
    //         }
    //     }
    // }
}