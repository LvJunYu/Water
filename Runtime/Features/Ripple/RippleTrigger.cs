using System;
using UnityEngine;
using LYU.WaterSystem.Data;

[ExecuteAlways]
public class RippleTrigger : MonoBehaviour
{
    public Vector3 Offset;
    public float radius;
    private Vector3 _lastPos;

    private void OnEnable()
    {
        _lastPos = Vector3.positiveInfinity;
    }

    protected virtual void Update()
    {
        RippleSetting.UpdateRippleTrigger(this);
        _lastPos = transform.position;
    }

    private void ClearUp()
    {
        RippleSetting.ClearUpTrigger(this);
    }

    private void OnDisable()
    {
        ClearUp();
    }

    private void OnDestroy()
    {
        ClearUp();
    }

    public bool CheckSamePos()
    {
        return _lastPos == transform.position;
    }
}