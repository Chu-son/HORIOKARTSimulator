using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Unity.Robotics.ROSTCPConnector;
using Unity.Robotics.ROSTCPConnector.MessageGeneration;
using RosMessageTypes.Sensor;

namespace HJ.Simulator
{
    [RequireComponent(typeof(Camera))]
    [RequireComponent(typeof(HJ.Simulator.DepthCamera))]
    [RequireComponent(typeof(HJ.Simulator.ColorCamera))]
    public class RGBDCameraPublisher : MonoBehaviour
    {

        // [SerializeField] private string _topicNameSpace = "/camera";

        [SerializeField] private string _depthTopicName = "/camera/aligned_depth_to_color/image_raw";
        [SerializeField] private string _depthFrameId = "depth_camera_frame";

        [SerializeField] private string _colorTopicName = "/camera/color/image_raw";
        [SerializeField] private string _colorFrameId = "color_camera_frame";
        [SerializeField] private string _cameraInfoTopicName = "/camera/camera_info";

        [SerializeField] private int width = 1280;
        [SerializeField] private int height = 720;

        [SerializeField] private int _publishRate = 20;

        private float _timeElapsed = 0f;
        private float _timeStamp = 0f;

        private ROSConnection _ros;

        private ImageMsg _depthMessage;
        private ImageMsg _colorMessage;

        private HJ.Simulator.DepthCamera _depthCamera;
        private HJ.Simulator.ColorCamera _colorCamera;

        void Start()
        {
            // depth topic setting
            this._depthCamera = GetComponent<HJ.Simulator.DepthCamera>();
            this._depthCamera.Init(this.width, this.height);

            this._depthMessage = new ImageMsg();
            this._depthMessage.header.frame_id = this._depthFrameId;
            this._depthMessage.encoding = "32FC1";
            this._depthMessage.is_bigendian = 0;
            this._depthMessage.step = (uint)this.width * 4;
            this._depthMessage.height = (uint)this.height;
            this._depthMessage.width = (uint)this.width;

            // color topic setting
            this._colorCamera = GetComponent<HJ.Simulator.ColorCamera>();
            this._colorCamera.Init(this.width, this.height);

            this._colorMessage = new ImageMsg();
            this._colorMessage.header.frame_id = this._colorFrameId;
            this._colorMessage.encoding = "rgb8";
            this._colorMessage.is_bigendian = 0;
            this._colorMessage.step = (uint)this.width * 3;
            this._colorMessage.height = (uint)this.height;
            this._colorMessage.width = (uint)this.width;

            // setup ROS
            this._ros = ROSConnection.GetOrCreateInstance();
            this._ros.RegisterPublisher<ImageMsg>(this._depthTopicName);
            this._ros.RegisterPublisher<ImageMsg>(this._colorTopicName);
            this._ros.RegisterPublisher<CameraInfoMsg>(this._cameraInfoTopicName);

        }

        void Update()
        {
            this._timeElapsed += Time.deltaTime;

            if (this._timeElapsed > (1f / this._publishRate))
            {

                UpdateHeader();

                PublishColorImage();
                PublishDepthImage();

                CameraInfoMsg cameraInfoMessage = CameraInfoGenerator.ConstructCameraInfoMessage(this._colorCamera._camera, this._depthMessage.header, 0.0f, 0.01f);
                this._ros.Publish(_cameraInfoTopicName, cameraInfoMessage);

                // Update time
                this._timeElapsed = 0;
                this._timeStamp = Time.time;
            }
        }

        private void UpdateHeader()
        {
            // Update ROS Message
            uint sec = (uint)Math.Truncate(this._timeStamp);
            uint nanosec = (uint)((this._timeStamp - sec) * 1e+9);

            this._depthMessage.header.stamp.sec = sec;
            this._depthMessage.header.stamp.nanosec = nanosec;

            this._colorMessage.header.stamp.sec = sec;
            this._colorMessage.header.stamp.nanosec = nanosec;
        }

        private void PublishDepthImage()
        {
            this._depthCamera.UpdateImage();

            this._depthMessage.data = this._depthCamera.texture.GetRawTextureData();
            // this._depthMessage.data = this._depthCamera.texture.EncodeToPNG();

            this._ros.Publish(this._depthTopicName, this._depthMessage);
        }

        private void PublishColorImage()
        {
            this._colorCamera.UpdateImage();
            // this._colorMessage.data = this._colorCamera.texture.EncodeToPNG();
            this._colorMessage.data = this._colorCamera.texture.GetRawTextureData();
            this._ros.Publish(this._colorTopicName, this._colorMessage);
        }
    }
}
