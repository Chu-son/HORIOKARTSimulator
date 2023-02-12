using System;
using System.Collections.Generic;
using System.Linq;
using RosMessageTypes.Std;
using RosMessageTypes.BuiltinInterfaces;
using Unity.Robotics.ROSTCPConnector;
using Unity.Robotics.Core;
using UnityEngine;

namespace HJ.Simulator
{
    public class HeaderMsgUpdater :HeaderMsg
    {
        private TimeStamp timestamp;

        public HeaderMsgUpdater(string frame_id)
        {
            timestamp = new TimeStamp(Clock.time);

            this.seq = 0;
            this.stamp = new TimeMsg{
                sec = timestamp.Seconds,
                nanosec = timestamp.NanoSeconds,
            };
            this.frame_id = frame_id;
        }

        public HeaderMsg update()
        {
            timestamp = new TimeStamp(Clock.time);
            this.seq++;
            this.stamp = new TimeMsg{
                sec = timestamp.Seconds,
                nanosec = timestamp.NanoSeconds,
            };
            return this;
        }


    }
}