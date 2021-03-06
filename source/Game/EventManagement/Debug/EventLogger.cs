﻿using System;
using Game.EventManagement.Events;

namespace Game.EventManagement.Debug
{

    class EventLogger : IEventListener
    {
        public void OnEvent(Event evt)
        {
            Console.WriteLine("[" + this.GetType().Name +"] Event triggered: " + evt);
        }
    }

}
