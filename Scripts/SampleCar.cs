using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class SampleCar : MonoBehaviour
{
    [SerializeField] private Transform _transformFL;
    [SerializeField] private Transform _transformFR;
    [SerializeField] private Transform _transformRL;
    [SerializeField] private Transform _transformRR;

    [SerializeField] private WheelCollider _colliderFL;
    [SerializeField] private WheelCollider _colliderFR;
    [SerializeField] private WheelCollider _colliderRL;
    [SerializeField] private WheelCollider _colliderRR;

    [SerializeField] private float _force;

    private float _maxAngle = 40f;


    private void FixedUpdate()
    {
        _colliderFL.motorTorque = Input.GetAxis("Vertical") * _force;
        _colliderFR.motorTorque = Input.GetAxis("Vertical") * _force;

        if (Input.GetKey(KeyCode.Space))
        {
            _colliderFL.brakeTorque = 3000f;
            _colliderFR.brakeTorque = 3000f;
            _colliderRL.brakeTorque = 3000f;
            _colliderRR.brakeTorque = 3000f;
        }
        else
        {
            _colliderFL.brakeTorque = 0f;
            _colliderFR.brakeTorque = 0f;
            _colliderRL.brakeTorque = 0f;
            _colliderRR.brakeTorque = 0f;
        }


        _colliderFL.steerAngle = _maxAngle * Input.GetAxis("Horizontal");
        _colliderFR.steerAngle = _maxAngle * Input.GetAxis("Horizontal");

        RotateWheel(_colliderFL, _transformFL);
        RotateWheel(_colliderFR, _transformFR);
        RotateWheel(_colliderRL, _transformRL);
        RotateWheel(_colliderRR, _transformRR);
    }


    private void RotateWheel(WheelCollider collider, Transform transform)
    {
        Vector3 position;
        Quaternion rotation;

        collider.GetWorldPose(out position, out rotation);

        transform.position = position;
        transform.rotation = rotation;
    }

}
