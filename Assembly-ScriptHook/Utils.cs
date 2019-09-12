using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace ArenaConceder
{
    public static class Utils
    {
        /// <summary>
        /// Prints out a list of all the events in the current scene.
        /// This is useful for figuring out the internal names for each event.
        /// </summary>
        public static void OutputEventNames()
        {
            var events = GameObject.FindObjectsOfType<EventTile>();

            foreach (var e in events)
                Debug.Log(e.EventContext.eventInfo.PublicEventName + " :: " + e.EventContext.eventInfo.InternalEventName);
        }

        /// <summary>
        /// Returns the internal event name for a given internal or public event name.
        /// </summary>
        /// <param name="eventName">Internal or public event name.</param>
        /// <returns>Internal event name or null if failed.</returns>
        public static string GetInternalEventName(string eventName)
        {
            // Get all events
            var events = GameObject.FindObjectsOfType<EventTile>();

            // Normalize
            eventName = eventName.ToLower();

            // Try and find event
            foreach (var e in events)
            {
                if (e.EventContext.eventInfo.PublicEventName.ToLower() == eventName || e.EventContext.eventInfo.InternalEventName.ToLower() == eventName)
                    return e.EventContext.eventInfo.InternalEventName;
            }

            return null;
        }
    }
}
