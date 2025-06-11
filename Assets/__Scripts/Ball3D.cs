using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ball3D : MonoBehaviour
{
    public float radius = .1f;
    public GameObject ball2D;
    Rigidbody2D _rb;
    
    private void Start()
    {
        _rb = ball2D.GetComponent<Rigidbody2D>();
    }

    private void FixedUpdate()
    {
        Physics.Raycast(transform.position + new Vector3(0f, 5f, 0f), Vector3.down, out RaycastHit hit, 10f, _rb.includeLayers);
        Vector3 pos = transform.position;
        pos.y = hit.point.y + radius;
        transform.position = pos;
    }
}
