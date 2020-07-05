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
        private readonly List<MonoGameAnimator> _animators = new List<MonoGameAnimator>();
        private readonly List<MonoGameAnimator> _robots = new List<MonoGameAnimator>();
        private MonoGameAnimator _animator;
        private KeyboardState _oldKeyboard;
        private MouseState _oldMouse;
        private float _elapsedTime;
        private Color _backgroundColor;
        private SpriteFont _spriteFont;

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
                }

                var robot = new MonoGameDebugAnimator(loader.Spriter.Entities[0], GraphicsDevice, factory, drawInfoPool);
                _robots.Add(robot);
                robot.Position = new Vector2(150 * _robots.Count, screenCenter.Y);
                robot.Scale = new Vector2(1f, 1f);
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


            _animator.Update(deltaTime);
            _robots.ForEach(x => x.Update(deltaTime));

            bool wasCollision = BoundingBoxCollision(mouseState.X, mouseState.Y);
            _backgroundColor = wasCollision ? Color.White : Color.CornflowerBlue;

            _oldKeyboard = Keyboard.GetState();
            _oldMouse = mouseState;

            _elapsedTime += deltaTime;
            if (_elapsedTime >= 100)
            {
                _elapsedTime -= 100;
            }
        }

        private bool BoundingBoxCollision(int x, int y)
        {
            foreach (var key in _animator.FrameData.BoxData.Keys)
            {
                if (CheckBoundingBox(key, _animator.FrameData.BoxData[key], x, y)) //&& CheckPerPixel(info, x, y))
                {
                    return true;
                }
            }

            return false;
        }


        private bool CheckBoundingBox(int id, SpriterObject info, int x, int y)
        {
            var objectData = _animator.Entity.ObjectInfos[id];
            Box box = _animator.GetBoundingBox(info, objectData.Width, objectData.Height);
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
            if (sprite is TexturePackerSprite textureSprite )
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
            _robots.ForEach(x => x.Draw(spriteBatch));
            DrawText($"Mouse [X: {_oldMouse.X}, Y: {_oldMouse.Y}]", new Vector2(100, 10), 0.6f, Color.Black);
            DrawText($"Animator [X: {_animator.Position.X}, Y: {_animator.Position.Y}]", new Vector2(100, 30), 0.6f, Color.Black);
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
