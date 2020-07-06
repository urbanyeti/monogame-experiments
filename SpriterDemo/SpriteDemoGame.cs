using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;
using SpriterDotNet.MonoGame;
using SpriterDotNet.Providers;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Extensions.Options;
using SpriterDotNet.MonoGame.Content;
using SpriterDotNet;
using System.Diagnostics;
using System.Linq;
using System;
using System.Drawing.Drawing2D;
using SpriterDotNet.MonoGame.Sprites;

namespace SpriterDemo
{
    public class SpriteDemoGame : Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        private readonly IOptionsMonitor<SpriterDemoOptions> _options;
        private readonly List<MonoGameDebugAnimator> _animators = new List<MonoGameDebugAnimator>();
        private readonly Dictionary<string, MonoGameDebugAnimator> _robots = new Dictionary<string, MonoGameDebugAnimator>();
        private MonoGameDebugAnimator _animator;
        private KeyboardState _oldKeyboard;
        private MouseState _oldMouse;
        private Dictionary<string, bool> _oldCollisions = new Dictionary<string, bool>();
        private float _elapsedTime;
        private Color _backgroundColor;
        private SpriteFont _spriteFont;
        private string _hoverText = "";
        private string _currentName = "";

        public SpriteDemoGame(IOptionsMonitor<SpriterDemoOptions> options)
        {
            _options = options;
            graphics = new GraphicsDeviceManager(this)
            {
                PreferredBackBufferWidth = _options.CurrentValue.WindowWidth,
                PreferredBackBufferHeight = _options.CurrentValue.WindowHeight,
                IsFullScreen = _options.CurrentValue.FullScreen
            };
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
        }

        protected override void Initialize()
        {
            base.Initialize();
            spriteBatch = new SpriteBatch(GraphicsDevice);

        }

        protected override void LoadContent()
        {
            Vector2 modelScale = new Vector2(_options.CurrentValue.ModelScale, _options.CurrentValue.ModelScale);
            _backgroundColor = Color.CornflowerBlue;
            Vector2 screenCenter = new Vector2(_options.CurrentValue.WindowWidth * 0.5f, _options.CurrentValue.WindowHeight * 0.5f);

            _spriteFont = Content.Load<SpriteFont>(_options.CurrentValue.FontName);

            DefaultProviderFactory<ISprite, SoundEffect> factory = new DefaultProviderFactory<ISprite, SoundEffect>(SpriterDemoOptions.Config, true);

            foreach (string scmlPath in _options.CurrentValue.ScmlFiles)
            {
                SpriterDemoContentLoader loader = new SpriterDemoContentLoader(Content, scmlPath);
                loader.Fill(factory);

                Stack<SpriteDrawInfo> drawInfoPool = new Stack<SpriteDrawInfo>();

                foreach (SpriterEntity entity in loader.Spriter.Entities)
                {

                    var animator = new MonoGameDebugAnimator(entity, GraphicsDevice, factory, drawInfoPool);
                    _animators.Add(animator);
                    //animator.EventTriggered += x => Debug.WriteLine("Event Happened: " + x);
                    animator.Scale = modelScale;
                    animator.Position = new Vector2(_options.CurrentValue.WindowWidth - (100 * modelScale.X), screenCenter.Y);
                    animator.DrawBoxOutlines = true;
                    animator.AnimationFinished += AnimatorFinished;
                }

                var robot = new MonoGameDebugAnimator(loader.Spriter.Entities[0], GraphicsDevice, factory, drawInfoPool);
                _robots[robot.Entity.Name] = robot;
                robot.Position = new Vector2(150 * _robots.Count, screenCenter.Y);
                robot.Scale = new Vector2(1f, 1f);
                robot.AnimationFinished += RobotsFinished;
            }

            _animator = _animators.First();
        }

        protected override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            if (WasPressed(Keys.Right)) SwitchEntityForward();
            if (WasPressed(Keys.Left)) SwitchEntityBack();
            if (WasPressed(Keys.OemTilde)) (_animator as MonoGameDebugAnimator).DrawBoxOutlines = !(_animator as MonoGameDebugAnimator).DrawBoxOutlines;

            var mouseState = Mouse.GetState();

            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            float deltaTime = gameTime.ElapsedGameTime.Ticks / (float)TimeSpan.TicksPerMillisecond;

            var collisions = new Dictionary<string, bool>();

            foreach (var key in _robots.Keys)
            {
                _robots[key].Update(deltaTime);
                collisions[key] = BoundingBoxCollision(_robots[key], mouseState.X, mouseState.Y);
                if (collisions[key])
                {
                    _hoverText = $"Model: [{key}], Animation: [{_robots[key].Name}]";
                }
            }
            _animator.Update(deltaTime);
            collisions[_currentName] = BoundingBoxCollision(_animator, mouseState.X, mouseState.Y);
            if (collisions[_currentName])
            {
                _hoverText = $"Model: [{_currentName}], Animation: [{_animator.Name}]";
            }
            _hoverText = collisions.Values.Any(x => x) ? _hoverText : "";
            _backgroundColor = collisions.Values.Any(x => x) ? Color.White : Color.CornflowerBlue;

