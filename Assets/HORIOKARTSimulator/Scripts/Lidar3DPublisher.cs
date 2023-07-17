using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using UnityEngine.Jobs;
using Unity.Jobs;
using UnityEngine;

using Unity.Robotics.ROSTCPConnector;
using RosMessageTypes.Sensor;
using Unity.Collections;

[RequireComponent(typeof(FRJ.Sensor.RotateLidar))]
public class Lidar3DPublisher : MonoBehaviour
{
    [SerializeField] private string _topicName = "scan";
    [SerializeField] private string _frameId = "scan_link";

    private JobHandle _handle;
    private float _timeElapsed = 0f;
    private float _timeStamp = 0f;

    private ROSConnection _ros;
    private PointCloud2Msg _message;
    private int _numOfPoints;

    private FRJ.Sensor.RotateLidar _lidar;

    float Deg2Rad(float deg)
    {
        return deg * Mathf.PI / 180f;
    }

    void Start()
    {
        // Get Rotate Lidar
        this._lidar = GetComponent<FRJ.Sensor.RotateLidar>();
        this._lidar.Init();

        // setup ROS
        this._ros = ROSConnection.GetOrCreateInstance();
        this._ros.RegisterPublisher<PointCloud2Msg>(this._topicName);

        // setup ROS Message
        this._message = new PointCloud2Msg();
        this._message.header.frame_id = this._frameId;

        this._numOfPoints = this._lidar.numOfIncrements * this._lidar.numOfLayers;
        this._message.height = 1;
        this._message.width = (uint)this._numOfPoints;

        this._message.fields = new PointFieldMsg[3];
        this._message.fields[0] = new PointFieldMsg();
        this._message.fields[0].name = "x";
        this._message.fields[0].offset = 0;
        this._message.fields[0].datatype = 7;
        this._message.fields[0].count = 1;

        this._message.fields[1] = new PointFieldMsg();
        this._message.fields[1].name = "y";
        this._message.fields[1].offset = 4;
        this._message.fields[1].datatype = 7;
        this._message.fields[1].count = 1;

        this._message.fields[2] = new PointFieldMsg();
        this._message.fields[2].name = "z";
        this._message.fields[2].offset = 8;
        this._message.fields[2].datatype = 7;
        this._message.fields[2].count = 1;

        this._message.is_bigendian = false;
        this._message.point_step = 12;
        this._message.row_step = (uint)this._numOfPoints * this._message.point_step;

        this._message.data = new byte[this._numOfPoints * this._message.point_step];

        this._message.is_dense = true;

    }

    void OnDisable()
    {
        this._handle.Complete();
        this._lidar.Dispose();
    }

    void Update()
    {
        this._handle.Complete();
        this._timeElapsed += Time.deltaTime;

        if (this._timeElapsed > (1f / this._lidar.scanRate))
            if (this._timeElapsed > (1f / this._lidar.scanRate))
            {
                // Update ROS Message
#if ROS2
            int sec = (int)Math.Truncate(this._timeStamp);
#else
                uint sec = (uint)Math.Truncate(this._timeStamp);
#endif
                uint nanosec = (uint)((this._timeStamp - sec) * 1e+9);
                this._message.header.stamp.sec = sec;
                this._message.header.stamp.nanosec = nanosec;

                // Convert distances to point cloud
                for (int i = 0; i < this._lidar.numOfIncrements * this._lidar.numOfLayers; i++)
                {
                    Vector3 point = new Vector3(0, 0, 0);
                    if (this._lidar.distances[i] > 0)
                    {
                        // Convert distance to 3D point
                        Vector3 direction = this._lidar.commandDirVecs[i].normalized;
                        point = direction * this._lidar.distances[i];

                    }
                    // Add point to point cloud message's data array
                    int byteOffset = i * (int)this._message.point_step;
                    BitConverter.GetBytes(point.z).CopyTo(this._message.data, byteOffset);
                    BitConverter.GetBytes(-point.x).CopyTo(this._message.data, byteOffset + 4);
                    BitConverter.GetBytes(point.y).CopyTo(this._message.data, byteOffset + 8);
                }

                // Publish point cloud message
                _ros.Publish(this._topicName, this._message);

                // Update time
                this._timeElapsed = 0;
                this._timeStamp = Time.time;

                // Update Raycast Command
                for (int incr = 0; incr < this._lidar.numOfIncrements; incr++)
                {
                    for (int layer = 0; layer < this._lidar.numOfLayers; layer++)
                    {
                        int index = layer + incr * this._lidar.numOfLayers;
                        this._lidar.commands[index] =
                            new RaycastCommand(this.transform.position,
                                               this.transform.rotation * this._lidar.commandDirVecs[index],
                                               this._lidar.maxRange);
                    }
                }

                // Update Parallel Jobs
                var raycastJobHandle = RaycastCommand.ScheduleBatch(this._lidar.commands, this._lidar.results, 360);
                // Update Distance data
                if (this._lidar.randomSeed++ == 0)
                    this._lidar.randomSeed = 1;
                this._lidar.job.random.InitState(this._lidar.randomSeed);
                this._handle = this._lidar.job.Schedule(this._lidar.results.Length, 360, raycastJobHandle);
                JobHandle.ScheduleBatchedJobs();
            }
    }

}