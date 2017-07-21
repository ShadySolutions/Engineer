﻿//#define FixedPipeline
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using Engineer.Mathematics;
using Engineer.Data;
using Engineer.Draw;
using Engineer.Draw.OpenGL;
using Engineer.Draw.OpenGL.FixedGL;
using Engineer.Draw.OpenGL.GLSL;
using Engineer.Engine;
using OpenTK.Input;
using System.ComponentModel;

namespace Engineer.Runner
{
    public enum WindowState
    {
        Normal = 0,
        Minimized = 1,
        Maximized = 2,
        Fullscreen = 3
    }
    public class Runner : OpenTK.GameWindow
    {
        private int _Seed;
        private int _FrameUpdateRate;
        protected bool _GameInit;
        protected bool _EngineInit;
        protected Timer _Time;
        protected Scene _NextScene;
        protected Scene _PrevScene;
        protected Scene _CurrentScene;
        protected Game _CurrentGame;
        protected DrawEngine _Engine;
        public int FrameUpdateRate { get => _FrameUpdateRate; set => _FrameUpdateRate = value; }
        public Runner(int Width, int Height, GraphicsMode Mode, string Title) : base(Width, Height, Mode, Title)
        {
            this._Seed = 0;
            this._FrameUpdateRate = 6;
            this._GameInit = false;
            this._EngineInit = false;
            this._Time = new Timer(8.33);
            this._Time.Elapsed += Event_TimerTick;
            this._Time.AutoReset = true;
        }
        public void SetWindowState(WindowState State)
        {
            this.WindowState = (OpenTK.WindowState)State;
        }
        private void EngineInit()
        {
            this._EngineInit = true;
            _Engine = new DrawEngine();
            GLSLShaderRenderer Render = new GLSLShaderRenderer();
            Render.RenderDestination = this;
            Render.TargetType = RenderTargetType.Runner;
            _Engine.CurrentRenderer = Render;
            GLSLShaderMaterialTranslator Translator = new GLSLShaderMaterialTranslator();
            _Engine.CurrentTranslator = Translator;
            _Engine.SetDefaults();
        }
        public void Init(Game CurrentGame, Scene CurrentScene)
        {
            if (!_EngineInit) EngineInit();
            this._Time.Stop();
            this._GameInit = true;
            this._CurrentGame = CurrentGame;
            this._CurrentScene = CurrentScene;
            this.Closing += new EventHandler<System.ComponentModel.CancelEventArgs>(Event_Closing);
            this.KeyDown += new EventHandler<KeyboardKeyEventArgs>(Event_KeyPress);
            this.KeyDown += new EventHandler<KeyboardKeyEventArgs>(Event_KeyDown);
            this.KeyUp += new EventHandler<KeyboardKeyEventArgs>(Event_KeyUp);
            this.MouseDown += new EventHandler<MouseButtonEventArgs>(Event_MouseClick);
            this.MouseDown += new EventHandler<MouseButtonEventArgs>(Event_MouseDown);
            this.MouseUp += new EventHandler<MouseButtonEventArgs>(Event_MouseUp);
            this.MouseMove += new EventHandler<MouseMoveEventArgs>(Event_MouseMove);
            this.MouseWheel += new EventHandler<MouseWheelEventArgs>(Event_MouseWheel);
            PrepareEvents();
            this._Time.Start();
            Event_Load();
        }
        public void SwitchScene(Scene NextScene)
        {
            BackgroundWorker Worker = new BackgroundWorker();
            Worker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(this.SwitchSceneFinishPreload);
            Worker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(this.Event_OperationFinished);
            Worker.ProgressChanged += new ProgressChangedEventHandler(this.Event_OperationProgress);
            this._NextScene = NextScene;
            if (NextScene.Type == SceneType.Scene2D) this._Engine.Preload2DScene((Scene2D)NextScene, Worker);
        }
        public void SwitchScene(string SceneName)
        {
            for(int i = 0; i < this._CurrentGame.Scenes.Count; i++)
            {
                if(this._CurrentGame.Scenes[i].Name == SceneName)
                {
                    this.SwitchScene(this._CurrentGame.Scenes[i]);
                }
            }
        }
        private void SwitchSceneFinishPreload(object sender, RunWorkerCompletedEventArgs e)
        {
            this.Init(this._CurrentGame, this._NextScene);
            if(this._PrevScene.Type == SceneType.Scene2D) this._Engine.Destroy2DScene((Scene2D)this._PrevScene, null);
        }
        protected virtual void PrepareEvents()
        {

        }
        protected override void OnResize(EventArgs e)
        {
            EventArguments Arguments = new EventArguments();
            Arguments.Size = new Vertex(this.Width, this.Height, 0);
            CallEvents("Resize", Arguments);
        }
        protected override void OnRenderFrame(FrameEventArgs e)
        {
            if (!_GameInit || !_EngineInit) return;
            this.MakeCurrent();
            if (_CurrentScene.Type == SceneType.Scene2D)
            {
                _Engine.Draw2DScene((Scene2D)_CurrentScene, this.ClientRectangle.Width, this.ClientRectangle.Height);
            }
            else if (_CurrentScene.Type == SceneType.Scene3D)
            {
                _Engine.Draw3DScene((Scene3D)_CurrentScene, this.ClientRectangle.Width, this.ClientRectangle.Height);
            }
            Event_RenderFrame(this, e);
            SwapBuffers();
        }
        private void Event_Closing(object sender, EventArgs e)
        {
            EventArguments Arguments = new EventArguments();
            CallEvents("Closing", Arguments);
        }
        private void Event_KeyDown(object sender, KeyboardKeyEventArgs e)
        {
            EventArguments Arguments = new EventArguments();
            Arguments.KeyDown = (KeyType)e.Key;
            Arguments.Control = e.Control;
            Arguments.Alt = e.Alt;
            Arguments.Shift = e.Shift;
            CallEvents("KeyDown", Arguments);
        }
        private void Event_KeyUp(object sender, KeyboardKeyEventArgs e)
        {
            EventArguments Arguments = new EventArguments();
            Arguments.KeyDown = (KeyType)e.Key;
            Arguments.Control = e.Control;
            Arguments.Alt = e.Alt;
            Arguments.Shift = e.Shift;
            CallEvents("KeyUp", Arguments);
        }
        private void Event_KeyPress(object sender, KeyboardKeyEventArgs e)
        {
            EventArguments Arguments = new EventArguments();
            Arguments.KeyDown = (KeyType)e.Key;
            Arguments.Control = e.Control;
            Arguments.Alt = e.Alt;
            Arguments.Shift = e.Shift;
            CallEvents("KeyPress", Arguments);
        }
        private void Event_Load()
        {
            EventArguments Arguments = new EventArguments();
            CallEvents("Load", Arguments);
        }
        private void Event_MouseDown(object sender, MouseButtonEventArgs e)
        {
            EventArguments Arguments = new EventArguments();
            Arguments.Location = new Vertex(e.X, e.Y, 0);
            Arguments.ButtonDown = (MouseClickType)e.Button;
            Arguments.Handled = false;
            if (_CurrentScene.Type == SceneType.Scene2D)
            {
                Scene2D Current2DScene = (Scene2D)_CurrentScene;
                Vertex STrans = Current2DScene.Transformation.Translation;
                for (int i = _CurrentScene.Objects.Count - 1; i >= 0; i--)
                {
                    if (_CurrentScene.Objects[i].Type == SceneObjectType.DrawnSceneObject)
                    {
                        DrawnSceneObject Current = (DrawnSceneObject)_CurrentScene.Objects[i];
                        Vertex Trans = Current.Visual.Translation;
                        Vertex Scale = Current.Visual.Scale;
                        if (STrans.X + Trans.X < e.X && e.X < STrans.X + Trans.X + Scale.X &&
                            STrans.Y + Trans.Y < e.Y && e.Y < STrans.Y + Trans.Y + Scale.Y)
                        {
                            Arguments.Target = Current;
                            CallObjectEvents(i, "MouseDown", Arguments);
                            Arguments.Handled = true;
                        }
                    }
                }
            }
            Arguments.Target = null;
            CallEvents("MouseDown", Arguments);
        }
        private void Event_MouseUp(object sender, MouseButtonEventArgs e)
        {
            EventArguments Arguments = new EventArguments();
            Arguments.Location = new Vertex(e.X, e.Y, 0);
            Arguments.ButtonDown = (MouseClickType)e.Button;
            Arguments.Handled = false;
            if (_CurrentScene.Type == SceneType.Scene2D)
            {
                Scene2D Current2DScene = (Scene2D)_CurrentScene;
                Vertex STrans = Current2DScene.Transformation.Translation;
                for (int i = _CurrentScene.Objects.Count - 1; i >= 0; i--)
                {
                    if (_CurrentScene.Objects[i].Type == SceneObjectType.DrawnSceneObject)
                    {
                        DrawnSceneObject Current = (DrawnSceneObject)_CurrentScene.Objects[i];
                        Vertex Trans = Current.Visual.Translation;
                        Vertex Scale = Current.Visual.Scale;
                        if (STrans.X + Trans.X < e.X && e.X < STrans.X + Trans.X + Scale.X &&
                            STrans.Y + Trans.Y < e.Y && e.Y < STrans.Y + Trans.Y + Scale.Y)
                        {
                            Arguments.Target = Current;
                            CallObjectEvents(i, "MouseUp", Arguments);
                            Arguments.Handled = true;
                        }
                    }
                }
            }
            Arguments.Target = null;
            CallEvents("MouseUp", Arguments);
        }
        private void Event_MouseClick(object sender, MouseButtonEventArgs e)
        {
            EventArguments Arguments = new EventArguments();
            Arguments.Location = new Vertex(e.X, e.Y, 0);
            Arguments.ButtonDown = (MouseClickType)e.Button;
            Arguments.Handled = false;
            if (_CurrentScene.Type == SceneType.Scene2D)
            {
                Scene2D Current2DScene = (Scene2D)_CurrentScene;
                Vertex STrans = Current2DScene.Transformation.Translation;
                for (int i = _CurrentScene.Objects.Count - 1; i >= 0; i--)
                {
                    if (_CurrentScene.Objects[i].Type == SceneObjectType.DrawnSceneObject)
                    {
                        DrawnSceneObject Current = (DrawnSceneObject)_CurrentScene.Objects[i];
                        Vertex Trans = Current.Visual.Translation;
                        Vertex Scale = Current.Visual.Scale;
                        if (STrans.X + Trans.X < e.X && e.X < STrans.X + Trans.X + Scale.X &&
                            STrans.Y + Trans.Y < e.Y && e.Y < STrans.Y + Trans.Y + Scale.Y)
                        {
                            Arguments.Target = Current;
                            CallObjectEvents(i, "MouseClick", Arguments);
                            Arguments.Handled = true;
                        }
                    }
                }
            }
            Arguments.Target = null;
            CallEvents("MouseClick", Arguments);
        }
        private void Event_MouseMove(object sender, MouseMoveEventArgs e)
        {
            EventArguments Arguments = new EventArguments();
            Arguments.Location = new Vertex(e.X, e.Y, 0);
            Arguments.Handled = false;
            if (_CurrentScene.Type == SceneType.Scene2D)
            {
                Scene2D Current2DScene = (Scene2D)_CurrentScene;
                Vertex STrans = Current2DScene.Transformation.Translation;
                for (int i = _CurrentScene.Objects.Count - 1; i >= 0; i--)
                {
                    if (_CurrentScene.Objects[i].Type == SceneObjectType.DrawnSceneObject)
                    {
                        DrawnSceneObject Current = (DrawnSceneObject)_CurrentScene.Objects[i];
                        Vertex Trans = Current.Visual.Translation;
                        Vertex Scale = Current.Visual.Scale;
                        if (STrans.X + Trans.X < e.X && e.X < STrans.X + Trans.X + Scale.X &&
                            STrans.Y + Trans.Y < e.Y && e.Y < STrans.Y + Trans.Y + Scale.Y)
                        {
                            Arguments.Target = Current;
                            CallObjectEvents(i, "MouseMove", Arguments);
                            Arguments.Handled = true;
                        }
                    }
                }
            }
            Arguments.Target = null;
            CallEvents("MouseMove", Arguments);
        }
        private void Event_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            EventArguments Arguments = new EventArguments();
            Arguments.Location = new Vertex(e.X, e.Y, 0);
            Arguments.Delta = e.Delta;
            CallEvents("MouseWheel", Arguments);
        }
        private void Event_RenderFrame(object sender, EventArgs e)
        {
            EventArguments Arguments = new EventArguments();
            CallEvents("RenderFrame", Arguments);
        }
        private void Event_TimerTick(object sender, ElapsedEventArgs e)
        {
            this._Seed++;
            if (_CurrentScene.Type == SceneType.Scene2D && _Seed % this.FrameUpdateRate == 0)
            {
                Scene2D C2DS = (Scene2D)_CurrentScene;
                for (int i = 0; i < C2DS.Sprites.Count; i++)
                {
                    C2DS.Sprites[i].RaiseIndex();
                }
            }
            EventArguments Arguments = new EventArguments();
            CallEvents("TimerTick", Arguments);
        }
        private void Event_OperationProgress(object sender, ProgressChangedEventArgs e)
        {
            EventArguments Arguments = new EventArguments();
            Arguments.Progress = e.ProgressPercentage;
            CallEvents("OperationProgress", Arguments);
        }
        private void Event_OperationFinished(object sender, RunWorkerCompletedEventArgs e)
        {
            EventArguments Arguments = new EventArguments();
            CallEvents("OperationFinished", Arguments);
        }
        protected virtual void CallEvents(string EventName, EventArguments Args)
        {
        }
        protected virtual void CallObjectEvents(int Index, string EventName, EventArguments Args)
        {
        }
    }
}