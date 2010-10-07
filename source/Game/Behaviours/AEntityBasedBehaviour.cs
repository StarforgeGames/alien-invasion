﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Game.Entities;
using System.Collections.ObjectModel;
using Game.EventManagement.Events;

namespace Game.Behaviours
{

    abstract class AEntityBasedBehaviour : IBehaviour
    {
        protected Entity entity;

        public AEntityBasedBehaviour(Entity entity)
        {
            this.entity = entity;
        }

        protected List<Type> handledEventTypes;
        public ReadOnlyCollection<Type> HandledEventTypes {
            get { return handledEventTypes.AsReadOnly(); }
        }

        public abstract void OnUpdate(float deltaTime);
        public abstract void OnEvent(Event evt);
    }

}
