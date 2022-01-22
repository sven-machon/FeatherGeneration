using System.Collections;
using System.Collections.Generic;
using UnityEngine;


struct Feather
{
    GameObject feather;
    Vector3 normal;
    Vector3 orientation;
}

struct LinkedFeather
{
    GameObject feather;
    int nextFeatherIdx;

}


public class FeatherGenerator : MonoBehaviour
{
    //Control class that handles all the generation

    #region Variables
    [SerializeField] private GameObject _feather=null;
    [SerializeField] private List<GameObject> _guides = null;
    [SerializeField] private List<LinkedGuide> _links = null;
    [SerializeField] private MeshData _data = null;

    [SerializeField] private Transform _parentTransform=null;

    private List<Vector3> _vertices = null;
    private List<Vector3> _normals = null;
    private List<BoneWeight> _weights = null;
    List<Transform> _bones = null;


    private List<GameObject> _FirstCoat = null;
    private List<int> _rootIndices = null;

    private List<Vector3> _orientationField = null;
    private List<int> _growOrder = null;

    private int _closeFeatherAmount = 5;

    [SerializeField] private bool _renderOrientation = true;
    [SerializeField] private int _verticesPerFeather = 3;
    [SerializeField] private int _neighboursToCheck = 10;
    [SerializeField] private float _gamma = 0.6f;

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

        //Get data from mesh
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

        _rootIndices = new List<int>();
        _FirstCoat = GenerateFirstCoat();

        _orientationField= RefineOrientationField();

        _growOrder = GrowingOrder();



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

        for (int i = 0; i < _vertices.Count; i+=_verticesPerFeather)
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
            _rootIndices.Add(i);
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
                        totalScale += links[i].Guide.transform.localScale * weights[i] ;
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
                        totalScale += links[i].Guide.transform.localScale * weights[i];
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

       // feather.transform.parent = _parentTransform;
        feather.name = idx.ToString();
        feather.transform.localScale = totalScale;

        if (_renderOrientation)
            feather.SetActive(false);



