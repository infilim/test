using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.VisualScripting;
using UnityEngine;

public class CarController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Rigidbody _rb;
    [SerializeField] private Transform[] _rayPoints;
    [SerializeField] private LayerMask _drivable;
    [SerializeField] private Transform _accelerationPoint;
    [SerializeField] private GameObject[] _tires = new GameObject[4];
    [SerializeField] private GameObject[] _frontTireParents = new GameObject[2];
    [SerializeField] private TrailRenderer[] _skidMarks = new TrailRenderer[2];
    [SerializeField] private ParticleSystem[] _skidSmokes = new ParticleSystem[2];

    [Header("Suspension Settings")]
    [SerializeField] private float _springStiffness;
    [SerializeField] private float _damperStiffness;
    [SerializeField] private float _restLenght;
    [SerializeField] private float _springTravel;
    [SerializeField] private float _wheelRadius;

    [Header("Input")]
    private float _moveInput = 0;
    private float _steerInput = 0;

    [Header("Car Settings")]
    [SerializeField] private float _acceleration = 25f;
    [SerializeField] private float _deceleration = 10f;
    [SerializeField] private float _maxSpeed = 100f;
    [SerializeField] private float _steerStrength = 15f;
    [SerializeField] private AnimationCurve _turningCurve;
    [SerializeField] private float _dragCoefficient = 1f;

    [Header("Visuals")]
    [SerializeField] private float _tireRotSpeed = 3000f;
    [SerializeField] private float _maxSteeringAngle = 30f;
    [SerializeField] private float _minSideSkidVelocity = 10f;

    private  Vector3 _currentCarLocalVelocity = Vector3.zero;
    private float _carVelocityRatio = 0;
    

    private int[] _wheelsIsGrounded = new int[4];
    private bool _isGrounded = false;



    #region Unity Dunction


    private void Start()
    {
        _rb = GetComponent<Rigidbody>();
    }


    private void FixedUpdate()
    {
        Suspension();
        GroundCheck();
        CalculateCarVelocity();
        Movement();
        Visuals();
    }

    private void Update()
    {
        GetPlayerInput();
    }

    #endregion

    #region Movement


    private void Movement()
    {
        if (_isGrounded)
        {
            Acceleration();
            Deceleration();
            Turn();
            SidewaysDrag();
        }
    }


    private void Acceleration()
    {
        _rb.AddForceAtPosition(_acceleration * _moveInput * transform.forward, _accelerationPoint.position, ForceMode.Acceleration);
    }


    private void Deceleration()
    {
        _rb.AddForceAtPosition(_deceleration * _moveInput * -transform.forward, _accelerationPoint.position, ForceMode.Acceleration);
    }


    private void Turn()
    {
        _rb.AddTorque(_steerStrength * _steerInput * _turningCurve.Evaluate(Mathf.Abs(_carVelocityRatio)) * Mathf.Sign(_carVelocityRatio) * transform.up, ForceMode.Acceleration);
    }


    private void SidewaysDrag()
    {
        float currentSidewaysSpeed = _currentCarLocalVelocity.x;
        float dragMagnitude = -currentSidewaysSpeed * _dragCoefficient;

        Vector3 dragForce = transform.right * dragMagnitude;

        _rb.AddForceAtPosition(dragForce, _rb.worldCenterOfMass, ForceMode.Acceleration);
    }

    #endregion

    #region Visuals


    private void Visuals()
    {
        TireVisuals();
        Vfx();
    }


    private void TireVisuals()
    {
        float steeringAngle = _maxSteeringAngle * _steerInput;

        for (int i = 0; i < _tires.Length; i++)
        {
            if(i < 2)
            {
                _tires[i].transform.Rotate(Vector3.right, _tireRotSpeed * _carVelocityRatio * Time.deltaTime, Space.Self);

                _frontTireParents[i].transform.localEulerAngles = new Vector3(_frontTireParents[i].transform.localEulerAngles.x, steeringAngle, _frontTireParents[i].transform.localEulerAngles.z);
            }
            else
            {
                _tires[i].transform.Rotate(Vector3.right, _tireRotSpeed * _moveInput * Time.deltaTime, Space.Self);
            }
        }
    }


    private void Vfx()
    {
        if (_isGrounded && _currentCarLocalVelocity.x > _minSideSkidVelocity)
        {
            ToggleSkidMarks(true);
            ToggleSkinedSmokes(true);
        }
        else
        {
            ToggleSkidMarks(false);
            ToggleSkinedSmokes(false);
        }
    }


    private void ToggleSkidMarks(bool toggle)
    {
        foreach (var skidMark in _skidMarks)
        {
            skidMark.emitting = toggle;
        }
    }


    private void ToggleSkinedSmokes(bool toggle)
    {
        foreach (var smoke in _skidSmokes)
        {
            if (toggle)
            {
                smoke.Play();
            }
            else
            {
                smoke.Stop();
            }
        }
    }

    private void SetTirePosition(GameObject tire, Vector3 targetPosition)
    {
        tire.transform.position = targetPosition;
    }
    #endregion

    #region Car Status Check


    private void GroundCheck()
    {
        int tempGroundedWheels = 0;

        for (int i = 0; i < _wheelsIsGrounded.Length; i++)
        {
            tempGroundedWheels += _wheelsIsGrounded[i];
        }

        if (tempGroundedWheels > 1)
        {
            _isGrounded = true;
        }
        else
        {
            _isGrounded = false;
        }
    }


    private void CalculateCarVelocity()
    {
        _currentCarLocalVelocity = transform.InverseTransformDirection(_rb.velocity);
        _carVelocityRatio = _currentCarLocalVelocity.z / _maxSpeed;
    }



    #endregion

    #region Input Handling


    private void GetPlayerInput()
    {
        _moveInput = Input.GetAxis("Vertical");
        _steerInput = Input.GetAxis("Horizontal");
    }

    #endregion

    #region Suspension Functions


    private void Suspension()
    {
        for (int i = 0; i < _rayPoints.Length; i++)
        {
            RaycastHit hit;
            float maxDistance = _restLenght + _springTravel;

            if (Physics.Raycast(_rayPoints[i].position, -_rayPoints[i].up, out hit, maxDistance + _wheelRadius, _drivable))
            {
                _wheelsIsGrounded[i] = 1;

                float currentSpringLenght = hit.distance - _wheelRadius;
                float springCompression = (_restLenght - currentSpringLenght) / _springTravel;

                float springVelocity = Vector3.Dot(_rb.GetPointVelocity(_rayPoints[i].position), _rayPoints[i].up);
                float dampForce = _damperStiffness * springCompression;
                
                float springForce = _springStiffness * springCompression;

                float netForce = springForce - dampForce;

                _rb.AddForceAtPosition(netForce * _rayPoints[i].up, _rayPoints[i].position);

                //Visuals

                SetTirePosition(_tires[i], hit.point + _rayPoints[i].up * _wheelRadius);

                Debug.DrawLine(_rayPoints[i].position, hit.point, Color.red);
            }
            else
            {
                _wheelsIsGrounded[i] = 0;

                //Visuals

                SetTirePosition(_tires[i], _rayPoints[i].position - _rayPoints[i].up * maxDistance);

                Debug.DrawLine(_rayPoints[i].position, _rayPoints[i].position + (_wheelRadius + maxDistance) * -_rayPoints[i].up, Color.green);
            }
        }
    }
    #endregion
}
