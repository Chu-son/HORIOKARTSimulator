using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]

public class AgentSpawner : MonoBehaviour
{
    [SerializeField] GameObject agent;
    [SerializeField] Transform central;

    [SerializeField] float x_max = 30;
    [SerializeField] float z_max = 30;
    [SerializeField] float y_ = 0;

    [SerializeField] int num = 10;

    private Vector3 pos;

    Vector3 getRandomPoint()
    {
        float posX = Random.Range(-1 * x_max, x_max);
        float posZ = Random.Range(-1 * z_max, z_max);

        pos = central.position;
        pos.x += posX;
        pos.z += posZ;

        return new Vector3(pos.x, y_, pos.z);
    }

    void Spawn()
    {

        Vector3 target = getRandomPoint();

        NavMeshHit hit;
        if (NavMesh.SamplePosition(target, out hit, 1.0f,NavMesh.AllAreas))
        {
            // 位置をNavMesh内に補正
            target = hit.position;
        }

        Instantiate(agent, target, Quaternion.identity);

    }

    void Start()
    {
        for(int i=0; i < num; i++)
        {
            Spawn();
        }
    }
}