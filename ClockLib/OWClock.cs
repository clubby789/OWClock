﻿using OWML.Common;
using OWML.Common.Menus;
using OWML.ModHelper;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Clock
{
    public class OWClock : ModBehaviour
    {
        private static EventFile _save;
        private List<TimeEvent> _eventList;
        private Font _hudFont;
        private float _xPos;
        private float _yPos;
        private float _width;
        private int _displayHeight;
        private int _displayWidth;
        private bool _displayOnlyWhenSuitOn;
        private bool _displayed = true;

        internal static IModHelper Helper;
        public static bool CountUp { get; private set; }
        public static bool Milliseconds { get; private set; }
        public static int EventCount { get; private set; }
        public static float HudScale { get; private set; }
        public static List<int> EnabledTypes { get; private set; } = new List<int>();
        public static EventFile Save { get => _save; set => _save = value; }

        #region "Magic methods from Unity MonoBehaviour"
        /// <summary>
        /// Called once when the mod is loaded
        /// </summary>
        internal void Start()
        {
            Helper = ModHelper;
            Save = EventFile.LoadSaveFile();
            _hudFont = Resources.Load<Font>(@"fonts/english - latin/SpaceMono-Regular_Dynamic");
#pragma warning disable 0618 // New menu system doesn't support text input in pause menu
            ModHelper.Menus.PauseMenu.OnInit += AddMenuItem;
#pragma warning restore 0618

            ModHelper.Console.WriteLine($"OWClock mod loaded at " + DateTime.Now.ToString("s"), type: MessageType.Success);

            GlobalMessenger<GraphicSettings>.AddListener("GraphicSettingsUpdated", GetDisplaySettings);

            // We need the wake event to reload our eventlist because we are going to remove expired items from the list as they pass.
            //ModHelper.Events.Subscribe<PlayerBody>(Events.AfterAwake);
            //ModHelper.Events.Event += OnEvent;
            GlobalMessenger.AddListener("WakeUp", OnWakeup);
        }

        /// <summary>
        /// OnGUI is called for rendering and handling GUI events by Unity. 
        /// </summary>
        internal void OnGUI()
        {
            if (!_displayed)
            {
                return;
            }

            if (_eventList == null)
            {
                return;
            }

            if (GUIMode.IsHiddenMode() || PlayerState.UsingShipComputer() || LoadManager.GetCurrentScene() != OWScene.SolarSystem)
            {
                return;
            }

            // It would be cool to render the UI on the helmet display if this is enabled
            if (_displayOnlyWhenSuitOn && !PlayerState.IsWearingSuit())
            {
                return;
            }

            var elapsed = TimeLoop.GetSecondsElapsed();
            if (elapsed < 1f)
            {
                return;
            }

            var style = new GUIStyle
            {
                font = _hudFont,
                fontSize = 30,
                wordWrap = true
            };

            style.normal.textColor = Color.white;

            var timestamp = CountUp ? "Time Elapsed: " + ParseTime(elapsed) : "Time Remaining: " + ParseTime(Math.Max(0, TimeLoop.GetSecondsRemaining()));
            GUI.Label(new Rect(_xPos, _yPos, _width, 60f), timestamp, style);

            style.fontSize = 20;
            int shown = 0;

            // Loop until desired number of events are shown
            // OR we reach end of list
            float yOff = 0;
            for (int i = 0; (i < _eventList.Count) && (shown < EventCount); i++)
            {
                var timeEvent = _eventList[i];
                if (timeEvent.Timestamp < elapsed)
                {
                    // If the event has passed we should stop looking at it.
                    _eventList.RemoveAt(i);
                    continue;
                }

                if (EnabledTypes.IndexOf((int)timeEvent.type) == -1)
                {
                    continue;
                }

                var scaleFactor = (timeEvent.Timestamp - elapsed) / 20;
                style.normal.textColor = Color.Lerp(Color.red, Color.white, scaleFactor);
                var timeString = CountUp ? ParseTime(timeEvent.Timestamp) : ParseTime(timeEvent.Timestamp - elapsed);
                GUIContent guiText = new GUIContent($"{timeString} - {timeEvent.Name}");
                float labelSize = style.CalcHeight(guiText, _width);
                yOff += labelSize;
                GUI.Label(new Rect(_xPos, _yPos - yOff, _width, labelSize), guiText, style);
                shown++;
            }
        }

        /// <summary>
        /// Update is called once per frame by Unity
        /// </summary>
        internal void Update()
        {
            if (Keyboard.current[Key.H].wasPressedThisFrame)
            {
                _displayed = !_displayed;
            }
        }

        /// <summary>
        /// Called by OWML; once at the start and upon each config setting change.
        /// </summary>
        /// <param name="config">The new settings passed from OWML</param>
        public override void Configure(IModConfig config)
        {
            CountUp = config.GetSettingsValue<bool>("Count Up");
            Milliseconds = config.GetSettingsValue<bool>("Count In Milliseconds");
            EventCount = config.GetSettingsValue<int>("Events to Display");
            HudScale = config.GetSettingsValue<float>("HudScale");
            EnabledTypes.Clear();
            for (int i = 0; i < Enum.GetNames(typeof(TimeEvent.Type)).Length; i++)
            {
                var name = Enum.GetName(typeof(TimeEvent.Type), i);
                if (config.GetSettingsValue<bool>(name))
                {
                    EnabledTypes.Add(i);
                }
            }

            // When the HudScale changes, we need to scale the HUD
            RecalculatePosition();
            _displayOnlyWhenSuitOn = config.GetSettingsValue<bool>("Only Display When Suit Equipped");
        }

        #endregion

        #region "Event Handlers"
        /// <summary>
        /// Handles wakeup by reloading the event list
        /// </summary>
        private void OnWakeup()
        {
            // Start the list over from the save file when you wake up. This allows us to remove events as they happen.
            ModHelper.Console.WriteLine("Loading the event list for the clock.", type: MessageType.Debug);
            _eventList = Save.eventList.ToList();
        }

        /// <summary>
        /// Adds custom menu items to the game.
        /// </summary>
#pragma warning disable 0618 // New menu system doesn't support text input in pause menu
        private void AddMenuItem()
        {
            var addEventMenu = ModHelper.Menus.PauseMenu.Copy("ADD EVENT");
            var addEventInputButton = ModHelper.Menus.PauseMenu.ResumeButton.Duplicate("ADD EVENT");
            addEventInputButton.OnClick += EventPopup;

            var debugEventMenu = ModHelper.Menus.PauseMenu.Copy("DEBUG TIME");
            var debugEventInputButton = ModHelper.Menus.PauseMenu.ResumeButton.Duplicate("DEBUG TIME");
            debugEventInputButton.OnClick += LogTime;
        }
#pragma warning restore 0618

        /// <summary>
        /// Print the current time to the OWML log window.
        /// </summary>
        private void LogTime()
        {
            var currentTime = TimeLoop.GetSecondsElapsed();
            ModHelper.Console.WriteLine($"Time is {currentTime}", type: MessageType.Info);
        }

        /// <summary>
        /// Handles the GraphicSettingsUpdated event from Unity.
        /// </summary>
        /// <param name="settings">The settings passed from Unity.</param>
        private void GetDisplaySettings(GraphicSettings settings)
        {
            // Store the current resolution so we can update our position based on changes to HudScale later.
            _displayHeight = settings.displayResHeight;
            _displayWidth = settings.displayResWidth;
            RecalculatePosition();
        }

        /// <summary>
        /// Get user input for the menu item "ADD EVENT".
        /// </summary>
#pragma warning disable 0618 // New menu system doesn't support text input in pause menu
        private void EventPopup()
        {
            var popup = ModHelper.Menus.PopupManager.CreateInputPopup(InputType.Text, "Event Name");
            popup.OnConfirm += AddEvent;
        }
#pragma warning restore 0618

        /// <summary>
        /// Save the new event that the user created from the menu.
        /// </summary>
        /// <param name="text">Name of the event.</param>
        private void AddEvent(string text)
        {
            Save.AddEvent(TimeLoop.GetSecondsElapsed(), text);
        }

        #endregion

        /// <summary>
        /// Determine where the Time events and clock should be positioned in the HUD.
        /// </summary>
        private void RecalculatePosition()
        {
            _yPos = _displayHeight - 60f;
            _xPos = Milliseconds ? _displayWidth * (1 - HudScale / 100) - 80f : _displayWidth * (1 - HudScale / 100) - 20f;
            _width = _displayWidth * (HudScale / 100);
        }

        /// <summary>
        /// Format for the clock.
        /// </summary>
        /// <param name="timestamp">From TimeLoop.GetSecondsElapsed</param>
        /// <returns>The clock display in string format, ready to be put on a GUI.Label</returns>
        static string ParseTime(float timestamp)
        {
            var minutes = Mathf.Floor(timestamp / 60f).ToString().PadLeft(2, '0');
            var seconds = Math.Truncate(timestamp % 60f).ToString().PadLeft(2, '0');
            var clock = $"{minutes}:{seconds}";
            if (Milliseconds)
            {
                var milliseconds = Math.Truncate((timestamp - Math.Floor(timestamp)) * 1000).ToString().PadLeft(3, '0');
                clock = $"{clock}.{milliseconds}";
            }

            return clock;
        }
    }
}
