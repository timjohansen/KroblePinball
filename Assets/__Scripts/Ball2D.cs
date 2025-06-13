using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Ball2D : MonoBehaviour
{
    private Ball _parentBall;


    private void Awake()
    {
        _parentBall = GetComponentInParent<Ball>();
    }
    
    /*private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Flipper"))
        {
            _parentBall.touchingFlipper = true;
        }
    }
    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Flipper"))
        {
            _parentBall.touchingFlipper = false;
        }
    }*/
}
