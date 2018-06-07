using Newtonsoft.Json;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
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

            LoadElementsFromFile();

            AddBaseElements(new Point(Width / 2, Height / 2));
        }

        private void LoadElementsFromFile()
        {
            var file = "Elements.json";

            if (!File.Exists(file))
            {
                var obj = new CustomElementJson(new[]
                {
                    new ElementNode("Water", "water", true),
                    new ElementNode("Fire", "fire", true),
                    new ElementNode("Air", "air", true),
                    new ElementNode("Earth", "earth", true),
                    new ElementNode("Steam", "steam"),
                    new ElementNode("Mud", "mud"),
                    new ElementNode("Dust", "dust"),
                    new ElementNode("Sand", "sand"),
                    new ElementNode("Glass", "glass"),
                    new ElementNode("Obsidian", "obsidian"),
                    new ElementNode("Lava", "lava")
                }, new[]
                {
                    new ElementCombinationNode("Fire", "Earth", "Lava"),
                    new ElementCombinationNode("Fire", "Sand", "Glass"),
                    new ElementCombinationNode("Fire", "Water", "Steam", "Steam"),
                    new ElementCombinationNode("Water", "Earth", "Mud"),
                    new ElementCombinationNode("Water", "Lava", "Obsidian"),
                    new ElementCombinationNode("Earth", "Air", "Dust"),
                    new ElementCombinationNode("Dust", "Dust", "Sand")
                });

                var jsonString = JsonConvert.SerializeObject(obj, Formatting.Indented);

                File.WriteAllText(file, jsonString);
            }

            var json = File.ReadAllText(file);

            var cej = JsonConvert.DeserializeObject<CustomElementJson>(json);

            foreach (var element in cej.Elements)
            {
                ElementRegistry.RegisterElement(new Element(element.Name, element.TextureName, element.IsBaseElement));
            }

            foreach (var combination in cej.ElementCombinations)
            {
                var e1 = ElementRegistry.GetElement(combination.Element1);
                var e2 = ElementRegistry.GetElement(combination.Element2);

                var products = new Element[combination.Products.Length];

                for (var index = 0; index < combination.Products.Length; index++)
                {
                    products[index] = ElementRegistry.GetElement(combination.Products[index]);
                }

                ElementRegistry.RegisterCombination(e1, e2, products);
            }
        }

        private void AddBaseElements(Point p = new Point())
        {
            var radius = ElementEntity.ElementIconSize;

            var baseElements = ElementRegistry.GetBaseElements();

            for (int i = 0; i < baseElements.Length; i++)
            {
                var element = baseElements[i];
                var progress = (float)i / baseElements.Length;

                var x = (float)Math.Cos(progress * MathHelper.TwoPi + MathHelper.PiOver4) * radius;
                var y = (float)Math.Sin(progress * MathHelper.TwoPi + MathHelper.PiOver4) * radius;

                SpawnElementEntity(p.X - x, p.Y - y, element);
            }
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
                    if (product == null)
                        continue;

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
}