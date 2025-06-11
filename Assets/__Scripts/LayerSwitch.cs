using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LayerSwitch : MonoBehaviour
{
    public string layerName;
    
    private void OnTriggerExit2D(Collider2D collision)
    {
        GameObject obj = collision.gameObject;
        if (obj.CompareTag("Ball"))
        {
            obj.GetComponentInParent<Ball>().ChangeLayer(layerName);
            Debug.Log(obj.transform.position + " " + layerName);
        }
    }
}
