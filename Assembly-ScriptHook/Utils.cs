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
    }
}
