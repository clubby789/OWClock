using System;

namespace Clock
{
    [Serializable]
    public class TimeEvent
    {
        public enum Type
        {
            Sun,
            TimeLoop,
            WarpPlatforms,
            Chert,
            Misc
        }
        public float Timestamp;
        public string Name;
        public Type type = Type.Misc;
        public TimeEvent(float timestamp, string name)
        {
            Timestamp = timestamp;
            Name = name;
        }
    }
}
