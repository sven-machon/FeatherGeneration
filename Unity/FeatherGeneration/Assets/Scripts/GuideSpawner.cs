using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GuideSpawner : MonoBehaviour
{
    // Start is called before the first frame update
    [SerializeField] private List<Transform> _bones = null;
    [SerializeField] private GameObject _feather = null;

    void Start()
    {
        foreach(Transform bone in _bones)
        {
            Instantiate(_feather, bone.transform.position,Quaternion.identity);
        }
    }
}
