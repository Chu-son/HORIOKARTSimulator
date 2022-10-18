using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]

public class AgentMove : MonoBehaviour
{
    public Transform central;
    private NavMeshAgent agent;

    [SerializeField] float x_max = 30;
    [SerializeField] float z_max = 30;

    [SerializeField] float stop_time_max = 5;

    Animator anim;
    Vector3 pos;

    private float time_counter = 0;
    private float stop_time = 0;
    
    Vector3 getRandomPoint()
    {
        float posX = Random.Range(-1 * x_max, x_max);
        float posZ = Random.Range(-1 * z_max, z_max);

        pos = central.position;
        pos.x += posX;
        pos.z += posZ;

        return new Vector3(pos.x, transform.position.y, pos.z);
    }
    void setTargetPoint()
    {

        Vector3 target = getRandomPoint();

        NavMeshHit hit;
        if (NavMesh.SamplePosition(target, out hit, 1.0f,NavMesh.AllAreas))
        {
            // 位置をNavMesh内に補正
            target = hit.position;
        }

        transform.rotation = Quaternion.LookRotation(target - transform.position, Vector3.up);
        agent.destination = target;

        agent.isStopped = false;
    }

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        anim = GetComponent<Animator>();

        agent.autoBraking = false;
        agent.updateRotation = false;


        setTargetPoint();
    }

    void Update()
    {
        if(agent.isStopped)
        {
            time_counter += Time.deltaTime;
            if (time_counter > stop_time)
            {
                time_counter = 0;

                setTargetPoint();
            }

        }

        if (agent.remainingDistance < 0.5f)
        {
            stop_time = Random.Range(0, stop_time_max);
            agent.isStopped = true;
        }

        anim.SetFloat("Blend", agent.velocity.sqrMagnitude);
    }
}