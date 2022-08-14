using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Unity.Robotics.ROSTCPConnector;
using RosMessageTypes.Geometry;
using Unity.Robotics.UrdfImporter.Control;

namespace HJ.Simulator
{

    public class TwistWheelController : MonoBehaviour
    {
        public enum WheelType
        {
            Left,
            Right
        }
        [SerializeField] WheelType type;
        private bool isMessageReceived;

        private ArticulationBody wheel;

        public float maxLinearSpeed = 2; //  m/s
        public float maxRotationalSpeed = 1;//
        public float wheelRadius = 0.033f; //meters
        public float trackWidth = 0.288f; // meters Distance between tyres
        public float forceLimit = 10;
        public float damping = 10;

        private RotationDirection direction;
        private float rosLinear = 0f;
        private float rosAngular = 0f;

        public float ROSTimeout = 0.5f;
        private float lastCmdReceived = 0f;

        ROSConnection ros;

        void Start()
        {
            wheel = GetComponent<ArticulationBody>();
            SetParameters(wheel);

            ros = ROSConnection.GetOrCreateInstance();
            ros.Subscribe<TwistMsg>("cmd_vel", ReceiveROSCmd);
        }

        void ReceiveROSCmd(TwistMsg cmdVel)
        {            
            rosLinear = (float)cmdVel.linear.x;
            rosAngular = (float)cmdVel.angular.z;

            isMessageReceived = true;
            lastCmdReceived = Time.time;
        }

        void FixedUpdate()
        {            
            if (Time.time - lastCmdReceived > ROSTimeout)
            {
                isMessageReceived = true;
                rosLinear = 0;
                rosAngular = 0;
            }
            if (isMessageReceived)
            {
                ProcessMessage();
            }
        }        
        private void ProcessMessage()
        {            
            
            if (rosLinear > maxLinearSpeed)
            {
                rosLinear = maxLinearSpeed;
            }
            if (rosAngular > maxRotationalSpeed)
            {
                rosAngular = maxRotationalSpeed;
            }
            float wheel1Rotation = (rosLinear / wheelRadius);
            float wheelSpeedDiff = ((rosAngular * trackWidth) / wheelRadius);
            if (rosAngular != 0)
            {
                if (type.ToString() == "Left")
                {
                    wheel1Rotation = (wheel1Rotation - (wheelSpeedDiff / 1)) * Mathf.Rad2Deg;
                }
                else if (type.ToString() == "Right")
                {
                    wheel1Rotation = (wheel1Rotation + (wheelSpeedDiff / 1)) * Mathf.Rad2Deg;
                }
            }
            else
            {
                wheel1Rotation *= Mathf.Rad2Deg;
            }
            SetSpeed(wheel, wheel1Rotation);

            isMessageReceived = false;
        }        
        
        private void SetParameters(ArticulationBody joint)
        {

            ArticulationDrive drive = joint.xDrive;
            drive.forceLimit = forceLimit;
            drive.damping = damping;
            joint.xDrive = drive;

        }
        private void SetSpeed(ArticulationBody joint, float wheelSpeed = float.NaN)
        {
            ArticulationDrive drive = joint.xDrive;
            if (float.IsNaN(wheelSpeed))
            {
                drive.targetVelocity = ((2 * maxLinearSpeed) / wheelRadius) * Mathf.Rad2Deg * (int)direction;
            }
            else
            {
                drive.targetVelocity = wheelSpeed;
            }
            joint.xDrive = drive;
        }

    }
}
