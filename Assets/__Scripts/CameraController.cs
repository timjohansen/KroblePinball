using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Splines;

public class CameraController : MonoBehaviour
{
    public Vector3 ballScreenPos;
    public float attractAnimTime = 0f;
    private Vector3[] _attractCameraLocations;
    private int _attractCameraStep;
    
    Vector3 _camBottomPos = new(0f, 3f, -3.9f);
    Vector3 _camTopPos = new(0f, 3.05f, -1.82f);
    private Vector3 _multiballPos = new(0f, 4.4f, -3.45f);

    private Quaternion _gameRotation = Quaternion.Euler(75, 0, 0);

    private float _ballTopZ = 1f;
    private float _ballBottomZ = -3f;
    
    public float boxRadius = .25f;
    public float boxPos = 0f;
    

    private float _normalizedCamPos = 0f;
    private float _normalizedBoxPos = 0f;
    private Camera _camera;

    private float _nudgeAnimTimer;
    private float _nudgeDuration = .1f;
    private Vector3 _nudgeVector;

    private void Awake()
    {
        _camera = Camera.main;
        if (_camera)
            _camera.transform.localPosition = _camBottomPos;
        _attractCameraLocations = new Vector3[3]
        {
            _multiballPos, _camBottomPos, _camTopPos
        };
        boxPos = _camBottomPos.z;

        transform.position = Vector3.zero;
    }
    
    void Update()
    {
        if (GM.inst.mode == GM.GameMode.Title)
        {
            attractAnimTime = Mathf.MoveTowards(attractAnimTime, 1f, Time.deltaTime * .2f);
            if (attractAnimTime >= 1f)
            {
                _attractCameraStep++;
                _attractCameraStep %= _attractCameraLocations.Length;
                attractAnimTime = 0f;
            }
            Vector3 posA = _attractCameraLocations[_attractCameraStep];
            Vector3 posB = _attractCameraLocations[(_attractCameraStep + 1) % _attractCameraLocations.Length];
            _camera.transform.localPosition = Vector3.Lerp(posA, posB, attractAnimTime);

            return;
        }
        
        
        if (_nudgeAnimTimer > 0f)
        {
            _nudgeAnimTimer = Mathf.MoveTowards(_nudgeAnimTimer, 0f, Time.deltaTime);
            float value = _nudgeAnimTimer / _nudgeDuration;
            if (value < .5f)
            {
                value *= 2f;
                transform.position = Vector3.Lerp(Vector3.zero, _nudgeVector, value);
            }
            else
            {
                value = (value - .5f) * 2f;
                transform.position = Vector3.Lerp(_nudgeVector, Vector3.zero, value);
            }
        }
        
        // Move camera to see the whole board if more than one ball is in play
        if (GM.inst.ballDispenser.ballsInPlay > 1)
        {
            _camera.transform.localPosition = Vector3.MoveTowards(_camera.transform.localPosition, _multiballPos, 1f * Time.deltaTime);
            return;
        }
        
        // Otherwise, move it as if the ball is inside a sliding box. If the ball pushes on the upper edge, the box
        // moves upward with the ball, and the same goes for the lower edge. This keeps camera movement from getting
        // too erratic.
        
        if (GM.inst.ballDispenser.ballsInPlay == 0)
            _normalizedBoxPos = 0f;
        else
        {
            List<GameObject> activeBalls = GM.inst.ballDispenser.GetActiveBalls();
            if (activeBalls == null || activeBalls.Count == 0)
            {
                return;
            }
            Ball ball = activeBalls[0].GetComponent<Ball>();
            Vector3 ballWorldPos = ball.ball3D.transform.position;

            // Move the camera box if the ball has moved outside of it
            if (ballWorldPos.z > boxPos + boxRadius)
            {
                boxPos = ballWorldPos.z - boxRadius;
            }
            else if (ballWorldPos.z < boxPos - boxRadius)
            {
                boxPos = ballWorldPos.z + boxRadius;
            }

            _normalizedBoxPos = Mathf.InverseLerp(_ballBottomZ, _ballTopZ, boxPos);
            _normalizedBoxPos = Mathf.Clamp01(_normalizedBoxPos);

        }
        _normalizedCamPos = Mathf.InverseLerp(_camBottomPos.z, _camTopPos.z, _camera.transform.localPosition.z);
        Vector3 camTarget = Vector3.Lerp(_camBottomPos, _camTopPos, _normalizedBoxPos);
        float distFromBox = Mathf.Abs(_normalizedCamPos - _normalizedBoxPos);

        // The camera's speed is determined by distance from it and the ball/box.
        // If the ball/box is getting farther from the camera, the camera will move faster to catch up.
        float speed = 40f * distFromBox;
        _camera.transform.localRotation = _gameRotation;
        _camera.transform.localPosition = Vector3.MoveTowards(_camera.transform.localPosition, camTarget, speed * Time.deltaTime);

    }

    public void Nudge(Vector2 direction)
    {
        _nudgeAnimTimer = _nudgeDuration;
        
        if (direction == Vector2.up)
            _nudgeVector = new Vector3(0f, 0f, .1f);
        else if (direction == Vector2.left)
            _nudgeVector = new Vector3(-.1f, 0f, 0f);
        else if (direction == Vector2.right)
            _nudgeVector = new Vector3(.1f, 0f, 0f);
    }
    
    private void OnDrawGizmos()
    {
        // Visualizes the camera box in the editor
        Gizmos.color = Color.white;
        Gizmos.matrix = Matrix4x4.TRS(new Vector3(0f, 0f, boxPos), Quaternion.identity, new Vector3(10f, .25f, boxRadius * 2f));
        Gizmos.DrawWireCube(Vector3.zero, Vector3.one);
           
        
    }
}
