﻿using System;
using Game.Entities;
using Game.EventManagement.Events;
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
            entity.AddAttribute(Key_ProjectileOwner, new Attribute<Entity>(null));
        }

        public override void OnUpdate(float deltaTime)
        {
            Attribute<Vector2D> position = entity[SpatialBehavior.Key_Position] as Attribute<Vector2D>;

            if (position.Value.Y >= 600 - 15) { // ToDo: Size is currently hard-coded, should refer to window size and projectile height
                killEntity();

                Console.WriteLine("[" + this.GetType().Name +"] " + entity.Type + " died in vain.");
            }
        }

        protected override void initializeHandledEventTypes()
        {
            handledEventTypes.Add(typeof(CollisionEvent));
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
