using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MaterialSwapper : MonoBehaviour, INeedReset
{
    public Material[] materials;
    private Renderer _rend;
    private int _currentIndex = 0;

    void Start()
    {
        _rend = GetComponent<Renderer>();
        ResetForNewGame();
    }

    public void ResetForNewGame()
    {
        SetMaterial(0);
    }

    public void SetMaterial(int index)
    {
        if (_currentIndex == index)
            return;
        if (index < 0 || index >= materials.Length)
        {
            Debug.LogWarning("Invalid material index", gameObject);
            return;
        }
        if (!materials[index])
        {
            Debug.LogWarning("Invalid material", gameObject);
            return;
        }
        _rend.material = materials[index];
        _currentIndex = index;
    }
}
