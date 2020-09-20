using System;
using System.Collections.Generic;

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
            Save();
        }

        private void Save()
        {
            OWClock.Helper.Storage.Save(this, _fileName);
        }

        public static EventFile LoadSaveFile()
        {
            var save = OWClock.Helper.Storage.Load<EventFile>(_fileName);
            return save ?? new EventFile();
        }
    }
}