using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[ExecuteInEditMode]
public class ObjectLink : MonoBehaviour
{
    // A class to link a 3D object with its underlying 2D object, and to generate colliders for the latter if needed.
    // All physics and collision happens in 2D, with the 3D models converting 2D X and Y positions into 3D X and Z.
    
    // Polygon Colliders are generated from a text file exported from Blender with Polygon2DCreator.py
    
    public GameObject obj3D;
    public GameObject obj2D;
    public bool generateColliders;                  // If checked, polygons will be generated from the following text file.
                                                    // Checking and unchecking can also be used to update colliders.
    public TextAsset textFile;                      // Text file from Blender
    public Vector3 sharedRotation;                  // Used to rotate the two objects in sync.
    public bool invert3DRotation;
    public bool invert2DRotation;

    public float baseRotation2D;
    public float angle2D;

    private void Start()
    {
        SetBaseAngles();
    }

    private void Update()
    {
        if (obj2D && obj3D)
        {
            Vector3 euler2D = obj2D.transform.rotation.eulerAngles;
            if (invert3DRotation)
            {
                obj3D.transform.localRotation = Quaternion.Euler(euler2D.x, -euler2D.z, euler2D.y);
            }
            else
            {
                obj3D.transform.localRotation = Quaternion.Euler(euler2D.x, euler2D.z, euler2D.y);
            }
        }

    }

    void SetBaseAngles()
    {
        if (obj3D && obj2D)
            baseRotation2D = obj2D.transform.localRotation.eulerAngles.z;
    }

    public void RotateToAngle(float angle)
    {
        if (!obj3D || !obj2D)
            return;
        angle2D = angle;
        float angle3D = angle;
        if (invert3DRotation)
        {
            angle3D = -angle;
        }
        if (invert2DRotation)
        {
            angle2D = -angle;
        }
        //transform.localRotation = Quaternion.AngleAxis(angle3D, transform.up) * baseRotation3D;
        Rigidbody2D rb2D = obj2D.GetComponent<Rigidbody2D>();
        if (rb2D)
        {
            rb2D.MoveRotation(baseRotation2D + angle2D);
        }
    }

    void OnValidate()
    {        
        if (!obj2D && obj3D && generateColliders)
        {
            obj2D = new GameObject(gameObject.name + "_2D");
            obj2D.transform.SetParent(transform);
        }
        
        if (obj2D && obj3D && generateColliders)
        {
            if (textFile)
            {
                BuildColliders();
            }
        }

        if (!obj3D || !obj2D)
            return;
        obj3D.transform.localRotation = Quaternion.Euler(sharedRotation);
        obj2D.transform.localRotation = Quaternion.Euler(sharedRotation.x, sharedRotation.z, -sharedRotation.y);
        GM gm = FindAnyObjectByType<GM>();
        if (!gm)
            return;
        obj2D.transform.position =
            new Vector3(obj3D.transform.position.x, obj3D.transform.position.z, obj3D.transform.position.y) +
            gm.offset2D;
    }

#if (UNITY_EDITOR) 
    public void TriggerUpdate(string reimportedAsset)
    {
        if (textFile && reimportedAsset == AssetDatabase.GetAssetPath(textFile))
        {
            OnValidate();
        }
    }
#endif

    private void BuildColliders()
    {
        if (!textFile)
        {
            return;
        }
        List<List<Vector2>> pointLists = new();
        string[] textValues = textFile.text.Split(',', '\n');

        List<Vector2> pList = new();

        bool createNew = true;

        for (int i = 0; i < textValues.Length; i++)
        {
            if (createNew)
            {                
                pList = new();
                createNew = false;                
            }
            if (textValues[i] == "end")
            {
                pointLists.Add(pList);
                createNew = true;
                continue;
            }

            Vector2 newV2 = new(float.Parse(textValues[i]), float.Parse(textValues[i + 1]));
            pList.Add(newV2);
            i += 2;
        }        
        PolygonCollider2D[] polyCols = obj2D.GetComponents<PolygonCollider2D>();
        List<PolygonCollider2D> polyColsList = new();
        foreach (PolygonCollider2D col in polyCols)
        {
            polyColsList.Add(col);
        }

        int count = 0;
        while (pointLists.Count < polyColsList.Count)
        {
            count++;
            if (count > 100)
            {
                Debug.Log("Sanity counter tripped when deleting excess colliders");
                break;
            }
            PolygonCollider2D toDestroy = polyColsList[0];
            polyColsList.RemoveAt(0);
            DestroyImmediate(toDestroy);
        }

        count = 0;
        while (polyColsList.Count < pointLists.Count)
        {
            count++;
            if (count == 100)
            {
                Debug.Log("Sanity counter tripped");
                return;
            }
            PolygonCollider2D newPolyCol = obj2D.AddComponent<PolygonCollider2D>();
            polyColsList.Add(newPolyCol);
        }

        if (count < polyColsList.Count)
        {
            
        }

        for (int i = 0; i < polyColsList.Count; i++)
        {
            polyColsList[i].points = pointLists[i].ToArray();
        }
    }
}

#if (UNITY_EDITOR) 
public class PostProc : AssetPostprocessor
{

    static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths, bool didDomainReload)
    {
        foreach (string str in importedAssets)
        {
            //Debug.Log("Reimported Asset: " + str);
            ObjectLink[] objs = Object.FindObjectsByType<ObjectLink>(FindObjectsSortMode.None);
            foreach (ObjectLink obj in objs)
            {                
                obj.TriggerUpdate(str);
            }
        }

    }
}

#endif

