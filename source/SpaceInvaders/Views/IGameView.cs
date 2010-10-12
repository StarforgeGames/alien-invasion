﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Game.Entities;
using Game.EventManagement;

namespace SpaceInvaders.Views
{
    public enum GameViewType
    {
        PlayerView,
        AIView,
        RemotePlayerView
    }

    interface IGameView : IEventListener
    {
        GameViewType Type { get; }
        int ID { get; }

        void OnUpdate(float deltaTime);
        void OnAttach(Entity entity);
    }

}
