﻿using System.Collections.Generic;
using Game;
using Game.EventManagement.Events;
using Graphics.ResourceManagement;
using Graphics.ResourceManagement.Debug;
using Graphics.ResourceManagement.Loaders;
using Graphics.ResourceManagement.Wipers;
using SlimDX.Windows;
using SpaceInvaders.Views;

namespace SpaceInvaders
{

    /// <summary>
    /// Concrete class for the game.
    /// </summary>
    class Application
    {
        public GameLogic Game { get; private set; }
        public List<IGameView> Views { get; private set; }

        public double LifeTime { get; private set; }

        private GameTimer timer = new GameTimer();
        private ResourceManager resourceManager;
        
        private List<ARendererLoader> rendererLoaders = new List<ARendererLoader>();

        AWiper debugWiper = new DebugWiper();

        public Application()
        {
            Game = new GameLogic(800, 600);

            Views = new List<IGameView>();
            PlayerView playerView = new PlayerView(Game);
            Views.Add(playerView);

            LifeTime = 0d;

            resourceManager = new ResourceManager(new ThreadPoolExecutor());            
            resourceManager.AddLoader(new DummyLoader());
            rendererLoaders.Add(new TextureLoader(playerView.Renderer));
            rendererLoaders.Add(new MeshLoader(playerView.Renderer));

            foreach (var rendererLoader in rendererLoaders)
            {
                resourceManager.AddLoader(rendererLoader);
            }

            resourceManager.AddWiper(debugWiper);

            // some testing code
            resourceManager.GetResource("player", "texture");
            resourceManager.GetResource("quad", "mesh");
            // end of testing code

            Game.ChangeState(GameState.Loading);
        }

        public void Update(float deltaTime)
        {
            LifeTime += deltaTime;
            Game.Update(deltaTime);

            foreach (IGameView view in Views) {
                view.OnUpdate(deltaTime);
            }
        }

        /// <summary>
        /// Run main loop.
        /// </summary>
        public void Run()
        {
            timer.Reset();
            PlayerView playerView = Views.Find(x => x.Type == GameViewType.PlayerView) as PlayerView;

            MessagePump.Run(playerView.RenderForm, () =>
            {
                timer.Tick();
                float deltaTime = timer.DeltaTime;
                Update(deltaTime);
            });

            resourceManager.Dispose();

            foreach (var rendererLoader in rendererLoaders)
            {
                rendererLoader.Dispose();
            }

            playerView.Dispose();
        }
    }

}
