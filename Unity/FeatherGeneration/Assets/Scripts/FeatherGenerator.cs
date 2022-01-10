using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class FeatherGenerator : MonoBehaviour
{

    //Control class that handles all the generation

    [SerializeField] MeshData _data = null;
    [SerializeField] GameObject _mesh = null;
    [SerializeField] private List<GameObject> _guides=null;


    // Start is called before the first frame update
    void Start()
    {
        if (_data == null)
            Debug.LogError("No MeshData assigned to generator.");

        if (_mesh == null)
            Debug.LogError("No Mesh assigned to generator.");

        

    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
