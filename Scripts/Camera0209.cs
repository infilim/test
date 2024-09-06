using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Camera0209 : MonoBehaviour
{
    [SerializeField] private Transform _target;
    [SerializeField] private Vector3 _offset;
    [SerializeField] private Vector3 _eulerRotation;
    [SerializeField] private float _damper;


    private void Start()
    {
        transform.eulerAngles = _eulerRotation;
    }


    private void Update()
    {
        if (_target == null)
            return;

        transform.position = Vector3.Lerp(transform.position, _target.position + _offset, _damper * Time.deltaTime);
    }
}
