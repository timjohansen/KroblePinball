using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Splines;
using Unity.Mathematics;

public class BlenderCurveToSpline : MonoBehaviour
{
    public TextAsset sourceTextFile;
    public bool clickToUpdate;
    private void OnValidate()
    {
        if (!sourceTextFile)
        {
            return;
        }

        string[] textValues = sourceTextFile.text.Split('\n');
        int totalKnotCount = int.Parse(textValues[0]);
        int knotCount = 0;
        int i = 1;

        List<BezierKnot> knotList = new();

        while (knotCount < totalKnotCount)
        {
            float3 position = new(float.Parse(textValues[i]), float.Parse(textValues[i + 1]), float.Parse(textValues[i + 2]));
            float3 rightHandle = new(float.Parse(textValues[i + 3]), float.Parse(textValues[i + 4]), float.Parse(textValues[i + 5]));
            float3 leftHandle = new(float.Parse(textValues[i + 6]), float.Parse(textValues[i + 7]), float.Parse(textValues[i + 8]));

            BezierKnot newKnot = new BezierKnot(position, -rightHandle, -leftHandle);
            knotList.Add(newKnot);
            knotCount += 1;
            i += 9;
        }

        SplineContainer sp = GetComponent<SplineContainer>();
        if (sp == null)
        {
            sp = gameObject.AddComponent<SplineContainer>();
        }

        Spline newSpline = new Spline();
        foreach (var knot in knotList)
        {
            newSpline.Add(knot);
        }

        sp.Spline = newSpline;
        gameObject.name = sourceTextFile.name;

    }
}
