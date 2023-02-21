using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshCollider))]
public class CreateSkinnedMeshCollider : MonoBehaviour
{

    [SerializeField] public float interval = 0.5f;

    private MeshCollider meshCollider;
    private SkinnedMeshRenderer skinnedMeshRenderer;
    private float nextTime;

    // Start is called before the first frame update
    void Start()
    {
        meshCollider = GetComponent<MeshCollider>();
        skinnedMeshRenderer = GetComponent<SkinnedMeshRenderer>();
        nextTime = 0.0f;
    }

    // Update is called once per frame
    void Update()
    {
        nextTime -= Time.deltaTime;
        if (nextTime < 0.0f)
        {
            nextTime = interval;

            Mesh bakedMesh = new Mesh();
            skinnedMeshRenderer.BakeMesh(bakedMesh);
            Destroy(meshCollider.sharedMesh); // これがないとメモリ量が増加し続ける。GCによる開放が間に合わない？
            meshCollider.sharedMesh = null;
            meshCollider.sharedMesh = bakedMesh;
        }
    }
}