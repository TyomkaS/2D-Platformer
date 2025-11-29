using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Assets.Scripts;
using System;

public class EnemyTypeB : MonoBehaviour, IEnemy
{
    [SerializeField] private LayerMask targetToAttack;
    [SerializeField] private Transform beginMovingPoint;
    [SerializeField] private Transform endMovingPoint;
    [SerializeField] private float horizontalAcceleration;
    [SerializeField] private float rotationSpeed;
    [SerializeField] private float flightHeight;
    [SerializeField] private float verticalSpeed;
    [SerializeField] private float bombspeed;
    [SerializeField] private float preemptionCoef;
    [SerializeField] private float reloadTime;
    [SerializeField] private float fwdShootDetectionDistance;
    [SerializeField] private Transform _bullet;

    public bool isAttackMode;

    private Rigidbody _rb;
    private float _health;
    private float _reloadDown;
    private float _reloadFwd;
    private bool _hasRigidbody;
    private bool _isTurnedToFirst;
    private bool _isTurnedToLast;
    private bool _isFirstDestination;
    private bool _isDead;


    public void Attack()
    {
        Ray[] rayarray = new Ray[3];
        Ray fwd = new Ray();
        Color[] colorarray = new Color[3] { Color.blue, Color.magenta, Color.yellow };

        float bombPreemption = (((float)Math.Sqrt(2 * flightHeight / 9.81)) * bombspeed) + preemptionCoef;

        if (_isFirstDestination)
        {
            fwd = new Ray(transform.position, Vector3.left);
            for (int i = 0; i < 3; i++)
            {
                rayarray[i] = new Ray(transform.position, new Vector3(-(bombPreemption * (i + 1)), -flightHeight, 0));
            }
        }
        else
        {
            fwd = new Ray(transform.position, Vector3.right);
            for (int i = 0; i < 3; i++)
            {
                rayarray[i] = new Ray(transform.position, new Vector3(bombPreemption * (i + 1), -flightHeight, 0));
            }
        }

        if (Physics.Raycast(fwd, fwdShootDetectionDistance, targetToAttack))
        {
            //Debug.Log("Attack");
            ShootFwD();
        }
        Debug.DrawRay(fwd.origin, fwd.direction * fwdShootDetectionDistance, Color.green);

        for (int i = 0; i < 3; i++)
        {
            if (Physics.Raycast(rayarray[i], ((float)Math.Sqrt(bombPreemption * bombPreemption + flightHeight * flightHeight)) * 2.2f, targetToAttack))
            {
                //Debug.Log("Attack");
                ShootDown();
            }
            Debug.DrawRay(rayarray[i].origin, rayarray[i].direction * ((float)Math.Sqrt(bombPreemption * bombPreemption + flightHeight * flightHeight)) * 2.2f, colorarray[i]);
        }
    }

    private void Die()
    {
        isAttackMode = false;

        if (_hasRigidbody)
        {
            _rb.useGravity = true;
            _rb.constraints = RigidbodyConstraints.None;
            _rb.AddForce(new Vector3(0, 150, -550));
            _rb.AddTorque(0, 50, 5);
        }

    }
    private float GetHorizAngle(Transform targetobj)
    {
        //Определяет горизонтальный угол (в плоскости XZ) между целью и текущим объектом
        Vector2 target = new Vector2(targetobj.position.x, targetobj.position.z);
        Vector2 obj = new Vector2(transform.position.x, transform.position.z);
        Vector2 frw = new Vector2(transform.up.x, transform.up.z);

        Vector2 direction = target - obj;


        float angle = Vector2.Angle(frw, direction);
        //Debug.Log("Angle" + angle.ToString() + "Obj=" + obj + "|Target=" + target + "|FRW=" + frw + "|Direction=" + direction);
        //Debug.Log("Test angle" + angle.ToString());
        return angle;
    }

    void FixedUpdate()
    {
        if (_health > 0)
        {
            transform.Translate(0, 0, -transform.position.z, Space.World);      //костыль,для не предотвращения перемещения объекта по оси Z
            transform.Translate(0, 0, -transform.position.z, Space.Self);      //костыль,для не предотвращения перемещения объекта по оси Z

            bool isDirectToFirst = (_isTurnedToFirst && _isFirstDestination);
            bool isDirectToLast = (_isTurnedToLast && !_isFirstDestination);


            if (isDirectToFirst || isDirectToLast)
            {
                Move();
                Attack();
            }
        }
        else
        {
            if (!_isDead)
            {
                Die();
                _isDead = true;
            }
        }

    }

    public void Move()
    {
        if (_hasRigidbody)
        {
            if (_isFirstDestination)
            {
                _rb.AddForce(-horizontalAcceleration, 0, 0, ForceMode.Force);
            }
            else
            {
                if (_isTurnedToLast)
                {
                    _rb.AddForce(horizontalAcceleration, 0, 0, ForceMode.Force);
                }
            }
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            //Debug.Log("Collision with Player " + collision.impulse.y.ToString());
            if (_isTurnedToFirst)
            {
                _rb.AddForce(-horizontalAcceleration, -collision.impulse.y * 75, 0, ForceMode.Force);
            }
            else
            {
                _rb.AddForce(horizontalAcceleration, -collision.impulse.y * 75, 0, ForceMode.Force);
            }

            PhisicController player = collision.gameObject.GetComponent<PhisicController>();
            if (player != null)
            {
                player.TakeDamage(collision.impulse * 7.5f);
            }
        }

        _isFirstDestination = !_isFirstDestination;
    }

