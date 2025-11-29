using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Assets.Scripts;

public class EnemyBullet : MonoBehaviour
{
    [SerializeField] private float damageMultiplayer;
    [SerializeField] private int lifeTimeCirlceCount;

    private Rigidbody _rb;
    private int circleCount=0;
    private int _countCollision = 0;

    public void SetVelocity(Vector3 value)
    {
        //Debug.Log("Velocity Setted");
        if (value != null)
        {
            _rb = GetComponent<Rigidbody>();
            if (_rb != null)
            {
                //Debug.Log("Force Applied");
                _rb.AddForce(value, ForceMode.VelocityChange);
            }
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        //Debug.Log("BulletCollision with"+collision.gameObject.name);
        _countCollision++;

        if (_countCollision == 1)
        {
            if (collision.gameObject.CompareTag("Player"))
            {
                //Debug.Log("BulletCollision with Player" + collision.gameObject.name + "/Velocity" + _rb.velocity);
                PhisicController player = collision.gameObject.GetComponent<PhisicController>();
                if (player != null)
                {
                    //Vector3 currentVelocity = _rb.velocity;
                    player.TakeDamage(collision.impulse * damageMultiplayer);
                }
            }
        }
    }

    private void Update()
    {

        //Debug.Log("Bullet Update" + transform.position.y);
        circleCount++;
        if (circleCount > lifeTimeCirlceCount)
        {
            Destroy(this.gameObject);
        }

        if (transform.position.y < -2)
        {
            //Debug.Log("Distroy works");
            Destroy(this.gameObject);
        }
    }
}
