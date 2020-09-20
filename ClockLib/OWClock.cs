using OWML.Common;
using OWML.Common.Menus;
using OWML.ModHelper;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Clock
{
    public class OWClock : ModBehaviour
    {
        private static EventFile _save;
        private List<string> _eventListStr = new List<string>();
        private List<KeyValuePair<float, string>> _eventList = new List<KeyValuePair<float, string>>();
        private Font _hudFont;
        private Resolution _resolution;
        private float _xPos;
        private float _yPos;

        public static bool CountUp { get; private set; }
        public static bool Milliseconds { get; private set; }
        public static EventFile Save { get => _save; set => _save = value; }

        private void Start()
        {
            Save = EventFile.LoadSaveFile();
            _hudFont = Resources.Load<Font>(@"fonts/english - latin/SpaceMono-Regular_Dynamic");
            ModHelper.Menus.PauseMenu.OnInit += AddMenuItem;

            ModHelper.Console.WriteLine($"My mod {nameof(Clock)} is loaded!", MessageType.Success);
        }

        private void AddMenuItem()
        {
            var eventMenu = ModHelper.Menus.PauseMenu.Copy("ADD EVENT");
            var openInputButton = ModHelper.Menus.PauseMenu.ResumeButton.Duplicate("ADD EVENT");
            openInputButton.OnClick += EventPopup;

            var eventMenu2 = ModHelper.Menus.PauseMenu.Copy("DEBUG TIME");
            var openInputButton2 = ModHelper.Menus.PauseMenu.ResumeButton.Duplicate("DEBUG TIME");
            openInputButton2.OnClick += LogTime;
        }

        private void LogTime()
        {
            var currentTime = TimeLoop.GetSecondsElapsed();
            ModHelper.Console.WriteLine($"Time is {currentTime}");
        }

        private void RecalculatePosition()
        {
            _resolution = Screen.currentResolution;
            _yPos = _resolution.height - 60f;
            _xPos = Milliseconds ? _resolution.width * 4 / 5 - 80f : _resolution.width * 4 / 5 - 20f;
        }

        private void OnGUI()
        {
            if (GUIMode.IsHiddenMode() || PlayerState.UsingShipComputer())
            {
                return;
            }

            var currentRes = Screen.currentResolution;
            if (_resolution.height != currentRes.height || _resolution.width != currentRes.width)
            {
                RecalculatePosition();
            }

            var elapsed = TimeLoop.GetSecondsElapsed();
            if (elapsed < 1f)
            {
                return;
            }

            var style = new GUIStyle();
            style.normal.textColor = Color.white;
            style.font = _hudFont;
            style.fontSize = 30;

            var timestamp = CountUp ? "Time Elapsed: " + ParseTime(elapsed) : "Time Remaining: " + ParseTime(TimeLoop.GetSecondsRemaining());
            GUI.Label(new Rect(_xPos, _yPos, 200f, 60f), timestamp, style);

            style.fontSize = 20;
            for (int i = 0; i < Save.eventList.Count; i++)
            {
                var timeEvent = Save.eventList[i];
                if (i > 5)
                {
                    break;
                }
                if (timeEvent.Timestamp < elapsed)
                {
                    continue;
                }
                var scaleFactor = (timeEvent.Timestamp - elapsed) / 20;
                style.normal.textColor = Color.Lerp(Color.red, Color.white, scaleFactor);
                _yPos -= 20;
                string timeString;
                if (CountUp)
                {
                    timeString = ParseTime(timeEvent.Timestamp);
                }
                else
                {
                    timeString = ParseTime(timeEvent.Timestamp - elapsed);
                }
                GUI.Label(new Rect(xPos, yPos, 200f, 20f), $"{timeString} - {timeEvent.Name}", style);
            }
        }

        private void EventPopup()
        {
            var popup = ModHelper.Menus.PopupManager.CreateInputPopup(InputType.Text, "Event Name");
            popup.OnConfirm += AddEvent;

        }

        private void AddEvent(string text)
        {
            Save.AddEvent(TimeLoop.GetSecondsElapsed(), text);
        }

        string ParseTime(float timestamp)
        {
            var minutes = Mathf.Floor(timestamp / 60f).ToString().PadLeft(2, '0');
            var seconds = Mathf.Round(timestamp % 60f * 100f / 100f).ToString().PadLeft(2, '0');
            var clock = $"{minutes}:{seconds}";
            if (Milliseconds)
            {
                var milliseconds = Math.Round((timestamp - Math.Floor(timestamp)) * 1000).ToString().PadLeft(3, '0');
                clock = $"{clock}.{milliseconds}";
            }
            return clock;
        }

        public override void Configure(IModConfig config)
        {
            CountUp = config.GetSettingsValue<bool>("Count Up");
            Milliseconds = config.GetSettingsValue<bool>("Count In Milliseconds");
        }
    }
}
