using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LinkedGuide : System.Object
{
    private GameObject _guide = null;
    private GameObject _joint = null;
    private int _jointIdx = 0;


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

    public int JointIdx { 
        get { return _jointIdx; } 
        set { _jointIdx = value; }
    
    }

}
