﻿using Engineer.Engine;
using Engineer.Engine.Physics;
using Engineer.Mathematics;
using Engineer.Runner;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace GameJam.FrogShift
{
    public class GameLogic
    {
        public static bool updateFlag = false;
        public static int DiffTime = 0;
        public static bool Up = true;
        public static bool GameOver = false;
        public static bool Switch = false;
        public static bool SplashFlag = false;
        public static bool Predator = false;
        public static bool PredatorDone = false;
        private DrawnSceneObject PredatorObject = null;
        private DrawnSceneObject splash;
        private DrawnSceneObject GameOverLabel;
        private DrawnSceneObject Close;
        private HighScore hScore;
        private GameTimer gtimer;
        private CameraMove Camera;
        private int counter = 0;
        private int counter1 = 0;
        public static float _GlobalScale;
        private DrawnSceneObject _Player;
        private Runner _Runner;
        private Game _CGame;
        private Scene _CScene;
        private ResourceManager _ResMan;
        private Movement _Movement;
        private List<DrawnSceneObject> _Colliders = new List<DrawnSceneObject>();
        public Runner Runner
        {
            get => _Runner; set => _Runner = value;
        }
        public Game CGame
        {
            get => _CGame; set => _CGame = value;
        }
        public Scene CScene
        {
            get => _CScene; set => _CScene = value;
        }
        public GameLogic()
        {
            _ResMan = ResourceManager.RM;
            DiffTime = 0;
            Up = true;
            GameOver = false;
            Switch = false;
            SplashFlag = false;
            Predator = false;
            PredatorDone = false;
        }
        public void Init(Runner NewRunner, Game NewGame, Scene CurrentScene)
        {
            GameLogic._GlobalScale = NewRunner.Height / 1080.0f;
            this._Runner = NewRunner;
            this._CGame = NewGame;
            this._CScene = CurrentScene;
            CreateFloor();
            CreateCharacter();
            this.gtimer = new GameTimer(_CScene, Runner);
            this._Movement = new Movement(_Runner, _Player, _Colliders, gtimer);
            CScene.Events.Extern.TimerTick += new GameEventHandler(GameUpdateEvent);
            CScene.Events.Extern.KeyDown += new GameEventHandler(_Movement.KeyDownEvent);
            CScene.Events.Extern.KeyUp += new GameEventHandler(_Movement.KeyUpEvent);
            CScene.Events.Extern.KeyPress += new GameEventHandler(_Movement.KeyPressEvent);
            this.hScore = new HighScore(_CScene, Runner);
            Camera = new CameraMove(_CScene);
        }
        private void CreateCharacter()
        {
            DrawnSceneObject Char = Character.Create((Scene2D)CScene);
            _Player = Char;
            _Player.Data["Direction"] = 0;
            _Player.Data["Collision"] = true;
            _Player.Data["skokBrojac"] = 0;
            _Player.Data["padBrojac"] = 0;
            _Player.Data["colliding"] = true;
            _Player.Data["flying"] = false;
        }
        private void CreateFloor()
        {
            _CScene.Data["Colliders"] = this._Colliders;
            Level.Create((Scene2D)_CScene, (ExternRunner)Runner);
        }
        public void CloseGameEvent(Game G, EventArguments E)
        {
            CScene.Events.Extern.TimerTick -= new GameEventHandler(GameUpdateEvent);
            CScene.Events.Extern.KeyDown -= new GameEventHandler(_Movement.KeyDownEvent);
            CScene.Events.Extern.KeyUp -= new GameEventHandler(_Movement.KeyUpEvent);
            CScene.Events.Extern.KeyPress -= new GameEventHandler(_Movement.KeyPressEvent);
            CScene.Objects.Clear();
            _Runner.Init(G, G.Scenes[0]);
        }
        public void GameUpdateEvent(Game G, EventArguments E)
        {
            if (!updateFlag)
            {
                updateFlag = true;
                if (SplashFlag)
                {
                    AudioPlayer.PlaySplash();
                    SplashFlag = false;
                    splash = Splash.MakeSplash(_CScene, _Player);
                }
                if (splash != null)
                {
                    splash.Data["Life"] = (int)splash.Data["Life"] - 1;
                    if ((int)splash.Data["Life"] == 0)
                    {
                        splash.Visual.Active = false;
                        splash = null;
                        _CScene.Data["Splash"] = null;
                    }
                }

                if (counter1 >= 1) { Camera.MoveCamera(this._CScene, this._Runner); counter1 = 0; }
                counter1++;
                Character.UpdateLegs(_Player);

                ((DrawnSceneObject)CScene.Data["JumpIn"]).Visual.Active = !GameOver && Up && Switch;
                ((DrawnSceneObject)CScene.Data["JumpUp"]).Visual.Active = !GameOver && (!Up) && Switch;

                _Movement.CheckCollision();
                _Movement.CheckWaterLevel((Scene2D)_CScene, Runner);

                if (GameLogic.GameOver)
                {
                    if (GameOverLabel == null)
                    {
                        GameOverLabel = GameLogic.CreateStaticSprite("GameOverLabel", global::GameJam.FrogShift.Properties.Resources.gameover, new Vertex(400, 200, 0), new Vertex(1200, 700, 0));
                        if (!Up) GameOverLabel.Visual.Translation = new Vertex(400 * _GlobalScale, 750 * _GlobalScale, 0);
                        CScene.Data["GameOverLabel"] = GameOverLabel;
                        CScene.AddSceneObject(GameOverLabel);

                        Close = GameLogic.CreateStaticSprite("Close", global::GameJam.FrogShift.Properties.Resources.kvit, new Engineer.Mathematics.Vertex(1100, 840, 0), new Engineer.Mathematics.Vertex(300, 120, 0), false);
                        if (!Up) Close.Visual.Translation = new Vertex(1100 * _GlobalScale, 1390 * _GlobalScale, 0);
                        Close.Events.Extern.MouseClick += new GameEventHandler(this.CloseGameEvent);
                        CScene.AddSceneObject(Close);
                    }
                    else
                    {
                        try
                        {
                            if (Up)
                            {
                                GameOverLabel.Visual.Translation = new Vertex(400 * _GlobalScale, 200 * _GlobalScale, 0);
                                Close.Visual.Translation = new Vertex(1100 * _GlobalScale, 840 * _GlobalScale, 0);
                            }
                            else
                            {
                                GameOverLabel.Visual.Translation = new Vertex(400 * _GlobalScale, 750 * _GlobalScale, 0);
                                Close.Visual.Translation = new Vertex(1100 * _GlobalScale, 1390 * _GlobalScale, 0);
                            }
                        }
                        catch { }
                    }

                    if (!PredatorDone)
                    {
                        if (Predator)
                        {
                            if (!Up)
                            {
                                this.PredatorObject.Visual.Translation = new Vertex(this.PredatorObject.Visual.Translation.X + 20, _Player.Visual.Translation.Y - (250 * _GlobalScale), 0);
                                if (_Player.Visual.Translation.X - (this.PredatorObject.Visual.Translation.X + this.PredatorObject.Visual.Scale.X) < -50)
                                {
                                    ((Sprite)this.PredatorObject.Visual).UpdateSpriteSet(2);
                                    _Player.Visual.Active = false;
                                    ((DrawnSceneObject)_Player.Data["LL"]).Visual.Active = false;
                                    ((DrawnSceneObject)_Player.Data["RL"]).Visual.Active = false;
                                }
                                else if (_Player.Visual.Translation.X - (this.PredatorObject.Visual.Translation.X + this.PredatorObject.Visual.Scale.X) < 50)
                                {
                                    ((Sprite)this.PredatorObject.Visual).UpdateSpriteSet(1);
                                }
                            }
                            else
                            {
                                this.PredatorObject.Visual.Translation = new Vertex(this.PredatorObject.Visual.Translation.X - 20, _Player.Visual.Translation.Y - (600 * _GlobalScale), 0);
                                if ((this.PredatorObject.Visual.Translation.X) - _Player.Visual.Translation.X < -250)
                                {
                                    ((Sprite)this.PredatorObject.Visual).UpdateSpriteSet(2);
                                    _Player.Visual.Active = false;
                                    ((DrawnSceneObject)_Player.Data["LL"]).Visual.Active = false;
                                    ((DrawnSceneObject)_Player.Data["RL"]).Visual.Active = false;
                                }
                                else if ((this.PredatorObject.Visual.Translation.X) - _Player.Visual.Translation.X < -150)
                                {
                                    ((Sprite)this.PredatorObject.Visual).UpdateSpriteSet(1);
                                }
                            }
                        }
                        else
                        {
                            try
                            {
                                if (!Up)
                                {
                                    AudioPlayer.PlaySnake();
                                    Predators.CreateSnake();
                                    this.PredatorObject = Predators.Snake;
                                    this.PredatorObject.Visual.Translation = new Vertex(-2000 * _GlobalScale, _Player.Visual.Translation.Y, 0);
                                    _CScene.AddSceneObject(this.PredatorObject);
                                    Predator = true;
                                }
                                else
                                {
                                    AudioPlayer.PlayStork();
                                    Predators.CreateStork();
                                    this.PredatorObject = Predators.Stork;
                                    this.PredatorObject.Visual.Translation = new Vertex(2000 * _GlobalScale, _Player.Visual.Translation.Y, 0);
                                    _CScene.AddSceneObject(this.PredatorObject);
                                    Predator = true;
                                }
                            }
                            catch { }
                        }
                    }

                    _Movement._ADown = false;
                    _Movement._DDown = false;
                    updateFlag = false;
                    return;
                }

                if (counter++ >= 120) { gtimer.DecTime(); counter = 0; DiffTime++; }
                hScore.updateHighscore();
                if (_Player.Visual.Translation.X + _Player.Visual.Scale.X / 2 < 0)
                {
                    GameLogic.GameOver = true;
                }

                SeqGen SG = new SeqGen();
                SG.CheckEnd(CScene);
                updateFlag = false;
            }
        }
        public static DrawnSceneObject CreateStaticSprite(string Name, Bitmap Image, Vertex Positon, Vertex Size, bool ApplyGlobalScale = true)
        {
            if (ApplyGlobalScale)
            {
                Positon = new Vertex(Positon.X * GameLogic._GlobalScale, Positon.Y * GameLogic._GlobalScale, 0);
                Size = new Vertex(Size.X * GameLogic._GlobalScale, Size.Y * GameLogic._GlobalScale, 0);
            }
            SpriteSet StaticSet = new SpriteSet("Static", Image);
            Sprite StaticSprite = new Sprite();
            StaticSprite.SpriteSets.Add(StaticSet);
            StaticSprite.Translation = Positon;
            StaticSprite.Scale = Size;
            DrawnSceneObject Static = new DrawnSceneObject(Name, StaticSprite);
            return Static;
        }

    }
}