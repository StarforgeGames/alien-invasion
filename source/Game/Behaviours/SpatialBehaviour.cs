﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Game.Entities;
using Game.Messages;
using Game.Utility;

namespace Game.Behaviours
{
    /// <summary>
    /// Behaviour that gives an entity the feature if having a position in the world and allowing it to move around
    /// in the world.
    /// </summary>
    class SpatialBehaviour : AEntityBasedBehaviour
    {
        // Attribute Keys
        public const string Key_Position = "Position";
        public const string Key_Bounds = "Bounds";
        public const string Key_Orientation = "Orientation";
        public const string Key_Speed = "Speed";
        public const string Key_IsMoving = "IsMoving";

        public SpatialBehaviour(Entity entity, float x, float y, float width, float height, float speed)
            : base (entity)
        {
            Vector2D position = new Vector2D(x, y);
            entity.AddAttribute(Key_Position, new Attribute<Vector2D>(position));
            Rectangle bounds = new Rectangle(position, width, height);
            entity.AddAttribute(Key_Bounds, new Attribute<Rectangle>(bounds));
            entity.AddAttribute(Key_Orientation, new Attribute<Vector2D>(Vector2D.Empty));
            entity.AddAttribute(Key_Speed, new Attribute<float>(speed));
            entity.AddAttribute(Key_IsMoving, new Attribute<bool>(false));
        }

        #region IBehaviour Members

        List<Type> supportedMessages = new List<Type>() {
            typeof(MoveMessage)
        };
        public override ReadOnlyCollection<Type> SupportedMessages
        {
            get { return supportedMessages.AsReadOnly(); }
        }

        public override void OnUpdate(float deltaTime)
        {
            bool isMoving = entity[Key_IsMoving] as Attribute<bool>;

            if (isMoving) {
                Attribute<float> speed = entity[Key_Speed] as Attribute<float>;
                Attribute<Vector2D> position = entity[Key_Position] as Attribute<Vector2D>;
                Attribute<Vector2D> direction = entity[Key_Orientation] as Attribute<Vector2D>;

                position.Value.X += direction.Value.X * speed * deltaTime;
                position.Value.Y += direction.Value.Y * speed * deltaTime;
                checkBounds(position);

                Console.WriteLine(entity.Name + " moved " + direction.ToString() + " to ("
                    + position.Value.X + "/" + position.Value.Y + ")");
            }
        }

        public override void OnMessage(Message msg)
        {
            switch (msg.Type) {
                case MoveMessage.START_MOVING: {
                    MoveMessage moveMsg = (MoveMessage)msg;
                    setMovement(true, moveMsg.Direction);
                    break;
                }
                case MoveMessage.STOP_MOVING: {
                    MoveMessage moveMsg = (MoveMessage)msg;
                    setMovement(false, moveMsg.Direction);
                    break;
                }
            }
        }

        #endregion

        private void checkBounds(Attribute<Vector2D> position)
        {
            Attribute<Rectangle> bounds = entity[Key_Bounds] as Attribute<Rectangle>;

            if (bounds.Value.Left <= 0) {
                position.Value.X = 0;
                setMovement(false, Direction.West);
            }
            else if (bounds.Value.Right > entity.Game.WorldWidth) {
                position.Value.X = entity.Game.WorldWidth - bounds.Value.Width;
                setMovement(false, Direction.East);
            }

            if (bounds.Value.Top <= 0) {
                position.Value.Y = 0;
                setMovement(false, Direction.North);
            }
            else if (bounds.Value.Bottom >= entity.Game.WorldHeight) {
                position.Value.Y = entity.Game.WorldHeight - bounds.Value.Height;
                setMovement(false, Direction.South);
            }
        }

        private void setMovement(bool isMoving, Direction direction)
        {
            Attribute<Vector2D> entityDirection = entity[Key_Orientation] as Attribute<Vector2D>;
            switch (direction) {
                case Direction.North:
                    entityDirection.Value.Y = isMoving ? -1f : 0f;
                    break;
                case Direction.West:
                    entityDirection.Value.X = isMoving ? -1f : 0f;
                    break;
                case Direction.South:
                    entityDirection.Value.Y = isMoving ? 1f : 0f;
                    break;
                case Direction.East:
                    entityDirection.Value.X = isMoving ? 1f : 0f;
                    break;
            }

            Attribute<bool> entityIsMoving = entity[Key_IsMoving] as Attribute<bool>;
            if (entityDirection.Value.X != 0f || entityDirection.Value.Y != 0f) {
                entityIsMoving.Value = true;
            }
            else {
                entityIsMoving.Value = false;
            }
        }
    }

}
