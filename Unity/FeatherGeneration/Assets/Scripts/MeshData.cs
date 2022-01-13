using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeshData : MonoBehaviour
{
    [SerializeField] private SkinnedMeshRenderer _skin=null;
    public SkinnedMeshRenderer Skin { get { return _skin; } }
    [SerializeField]private Mesh _mesh;
    public Mesh Mesh { get { return _mesh; } }

   [SerializeField]private List<Vector3> _vertices = null;

   public List<Vector3> Vertices { 
        get
        {
            if (_mesh == null)
            {
                _mesh = new Mesh();
                _skin.BakeMesh(_mesh);
            }

            if (_vertices.Count == 0)
            {

                _vertices = new List<Vector3>(_mesh.vertices);

               
                
                Matrix4x4 localToWorld = _skin.transform.localToWorldMatrix;

                for (int i=0;i<_vertices.Count;i++)
                {
                    Debug.Log("convert");
                 _vertices[i]= localToWorld.MultiplyPoint3x4(_vertices[i]);
                   _vertices[i].Scale(_skin.transform.lossyScale);
                }
            }

            return _vertices;
        } 
    }

   [SerializeField]private List<Vector3> _normals = null;
    public List<Vector3> Normals { 
        get
        {
            if (_normals.Count==0)
                _normals = new List<Vector3>(_mesh.normals);

            return _normals;
        } 
    }

   [SerializeField]private List<BoneWeight> _weights = null;
    public List<BoneWeight> Weights
    {
        get
        {
            if (_weights.Count==0)
                _weights = new List<BoneWeight>(_skin.sharedMesh.boneWeights);

            return _weights;
        }

    }
   
    [SerializeField]private List<Transform> _bones=null;
    public List<Transform> Bones
    {
        get
        {
            if (_bones.Count==0)
                _bones = new List<Transform>(_skin.bones);

            return _bones;
        }
    }



    void Start()
    {
        if (_skin == null)
            Debug.LogError("No Skinned mesh assigned to MeshData.");

       
        _skin.BakeMesh(_mesh);
        if (_mesh == null)
            Debug.LogError("No mesh found");


    }
}
