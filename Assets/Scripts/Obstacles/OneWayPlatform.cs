using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class OneWayPlatform : MonoBehaviour
{
    [SerializeField] private DetectPlayerTrigger upDetection;
    [SerializeField] private DetectPlayerTrigger downDetection;
    [SerializeField] private BoxCollider2D collision;

    private bool downColliterIsInteractingWithPlayer;
    private bool upColliterIsInteractingWithPlayer;

    private static bool isPressingDown;

    private PlayerInput input;

    private void Awake()
    {
        downDetection.playersOnRangeChanged = DetectPleyerTrigger_OnDownColliderChanged;
        upDetection.playersOnRangeChanged = DetectPleyerTrigger_OnUpColliderChanged;

        input = new PlayerInput();

        input.Movement.Down.performed += OnDownStarted;
        input.Movement.Down.canceled += OnDownEnded;


    }

    private void OnEnable()
    {
        input.Enable();
    }

    private void OnDisable()
    {
        input.Disable();
    }

    private void Update()
    {
        HandleColision();
    }

    private void OnDownEnded(InputAction.CallbackContext context)
    {
        isPressingDown = false;
    }

    private void OnDownStarted(InputAction.CallbackContext context)
    {
        isPressingDown = true;
    }

    private void DetectPleyerTrigger_OnUpColliderChanged(List<WaterPriestess> list)
    {
        if (list.Count > 0)
        {
            upColliterIsInteractingWithPlayer = true;
        }
        else
        {
            upColliterIsInteractingWithPlayer = false;
        }
    }

    private void DetectPleyerTrigger_OnDownColliderChanged(List<WaterPriestess> list)
    {
        if(list.Count > 0)
        {
            downColliterIsInteractingWithPlayer = true;
        }
        else
        {
            downColliterIsInteractingWithPlayer = false;
        }
    }

    private void HandleColision()
    {
        if(downColliterIsInteractingWithPlayer || (upColliterIsInteractingWithPlayer && isPressingDown))
        {
            collision.enabled = false;
        }
        else
        {
            collision.enabled = true;
        }
    }
}
