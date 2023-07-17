using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class AttachMechColliderAllChildren : MonoBehaviour
{
    // Start is called before the first frame update
    void AttachMeshCollider(GameObject gob)
    {
        Transform[] children = gob.GetComponentsInChildren<Transform>();
        if (children.Length > 1) // 自分+子のため
        {
            foreach (Transform ob in children)
            {
                if (ob.gameObject == gob.gameObject) continue;
                AttachMeshCollider(ob.gameObject);
            }
        }
        else
        {
            if (!gob.gameObject.GetComponent<MeshCollider>())
            {
                gob.gameObject.AddComponent<MeshCollider>();
            }
        }
    }
    void Start()
    {
        AttachMeshCollider(this.gameObject);
    }

    // Update is called once per frame
    void Update()
    {

    }
}
