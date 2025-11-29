using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Assets.Scripts;
using System;

public class PhisicController : MonoBehaviour
{
    [SerializeField] private LayerMask _ground;
    [SerializeField] private float _verticalAcceleration;
    [SerializeField] private float _horizontalAcceleration;
    [SerializeField] private float _rotationSpeed;
    [SerializeField] private float _bulletHorizontalSpeed;
    [SerializeField] private float _bulletVerticalSpeed;
    [SerializeField] private Transform _bullet;

    private Rigidbody _rb;
    private float _health;
    private bool _hasRigidbody;
    private KeyCode _downedKey;
    private bool _isGrounded;
    private bool _isStandUp;
    private bool _isDead;
    public Action<GameObject> OnDestroy;
    public Action<GameObject, int> OnDamage;

    private void Awake()
    {
        _health = 100;
    }
    private void Destroy()
    {
        OnDestroy?.Invoke(this.gameObject);
        Destroy(this.gameObject);
    }

    private void Duck()
    {
        transform.localScale = new Vector3(1, 0.5f, 1);
        _isStandUp = false;
        _downedKey = KeyCode.None;
    }

    private void Die()
    {
        if (_hasRigidbody)
        {
            _rb.constraints = RigidbodyConstraints.None;
            _rb.AddForce(new Vector3(0, 250, -550));
        }

    }
    private void GoLeft()
    {
        //Debug.Log("GoLeft Works");
        if (transform.rotation.y == 1)
        {

            if (IsGrounded())
            {
                _rb.AddForce(-_horizontalAcceleration, 0, 0, ForceMode.Acceleration);
            }
            else
            {
                _rb.AddForce(-_horizontalAcceleration * 0.5f, 0, 0, ForceMode.Acceleration);
            }
            _downedKey = KeyCode.None;
        }
    }
    private void GoRight()
    {
        //Debug.Log("GoRight Works");
        if (transform.rotation.y == 0)
        {
            if (IsGrounded())
            {
                _rb.AddForce(_horizontalAcceleration, 0, 0, ForceMode.Acceleration);
            }
            else
            {
                _rb.AddForce(_horizontalAcceleration * 0.5f, 0, 0, ForceMode.Acceleration);
            }
            _downedKey = KeyCode.None;
        }
    }

    public float GetHealthF()
    {
        if (_health > 0)
        {
            return _health;
        }
        else
        {
            return 0;
        }  
    }

    public int GetHealthI()
    {
        if (_health > 0)
        {
            return (int)_health;
        }
        else
        {
            return 0;
        }
    }

    private bool IsGrounded()
    {
        Ray ray = new Ray(transform.position, -Vector3.up);
        Debug.DrawRay(ray.origin,ray.direction*1f, Color.red);

        if (Physics.Raycast(ray, 1.1f, _ground))
        {_isGrounded = true;}
        else
        { _isGrounded = false;}

        //Debug.Log("Phisics IsGrounded=" +_isGrounded);
        return _isGrounded;


    }
    void FixedUpdate()
    {
        if (_health > 0)
        {
            transform.Translate(0, 0, -transform.position.z, Space.World);      //костыль,для не предотвращения перемещения объекта по оси Z
                                                                                //Debug.Log("FixedUpdate Works");
            if (_hasRigidbody)
            {
                switch (_downedKey)
                {
                    case KeyCode.UpArrow:
                        Jump();
                        //Debug.Log("UpArrow pressed");
                        break;
                    case KeyCode.DownArrow:
                        Duck();
                        //Debug.Log("DownArrow pressed");
                        break;
                    case KeyCode.LeftArrow:
                        GoLeft();
                        //Debug.Log("DownArrow pressed");
                        break;
                    case KeyCode.RightArrow:
                        GoRight();
                        //Debug.Log("DownArrow pressed");
                        break;
                    case KeyCode.None:
                        StandUp();
                        //Debug.Log("Nothing pressed");
                        break;
                    default:
                        //Debug.Log("Other key pressed");
                        break;
                }
            }
        }
        else
        {
            if (!_isDead)
            {
                _isDead = true;
                Die();
            }
        }
    }

    private void Jump()
    {
        if (IsGrounded())
        {
            _rb.AddForce(0, _verticalAcceleration, 0, ForceMode.Acceleration);
        }
        _downedKey = KeyCode.None;
    }

    void OnCollisionEnter(Collision collision)
    {
        //Debug.Log("Player Collision" + collision.gameObject.name);

        if (collision.gameObject.name == "Ceiling")
        {
            if (_hasRigidbody)
            {
                _rb.AddForce(-collision.impulse);
            }
        }
    }

