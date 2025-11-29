using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Assets.Scripts;
using System;

public class EnemyTypeA : MonoBehaviour, IEnemy
{
    [SerializeField] private LayerMask ground;
    [SerializeField] private LayerMask targetToAttack;
    [SerializeField] private Transform biginMovingPoint;
    [SerializeField] private Transform endMovingPoint;
    [SerializeField] private float horizontalAcceleration;
    [SerializeField] private float rotationSpeed;

    public bool isAttackMode;

    private Rigidbody _rb;
    private float _health;
    private bool _hasRigidbody;
    private bool _isGrounded;
    private bool _isTurnedToFirst;
    private bool _isTurnedToLast;
    private bool _isFirstDestination;
    private bool _isAttacking;
    private bool _isDead;
    public void Attack()
    {
        
        Debug.Log("ATTACK");
        if (IsGrounded())
        {
            //Debug.Log("_isTurnedToFirst=" + _isTurnedToFirst + "|_isTurnedToLast=" + _isTurnedToLast+ "|_isFirstDestination=" + _isFirstDestination);
            if (_hasRigidbody)
            {
                if (_isFirstDestination)
                {
                    if (_isTurnedToFirst)
                    {
                        _rb.AddForce(-horizontalAcceleration * 2.5f, 0, 0, ForceMode.Force);
                    }
                }
                else
                {
                    if (_isTurnedToLast)
                    {
                        _rb.AddForce(horizontalAcceleration * 2.5f, 0, 0, ForceMode.Force);
                    }
                }
            }
            
        }
    }

    private void Die()
    {
        if (_hasRigidbody)
        {
            _rb.constraints = RigidbodyConstraints.None;
            _rb.AddForce(new Vector3(0, 150, -250));
            _rb.AddTorque(0, 0, 10);
        }
        
    }

    private bool IsGrounded()
    {
        Ray ray = new Ray(transform.position, -Vector3.up);
        //Debug.DrawRay(ray.origin, ray.direction * 1f, Color.blue);

        if (Physics.Raycast(ray, 1.1f, ground))
        { _isGrounded = true; }
        else
        { _isGrounded = false; }

        //Debug.Log("Enemy Type A IsGrounded=" + _isGrounded);
        return _isGrounded;
    }

    private float GetHorizAngle(Transform targetobj)
    {
        //Определяет горизонтальный угол (в плоскости XZ) между целью и текущим объектом
        Vector2 target = new Vector2(targetobj.position.x, targetobj.position.z);
        Vector2 obj = new Vector2(transform.position.x, transform.position.z);
        Vector2 frw = new Vector2(transform.forward.x, transform.forward.z);

        Vector2 direction = target - obj;
        float angle = Vector2.Angle(frw, direction);
        return angle;
    }

