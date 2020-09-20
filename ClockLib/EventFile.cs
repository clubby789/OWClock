using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Clock
{
    [Serializable]
    public class EventFile
    {
        public List<TimeEvent> eventList = new List<TimeEvent>();
        private const string _fileName = "events.json";

        public void AddEvent(float timestamp, string name)
        {
            eventList.Add(new TimeEvent(timestamp, name));
            eventList.Sort(SortTimestamp);
            Save();
        }

        private void Save()
        {
            OWClock.Helper.Storage.Save(this, _fileName);
        }

        public static EventFile LoadSaveFile()
        {
            var save = OWClock.Helper.Storage.Load<EventFile>(_fileName);
            save.eventList.Sort(SortTimestamp);
            return save ?? new EventFile();
        }

        private static int SortTimestamp(TimeEvent e1, TimeEvent e2)
        {
            return e1.Timestamp.CompareTo(e2.Timestamp);
        }
    }
}