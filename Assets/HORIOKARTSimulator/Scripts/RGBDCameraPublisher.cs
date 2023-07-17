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

        [SerializeField] private string _topicNameSpace = "/namespace";
        [SerializeField] private string _frameId = "frame_id";

        [SerializeField] private string _depthTopicName = "/camera/aligned_depth_to_color/image_raw";
        [SerializeField] private string _colorTopicName = "/camera/color/image_raw";
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

        private CameraInfoMsg _cameraInfoMessage;

        void Start()
        {
            // depth topic setting
            this._depthCamera = GetComponent<HJ.Simulator.DepthCamera>();
            this._depthCamera.Init(this.width, this.height);

            this._depthMessage = new ImageMsg();
            this._depthMessage.header.frame_id = this._frameId;
            // this._depthMessage.encoding = "32FC1";
            this._depthMessage.encoding = "16UC1";
            this._depthMessage.is_bigendian = 0;
            // this._depthMessage.step = (uint)this.width * 4;
            this._depthMessage.step = (uint)this.width * 2;
            this._depthMessage.height = (uint)this.height;
            this._depthMessage.width = (uint)this.width;

            // color topic setting
            this._colorCamera = GetComponent<HJ.Simulator.ColorCamera>();
            this._colorCamera.Init(this.width, this.height);

            this._colorMessage = new ImageMsg();
            this._colorMessage.header.frame_id = this._frameId;
            this._colorMessage.encoding = "rgb8";
            this._colorMessage.is_bigendian = 0;
            this._colorMessage.step = (uint)this.width * 3;
            this._colorMessage.height = (uint)this.height;
            this._colorMessage.width = (uint)this.width;

            // if frame id is empty, set game object name
            if (this._frameId == string.Empty)
            {
                this._frameId = gameObject.name;
            }

            // namespace
            if (this._topicNameSpace != string.Empty)
            {
                this._depthTopicName = this._topicNameSpace + this._depthTopicName;
                this._colorTopicName = this._topicNameSpace + this._colorTopicName;
                this._cameraInfoTopicName = this._topicNameSpace + this._cameraInfoTopicName;
            }

            // setup ROS
            this._ros = ROSConnection.GetOrCreateInstance();
            this._ros.RegisterPublisher<ImageMsg>(this._depthTopicName);
            this._ros.RegisterPublisher<ImageMsg>(this._colorTopicName);
            this._ros.RegisterPublisher<CameraInfoMsg>(this._cameraInfoTopicName);

            _cameraInfoMessage = CameraInfoGenerator.ConstructCameraInfoMessage(this._colorCamera._camera, this._depthMessage.header, 0.0f, 0.01f);
            _cameraInfoMessage.height = (uint)this.height;
            _cameraInfoMessage.width = (uint)this.width;
            float focalLength = GetFocalLength(this._colorCamera._camera)*1000;
            _cameraInfoMessage.K[0] = focalLength;
            _cameraInfoMessage.K[4] = focalLength;
            _cameraInfoMessage.K[2] = this.width / 2;
            _cameraInfoMessage.K[5] = this.height / 2;
            _cameraInfoMessage.P[0] = focalLength;
            _cameraInfoMessage.P[5] = focalLength;
            _cameraInfoMessage.P[2] = this.width / 2;
            _cameraInfoMessage.P[6] = this.height / 2;

        }

        void Update()
        {
            this._timeElapsed += Time.deltaTime;

            if (this._timeElapsed > (1f / this._publishRate))
            {

                UpdateHeader();

                PublishColorImage();
                PublishDepthImage();

                this._ros.Publish(_cameraInfoTopicName, _cameraInfoMessage);

                // Update time
                this._timeElapsed = 0;
                this._timeStamp = Time.time;
            }
        }

        private float GetFocalLength(Camera camera)
        {
            // カメラの視野角を取得
            float verticalFOV = camera.fieldOfView * Mathf.Deg2Rad;

            // 画像平面のサイズを計算
            float imagePlaneHeight = 2.0f * Mathf.Tan(verticalFOV / 2.0f);
            float imagePlaneWidth = imagePlaneHeight * camera.aspect;

            // 焦点距離を計算
            // float focalLength = imagePlaneHeight / 2.0f;
            float focalLength = imagePlaneWidth / 2.0f;

            return focalLength;
        }

        private void UpdateHeader()
        {
            // Update ROS Message
            int sec = (int)Math.Truncate(this._timeStamp);
            uint nanosec = (uint)((this._timeStamp - sec) * 1e+9);

            this._depthMessage.header.stamp.sec = sec;
            this._depthMessage.header.stamp.nanosec = nanosec;

            this._colorMessage.header.stamp.sec = sec;
            this._colorMessage.header.stamp.nanosec = nanosec;
        }

        private void PublishDepthImage()
        {
            this._depthCamera.UpdateImage();

            // this._depthMessage.data = this._depthCamera.texture.GetRawTextureData();
            this._depthMessage.data = this._depthCamera.depthData;
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