    void FixedUpdate()
    {
        if (_health > 0)
        {
            transform.Translate(0, 0, -transform.position.z, Space.World);      //костыль,для не предотвращения перемещения объекта по оси Z
            transform.Translate(0, 0, -transform.position.z, Space.Self);      //костыль,для не предотвращения перемещения объекта по оси Z

            Ray ray;

            if (_isFirstDestination)
            {
                ray = new Ray(transform.position, Vector3.left);
            }
            else
            {
                ray = new Ray(transform.position, Vector3.right);
            }


            //Debug.DrawRay(ray.origin, ray.direction * 8, Color.green);
            //Блок условий, для корректного включения метода Attack()

            bool isDirectToFirst = (_isTurnedToFirst && _isFirstDestination);
            bool isDirectToLast = (_isTurnedToLast && !_isFirstDestination);

            if (Physics.Raycast(ray, 8f, targetToAttack) && (isDirectToFirst || isDirectToLast))
            {
                isAttackMode = true;
                Attack();
            }
            else
            {
                isAttackMode = false;
                Move();
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
        if (IsGrounded())
        {
            if (_hasRigidbody)
            {
                if (_isFirstDestination)
                {
                    if (_isTurnedToFirst)
                    {
                        _rb.AddForce(-horizontalAcceleration, 0, 0, ForceMode.Force);
                    }
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
    }

    void OnCollisionEnter(Collision collision)
    {
        //Debug.Log("Enemy Collision" + collision.gameObject.name);
        if (collision.gameObject.CompareTag("Player"))
        {
            if (isAttackMode)
            {
                PhisicController player = collision.gameObject.GetComponent<PhisicController>();
                if (player != null)
                {
                    //Vector3 currentVelocity = _rb.velocity;
                    player.TakeDamage(collision.impulse*7.5f);
                }
            }
            //Debug.Log("BulletCollision with Player" + collision.gameObject.name + "/Velocity" + _rb.velocity);
            
        }
    }

    public void TakeDamage(Vector3 damage)
    {
        float result = (float)Math.Sqrt(damage.x * damage.x + damage.y * damage.y);
        _health -= result;
        //Debug.Log("Enemy type A took damage" + result+"|Health="+_health.ToString()+"|Vector damage="+ damage);
    }

    private void TurnTo(Transform targetobj)
    {
        Vector2 target = new Vector2(targetobj.position.x, targetobj.position.z);
        Vector2 obj = new Vector2(transform.position.x, transform.position.z);
        Vector2 direction2d = target - obj;
        Vector3 direction = new Vector3(direction2d.x, 0, direction2d.y);

        //Debug.Log("Turning Angle Before=" + GetHorizAngle(targetobj).ToString());
        Quaternion targetRotation = Quaternion.LookRotation(direction);
        transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, rotationSpeed);                                //для плавности разворота
        //transform.rotation = Quaternion.LookRotation(direction * (Time.deltaTime * rotationSpeed * rotationMultiplayer));     //слишком быстро, и нет возможности замедлить
        //Debug.Log("Turning Angle After=" + GetHorizAngle(targetobj).ToString());
    }

    // Start is called before the first frame update
    void Start()
    {
        _rb = GetComponent<Rigidbody>();

        if (_rb != null)
        {
            _hasRigidbody = true;
            _rb.freezeRotation = true;

        }
        else
        {
            _hasRigidbody = false;
            Debug.Log("Enemy Type A Rigidbody is null");
        }

        isAttackMode = false;

        _health = 100;
        _isFirstDestination=true;
        _isTurnedToFirst = true;
        _isTurnedToLast=false;
        _isDead = false;

    }

// Update is called once per frame
void Update()
    {
        if (transform.position.y > -5)
        {
            //Debug.Log("Enemy angle 2D to Begin position is " + GetHorizAngle(biginMovingPoint).ToString());
            //Debug.Log("Enemy angle 2D to End position is " + GetHorizAngle(endMovingPoint).ToString());
            if (!isAttackMode)
            {
                //Проверка к какой точке двигается объект
                if (transform.position.x <= biginMovingPoint.position.x)
                {
                    _isFirstDestination = false;
                }
                else if (transform.position.x >= endMovingPoint.position.x)
                {
                    _isFirstDestination = true;
                }

                //Debug.Log("Enemy _isFirstDestination " + _isFirstDestination);

                if (_isFirstDestination)
                {
                    //Проверка угла поворота на первую точку
                    if ((GetHorizAngle(biginMovingPoint)) > 0.05)
                    {
                        //Если объект не повёрнут, то довернуть
                        _isTurnedToFirst = false;
                        TurnTo(biginMovingPoint);
                    }
                    else
                    {
                        _isTurnedToFirst = true;
                    }
                    //Debug.Log("Enemy _isTurnedToFirst " + _isTurnedToFirst);
                }
                else
                {
                    if ((GetHorizAngle(endMovingPoint)) > 0.05)
                    {
                        //Если объект не повёрнут, то довернуть
                        _isTurnedToLast = false;
                        TurnTo(endMovingPoint);
                    }
                    else
                    {
                        _isTurnedToLast = true;
                        //Debug.Log("Enemy _isTurnedToLast angle " + GetHorizAngle(endMovingPoint).ToString());
                    }
                    //Debug.Log("Enemy _isTurnedToLast " + _isTurnedToLast);
                }
            }
        }
        else
        {
            Destroy(this.gameObject);
        }   
    }
}