                return feather;

        
    }

    private List<Vector3> RefineOrientationField()
    {
        List<Vector3> refinedOrientations = new List<Vector3>();
        //TO DO FIX ORIENTATION REFINEMENT
        {
          //vertex based
     // {
     //   for(int i = 0; i < _vertices.Count; i++)
     //   { 
     //       {
     //          if (_renderOrientation)
     //              Debug.DrawLine(_vertices[i], _vertices[i] + _normals[i] * 0.003f, Color.red, 100, true);
     // 
     //          List<int> neighbours = GetClosestVertices(_vertices[i], _closeFeatherAmount);
     // 
     //          Vector3 orientation = new Vector3();
     // 
     //          for (int j = 0; j < neighbours.Count; j++)
     //          {
     //              /*if(GameObject.Find(neighbours[j].ToString())==null)
     //                      continue; */
     // 
     //              float w = Mathf.Max(0, Vector3.Dot(_normals[i],GameObject.Find(neighbours[j].ToString()).transform.forward));
     //              Vector3 c = Vector3.Cross(_normals[i], _vertices[neighbours[j]]);
     //              c = Vector3.Cross(c, _normals[i]);
     //              orientation += w * c;
     //          }
     // 
     //           orientation.Normalize();
     //           refinedOrientations.Add(orientation);
     //            
     //           if (_renderOrientation)
     //              Debug.DrawLine(_vertices[i], _vertices[i] + orientation * 0.005f, Color.yellow, 100, true);
     //      }
     //   }
      }
          
            ////feather based, not vertex based
         {
        //  { 
        //  foreach (GameObject feather in _FirstCoat)
        //  {
        //
        //
        //      //when the feathers get generated their name gets assigned to their index, so we can get the index back from the name
        //      int featherIdx = int.Parse(feather.name);
        //      if (_renderOrientation)
        //      {
        //          Debug.DrawLine(_vertices[featherIdx], _vertices[featherIdx] + feather.transform.forward.normalized * 0.003f, Color.red, 100, true);
        //      }
        //      Debug.Log(feather.transform.rotation.eulerAngles);
        //
        //      List<GameObject> neighbours = GetClosestFeathers(feather, _closeFeatherAmount);
        //
        //      List<int> neighbourIndexes = new List<int>();
        //      foreach (GameObject neighbour in neighbours)
        //      {
        //          neighbourIndexes.Add(int.Parse(neighbour.name));
        //      }
        //
        //      Vector3 orientation = new Vector3();
        //
        //      for (int i = 0; i < neighbours.Count; i++)
        //      {
        //          float w = Mathf.Max(0, Vector3.Dot(_normals[featherIdx], _normals[neighbourIndexes[i]]));
        //          Vector3 c = Vector3.Cross(_normals[featherIdx], _FirstCoat.Find(x => x.name == neighbours[i].name).transform.forward.normalized);
        //          c = Vector3.Cross(_normals[featherIdx], c);
        //          orientation += w * c;
        //      }
        //
        //      orientation.Normalize();
        //      }
        //   
        //      refinedOrientations.Add(orientation);
        // 
        //      if (_renderOrientation)
        //          Debug.DrawLine(feather.transform.position,feather.transform.position + orientation*0.0025f,Color.yellow,100,true);
        // 
        //  }
         }

        foreach (GameObject feather in _FirstCoat)
            refinedOrientations.Add(feather.transform.forward.normalized);

        return refinedOrientations;
    }

    private List<GameObject> GetClosestFeathers(GameObject feather, int amount)
    {
        List<float> distances = new List<float>();
        List<GameObject> closestFeathers = new List<GameObject>();
        Vector3 pos= feather.transform.position;

        for(int i = 0; i < amount; i++)
        {
            distances.Add(Mathf.Infinity);
            closestFeathers.Add( null);
        }

        foreach(GameObject otherFeather in  _FirstCoat)
        {
            if (otherFeather == feather)
                continue;
            Vector3 diff = otherFeather.transform.position - pos;

            float currDist = diff.sqrMagnitude;

            float highestDist = 0;
            int currIdx = 0;

            for(int i=0; i < amount; i++)
            {
                if(distances[i]>highestDist)
                {
                    highestDist = distances[i];
                    currIdx = i;
                }
            }

            if(currDist<highestDist)
            {
                distances[currIdx] = currDist;
                closestFeathers[currIdx] = otherFeather;
            }
        }

        return closestFeathers;
    }

    private List<int> GetClosestVertices(Vector3 vertex,int amount)
    {
        List<float> distances = new List<float>();
        List<int> closestVertices = new List<int>();
        

        for (int i = 0; i < amount; i++)
        {
            distances.Add(Mathf.Infinity);
            closestVertices.Add(0);
        }

        for(int i=0;i<_vertices.Count;i++)
        {
            if (_vertices[i] == vertex)
                continue;
            Vector3 diff = _vertices[i] - vertex;

            float currDist = diff.sqrMagnitude;

            float highestDist = 0;
            int currIdx = 0;

            for (int j = 0; j < amount; j++)
            {
                if (distances[j] > highestDist)
                {
                    highestDist = distances[j];
                    currIdx = j;
                }
            }

            if (currDist < highestDist)
            {
                distances[currIdx] = currDist;
                closestVertices[currIdx] =i;
            }
        }

      //  foreach (Vector3 otherVertex in _vertices)
      //  {
      //      if (otherVertex == vertex)
      //          continue;
      //      Vector3 diff = otherVertex-vertex;
      //
      //      float currDist = diff.sqrMagnitude;
      //
      //      float highestDist = 0;
      //      int currIdx = 0;
      //
      //      for (int i = 0; i < amount; i++)
      //      {
      //          if (distances[i] > highestDist)
      //          {
      //              highestDist = distances[i];
      //              currIdx = i;
      //          }
      //      }
      //
      //      if (currDist < highestDist)
      //      {
      //          distances[currIdx] = currDist;
      //          closestVertices[currIdx] = otherVertex;
      //      }
      //  }

        return closestVertices;
    }

    private List<int> GrowingOrder()
    {
        for(int i=0;i<_FirstCoat.Count;i++)
        {
            List<GameObject> neighbours = GetClosestFeathers(_FirstCoat[i], _neighboursToCheck);

            for(int j = 0;j<neighbours.Count;j++)
            {
                if(!GetClosestFeathers(neighbours[j],_neighboursToCheck).Contains(_FirstCoat[i]))
                {
                    Debug.Log("they are not shared neighbours.");
                    continue;
                }
                Debug.Log("they are shared neighbours.");
                float diff = PriorityDifference(i,_FirstCoat[i].transform, neighbours[j].transform);

            }

        }

        return null;
    }


    float PriorityDifference(int Aidx,Transform A, Transform B)
    {
        float P = Vector3.Dot(_orientationField[Aidx], B.position - A.position) + _gamma * Vector3.Dot(_normals[int.Parse(A.name)], A.position - B.position);
        P++;
        return P;
    }

}


