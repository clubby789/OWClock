using System;

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
