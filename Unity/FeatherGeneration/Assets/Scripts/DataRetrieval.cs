using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DataRetrieval : MonoBehaviour
{
    // Start is called before the first frame update
    [SerializeField] private SkinnedMeshRenderer _skin;
    [SerializeField] private Mesh _mesh;
    [SerializeField] private List<Vector3> _verteces = null;
    [SerializeField] private List<Vector3> _normals = null;
    [SerializeField] private List<BoneWeight> _weights = null;

    void Start()
    {
        _mesh = _skin.sharedMesh;
        _verteces =new List<Vector3>(_mesh.vertices);
        _normals = new List<Vector3>(_mesh.normals);
        _weights = new List<BoneWeight>(_mesh.boneWeights);
        
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
