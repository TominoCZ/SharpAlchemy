using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace Alchemy
{
    public class Game : GameWindow
    {
        private readonly List<ElementEntity> _elementEntities = new List<ElementEntity>();
        private readonly List<Element> _learntElements = new List<Element>();

        private ElementEntity _holding;
        private ElementEntity _lastClicked;
        private PointF _clickOffset;
        private Point _mouseLast;

        private readonly Timer _doubleClickTimer;
        private int _clicks;

        private readonly Random _rand = new Random();

        public Game() : base(640, 480, new GraphicsMode(32, 24, 0, 8), "Alchleby")
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

            LearnElement(Element.Fire);
            LearnElement(Element.Water);

            AddBaseElements(new Point(Width / 2, Height / 2));
        }

        private void RegisterElementCombinations()
        {
            Element.Fire = new Element("Fire", "fire");
            Element.Water = new Element("Water", "water");

            var steam = new Element("Steam", "steam");

            ElementRegistry.RegisterCombination(Element.Fire, Element.Water, steam, steam);

            //TODO - call an event
        }

        private void AddBaseElements(Point p = new Point())
        {
            SpawnElementEntity(p.X - ElementEntity.ElementIconSize / 1.75f, p.Y, Element.Water);
            SpawnElementEntity(p.X + ElementEntity.ElementIconSize / 1.75f, p.Y, Element.Fire);
        }

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            ElementEntity over = GetTopElementAtPoint(_mouseLast);
            ElementEntity overOther = null;

            if (over != null)
                overOther = GetTopElementAtPoint(new PointF(over.X, over.Y), over);

            for (var index = 0; index < _elementEntities.Count; index++)
            {
                var entity = _elementEntities[index];

                if (entity == null)
                    continue;

                var isOtherEntity = overOther == entity;

                if (over == entity || isOtherEntity && _holding != null)
                {
                    var canCraft = overOther != null && ElementRegistry.GetProducts(over.Element, overOther.Element).Length > 0;

                    GL.BindTexture(TextureTarget.Texture2D, 0);

                    GL.Translate(entity.X, entity.Y, 0);
                    GL.Scale(ElementEntity.ElementIconSize, ElementEntity.ElementIconSize, 1);

                    if (isOtherEntity)
                    {
                        if (canCraft)
                            GL.Color3(0f, 1, 0.5f);
                        else
                            GL.Color3(1f, 0, 0.25f);
                    }
                    else
                    {
                        GL.Color3(0f, 0.5f, 1);
                    }

                    GL.Begin(PrimitiveType.LineLoop);
                    VertexUtil.PutCircle();
                    GL.End();

                    if (isOtherEntity && _holding != null)
                    {
                        if (canCraft)
                            GL.Color4(0f, 1, 0.5f, 0.2f);
                        else
                            GL.Color4(1f, 0, 0.25f, 0.2f);
                    }
                    else
                    {
                        GL.Color4(0f, 0.5f, 1, 0.2f);
                    }

                    GL.Begin(PrimitiveType.Polygon);
                    VertexUtil.PutCircle();
                    GL.End();

                    GL.Scale(1f / ElementEntity.ElementIconSize, 1f / ElementEntity.ElementIconSize, 1);
                    GL.Translate(-entity.X, -entity.Y, 0);
                }

                entity.Render();
            }

            SwapBuffers();
        }

        protected override void OnResize(EventArgs e)
        {
            GL.Viewport(0, 0, Width, Height);
            GL.MatrixMode(MatrixMode.Projection);
            GL.LoadIdentity();
            GL.Ortho(0, Width, Height, 0, 0, 1);

            OnRenderFrame(null);
        }

        protected override void OnMouseDown(MouseButtonEventArgs e)
        {
            if (e.Button != MouseButton.Left || _holding != null || !ClientRectangle.Contains(e.Position))
                return;

            _mouseLast = e.Position;

            //TODO - doesnt spawn base items after clicking an item

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

            _mouseLast = e.Position;
        }

        protected void LearnElement(Element e)
        {
            if (_learntElements.Contains(e))
                return;

            //TODO - maybe announce an achievement? call an event?

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
}