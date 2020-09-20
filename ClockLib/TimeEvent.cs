using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using System.Text;

namespace Clock
{
    [Serializable]
    public class TimeEvent
    {
        public float Timestamp;
        public string Name; 
        public TimeEvent(float timestamp, string name)
        {
            Timestamp = timestamp;
            Name = name;
        }
    }
}
