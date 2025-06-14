using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Splines;

public class CameraController : MonoBehaviour
{
    private Camera _camera;
    private Quaternion _portraitRotation = Quaternion.Euler(85f, 0, 0);
    private Quaternion _landscapeRotation = Quaternion.Euler(75f, 0, 0);
    private float _portraitFOV = 120f;
    private float _landscapeFOV = 70f;
    
    // Scrolling variables
    private Vector3 _camTopPos = new(0f, 3.05f, -1.82f);    // The highest possible camera position
    private Vector3 _camBottomPos = new(0f, 3f, -3.9f);     // The lowest possible camera position
    private Vector3 _multiballPos = new(0f, 4.4f, -3.45f);  // The camera's position when multiple balls are in play
    private float _normalizedCamPos;
    
    private float _ballTopZ = 1f;                           // The highest possible ball position
    private float _ballBottomZ = -3f;                       // The lowest possible ball position
    
    private float _boxRadius = .25f;
    private float _boxPos = 0f;
    private float _normalizedBoxPos;
    
    // Nudge animation variables
    private float _nudgeAnimTimer;
    private float _nudgeDuration = .1f;         
    private Vector3 _nudgeVector;               
    
    // Title screen animation variables
    private Vector3[] _titleCameraLocations;    // Locations the camera will cycle between on title screen
    private int _titleCameraStep;               // Which location is currently being moved toward
    private float _titleAnimTime = 0f;          // Progress toward current location
    

    private void Awake()
    {
        _camera = Camera.main;
        if (_camera)
            _camera.transform.localPosition = _camBottomPos;    // Set position to the bottom of the screen
        _titleCameraLocations = new Vector3[3]
        {
            _multiballPos, _camBottomPos, _camTopPos
        };
        _boxPos = _camBottomPos.z;

        transform.position = Vector3.zero;
    }
    
    void Update()
    {
        var rot = Screen.width > Screen.height ? _landscapeRotation : _portraitRotation;
        _camera.fieldOfView = Screen.width > Screen.height ? _landscapeFOV : _portraitFOV;
        if (GM.inst.mode == GM.GameMode.Title)
        {
            // Cycle through an array of locations while waiting for the game to start
            _titleAnimTime = Mathf.MoveTowards(_titleAnimTime, 1f, Time.deltaTime * .2f);
            if (_titleAnimTime >= 1f)
            {
                _titleCameraStep++;
                _titleCameraStep %= _titleCameraLocations.Length;
                _titleAnimTime = 0f;
            }
            Vector3 posA = _titleCameraLocations[_titleCameraStep];
            Vector3 posB = _titleCameraLocations[(_titleCameraStep + 1) % _titleCameraLocations.Length];
            _camera.transform.localPosition = Vector3.Lerp(posA, posB, _titleAnimTime);
            _camera.transform.localRotation = rot;

            return;
        }
        
        // If a nudge happened, move the camera's parent object (not the camera itself)
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
            if (ballWorldPos.z > _boxPos + _boxRadius)
            {
                _boxPos = ballWorldPos.z - _boxRadius;
            }
            else if (ballWorldPos.z < _boxPos - _boxRadius)
            {
                _boxPos = ballWorldPos.z + _boxRadius;
            }

            _normalizedBoxPos = Mathf.InverseLerp(_ballBottomZ, _ballTopZ, _boxPos);
            _normalizedBoxPos = Mathf.Clamp01(_normalizedBoxPos);

        }
        _normalizedCamPos = Mathf.InverseLerp(_camBottomPos.z, _camTopPos.z, _camera.transform.localPosition.z);
        Vector3 camTarget = Vector3.Lerp(_camBottomPos, _camTopPos, _normalizedBoxPos);
        float distFromBox = Mathf.Abs(_normalizedCamPos - _normalizedBoxPos);

        // The camera's speed is determined by distance from it and the ball/box.
        // If the ball/box is getting farther from the camera, the camera will move faster to catch up.
        float speed = 40f * distFromBox;
        _camera.transform.localRotation = rot;
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
        Gizmos.matrix = Matrix4x4.TRS(new Vector3(0f, 0f, _boxPos), Quaternion.identity, new Vector3(10f, .25f, _boxRadius * 2f));
        Gizmos.DrawWireCube(Vector3.zero, Vector3.one);
           
        
    }
}
