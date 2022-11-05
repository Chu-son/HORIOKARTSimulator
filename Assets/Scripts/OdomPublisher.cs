using System;
using System.Collections.Generic;
using System.Linq;
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
public class OdomPublisher : MonoBehaviour
{
    
    [SerializeField]
    double m_PublishRateHz = 20f;
    [SerializeField]
    public string topicName = "/odom";
    [SerializeField]
    public string frameId = "odom";

    [SerializeField]
    public bool isPublish = true;
    
    double m_LastPublishTimeSeconds;

    ROSConnection m_ROS;

    double PublishPeriodSeconds => 1.0f / m_PublishRateHz;

    bool ShouldPublishMessage => Clock.NowTimeInSeconds > m_LastPublishTimeSeconds + PublishPeriodSeconds;

    //public string rightWheelName = "right_wheel_link";
    //public string leftWheelName = "left_wheel_link";
    private float rightWheelRadius;
    private float leftWheelRadius;
    private float wheelSeparation;

    public GameObject rightWheel;
    public GameObject leftWheel;
    //private GameObject rightWheel;
    //private GameObject leftWheel;
    private ArticulationBody rightHinge;
    private ArticulationBody leftHinge;

    private OdometryMsg message;
    private float x;
    private float y;
    private float yaw;

    //public float publish_rate = 10.0f; //Hz
    private float pub_delta_time;
    private float d_time = 0.0f;

    public float rightBiasRate = 1.1f;
    public float leftBiasRate = 1.0f;

    public float rightNoizeRate = 1.1f;
    public float leftNoizeRate = 1.1f;

    private HeaderMsgUpdater odomHeaderUpdater;

    private float leftPrePos = 0.0f;
    private float rightPrePos = 0.0f;


    void Start()
    {
        //rightWheel = GameObject.Find(rightWheelName);
        //leftWheel = GameObject.Find(leftWheelName);

        Debug.Log("rightWheel: " + rightWheel.name);
        Debug.Log("leftWheel: " + leftWheel.name);

        TwistWheelController rightWheelScript = rightWheel.GetComponent<TwistWheelController>();
        rightWheelRadius = rightWheelScript.wheelRadius * rightBiasRate;
        TwistWheelController leftWheelScript = leftWheel.GetComponent<TwistWheelController>();
        leftWheelRadius = leftWheelScript.wheelRadius * leftBiasRate;

        Debug.Log("rightWheelRadius: " + rightWheelRadius);
        Debug.Log("leftWheelRadius: " + leftWheelRadius);

        wheelSeparation = rightWheelScript.trackWidth;

        Debug.Log("wheelSeparation: " + wheelSeparation);
        
        rightHinge = rightWheel.GetComponent<ArticulationBody>();
        leftHinge = leftWheel.GetComponent<ArticulationBody>();

        message = new OdometryMsg();
        InitializeMessage();

        m_LastPublishTimeSeconds = Clock.time + PublishPeriodSeconds;

        m_ROS = ROSConnection.GetOrCreateInstance();
        m_ROS.RegisterPublisher<OdometryMsg>(topicName);


        leftPrePos = leftHinge.jointPosition[0];
        rightPrePos = rightHinge.jointPosition[0];
    }

    private void InitializeMessage()
    {
        odomHeaderUpdater = new HeaderMsgUpdater(frameId);
        message = new OdometryMsg();
        message.header = odomHeaderUpdater.update();
        message.child_frame_id = "base_footprint";
        message.pose.pose.position = new PointMsg(0, 0, 0);
        message.pose.pose.orientation = new QuaternionMsg(0, 0, 0, 0);
    }

    void PublishMessage()
    {
        float dt = Time.deltaTime;
        // float omegaL = leftHinge.jointVelocity[0];// [rad/s]
        // float omegaR = rightHinge.jointVelocity[0];// [rad/s]
        // float omegaL = (float)Math.Ceiling(leftHinge.jointVelocity[0] * 100)/100;// [rad/s]
        // float omegaR = (float)Math.Ceiling(rightHinge.jointVelocity[0] * 100)/100;// [rad/s]

        float omegaL = (leftHinge.jointPosition[0] - leftPrePos)/dt;
        float omegaR = (rightHinge.jointPosition[0] - rightPrePos)/dt;

        omegaL = leftWheelRadius * omegaL; // [m/s]
        omegaR = rightWheelRadius * omegaR;

        float robotVelocity = (omegaR + omegaL) / 2.0f;
        float robotYawrate = (omegaL - omegaR) / wheelSeparation;
        
        x += robotVelocity * dt * Mathf.Cos(-yaw);
        y += robotVelocity * dt * Mathf.Sin(-yaw);
        yaw += robotYawrate * dt;

        yaw = Mathf.Atan2(Mathf.Sin(yaw), Mathf.Cos(yaw));

        //Debug.Log($"vel :{robotVelocity}");
        //Debug.Log($"yaw rad:{yaw}");
        //Debug.Log($"yaw deg:{yaw * Mathf.Rad2Deg}");
        //Debug.Log($"left, right angle deg:{leftHinge.jointPosition[0] * Mathf.Rad2Deg}, {rightHinge.jointPosition[0] * Mathf.Rad2Deg}");
        //Debug.Log($"left, right vel:{leftHinge.jointVelocity[0] * Mathf.Rad2Deg}, {rightHinge.jointVelocity[0] * Mathf.Rad2Deg}");
        //Debug.Log($"left, right vel?:{(leftHinge.jointPosition[0] - leftPrePos) * Mathf.Rad2Deg / dt}, {(rightHinge.jointPosition[0] - rightPrePos) * Mathf.Rad2Deg / dt}");

        leftPrePos = leftHinge.jointPosition[0];
        rightPrePos = rightHinge.jointPosition[0];

        d_time += dt;
        // if(d_time >= pub_delta_time){
            // message.header = new HeaderMsg
            // {
            //     frame_id = frameId,
            //     stamp = new TimeMsg
            //     {
            //         sec = timestamp.Seconds,
            //         nanosec = timestamp.NanoSeconds,
            //     }
            // };
        if(ShouldPublishMessage && isPublish){
            message.header = odomHeaderUpdater.update();
            message.pose.pose.position = new PointMsg(x, y, 0);
            message.pose.pose.orientation = GetQuaternionMsgFromRPY(0, 0, -yaw);
            message.twist.twist.linear.x = robotVelocity;
            message.twist.twist.angular.z = robotYawrate;

            m_ROS.Publish(topicName, message);

            d_time = 0.0f;
            m_LastPublishTimeSeconds = Clock.FrameStartTimeInSeconds;
        }

    }

    void Update()
    {
        PublishMessage();
    }

    private QuaternionMsg GetQuaternionMsgFromRPY(float roll, float pitch, float yaw)
    {
        Quaternion rotation = Quaternion.Euler(roll * Mathf.Rad2Deg, pitch * Mathf.Rad2Deg, yaw * Mathf.Rad2Deg);
        return new QuaternionMsg(rotation.x, rotation.y, rotation.z, rotation.w);
    }
}
}