﻿using System;
using System.Collections.Generic;
using Game;
using Game.Entities;
using Game.EventManagement;
using Game.EventManagement.Events;
using Game.Utility;
using Game.Behaviors;

namespace SpaceInvaders.Views
{
    class AiView : IGameView
    {
        public GameLogic Game { get; private set; }
        public IEventManager EventManager { get; private set; }

        public GameViewType Type { get { return GameViewType.PlayerView; } }
        public int ID
        {
            get { throw new NotImplementedException(); }
        }

        private List<Entity> invaders = new List<Entity>();
        private Vector2D currentDirection;

        private float moveDownTime;
        private const float totalMoveDownTime = 0.5f;

        private bool movementDirectionChanged = false;

        private float timeSinceLastShot = 0.0f;
        private float firingThreshold;
        private Random rng = new Random();


        private Entity mysteryShip;
        private readonly int minMysteryShipSpawnTime = 2;
        private readonly int maxMysteryShipSpawnTime = 15;
        private float timeSinceLastMysteryShipSpawn;
        private float timeToNextMysteryShipSpawn;


        public AiView(GameLogic game)
        {
            this.Game = game;
            this.EventManager = game.EventManager;

            this.firingThreshold = 0.5f + ((float)rng.NextDouble() * 1.5f);

            this.timeToNextMysteryShipSpawn = minMysteryShipSpawnTime + rng.Next(maxMysteryShipSpawnTime);

            registerGameEventListeners();
        }

        private void registerGameEventListeners()
        {
            EventManager.AddListener(this, typeof(NewEntityEvent));
            EventManager.AddListener(this, typeof(DestroyEntityEvent));
            EventManager.AddListener(this, typeof(GameStateChangedEvent));
            EventManager.AddListener(this, typeof(AiUpdateMovementEvent));
        }

        public void OnUpdate(float deltaTime)
        {
            if (Game.IsPaused || invaders.Count < 1) 
            {
                return;
            }

            handleShooting(deltaTime);
            handleMovement(deltaTime);
            handleMysteryShip(deltaTime);
        }

        private void handleShooting(float deltaTime)
        {
            timeSinceLastShot += deltaTime;
            Entity shooter = invaders[rng.Next(invaders.Count - 1)];
            float firingSpeed = shooter[CombatBehavior.Key_FiringSpeed];

            if (timeSinceLastShot * rng.NextDouble() + (1 - firingSpeed) >= firingThreshold) 
            {
                timeSinceLastShot = 0.0f;
                firingThreshold = 0.5f + ((float)rng.NextDouble() * 1f);
                EventManager.Trigger(FireWeaponEvent.SingleShot(shooter.ID));
            }
        }

        private void handleMovement(float deltaTime)
        {
            if (currentDirection.Y < 0) 
            {
                if (moveDownTime >= totalMoveDownTime) 
                {
                    currentDirection.Y = 0.0f;
                    moveDownTime = 0.0f;
                    movementDirectionChanged = true;
                }

                moveDownTime += deltaTime;
            }

            if (!movementDirectionChanged) 
            {
                return;
            }

            foreach (Entity invader in invaders) {
                var move = MoveEvent.Start(invader.ID, currentDirection);
                EventManager.QueueEvent(move);
            }

            movementDirectionChanged = false;
        }

        private void handleMysteryShip(float deltaTime)
        {
            if (mysteryShip != null)
            {
                return;
            }

            timeSinceLastMysteryShipSpawn += deltaTime;
            if (timeSinceLastMysteryShipSpawn >= timeToNextMysteryShipSpawn) 
            {
                var createEntity = CreateEntityEvent.New("mystery_ship");

                bool isSpawningOnTheRight = rng.Next(2) == 1;

                if (isSpawningOnTheRight) 
                {
                    Vector2D position = new Vector2D(Game.World.Width, Game.World.Height - 100);
                    createEntity.AddAttribute(SpatialBehavior.Key_Position, position);
                    Vector2D direction = new Vector2D(-1, 0);
                    createEntity.AddAttribute(SpatialBehavior.Key_MoveDirection, direction);
                }
                else 
                {
                    Vector2D position = new Vector2D(-70, Game.World.Height - 100);
                    createEntity.AddAttribute(SpatialBehavior.Key_Position, position);
                    Vector2D direction = new Vector2D(1, 0);
                    createEntity.AddAttribute(SpatialBehavior.Key_MoveDirection, direction);
                }
                

                EventManager.QueueEvent(createEntity);

                timeSinceLastMysteryShipSpawn = 0.0f;
                timeToNextMysteryShipSpawn = minMysteryShipSpawnTime + rng.Next(maxMysteryShipSpawnTime);
            }
        }

        public void OnEvent(Event evt)
        {
            switch (evt.Type) {
                case NewEntityEvent.NEW_ENTITY: {
                    var newEntityEvent = (NewEntityEvent)evt;
                    Entity entity = Game.World.Entities[newEntityEvent.EntityID];
                    if (entity.Type.StartsWith("alien_")) 
                    {
                        OnAttach(entity);
                    }
                    else if (entity.Type.Equals("mystery_ship")) 
                    {
                        mysteryShip = entity;
                    }
                    break;
                }
                case DestroyEntityEvent.DESTROY_ENTITY: {
                    var destroyEntityEvent = (DestroyEntityEvent)evt;
                    Entity entity = Game.World.Entities[destroyEntityEvent.EntityID];
                    if (entity.Type.StartsWith("alien_")) 
                    {
                        OnDetach(entity);
                    }
                    else if (entity.Type.Equals("mystery_ship")) 
                    {
                        mysteryShip = null;

                    }
                    break;
                }
                case GameStateChangedEvent.GAME_STATE_CHANGED: {
                    var stateChangedEvent = (GameStateChangedEvent)evt;
                    onGameStateChanged(stateChangedEvent.NewState);
                    break;
                    }
                case AiUpdateMovementEvent.AT_BORDER: {
                    var aiMovementUpdateEvent = (AiUpdateMovementEvent)evt;

                    Vector2D borderData = aiMovementUpdateEvent.BorderData;

                    if (borderData.Y < 0) 
                    {
                        // Victory for the Invaders!
                        EventManager.Trigger(GameStateChangedEvent.To(GameState.GameOver));
                    }

                    currentDirection.X = -borderData.X;
                    currentDirection.Y = -1;

                    movementDirectionChanged = true;
                    break;
                }
            }
        }

        public void OnAttach(Entity entity)
        {
            invaders.Add(entity);
        }

        public void OnDetach(Entity entity)
        {
            if (invaders.Contains(entity)) {
                invaders.Remove(entity);
            }
        }

        private void onGameStateChanged(GameState newState)
        {
            switch (newState) {               
                case GameState.Loading:
                    currentDirection = new Vector2D(1, 0);
                    movementDirectionChanged = true;
                    invaders.Clear();
                    break;              
                default:
                    break;
            }
        }

    }
}