    public void TakeDamage(Vector3 damage)
    {
        float result = (float)Math.Sqrt(damage.x * damage.x + damage.y * damage.y);
        _health -= result;
        //Debug.Log("Enemy type B took damage" + result + "|Health=" + _health.ToString() + "|Vector damage=" + damage);
    }

    private void TurnAnAngle(float angle)
    {
        //Debug.Log("TurnAnAngle Works on Angle" + angle.ToString());
        transform.Rotate(Vector3.right, angle, Space.Self);
    }

    private void ShootDown()
    {
        if (_reloadDown <= 0)
        {
            Vector3 startposition;
            Vector3 velosity;
            float offset = 1f;
            int directionMultiplayer = 1;

            if (_isFirstDestination)
            {
                directionMultiplayer = -1;
            }

            startposition = new Vector3(transform.position.x, transform.position.y - offset, transform.position.z);
            velosity = new Vector3(bombspeed * directionMultiplayer, 0, 0);

            Transform bullet = Instantiate(_bullet, startposition, Quaternion.identity);
            EnemyBullet pb = bullet.GetComponent<EnemyBullet>();
            //Debug.Log("Begin Bullet X" + bullet.position.x + " Y" + bullet.position.y + " Z" + bullet.position.z);
            pb.SetVelocity(velosity);

            _reloadDown = reloadTime;
        }

        _reloadDown -= Time.deltaTime;

    }

    private void ShootFwD()
    {
        if (_reloadFwd <= 0)
        {
            Vector3 startposition;
            Vector3 velosity;
            float offset = 1f;
            int directionMultiplayer = 1;

            if (_isFirstDestination)
            {
                directionMultiplayer = -1;
            }

            startposition = new Vector3(transform.position.x + 2 * offset * directionMultiplayer, transform.position.y, transform.position.z);
            velosity = new Vector3(bombspeed * 3 * directionMultiplayer, 2, 0);

            Transform bullet = Instantiate(_bullet, startposition, Quaternion.identity);
            EnemyBullet pb = bullet.GetComponent<EnemyBullet>();
            pb.SetVelocity(velosity);

            _reloadFwd = reloadTime / 1.5f;
        }

        _reloadFwd -= Time.deltaTime;
    }
    // Start is called before the first frame update
    void Start()
    {
        _rb = GetComponent<Rigidbody>();

        if (_rb != null)
        {
            _hasRigidbody = true;
            _rb.freezeRotation = true;
            _rb.useGravity = false;

        }
        else
        {
            _hasRigidbody = false;
            Debug.Log("Enemy Type B Rigidbody is null");
        }

        isAttackMode = true;

        _health = 100;
        _isFirstDestination = true;
        _isTurnedToFirst = true;
        _isTurnedToLast = false;
        _isDead = false;
        _reloadDown = reloadTime;
        _reloadFwd = reloadTime / 1.5f;
    }

    // Update is called once per frame
    void Update()
    {
        if (transform.position.y > -5)
        {
            if (!_isDead)
            {
                //Debug.Log("Enemy angle 2D to Begin position is " + GetHorizAngle(biginMovingPoint).ToString());
                //Debug.Log("Enemy angle 2D to End position is " + GetHorizAngle(endMovingPoint).ToString());

                //Проверка к какой точке двигается объект
                if (transform.position.x <= beginMovingPoint.position.x)
                {
                    _isFirstDestination = false;
                }
                else if (transform.position.x >= endMovingPoint.position.x)
                {
                    _isFirstDestination = true;
                }

                //Debug.Log("Enemy _isFirstDestination " + _isFirstDestination);
                float angle;
                if (_isFirstDestination)
                {
                    angle = GetHorizAngle(beginMovingPoint);
                    //Проверка угла поворота на первую точку
                    if (angle > 0.05)
                    {
                        //Если объект не повёрнут, то довернуть
                        _isTurnedToFirst = false;
                        //Debug.Log("Angle=" + angle.ToString() + "_isFirstDestination" + _isFirstDestination);
                        TurnAnAngle(angle * rotationSpeed * Time.deltaTime);
                    }
                    else
                    {
                        _isTurnedToFirst = true;
                    }
                    //Debug.Log("Enemy _isTurnedToFirst " + _isTurnedToFirst);
                }
                else
                {
                    angle = GetHorizAngle(endMovingPoint);
                    //Проверка угла поворота на первую точку
                    if (angle > 0.05)
                    {

                        //Если объект не повёрнут, то довернуть
                        _isTurnedToLast = false;
                        //Debug.Log("Angle=" + angle.ToString() + "_isTurnedToLast" + _isTurnedToLast);
                        TurnAnAngle(angle * rotationSpeed * Time.deltaTime);
                    }
                    else
                    {
                        _isTurnedToLast = true;
                        //Debug.Log("Enemy _isTurnedToLast angle " + GetHorizAngle(endMovingPoint).ToString());
                    }
                    //Debug.Log("Enemy _isTurnedToLast " + _isTurnedToLast);
                }

                if (transform.position.y != flightHeight)
                {

                    float heightDifference = transform.position.y - flightHeight;
                    if (Mathf.Abs(heightDifference) > 0.01)
                    {
                        float smt = heightDifference * verticalSpeed * 0.5f * Time.deltaTime;
                        //Debug.Log("HeightDiference" + heightDifference.ToString()+ "|HeighChange value" + smt.ToString());
                        //transform.Translate(0, -heightDifference * verticalSpeed * 10 * Time.deltaTime, 0);
                        transform.position += new Vector3(0, -(heightDifference * verticalSpeed * 0.1f * Time.deltaTime) + 0);
                    }
                }
            }

        }
        else
        {
            Destroy(this.gameObject);
        }
    }
}
