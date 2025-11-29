using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Assets.Scripts;
using System;

public class GamePoint : MonoBehaviour
{
    [SerializeField] Transform text;
    [SerializeField] float jumpTextHeight;
    [SerializeField] float jumpTextSpeed;

    [SerializeField] Transform transformEnterAwaiter;
    [SerializeField] Transform transformExitAwaiter;
    [SerializeField] String targetTag;

    private float _jumpTextMin;
    private float _jumpTextMax;
    private bool _isJumpTextDown;
    private bool _isTargetEntered;

    private EnterAwaiterCollider _enterAwaiter;
    private ExitAwaiterCollider _exitAwaiter;

    public Action<GamePoint, Collider> OnEnter;
    public Action<GamePoint, Collider> OnExit;

    private void Awake()
    {
        if (text != null)
        {
            _isJumpTextDown = true;
            _jumpTextMin = text.position.y - jumpTextHeight / 2;
            _jumpTextMax = text.position.y + jumpTextHeight / 2;
        }

        if (transformEnterAwaiter != null)
        {
            _enterAwaiter = transformEnterAwaiter.GetComponent<EnterAwaiterCollider>();
        }

        if (transformExitAwaiter != null)
        {
            _exitAwaiter = transformExitAwaiter.GetComponent<ExitAwaiterCollider>();
        }

        _isTargetEntered = false;

    }

    private void OnEnterAwaiter(Collider other)
    {
        //Debug.Log("On OnEnterAwaiter Await ");
        if (!_isTargetEntered)
        {
            if (other.gameObject.CompareTag(targetTag))
            {
                //Debug.Log("On OnEnterAwaiter Works " + other.gameObject.tag);
                _isTargetEntered = true;
                OnEnter?.Invoke(this,other);
            }
        }
        
    }

    private void OnExitAwaiter(Collider other)
    {
        //Debug.Log("On OnExitAwaiter Await" + other.gameObject.tag);
        if (_isTargetEntered)
        {
            if (other.gameObject.CompareTag(targetTag))
            {
                //Debug.Log("On OnExitAwaiter Works " + other.gameObject.tag);
                _isTargetEntered = false;
                OnExit?.Invoke(this, other);
            }
        }
    }
    private void JumpText()
    {
        float jumpStep = 0;
        if (_isJumpTextDown)
        {
            if (text.position.y > _jumpTextMin)
            {
                jumpStep = -jumpTextSpeed * Time.deltaTime;
            }
            else
            { _isJumpTextDown = false; }
        }
        else
        {
            if (text.position.y < _jumpTextMax)
            {
                jumpStep = jumpTextSpeed * Time.deltaTime;
            }
            else
            { _isJumpTextDown = true; }
        }
        text.Translate(0, jumpStep, 0);
    }

    private void OnEnable()
    {
        if (_enterAwaiter!=null)
        {
            _enterAwaiter.OnEnter += OnEnterAwaiter;
        }

        if (_exitAwaiter!=null)
        {
            _exitAwaiter.OnExit += OnExitAwaiter;
        }
    }

    private void OnDisable()
    {
        if (_enterAwaiter != null)
        {
            _enterAwaiter.OnEnter -= OnEnterAwaiter;
        }

        if (_exitAwaiter != null)
        {
            _exitAwaiter.OnExit -= OnExitAwaiter;
        }
    }
    // Start is called before the first frame update
    

    // Update is called once per frame
    void Update()
    {
        //Debug.Log("GamePoint Update");
        if (text != null)
        {
            JumpText();
        }   
    }
}
