using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LinkedGuide : MonoBehaviour
{
    private GameObject _guide = null;
    private GameObject _joint = null;
    

    public GameObject Guide
    {
        get { return _guide; }
        set { _guide = value; }
    }

    public GameObject Joint
    {
        get { return _joint; }
        set { _joint = value; }
    }
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
