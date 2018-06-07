﻿using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace Alchemy
{
    public class Game : GameWindow
    {
        private readonly List<AchievementToast> _toastQueue = new List<AchievementToast>();
        private readonly List<ElementEntity> _elementEntities = new List<ElementEntity>();
        private readonly List<Element> _learntElements = new List<Element>();

        private readonly Stopwatch _updateTimer = new Stopwatch();

        private ElementEntity _holding;
        private ElementEntity _lastClicked;
        private PointF _clickOffset;

        private readonly Timer _doubleClickTimer;
        private int _clicks;

        private readonly Random _rand = new Random();

        public Game() : base(640, 480, new GraphicsMode(32, 24, 0, 8), "Alchemy")
        {
            FontRenderer.Init();

            Init();

            _doubleClickTimer = new Timer
            {
                Interval = SystemInformation.DoubleClickTime / 2
            };

            _doubleClickTimer.Tick += (o, e) =>
            {
                _clicks = 0;
                _doubleClickTimer.Stop();
            };
        }

        private void Init()
        {
            OnResize(null);

            GL.Enable(EnableCap.Texture2D);
            GL.Enable(EnableCap.LineSmooth);
            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
            GL.ActiveTexture(TextureUnit.Texture0);

            RegisterElementCombinations();

            _learntElements.Add(Element.Fire);
            _learntElements.Add(Element.Water);
            _learntElements.Add(Element.Air);
            _learntElements.Add(Element.Earth);

            AddBaseElements(new Point(Width / 2, Height / 2));
        }

        private void RegisterElementCombinations()
        {
            Element.Fire = new Element("Fire", "fire");
            Element.Water = new Element("Water", "water");

            Element.Air = new Element("Air", "air");
            Element.Earth = new Element("Earth", "earth");

            var steam = new Element("Steam", "steam");
            var lava = new Element("Lava", "lava");
            var mud = new Element("Mud", "mud");
            var obsidian = new Element("Obsidian", "obsidian");
            var dust = new Element("Dust", "dust");
            var sand = new Element("Sand", "sand");
            var glass = new Element("Glass", "glass");
            
            ElementRegistry.RegisterCombination(Element.Fire, sand, glass);
            ElementRegistry.RegisterCombination(Element.Fire, Element.Water, steam, steam);
            ElementRegistry.RegisterCombination(Element.Fire, Element.Earth, lava);
            ElementRegistry.RegisterCombination(Element.Earth, Element.Water, mud);
            ElementRegistry.RegisterCombination(Element.Earth, Element.Air, dust);
            ElementRegistry.RegisterCombination(Element.Water, lava, obsidian);
            ElementRegistry.RegisterCombination(dust, dust, sand);

            //TODO - call an event
        }

        private void AddBaseElements(Point p = new Point())
        {
            var offset = ElementEntity.ElementIconSize / 1.5f;

            SpawnElementEntity(p.X - offset, p.Y - offset, Element.Water);
            SpawnElementEntity(p.X + offset, p.Y - offset, Element.Fire);

            SpawnElementEntity(p.X - offset, p.Y + offset, Element.Earth);
            SpawnElementEntity(p.X + offset, p.Y + offset, Element.Air);
        }

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            var partialTicks = (float)(_updateTimer.Elapsed.TotalMilliseconds / (TargetUpdatePeriod * 1000f));

            for (var index = 0; index < _elementEntities.Count; index++)
            {
                var entity = _elementEntities[index];

                entity?.Render(partialTicks);
            }

            if (_toastQueue.Count > 0)
                _toastQueue.First()?.Render(partialTicks);

            SwapBuffers();
        }

        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            for (var index = _elementEntities.Count - 1; index >= 0; index--)
            {
                var entity = _elementEntities[index];

                entity.Update();
            }

            if (_toastQueue.Count > 0)
            {
                var toast = _toastQueue.First();
                if (toast != null)
                {
                    toast.Update();

                    if (toast.IsDead)
                        _toastQueue.Remove(toast);
                }
            }

            _updateTimer.Restart();
        }

        protected override void OnMouseDown(MouseButtonEventArgs e)
        {
            if (e.Button != MouseButton.Left || _holding != null || !ClientRectangle.Contains(e.Position))
                return;

            //double click, clone double clicked item
            if (_clicks >= 1)
            {
                _clicks = 0;
                _doubleClickTimer.Stop();

                if (GetTopElementAtPoint(e.Position) is ElementEntity entity && _lastClicked == entity)
                {
                    _clickOffset = new PointF();
                    _holding = new ElementEntity(e.X, e.Y, entity.Element);

                    _elementEntities.Add(_holding);
                }
                if (_lastClicked == null)
                {
                    AddBaseElements(e.Position);
                }

                return;
            }

            _clicks++;

            _doubleClickTimer.Interval = _doubleClickTimer.Interval = SystemInformation.DoubleClickTime / 2;
            _doubleClickTimer.Start();

            if (_elementEntities.Count > 0 && GetTopElementAtPoint(e.Position) is ElementEntity top)
            {
                _clickOffset = new PointF(top.X - e.X, top.Y - e.Y);

                _lastClicked = _holding = top;

                _elementEntities.Remove(_holding);
                _elementEntities.Add(_holding);
            }
            else
            {
                _lastClicked = null;
            }
        }

        protected override void OnMouseUp(MouseButtonEventArgs e)
        {
            if (e.Button != MouseButton.Left || _holding == null || !ClientRectangle.Contains(e.Position))
                return;

            if (GetTopElementAtPoint(new PointF(_holding.X, _holding.Y), _holding) is ElementEntity ee)
            {
                var products = ElementRegistry.GetProducts(ee.Element, _holding.Element);

                var max = ElementEntity.ElementIconSize / 2;

                foreach (var product in products)
                {
                    var middleX = (_holding.X + ee.X) / 2;
                    var middleY = (_holding.Y + ee.Y) / 2;

                    _elementEntities.Remove(_holding);
                    _elementEntities.Remove(ee);

                    middleX += max - (float)_rand.NextDouble() * max * 2;
                    middleY += max - (float)_rand.NextDouble() * max * 2;

                    LearnElement(product);

                    SpawnElementEntity(middleX, middleY, product);
                }
            }

            _holding = null;
            _clickOffset = new PointF();
        }

        protected override void OnMouseMove(MouseMoveEventArgs e)
        {
            if (!ClientRectangle.Contains(e.Position))
                return;

            if (_holding != null)
            {
                _holding.X = _clickOffset.X + e.X;
                _holding.Y = _clickOffset.Y + e.Y;
            }
        }

        protected override void OnResize(EventArgs e)
        {
            GL.Viewport(0, 0, Width, Height);
            GL.MatrixMode(MatrixMode.Projection);
            GL.LoadIdentity();
            GL.Ortho(0, Width, Height, 0, 0, 1);

            OnRenderFrame(null);
        }

        protected override void OnMove(EventArgs e)
        {
            OnRenderFrame(null);
        }

        protected void LearnElement(Element e)
        {
            if (_learntElements.Contains(e))
                return;

            //TODO - call an event?
            _toastQueue.Add(new AchievementToast("New element found!", e + "!", e.TextureId, this));

            _learntElements.Add(e);
        }

        public void SpawnElementEntity(float x, float y, Element e)
        {
            _elementEntities.Add(new ElementEntity(x, y, e));
        }

        private ElementEntity GetTopElementAtPoint(PointF p, ElementEntity except = null)
        {
            for (var index = _elementEntities.Count - 1; index >= 0; index--)
            {
                var entity = _elementEntities[index];

                if (entity != except && entity.IsMouseOver(p))
                    return entity;
            }

            return null;
        }
    }

    internal class AchievementToast
    {
        public string Title;
        public string MessageText;

        private readonly int _iconTextureId;
        private readonly int _toastTextureId;

        private readonly Game _game;

        private long _ticks;
        private long _ticksLast;

        private readonly long _maxTicks = 60;

        public bool IsDead;

        public AchievementToast(string title, string messageText, int iconId, Game game)
        {
            Title = title;
            MessageText = messageText;

            _game = game;

            _iconTextureId = iconId;
            _toastTextureId = TextureManager.GetOrRegister("toast");
        }

        public void Update()
        {
            _ticksLast = _ticks;

            if (_ticks < _maxTicks)
                _ticks++;
            else
                IsDead = true;
        }

        public void Render(float partialTicks)
        {
            if (IsDead)
                return;

            float progress;

            var partialTick = _ticksLast + (_ticks - _ticksLast) * partialTicks;

            if (partialTick >= _maxTicks - 5)
                progress = (float)Math.Cos(Math.Min(5 - _maxTicks - partialTick, 5) / 5f * MathHelper.PiOver2);
            else
                progress = -(float)Math.Sin(Math.Min(partialTick, 10) / 10f * MathHelper.PiOver2);

            var centerX = _game.Width / 2;
            var centerY = -32 - 64 * progress;

            GL.Translate(centerX, centerY, 0);

            //render toast texture
            GL.BindTexture(TextureTarget.Texture2D, _toastTextureId);

            GL.Color4(1f, 1, 1, 1);
            GL.Scale(256, 64, 1);
            GL.Begin(PrimitiveType.Quads);
            VertexUtil.PutQuad();
            GL.End();
            GL.Scale(1f / 256, 1f / 64, 1);

            //render icon shadow
            GL.BindTexture(TextureTarget.Texture2D, _iconTextureId);

            GL.Color4(0f, 0, 0, 1);
            GL.Translate(-94.5F, 2, 0);
            GL.Scale(49, 49, 1);
            GL.Begin(PrimitiveType.Quads);
            VertexUtil.PutQuad();
            GL.End();
            GL.Scale(1f / 49, 1f / 49, 1);
            GL.Translate(94.5F, -2, 0);

            //render icon
            GL.Color4(1f, 1, 1, 1);
            GL.Translate(-96.5F, 0, 0);
            GL.Scale(49, 49, 1);
            GL.Begin(PrimitiveType.Quads);
            VertexUtil.PutQuad();
            GL.End();
            GL.Scale(1f / 49, 1f / 49, 1);
            GL.Translate(96.5F, 0, 0);

            //render title
            GL.Color4(0, 0.65f, 1, 1);
            GL.Scale(0.9f, 0.9f, 1);
            FontRenderer.DrawTextWithShadow(-67, -28, Title);
            GL.Scale(1 / 0.9f, 1 / 0.9f, 1);

            //render text
            GL.Color4(1f, 1, 1, 1);
            GL.Scale(0.9f, 0.9f, 1);
            FontRenderer.DrawTextWithShadow(-67, -4, MessageText);
            GL.Scale(1 / 0.9f, 1 / 0.9f, 1);

            GL.Translate(-centerX, -centerY, 0);
        }
    }
}