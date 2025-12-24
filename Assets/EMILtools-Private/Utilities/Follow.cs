using System.Collections;
using System.Collections.Generic;
using KBCore.Refs;
using UnityEngine;

public class Follow : ValidatedMonoBehaviour
{
    [SerializeField] Transform follow;
    [SerializeField] bool releaseFromParentOnSpawn = true;

    protected virtual void Start()
    {
        if (releaseFromParentOnSpawn)
        {
            name = name + $" [ Following {transform.parent.name} ]";
            transform.parent = null;
        }
    }

    protected virtual void FixedUpdate()
    {
        if(follow) transform.position = follow.position;
    }
}