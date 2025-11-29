using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollower : MonoBehaviour
{
    [SerializeField] private Transform _targerTransform;
    [SerializeField] private Vector3 _offset;
    [SerializeField] private float _smooth;

    // Update is called once per frame
    private void FixedUpdate()
    { 
        Move();
    }

    public void SetNewTarget(Transform newTarget)
    { _targerTransform = newTarget;}

    private void Move()
    {
        if (_targerTransform!=null)
        {
            var nextPosition = Vector3.Lerp(transform.position, _targerTransform.position + _offset, Time.fixedDeltaTime * _smooth);
            transform.position = nextPosition;
            //Debug.Log("======Player:X=" + _targerTransform.position.x + ",Y=" + _targerTransform.position.y + ",Z=" + _targerTransform.position.z);
            //Debug.Log("******Camera:X="+ transform.position.x+",Y=" + transform.position.y + ",Z=" + transform.position.z);
        }
    }
}
