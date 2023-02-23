using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class AttachNavMeshObstacle : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        Transform children = this.gameObject.GetComponentInChildren<Transform>();
        foreach (Transform ob in children)
        {
            if (!ob.gameObject.GetComponent<NavMeshObstacle>())
            {

                NavMeshObstacle mesh = ob.gameObject.AddComponent<NavMeshObstacle>();

                mesh.carving = true;

                if (ob.gameObject.GetComponent<BoxCollider>())
                {
                    mesh.shape = NavMeshObstacleShape.Box;
                }
                else if (ob.gameObject.GetComponent<CapsuleCollider>())
                {
                    mesh.shape = NavMeshObstacleShape.Capsule;
                }
                else
                {
                    Debug.Log("Not implement shape. Pass!");
                }

            }

        }
    }

    // Update is called once per frame
    void Update()
    {

    }
}
