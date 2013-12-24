﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using Audio;
using Game;
using Game.Entities;
using Game.EventManagement;
using Game.EventManagement.Events;
using Graphics;
using Graphics.Loaders;
using Graphics.Loaders.Material;
using Graphics.Loaders.Mesh;
using ResourceManagement.Loaders;
using SpaceInvaders.Controls;
using SpaceInvaders.Input;
using Audio.Loaders;
using Game.Behaviors;
using System.Diagnostics;
using Audio.Resources;

namespace SpaceInvaders.Views
{

	class PlayerView : IGameView, IDisposable
	{
		public GameLogic Game { get; private set; }
		public IEventManager EventManager { get; private set; }

		public Renderer Renderer { get; private set; }
		private Extractor extractor;
		public Form RenderForm { get; private set; }

		private IAudioPlayer audioPlayer;

		private GameMainMenu mainMenuControl;
		private HighscoreScreen highscoreControl;
		private PauseScreen pauseControl;
		private VictoryScreen victoryControl;
		private DefeatScreen gameOverControl;
		private Hud hud;

		public GameViewType Type { get { return GameViewType.PlayerView; } }
		public int ID
		{
			get { throw new NotImplementedException(); }
		}

		private Entity playerEntity;

		private GameController gameController;
		private PlayerController playerController;

		private List<IResourceLoader> rendererLoaders = new List<IResourceLoader>();
		private SoundEffectLoader audioLoader;
		private SoundStreamLoader streamLoader;

		private float timeSinceLastDebugUpdate;
		private int[] numOfGcCollectedObjects;

		public PlayerView(GameLogic game)
		{
			this.Game = game;
			this.EventManager = game.EventManager;

			/**
			 * Initialize Graphics Subsystem 
			 **/
			RenderForm = new Form();
			RenderForm.ClientSize = new Size(Game.World.Width, Game.World.Height);
			RenderForm.Text = "Alien Invasion v1.0";
			RenderForm.BackColor = Color.Empty;
			RenderForm.KeyPreview = true;
			RenderForm.FormBorderStyle = FormBorderStyle.FixedSingle;   // Disable resizing of window
			RenderForm.MaximizeBox = false;                             // Disable maximizing
			RenderForm.Leave += (s, e) => {
				this.RenderForm.Focus();    // When RenderForm loses focus player input won't be processed correctly
			};            

			extractor = new Extractor(game);
			Renderer = new Graphics.Renderer(RenderForm, extractor);
			Renderer.StartRender();
			
			rendererLoaders.Add(new TextureLoader(Renderer));
			rendererLoaders.Add(new MeshLoader(Renderer));
			rendererLoaders.Add(new EffectLoader(Renderer));
			
			foreach (var rendererLoader in rendererLoaders)
			{
				game.ResourceManager.AddLoader(rendererLoader);
			}            

			game.ResourceManager.AddLoader(new MaterialLoader(game.ResourceManager));

			/**
			* Initialize Input Subsystem 
			**/
			gameController = new GameController(EventManager);
			RenderForm.KeyDown += new KeyEventHandler(gameController.OnKeyDown);
			RenderForm.KeyUp += new KeyEventHandler(gameController.OnKeyUp);
			
			/**
			* Initialize Audio Subsystem 
			**/
			audioPlayer = new DefaultAudioPlayer();
			audioPlayer.Start();

			audioLoader = new SoundEffectLoader(audioPlayer);
			game.ResourceManager.AddLoader(audioLoader);

			streamLoader = new SoundStreamLoader(audioPlayer);
			game.ResourceManager.AddLoader(streamLoader);

			/**
			* Initialize GUI 
			**/
			mainMenuControl = new GameMainMenu(EventManager);
			mainMenuControl.Location = new Point(
				(RenderForm.ClientSize.Width - mainMenuControl.Width) / 2,
				(RenderForm.ClientSize.Height - mainMenuControl.Height) / 2);
			RenderForm.Controls.Add(mainMenuControl);

			highscoreControl = new HighscoreScreen(EventManager);
			highscoreControl.Location = new Point(
				(RenderForm.ClientSize.Width - highscoreControl.Width) / 2,
				(RenderForm.ClientSize.Height - highscoreControl.Height) / 2);
			RenderForm.Controls.Add(highscoreControl);

			pauseControl = new PauseScreen();
			pauseControl.Location = new Point(
				(RenderForm.ClientSize.Width - pauseControl.Width) / 2,
				(RenderForm.ClientSize.Height / 2) - pauseControl.Height);
			RenderForm.Controls.Add(pauseControl);

			victoryControl = new VictoryScreen(EventManager);
			victoryControl.Location = new Point(
				(RenderForm.ClientSize.Width - victoryControl.Width) / 2,
				(RenderForm.ClientSize.Height / 2) - (victoryControl.Height / 2));
			RenderForm.Controls.Add(victoryControl);

			gameOverControl = new DefeatScreen(EventManager);
			gameOverControl.Location = new Point(
				(RenderForm.ClientSize.Width - gameOverControl.Width) / 2,
				(RenderForm.ClientSize.Height / 2) - (gameOverControl.Height / 2));
			RenderForm.Controls.Add(gameOverControl);

			hud = new Hud(EventManager);
			hud.Location = new Point(RenderForm.ClientSize.Width - hud.Width, 0);
			RenderForm.Controls.Add(hud);


			numOfGcCollectedObjects = new int[GC.MaxGeneration];


			registerGameEventListeners();
		}

