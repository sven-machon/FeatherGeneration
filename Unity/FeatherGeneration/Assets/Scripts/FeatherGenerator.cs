using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class FeatherGenerator : MonoBehaviour
{
    //Control class that handles all the generation

    #region Variables
    [SerializeField] private GameObject _feather;
    [SerializeField] private List<GameObject> _guides=null;
    [SerializeField] private List<LinkedGuide> _links = null;
    [SerializeField] private MeshData _data = null;

    [SerializeField] private Transform _parentTransform;

    private List<GameObject> _FirstCoat = null;
    
#endregion

    void Start()
    {
        //Check if all required data is present
        if (_data == null)
            Debug.LogError("No MeshData assigned to generator.");

        if(_guides==null||_guides.Count==0)
        {
            _guides =new List<GameObject>( GameObject.FindGameObjectsWithTag("guide"));

            if (_guides == null || _guides.Count == 0)
                Debug.LogError("No associated guides found.");
        }

        if (!TryGetComponent<MeshData>(out _data))
            Debug.LogError("No Meshdata found");

        //Link every guide automatically to its nearest available joint (one guide per joint)
        _links = new List<LinkedGuide>();
        foreach(GameObject guide in _guides)
        {
            _links.Add(FindClosestBone(guide));
        }


       _FirstCoat= GenerateFirstCoat();

        //debugging print
       foreach(LinkedGuide link in _links)
        {
            Debug.Log("Guide: " + link.Guide.name + ", Joint: " + link.Joint.name);
        }


    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private LinkedGuide FindClosestBone(GameObject guide) {
        LinkedGuide link= new LinkedGuide();

        link.Guide = guide;

        GameObject closestBone = null;
        List<GameObject> joints = new List<GameObject>(GameObject.FindGameObjectsWithTag("bone"));

        float distance = Mathf.Infinity;
        Vector3 pos =guide.transform.position;

        foreach (GameObject joint in joints)
        {
            if (_links.FindIndex(i => i.Joint == joint) != -1)
                continue;

            Vector3 diff = joint.transform.position - pos;
            float currentDist = diff.sqrMagnitude;
            if (currentDist < distance)
            {
                closestBone = joint;
                distance = currentDist;
                link.Joint = joint;
            }
        }

        return link;
    }

    private List<GameObject> GenerateFirstCoat()
    {
        List<GameObject> feathers = new List<GameObject>();

        List<Vector3> vertices = _data.Vertices;
        List<Vector3> normals = _data.Normals;
        List<BoneWeight> weights = _data.Weights;
        List<Transform> bones = _data.Bones;


        for(int i = 0;  i < vertices.Count;i++)
        {
            List<LinkedGuide> associatedGuides = new List<LinkedGuide>();

            #region Find Associated guides
            int linkIndex = _links.FindIndex(j => j.Joint == bones[weights[i].boneIndex0].gameObject);
            if (linkIndex!= -1)
            {              
                associatedGuides.Add(_links[linkIndex]);
            }

            linkIndex = _links.FindIndex(j => j.Joint == bones[weights[i].boneIndex1].gameObject);
            if (linkIndex != -1)
            {
                associatedGuides.Add(_links[linkIndex]);
            }

            linkIndex = _links.FindIndex(j => j.Joint == bones[weights[i].boneIndex2].gameObject);
            if (linkIndex != -1)
            {
                associatedGuides.Add(_links[linkIndex]);
            }

            linkIndex = _links.FindIndex(j => j.Joint == bones[weights[i].boneIndex3].gameObject);
            if (linkIndex != -1)
            {
                associatedGuides.Add(_links[linkIndex]);
            }

            if(associatedGuides.Count==0)
            {
                Debug.Log("no feathers present");
                continue;
            }
            #endregion


            GameObject feather = Instantiate(_feather, vertices[i], Quaternion.identity); ;
            feather.transform.parent = _parentTransform;
            Debug.Log("generate");
        }

        return feathers;
    }

}
