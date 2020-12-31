﻿using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;


public class Captain : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float movementSpeed = 3.0f;
    [SerializeField] private float movementSmoothing = 0.05f;
    
    [Header("Tower variables")] 
    [SerializeField] private List<Swordsman> swordsmen;
    [SerializeField] private float rotationAngle = 3.0f;
    [SerializeField] private float rotationMultiplier = 1.0f;
    [SerializeField] private float firstRotationSmoothing = 0.05f;
    [SerializeField] private float secondRotationSmoothing = 0.05f;
    [SerializeField] private float thirdRotationSmoothing = 0.05f;
    [SerializeField] private float rotationDivisor = 3.0f;
    [SerializeField] private float fallRotation = 20.0f;

    [Space]
    [SerializeField] private bool isAI = false;
    
    private PlayerInputs inputs;
    private float dir = 0.0f, lastDir = 0.0f;
    private Vector2 tempVelocity = Vector2.zero;
    private float tempAngle = 0.0f;
    private bool isCaptain = false;
    private float rotationValue = 0.0f;
    private float lastRotationValue = 0.0f;
    private bool isPending = false;

    public delegate void Command();
    public delegate void CommandFloat(float val);

    public event Command OnAttack;
    public event CommandFloat OnMove;

    public Swordsman GetCaptain
    {
        get => swordsmen[0];
    }
    
    private void Awake()
    {
        if (!isAI)
        {
            inputs = new PlayerInputs();
            inputs.Gameplay.DirectionMovement.performed += Move;
            inputs.Gameplay.DirectionMovement.canceled += Move;
            inputs.Gameplay.Attack.performed += Attack;
        }

        swordsmen[0].EnableCaptain();
        swordsmen[0].Initialize(this);
        for (int i = 1; i < swordsmen.Count; i++)
        {
            swordsmen[i].DisableCaptain();
            swordsmen[i].Initialize(this);
        }
    }
    
    public void Attack(InputAction.CallbackContext obj)
    {
        OnAttack?.Invoke();
    }
    
    private void Move(InputAction.CallbackContext obj)
    {
        lastDir = dir;
        dir = obj.ReadValue<float>();

        if(dir != 0)
            swordsmen[0].Anim.SetBool("Running", true);
        else
            swordsmen[0].Anim.SetBool("Running", false);
        
        OnMove?.Invoke(dir);
    }
    
    public void Move(float newDir)
    {
        lastDir = dir;
        dir = newDir;

        if(dir != 0)
            swordsmen[0].Anim.SetBool("Running", true);
        else
            swordsmen[0].Anim.SetBool("Running", false);
        
        OnMove?.Invoke(dir);
    }
    
    private void FixedUpdate()
    {
        Vector3 targetVelocity = Vector3.zero;
        
        if (Mathf.Abs(dir) > 0.05f)
        {
            targetVelocity = new Vector2(dir * movementSpeed, swordsmen[0].Rigidbody.velocity.y);
            swordsmen[0].Rigidbody.velocity =
                Vector2.SmoothDamp(swordsmen[0].Rigidbody.velocity, targetVelocity, ref tempVelocity, movementSmoothing);
                
            lastRotationValue = rotationValue;
            rotationValue = Mathf.SmoothDampAngle(rotationValue, rotationAngle * dir, ref tempAngle, firstRotationSmoothing);
            isPending = true;
        }
        else
        {
            if (isPending)
            {
                rotationValue = Mathf.SmoothDampAngle(rotationValue, (lastRotationValue * (-1)), ref tempAngle,
                    secondRotationSmoothing);
                if (lastRotationValue * (-1) > 0.0f)
                {
                    if (rotationValue > (lastRotationValue * (-1))/rotationDivisor)
                        isPending = false;
                }
                else if (lastRotationValue * (-1) < 0.0f)
                {
                    if (rotationValue < (lastRotationValue * (-1))/rotationDivisor)
                        isPending = false;
                }
            }
            else
            {
                isPending = false;
                rotationValue = Mathf.SmoothDampAngle(rotationValue, 0.0f, ref tempAngle, thirdRotationSmoothing);
            }
        }

        float currentRotValue;
        for (int i = 1; i < swordsmen.Count; i++)
        {
            currentRotValue = rotationValue * Mathf.Pow(rotationMultiplier, i);
            swordsmen[i].RotateTower(currentRotValue);
            
            float tempAngles = swordsmen[i].transform.localEulerAngles.z % 360.0f;
        
            if (tempAngles > 180)
                tempAngles = tempAngles - 360;

            if (Mathf.Abs(tempAngles) > fallRotation)
            {
                for (int j = swordsmen.Count - 1; j >= i; j--)
                {
                    swordsmen[j].FallOff();
                    swordsmen.RemoveAt(j);
                }
                
                break;
            }
        }
        
        //if (aboveCharacter)
            //aboveCharacter.RotateTower(rotationValue);
        //rigidbody.AddForce(Vector2.right * (dir), ForceMode2D.Impulse);
    }
    
    private void OnEnable()
    {
        if(!isAI)
            inputs.Gameplay.Enable();
    }

    private void OnDisable()
    {
        if(!isAI)
            inputs.Gameplay.Disable();
    }
}