		private void registerGameEventListeners()
		{
			EventManager.AddListener(this, typeof(AudioEvent));
			EventManager.AddListener(this, typeof(NewEntityEvent));
			EventManager.AddListener(this, typeof(DestroyEntityEvent));
			EventManager.AddListener(this, typeof(GameStateChangedEvent));
		}

		public void OnUpdate(float deltaTime)
		{
			updateDebugOutput(deltaTime);

			extractor.OnUpdate(deltaTime);
		}

		[Conditional("DEBUG")]
		private void updateDebugOutput(float deltaTime)
		{
			timeSinceLastDebugUpdate += deltaTime;
			if (timeSinceLastDebugUpdate < 0.1f)
			{
				return;
			}
			timeSinceLastDebugUpdate = 0.0f;

			Renderer.DebugOutput["Cycle Time"] = string.Format("{0,4:0} µs", deltaTime * 1000000.0f);
			for (int i = 0; i < GC.MaxGeneration; ++i)
			{
				int diff = GC.CollectionCount(i) - numOfGcCollectedObjects[i];
				Renderer.DebugOutput["#Gen" + i] = String.Format("{0,2} (total: {1})", diff, GC.CollectionCount(i));
				numOfGcCollectedObjects[i] = GC.CollectionCount(i);
			}
			Renderer.DebugOutput["Memory"] = (GC.GetTotalMemory(false) / (1024 * 1024)) + " MiB";
		}

		public void OnAttach(Entity entity)
		{
			this.playerEntity = entity;

			playerController = new PlayerController(playerEntity);
			RenderForm.KeyDown += new KeyEventHandler(playerController.OnKeyDown);
			RenderForm.KeyUp += new KeyEventHandler(playerController.OnKeyUp);

			Console.WriteLine("[" + this.GetType().Name + "] New " + playerEntity + " found, attaching to controller");
		}

		public void OnDetach()
		{
			if (playerEntity == null) {
				return;
			}

			Console.WriteLine("[" + this.GetType().Name + "] Detaching " + playerEntity + " from controller");

			RenderForm.KeyUp -= new KeyEventHandler(playerController.OnKeyUp);
			RenderForm.KeyDown -= new KeyEventHandler(playerController.OnKeyDown);
			playerController = null;

			this.playerEntity = null;
		}

		public void OnEvent(Event evt)
		{
			switch (evt.Type) {
				case AudioEvent.PLAY_SOUND: {
					AudioEvent audioEvent = (AudioEvent)evt;
					if (audioEvent.Loop)
					{
						using (SoundResource res = (SoundResource)audioEvent.SoundResource.Acquire())
						{
							audioPlayer.CreateLoopingSound(SoundGroup.InGameEffect, res.Sound, false, audioEvent.Volume);
						}
					}
					else
					{
						using (SoundResource res = (SoundResource)audioEvent.SoundResource.Acquire())
						{
							audioPlayer.Play(res.Sound, audioEvent.Volume);
						}
					}
					break;
				}
				case NewEntityEvent.NEW_ENTITY: {
						NewEntityEvent newEntityEvent = (NewEntityEvent)evt;
						Entity entity = Game.World.Entities[newEntityEvent.EntityID];
						if (entity.Type == "player") {
							OnAttach(entity);

							int lifes = playerEntity[HealthBehavior.Key_Lifes];
							hud.Reset(lifes);
						}
						break;
					}
				case DestroyEntityEvent.DESTROY_ENTITY: {
						DestroyEntityEvent destroyEntityEvent = (DestroyEntityEvent)evt;
						Entity entity = Game.World.Entities[destroyEntityEvent.EntityID];
						if (entity.Type == "player") {
							OnDetach();
						}
						else if (entity.Type == "mystery_ship")
						{
							audioPlayer.StopLoopingSounds(SoundGroup.InGameEffect);
						}
						break;
					}
				case GameStateChangedEvent.GAME_STATE_CHANGED: {
						GameStateChangedEvent stateChangedEvent = (GameStateChangedEvent)evt;
						onGameStateChanged(stateChangedEvent.NewState);
						break;
					}
			}
		}

