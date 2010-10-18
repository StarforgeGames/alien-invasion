﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using Game.EventManagement.Events;
using Game.Entities;
using Game.Utility;

namespace Game.Behaviors
{

    class ProjectileBehavior : AEntityBasedBehavior
    {
        // Attribute Keys
        public const string Key_ProjectileOwner = "ProjectileOwner";

        public ProjectileBehavior(Entity entity)
            : base(entity)
        {
            handledEventTypes = new List<Type>() { typeof(CollisionEvent) };

            entity.AddAttribute(Key_ProjectileOwner, new Attribute<Entity>(null));
        }

        public override void OnUpdate(float deltaTime)
        {
            Attribute<Rectangle> bounds = entity[SpatialBehavior.Key_Bounds] as Attribute<Rectangle>;

            if (bounds.Value.Top <= 0 || bounds.Value.Bottom >= entity.Game.WorldHeight) {
                killEntity();

                Console.WriteLine("[" + this.GetType().Name +"] " + entity.Type + " died in vain.");
            }
        }

        private void killEntity()
        {
            entity.State = EntityState.Dead;
            EventManager.QueueEvent(new DestroyEntityEvent(DestroyEntityEvent.DESTROY_ENTITY, entity.ID));
        }

        public override void OnEvent(Event evt)
        {
            switch (evt.Type) {
                case CollisionEvent.ACTOR_COLLIDES: {
                    killEntity();

                    Console.WriteLine("[" + this.GetType().Name +"] " + entity.Type + " died fulfilling its honorable "
                        + "duty.");
                }
                break;
            }
        }
    }

}