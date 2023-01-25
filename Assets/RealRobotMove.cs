using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using RosMessageTypes.Std;
using RosMessageTypes.Nav;
using RosMessageTypes.Geometry;
using RosMessageTypes.BuiltinInterfaces;
using Unity.Robotics.ROSTCPConnector;
using Unity.Robotics.Core;
using UnityEngine;
using HJ.Simulator;

namespace HJ.Simulator
{
    public class RealRobotMove : MonoBehaviour
    {
        [SerializeField]
        public string odomTopicName = "/odom";

        [SerializeField]
        public string velTopicName = "/real/cmd_vel";

        private TwistMsg msg;
        // private HeaderMsgUpdater headerUpdater;
        ROSConnection ros;

        // public float wheelRadius = 0.033f;
        // public float wheelSeparation = 0.288f;

        // Start is called before the first frame update
        void Start()
        {
            ros = ROSConnection.GetOrCreateInstance();

            ros.Subscribe<OdometryMsg>(odomTopicName, ReceiveROSCmd);

            ros.RegisterPublisher<TwistMsg>(velTopicName);

            // headerUpdater = new headerUpdater("");
            msg = new TwistMsg();

        }

        // Update is called once per frame
        void Update()
        {
            
        }

        void ReceiveROSCmd(OdometryMsg odom)
        {            
            // msg.header = headerUpdater.update();
            msg.linear.x = odom.twist.twist.linear.x;
            msg.angular.z = odom.twist.twist.angular.z;

            ros.Publish(velTopicName, msg);
        }

    }
}
