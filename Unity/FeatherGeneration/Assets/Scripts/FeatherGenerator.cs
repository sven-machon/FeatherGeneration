using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class FeatherGenerator : MonoBehaviour
{
    //Control class that handles all the generation

    #region Variables
    [SerializeField] private GameObject _feather;
    [SerializeField] private List<GameObject> _guides = null;
    [SerializeField] private List<LinkedGuide> _links = null;
    [SerializeField] private MeshData _data = null;

    [SerializeField] private Transform _parentTransform;

    private List<Vector3> _vertices = null;
    private List<Vector3> _normals = null;
    private List<BoneWeight> _weights = null;
    List<Transform> _bones = null;


    private List<GameObject> _FirstCoat = null;

    #endregion

    void Start()
    {
        //Check if all required data is present
        if (_data == null)
            Debug.LogError("No MeshData assigned to generator.");

        if (_guides == null || _guides.Count == 0)
        {
            _guides = new List<GameObject>(GameObject.FindGameObjectsWithTag("guide"));

            if (_guides == null || _guides.Count == 0)
                Debug.LogError("No associated guides found.");
        }

        if (!TryGetComponent<MeshData>(out _data))
            Debug.LogError("No Meshdata found");


        _vertices = _data.Vertices;
        _bones = _data.Bones;
        _normals = _data.Normals;
        _weights = _data.Weights;


        //Link every guide automatically to its nearest available joint (one guide per joint)
        _links = new List<LinkedGuide>();
        foreach (GameObject guide in _guides)
        {
            _links.Add(FindClosestBone(guide));
        }


        _FirstCoat = GenerateFirstCoat();

        //debugging print
        foreach (LinkedGuide link in _links)
        {
            Debug.Log("Guide: " + link.Guide.name + ", Joint: " + link.JointIdx);
        }


    }


    private LinkedGuide FindClosestBone(GameObject guide)
    {
        LinkedGuide link = new LinkedGuide();

        link.Guide = guide;

        GameObject closestBone = null;
        List<Transform> bones = _data.Bones;

        float distance = Mathf.Infinity;
        Vector3 pos = guide.transform.position;

        //iterate over every bone
        for (int i = 0; i < bones.Count; i++)
        {
            //skip bones that already have a guide assigned
            if (_links.FindIndex(j => j.Joint == bones[i]) != -1)
                continue;



            Vector3 diff = bones[i].transform.position - pos;
            float currentDist = diff.sqrMagnitude;
            if (currentDist < distance)
            {
                closestBone = bones[i].gameObject;
                distance = currentDist;
                link.Joint = bones[i].gameObject;
                link.JointIdx = i;
            }
        }

        return link;
    }

    private List<GameObject> GenerateFirstCoat()
    {
        List<GameObject> feathers = new List<GameObject>();

        for (int i = 0; i < _vertices.Count; i++)
        {
            List<LinkedGuide> associatedGuides = new List<LinkedGuide>();
            //skip vertex if no associated guides
            #region Find Associated guides
            //go over all associated joints of the vertex and see if they have a guide linked to them
            int linkIndex = _links.FindIndex(j => j.Joint == _bones[_weights[i].boneIndex0].gameObject);
            if (linkIndex != -1)
            {
                associatedGuides.Add(_links[linkIndex]);
            }

            linkIndex = _links.FindIndex(j => j.Joint == _bones[_weights[i].boneIndex1].gameObject);
            if (linkIndex != -1)
            {
                associatedGuides.Add(_links[linkIndex]);
            }

            linkIndex = _links.FindIndex(j => j.Joint == _bones[_weights[i].boneIndex2].gameObject);
            if (linkIndex != -1)
            {
                associatedGuides.Add(_links[linkIndex]);
            }

            linkIndex = _links.FindIndex(j => j.Joint == _bones[_weights[i].boneIndex3].gameObject);
            if (linkIndex != -1)
            {
                associatedGuides.Add(_links[linkIndex]);
            }

            if (associatedGuides.Count == 0)
            {
                Debug.Log("no feathers present");
                continue;
            }
            #endregion





            feathers.Add(GenerateFeather(_vertices[i], associatedGuides, _normals[i], _weights[i], i));

        }

        return feathers;
    }

    private GameObject GenerateFeather(Vector3 vertex, List<LinkedGuide> links, Vector3 normal, BoneWeight weight, int idx)
    {

        Debug.Log(links[0].JointIdx);
        Vector3 totalRotate = new Vector3();
        Vector3 totalScale = new Vector3(/*1,1,1*/);
        Quaternion rotation = Quaternion.identity;

        switch (links.Count)
        {
            case 1:
                {

                    totalRotate = links[0].Guide.transform.rotation.eulerAngles;

                    if (links[0].JointIdx == weight.boneIndex0)
                    {
                        totalScale = links[0].Guide.transform.lossyScale * weight.weight0;
                    }
                    else if (links[0].JointIdx == weight.boneIndex1)
                    {
                        totalScale = links[0].Guide.transform.lossyScale * weight.weight1;
                    }
                    else if (links[0].JointIdx == weight.boneIndex2)
                    {
                        totalScale = links[0].Guide.transform.lossyScale * weight.weight2;
                    }
                    else if (links[0].JointIdx == weight.boneIndex3)
                    {
                        totalScale = links[0].Guide.transform.lossyScale * weight.weight3;
                    }

                    break;
                }


            case 2:
                {
                    float[] weights = new float[2] { 0, 0 };
                    for (int i = 0; i < 2; i++)
                    {
                        if (links[i].JointIdx == weight.boneIndex0)
                            weights[i] = weight.weight0;
                        else if (links[i].JointIdx == weight.boneIndex1)
                            weights[i] = weight.weight1;
                        else if (links[i].JointIdx == weight.boneIndex2)
                            weights[i] = weight.weight2;
                        else if (links[i].JointIdx == weight.boneIndex3)
                            weights[i] = weight.weight3;
                    }

                    float calculatedWeight = weights[0] + weights[1];
                    for (int i = 0; i < 2; i++)
                    {
                        totalRotate += links[i].Guide.transform.eulerAngles * weights[i] / calculatedWeight;
                        totalScale += links[i].Guide.transform.localScale * weights[i] / calculatedWeight;
                    }

                    break;
                          
                }

            case 3:
                {
                    float[] weights = new float[3] { 0, 0, 0 };
                    for (int i = 0; i < 3; i++)
                    {
                        if (links[i].JointIdx == weight.boneIndex0)
                            weights[i] = weight.weight0;
                        else if (links[i].JointIdx == weight.boneIndex1)
                            weights[i] = weight.weight1;
                        else if (links[i].JointIdx == weight.boneIndex2)
                            weights[i] = weight.weight2;
                        else if (links[i].JointIdx == weight.boneIndex3)
                            weights[i] = weight.weight3;
                    }

                    float calculatedWeight = weights[0] + weights[1] + weights[2];
                    for (int i = 0; i < 3; i++)
                    {
                        totalRotate += links[i].Guide.transform.eulerAngles * weights[i]/calculatedWeight;
                        totalScale += links[i].Guide.transform.localScale * weights[i]/calculatedWeight;
                    }


                    break;
                }

            case 4:
                {
                    float[] weights =new float[4]{ 0, 0, 0, 0 };
                    for(int i=0;i<4;i++)
                    {
                        if (links[i].JointIdx == weight.boneIndex0)
                            weights[i] = weight.weight0;
                        else if (links[i].JointIdx == weight.boneIndex1)
                            weights[i] = weight.weight1;
                        else if (links[i].JointIdx == weight.boneIndex2)
                            weights[i] = weight.weight2;
                        else if (links[i].JointIdx == weight.boneIndex3)
                            weights[i] = weight.weight3;
                    }


                    for(int i = 0; i < 4; i++)
                    {
                        totalRotate += links[i].Guide.transform.eulerAngles * weights[i];
                        totalScale += links[i].Guide.transform.localScale * weights[i];
                    }

                    break;
                }

        }

               




                GameObject feather = Instantiate(_feather, vertex, Quaternion.Euler(totalRotate));
               // feather.transform.Rotate(totalRotate);

                feather.transform.parent = _parentTransform;
                feather.name = idx.ToString();
                feather.transform.localScale = totalScale;
                // feather.transform.rotation=(Quaternion.Euler( totalRotate));


                return feather;

        
    }
}


