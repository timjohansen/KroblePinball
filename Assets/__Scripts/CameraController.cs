using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Splines;

public class CameraController : MonoBehaviour
{
    private Camera _camera;
    
    // Scrolling variables
    private Vector3[] _camTopPositions = new Vector3[]        // The highest possible camera position
    {
        new(0f, 4.22f, -3.6f),       // Portrait
        new(0f, 3.05f, -1.82f)      // Landscape
    };
    private Vector3[] _camBottomPositions = new Vector3[]    // The lowest possible camera position
    {
        new(0f, 3.15f, -3.6f),        // Portrait
        new(0f, 3f, -3.9f)          // Landscape
    };
    
    private Vector3[] _multiballPositions = new Vector3[]    // The camera's position when multiple balls are in play
    {
        new(0f, 4.3f, -4),          // Portrait
        new(0f, 4.4f, -3.45f)       // Landscape
    };
    private float[] _camFieldOfViews = new float[]
    {
        105f,                       // Portrait
        70f                         // Landscape
    }; 
    private Quaternion[] _camRotations = new Quaternion[]
    {
        Quaternion.Euler(75f, 0, 0),    // Portrait
        Quaternion.Euler(75f, 0, 0)     // Landscape
    };

    private Vector3[][] _titleScreenPositionArrays = new Vector3[][]
    {
        new Vector3[]
        {
            new (0f, 2.35f, -3.8f),
            new (0f, 1.80f, -1.46f),
            new (0f, 4.13f, -3.8f)
        },
        new Vector3[]
        {
            new (0f, 4.4f, -3.45f),
            new (0f, 3f, -3.9f),
            new (0f, 3.05f, -1.82f)
        }
    };

    private Vector3 _camTopPos;
    private Vector3 _camBottomPos;
    private Vector3 _multiballPos;
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
    private Vector3[] _titleScreenPosArray;    // Locations the camera will cycle between on title screen
    private int _titleCameraStep;               // Which location is currently being moved toward
    private float _titleAnimTime = 0f;          // Progress toward current location

    private int _prevWidth;
    private int _prevHeight;
    
    private void Awake()
    {
        _camera = Camera.main;
        RefreshCameraValues();
        if (_camera)
            _camera.transform.localPosition = _camBottomPos;    // Set position to the bottom of the screen
        
        transform.position = Vector3.zero;
        _boxPos = _camBottomPos.z;
    }
    
    void Update()
    {
        if (_prevWidth != Screen.width || _prevHeight != Screen.height)
        {
            RefreshCameraValues();
        }
        _prevWidth = Screen.width;
        _prevHeight = Screen.height;
        
        if (GM.inst.mode == GM.GameMode.Title)
        {
            // Cycle through an array of locations while waiting for the game to start
            _titleAnimTime = Mathf.MoveTowards(_titleAnimTime, 1f, Time.deltaTime * .2f);
            if (_titleAnimTime >= 1f)
            {
                _titleCameraStep++;
                _titleCameraStep %= _titleScreenPosArray.Length;
                _titleAnimTime = 0f;
            }
            Vector3 posA = _titleScreenPosArray[_titleCameraStep];
            Vector3 posB = _titleScreenPosArray[(_titleCameraStep + 1) % _titleScreenPosArray.Length];
            _camera.transform.localPosition = Vector3.Lerp(posA, posB, _titleAnimTime);
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
            List<GameObject> activeBalls = GM.inst.ballDispenser.activeBalls;
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
    
    private void RefreshCameraValues()
    {
        int index = Screen.height > Screen.width ? 0 : 1;
        _titleScreenPosArray = new Vector3[3]
        {
            _multiballPositions[index], _camBottomPositions[index], _camTopPositions[index]
        };
        _camTopPos = _camTopPositions[index];
        _camBottomPos = _camBottomPositions[index];
        _multiballPos = _multiballPositions[index];
        
        _camera.transform.localRotation = _camRotations[index];
        _camera.fieldOfView = _camFieldOfViews[index];
        
        _titleScreenPosArray = _titleScreenPositionArrays[index];
    }
    
    private void OnDrawGizmos()
    {
        // Visualizes the camera box in the editor
        Gizmos.color = Color.white;
        Gizmos.matrix = Matrix4x4.TRS(new Vector3(0f, 0f, _boxPos), Quaternion.identity, new Vector3(10f, .25f, _boxRadius * 2f));
        Gizmos.DrawWireCube(Vector3.zero, Vector3.one);
    }

    
}