    public void TakeDamage(Vector3 damage)
    {
        float result = (float)Math.Sqrt(damage.x * damage.x + damage.y * damage.y);
        _health -= result;
        Debug.Log("Player A took damage" + result+"|Health="+_health.ToString()+"|Vector damage="+ damage);

        int transmitted;
        if (_health > 0)
        {
            transmitted = (int)_health;
        }
        else
        {
            transmitted = 0;
        }

        OnDamage?.Invoke(this.gameObject, transmitted);
    }
    private void Shoot()
    {

        Vector3 startposition;
        Vector3 velosity;
        float offset=0;
        int directionMultiplayer = 1;

        if (transform.rotation.y == 0)
        {
            offset = 0.75f;
            //velosity = new Vector3(10, 0.1f, 0);
            //pb.SetVelocity(new Vector3(10, 0.1f, 0));
        }
        else if (transform.rotation.y == 1)
        {
            offset = -0.75f;
            directionMultiplayer = -1;
            //bullet.Translate(transform.position.x - 0.75f, transform.position.y, transform.position.z);


            //pb.SetVelocity(new Vector3(10, 0.1f, 0));
        }

        startposition = new Vector3(transform.position.x + offset, transform.position.y, transform.position.z);
        velosity = new Vector3(_bulletHorizontalSpeed * directionMultiplayer, _bulletVerticalSpeed, 0);

        Transform bullet = Instantiate(_bullet, startposition, Quaternion.identity);
        PlayerBullet pb = bullet.GetComponent<PlayerBullet>();
        //Debug.Log("Begin Bullet X" + bullet.position.x + " Y" + bullet.position.y + " Z" + bullet.position.z);
        pb.SetVelocity(velosity);

    }

    // Start is called before the first frame update
    void Start()
    {
        _rb = GetComponent<Rigidbody>();

        if (_rb != null)
        {
            _hasRigidbody = true;
            _rb.constraints = RigidbodyConstraints.FreezePositionZ;                             //наложение перемещения вращения по оси Z, чтобы не смещался по время столкновений
            _rb.freezeRotation = true;
            
        }
        else
        {
            _hasRigidbody = false;
            Debug.Log("Rigidbody is null");
        }

        _isGrounded = false;
        _isStandUp = true;
        _isDead = false;

    }

    private void StandUp()
    {
        if (!_isStandUp)
        {
            transform.localScale = new Vector3(1, 1, 1);
            _isStandUp = true;
        }

    }

    private void TurnLeft()
    {
        //Debug.Log("Turn Left Works.Rotation is " + transform.rotation.y);
        _rb.freezeRotation = false;                                                         //отмена ограничений на вращение по всем осям
        _rb.constraints = RigidbodyConstraints.FreezeRotationZ;                             //наложение ограничения вращения по оси Z, чтобы не упал
        if (transform.rotation.y != 1)
        {
            if (transform.rotation.y < 0.95)
            {
                transform.Rotate(0.0f, _rotationSpeed * Time.deltaTime, 0.0f, Space.Self);  //плавный поворот

            }
            else
            {
                //блок с мгновенным поворотом для нужного угла
                transform.rotation = Quaternion.Euler(0f, 180f, 0f);
            }
        }

        _rb.constraints = RigidbodyConstraints.None;                                        //отмена ограничения вращения по оси Z, чтобы не упал
        _rb.constraints = RigidbodyConstraints.FreezePositionZ;                             //ограничение перемещения по оси Z
        _rb.freezeRotation = true;                                                          //ограничение вращения всем осям
    }

    private void TurnRight()
    {
        //Debug.Log("Turn Right Works.Rotation is " + transform.rotation.y);
        _rb.freezeRotation = false;                                                         //отмена ограничений на вращение по всем осям
        _rb.constraints = RigidbodyConstraints.FreezeRotationZ;                             //наложение ограничения вращения по оси Z, чтобы не упал
        if (transform.rotation.y != 0)
        {
            if (transform.rotation.y > 0.05)
            {
                transform.Rotate(0.0f, -_rotationSpeed * Time.deltaTime, 0.0f, Space.Self);  //плавный поворот

            }
            else
            {
                //блок с мгновенным поворотом для нужного угла
                transform.rotation = Quaternion.Euler(0f, 0f, 0f);
            }
        }
        _rb.constraints = RigidbodyConstraints.None;                                        //отмена ограничения вращения по оси Z, чтобы не упал
        _rb.constraints = RigidbodyConstraints.FreezePositionZ;                             //ограничение перемещения по оси Z
        _rb.freezeRotation = true;                                                          //ограничение вращения всем осям
    }

    

    // Update is called once per frame
    void Update()
    {
        //Debug.Log("Update Works");
        if (transform.position.y > -5)
        {
            if (Input.anyKeyDown)
            {
                //_isKeyUp = false;
            }
            else
            {
                //_isKeyUp = true;
                //_downedKey = KeyCode.None;
            }


            if (Input.GetKey(KeyCode.UpArrow))
            {
                _downedKey = KeyCode.UpArrow;
                //_isKeyUp = true;
            }
            else if (Input.GetKey(KeyCode.DownArrow))
            {
                _downedKey = KeyCode.DownArrow;
                //_isKeyUp = true;
            }
            else if (Input.GetKey(KeyCode.LeftArrow))
            {
                if (transform.rotation.y != 1)
                { TurnLeft(); }
                else
                {
                    _downedKey = KeyCode.LeftArrow;
                    //_isKeyUp = true;
                }
            }
            else if (Input.GetKey(KeyCode.RightArrow))
            {
                if (transform.rotation.y != 0)
                { TurnRight(); }
                else
                {
                    _downedKey = KeyCode.RightArrow;
                    //_isKeyUp = true;
                }
            }
            else if (Input.GetKeyDown(KeyCode.Space))
            {
                Shoot();
            }
        }
        else
        {
            Destroy();
        }
        
        //else
        //{
        //    Debug.Log("Other key pressed");
        //}

    }
}

