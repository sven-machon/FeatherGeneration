using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeshData : MonoBehaviour
{
    [SerializeField] private SkinnedMeshRenderer _mesh=null;

    void Start()
    {
        if (_mesh == null)
            Debug.LogError("No Skinned mesh assigned to MeshData.");
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
