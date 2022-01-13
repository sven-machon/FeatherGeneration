using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FeatherLink : MonoBehaviour
{
    // Start is called before the first frame update
    // [SerializeField] private int _closestBone=0;
    [SerializeField] private GameObject _closestBone = null;

    [SerializeField] BoneWeight _weights;
    void Start()
    {
       List<GameObject> bones= new List<GameObject>(GameObject.FindGameObjectsWithTag("bone"));
       // GameObject closest = null;
        float distance = Mathf.Infinity;
        Vector3 pos = transform.position;

        foreach(GameObject bone in bones)
        {
            Vector3 diff = bone.transform.position - pos;
            float currentDist = diff.sqrMagnitude;
            if(currentDist<distance)
            {
                _closestBone = bone;
                distance = currentDist;
            }
        }


    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