            foreach (var key in collisions.Keys)
            {
                if (collisions[key] && (!_oldCollisions.ContainsKey(key) || !_oldCollisions[key]))
                {
                    if (_robots.ContainsKey(key))
                    {
                        _robots[key].PlaySafely("select_start");
                    }
                    else
                    {
                        _animator.PlaySafely("select_start");
                    }
                }
                if (!collisions[key] && (_oldCollisions.ContainsKey(key) && _oldCollisions[key]))
                {
                    if (_robots.ContainsKey(key))
                    {
                        _robots[key].PlaySafely("select_stop");
                    }
                    else
                    {
                        _animator.PlaySafely("select_stop");
                    }
                }

            }

            _oldKeyboard = Keyboard.GetState();
            _oldMouse = mouseState;
            _oldCollisions = collisions;

            _elapsedTime += deltaTime;
            if (_elapsedTime >= 100)
            {
                _elapsedTime -= 100;
                _currentName = $"current-{_animator.Entity.Name}";
            }
        }

        private void RobotsFinished(string animation)
        {
            switch (animation)
            {
                case "select_start":
                    _robots["offense_robot"].PlaySafely("select_loop");
                    break;
                case "select_stop":
                    _robots["offense_robot"].PlaySafely("idle");
                    break;
                default:
                    break;
            }
        }

        private void AnimatorFinished(string animation)
        {
            switch (animation)
            {
                case "select_start":
                    _animator.PlaySafely("select_loop");
                    break;
                case "select_stop":
                    _animator.PlaySafely("idle");
                    break;
                default:
                    break;
            }
        }

        private bool BoundingBoxCollision(MonoGameAnimator animator, int x, int y)
        {
            foreach (var key in animator.FrameData.BoxData.Keys)
            {
                if (CheckBoundingBox(animator, key, animator.FrameData.BoxData[key], x, y))
                {
                    return true;
                }
            }

            return false;
        }


        private bool CheckBoundingBox(MonoGameAnimator animator, int id, SpriterObject info, int x, int y)
        {
            var objectData = animator.Entity.ObjectInfos[id];
            Box box = animator.GetBoundingBox(info, objectData.Width, objectData.Height);
            GraphicsPath graphicsPath = new GraphicsPath();
            graphicsPath.AddPolygon(new System.Drawing.PointF[] {
                    new System.Drawing.PointF(box.Point1.X, box.Point1.Y),
                    new System.Drawing.PointF(box.Point2.X, box.Point2.Y),
                    new System.Drawing.PointF(box.Point3.X, box.Point3.Y),
                    new System.Drawing.PointF(box.Point4.X, box.Point4.Y)
                });

            if (graphicsPath.IsVisible(new System.Drawing.Point(x, y)))
            {
                return true;
            }

            return false;
        }

        private bool CheckPerPixel(SpriterObject info, int x, int y)
        {
            Texture2D sourceTexture = null;
            Rectangle sourceRectangle = new Rectangle();

            var sprite = _animator.SpriteProvider.Get(info.FolderId, info.FileId);
            if (sprite is TexturePackerSprite textureSprite)
            {
                sourceTexture = textureSprite.texture;
            }

            var sourceColors = new Color[sourceTexture.Width * sourceTexture.Height];
            sourceTexture.GetData(sourceColors);

            var sourceColor = sourceColors[((x - (int)info.X) + (y - ((int)info.Y)) * sourceTexture.Width)];

            if (sourceColor.A > 0)
            {
                return true;
            }

            return false;
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(_backgroundColor);
            spriteBatch.Begin(SpriteSortMode.BackToFront, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullCounterClockwise);

            _animator.Draw(spriteBatch);
            foreach (var robot in _robots.Values)
            {
                robot.Draw(spriteBatch);
            }

            DrawText($"Mouse [X: {_oldMouse.X}, Y: {_oldMouse.Y}]", new Vector2(100, 10), 0.6f, Color.Black);
            DrawText($"Selected: {_hoverText}", new Vector2(100, 30), 0.6f, Color.Black);
            spriteBatch.End();

            base.Draw(gameTime);
        }

        private void DrawText(string text, Vector2 position, float size, Color color)
        {
            spriteBatch.DrawString(_spriteFont, text, position, color, 0, Vector2.Zero, size, SpriteEffects.None, 0.0f);
        }

        private bool WasPressed(Keys key)
        {
            KeyboardState state = Keyboard.GetState();
            return _oldKeyboard.IsKeyUp(key) && state.IsKeyDown(key);
        }

        private void SwitchEntityForward()
        {
            int index = _animators.IndexOf(_animator);
            ++index;
            if (index >= _animators.Count) index = 0;
            _animator = _animators[index];
        }

        private void SwitchEntityBack()
        {
            int index = _animators.IndexOf(_animator);
            --index;
            if (index < 0) index = _animators.Count - 1;
            _animator = _animators[index];
        }
    }
}