		private void onGameStateChanged(GameState newState)
		{
			switch (newState) {
				case GameState.StartUp:
					mainMenuControl.Hide();
					highscoreControl.Hide();
					pauseControl.Hide();
					victoryControl.Hide();
					gameOverControl.Hide();
					hud.Hide();
					
					audioPlayer.CreateLoopingSound(SoundGroup.Menu, @"data\audio\menu.ogg", true, 0.8f);
					break;
				case GameState.Menu:
					mainMenuControl.Show();
					highscoreControl.Hide();
					pauseControl.Hide();
					victoryControl.Hide();
					gameOverControl.Hide();
					hud.Hide();

					audioPlayer.UnpauseLoopingSounds(SoundGroup.Menu);
					audioPlayer.PauseLoopingSounds(SoundGroup.InGameEffect);
					audioPlayer.PauseLoopingSounds(SoundGroup.InGameMusic);
					break;
				case GameState.Highscore:
					mainMenuControl.Hide();
					highscoreControl.Show();
					pauseControl.Hide();
					victoryControl.Hide();
					gameOverControl.Hide();
					hud.Hide();
					
					audioPlayer.UnpauseLoopingSounds(SoundGroup.Menu);
					audioPlayer.PauseLoopingSounds(SoundGroup.InGameEffect);
					audioPlayer.PauseLoopingSounds(SoundGroup.InGameMusic);
					break;
				case GameState.Loading:
					mainMenuControl.Hide();
					highscoreControl.Hide();
					pauseControl.Hide();
					victoryControl.Hide();
					gameOverControl.Hide();;
					hud.Hide();

					OnDetach();

					audioPlayer.StopLoopingSounds(SoundGroup.InGameMusic);
					audioPlayer.CreateLoopingSound(SoundGroup.InGameMusic, @"data\audio\ingame.ogg", true, 0.8f);
					break;
				case GameState.Running:
					mainMenuControl.Hide();
					highscoreControl.Hide();
					pauseControl.Hide();
					victoryControl.Hide();
					gameOverControl.Hide();
					hud.Show();

					audioPlayer.PauseLoopingSounds(SoundGroup.Menu);
					audioPlayer.UnpauseLoopingSounds(SoundGroup.InGameEffect);
					audioPlayer.UnpauseLoopingSounds(SoundGroup.InGameMusic);
					break;
				case GameState.Paused:
					pauseControl.Show();
					victoryControl.Hide();
					gameOverControl.Hide();
					hud.Hide();

					audioPlayer.PauseLoopingSounds(SoundGroup.InGameEffect);
					audioPlayer.PauseLoopingSounds(SoundGroup.InGameMusic);
					break;
				case GameState.Victory:
					int remainingLifes = playerEntity[HealthBehavior.Key_Lifes];
					int totalScore = 100 * remainingLifes + playerEntity[CollectsPointsBehavior.Key_PointsCollected];
					victoryControl.CurrentScore = totalScore;

					mainMenuControl.Hide();
					highscoreControl.Hide();
					pauseControl.Hide();
					victoryControl.Show();
					gameOverControl.Hide();
					hud.Hide();

					audioPlayer.StopLoopingSounds(SoundGroup.InGameEffect);
					audioPlayer.StopLoopingSounds(SoundGroup.InGameMusic);
					break;
				case GameState.GameOver:
					mainMenuControl.Hide();
					highscoreControl.Hide();
					pauseControl.Hide();
					victoryControl.Hide();
					gameOverControl.Show();
					hud.Hide();

					audioPlayer.StopLoopingSounds(SoundGroup.InGameEffect);
					audioPlayer.StopLoopingSounds(SoundGroup.InGameMusic);
					break;
				case GameState.Quit:
					audioPlayer.StopLoopingSounds(SoundGroup.Menu);
					RenderForm.Close();
					break;
			}
		}

		public void Dispose()
		{
			Renderer.StopRender();
			Renderer.WaitForStateChange();

			foreach (var rendererLoader in rendererLoaders) {
				Game.ResourceManager.RemoveLoader(rendererLoader.Type);

				if (rendererLoader is IDisposable) 
				{
					((IDisposable)rendererLoader).Dispose();
				}
			}

			Renderer.StopHandleCommands();
			Renderer.WaitForStateChange();
			Renderer.WaitForCompletion();
			
			Renderer.Dispose();

			Game.ResourceManager.RemoveLoader(audioLoader.Type);
			audioLoader.Dispose();

			audioPlayer.Dispose();
		}
	}

}
