using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Unity.Robotics.ROSTCPConnector;
using RosMessageTypes.Sensor;

namespace HJ.Simulator
{
    [RequireComponent(typeof(Camera))]
    [RequireComponent(typeof(HJ.Simulator.DepthCamera))]
    public class DepthCameraPublisher : MonoBehaviour
    {

        [SerializeField] private string _topicName = "/camera/depth";
        [SerializeField] private string _frameId = "depth_camera";

        [SerializeField] private int _width = 1280;
        [SerializeField] private int _height = 720;

        [SerializeField] private int _publishRate = 20;

        private float _timeElapsed = 0f;
        private float _timeStamp = 0f;

        private ROSConnection _ros;
        // private CompressedImageMsg _message;
        private ImageMsg _message;

        private HJ.Simulator.DepthCamera _camera;

        void Start()
        {
            // Get Rotate Lidar
            this._camera = GetComponent<HJ.Simulator.DepthCamera>();
            this._camera.Init(this._width, this._height);

            // setup ROS
            this._ros = ROSConnection.GetOrCreateInstance();
            // this._topicName += "/compressed";
            this._topicName += "/image";
            this._ros.RegisterPublisher<ImageMsg>(this._topicName);

            // setup ROS Message
            this._message = new ImageMsg();
            this._message.header.frame_id = this._frameId;
            this._message.encoding = "32FC1";
            this._message.is_bigendian = 0;

        }

        void Update()
        {
            this._timeElapsed += Time.deltaTime;

            if (this._timeElapsed > (1f / this._publishRate))
            {
                this._camera.UpdateImage();

                // Update ROS Message
                int sec = (int)Math.Truncate(this._timeStamp);
                uint nanosec = (uint)((this._timeStamp - sec) * 1e+9);
                this._message.header.stamp.sec = sec;
                this._message.header.stamp.nanosec = nanosec;

                this._message.step = (uint)this._camera.texture.width * 4;
                this._message.height = (uint)this._camera.texture.height;
                this._message.width = (uint)this._camera.texture.width;
                this._message.data = this._camera.texture.GetRawTextureData();
                // this._message.data = this._camera.texture.EncodeToPNG();

                this._ros.Publish(this._topicName, this._message);

                // Update time
                this._timeElapsed = 0;
                this._timeStamp = Time.time;
            }
        }
    }
}
