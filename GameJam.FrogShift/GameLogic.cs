﻿using Engineer.Engine;
using Engineer.Runner;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameJam.FrogShift
{
    public class GameLogic
    {
        private Runner _Runner;
        private Game _CGame;
        private Scene _CScene;
        private ResourceManager _ResMan;
        public Runner Runner { get => _Runner; set => _Runner = value; }
        public Game CGame { get => _CGame; set => _CGame = value; }
        public Scene CScene { get => _CScene; set => _CScene = value; }
        public GameLogic()
        {
            _ResMan = new ResourceManager();
            _ResMan.Init();
        }
        public void Init(Runner NewRunner, Game NewGame, Scene CurrentScene)
        {
            this._Runner = NewRunner;
            this._CGame = NewGame;
            this._CScene = CurrentScene;
        }
    }
}
