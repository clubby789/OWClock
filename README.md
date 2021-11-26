# OWClock

![image](https://user-images.githubusercontent.com/3955124/143557890-e64ad41d-9e19-4c10-9471-c2beaa1ed49b.png)

This mod adds a clock overlay to Outer Wilds. It can be set to count up, or down to the sun exploding.

There's also an additional event logging system; upcoming events defined in `events.json` are displayed above the clock, turning red as they approach.
New events can be added from the pause menu (timestamp will be set to the current loop time).

## Options
 - Count Up: Counts the elapsed seconds and minutes. When off, will count down to events/the end of the loop
 - Milliseconds: For speedrunners; uses millisecond timestamps
 - Events to Display: Number of upcoming events to display
 - Event List Width: Fraction of the available resolution that the event list can take up

## KNOWN ISSUES
Running this during more intensive moments seem to sometimes cause a BSOD with CRITICAL_PROCESS_DIED (At least on my PC). If anyone has this issue (or a solution), please let me know.
