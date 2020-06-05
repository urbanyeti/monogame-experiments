using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace WalkingGame
{
    /// <summary>
    /// This is the main type for your game.
    /// </summary>
    public class Game1 : Game
    {
        GraphicsDeviceManager graphics;
        VertexPositionTexture[] floorVerts;
        BasicEffect effect;
        Texture2D checkerboardTexture;
        Vector3 cameraPosition = new Vector3(0, 10, 10);
        BowlingBall bowlingBall;
        Camera camera;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this)
            {
                IsFullScreen = false
            };

            Content.RootDirectory = "Content";
        }

        protected override void Initialize()
        {
            floorVerts = new VertexPositionTexture[6];

            floorVerts[0].Position = new Vector3(-20, -20, 0);
            floorVerts[1].Position = new Vector3(-20, 20, 0);
            floorVerts[2].Position = new Vector3(20, -20, 0);

            floorVerts[3].Position = floorVerts[1].Position;
            floorVerts[4].Position = new Vector3(20, 20, 0);
            floorVerts[5].Position = floorVerts[2].Position;

            int repetitions = 20;

            floorVerts[0].TextureCoordinate = new Vector2(0, 0);
            floorVerts[1].TextureCoordinate = new Vector2(0, repetitions);
            floorVerts[2].TextureCoordinate = new Vector2(repetitions, 0);

            floorVerts[3].TextureCoordinate = floorVerts[1].TextureCoordinate;
            floorVerts[4].TextureCoordinate = new Vector2(repetitions, repetitions);
            floorVerts[5].TextureCoordinate = floorVerts[2].TextureCoordinate;

            effect = new BasicEffect(graphics.GraphicsDevice);

            bowlingBall = new BowlingBall();
            bowlingBall.Initialize(Content);

            camera = new Camera(graphics.GraphicsDevice);

            base.Initialize();
        }
        protected override void LoadContent()
        {
            checkerboardTexture = Content.Load<Texture2D>("checkerboard");
        }

        protected override void Update(GameTime gameTime)
        {
            bowlingBall.Update(gameTime);
            camera.Update(gameTime);
            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);
            DrawGround();

            float aspectRatio =
                graphics.PreferredBackBufferWidth / (float)graphics.PreferredBackBufferHeight;

            bowlingBall.Draw(camera);

            base.Draw(gameTime);
        }

        void DrawGround()
        {
            effect.View = camera.ViewMatrix;
            effect.Projection = camera.ProjectionMatrix;

            effect.TextureEnabled = true;
            effect.Texture = checkerboardTexture;

            foreach (var pass in effect.CurrentTechnique.Passes)
            {
                pass.Apply();

                graphics.GraphicsDevice.DrawUserPrimitives(
                    // We’ll be rendering two trinalges
                    PrimitiveType.TriangleList,
                    // The array of verts that we want to render
                    floorVerts,
                    // The offset, which is 0 since we want to start
                    // at the beginning of the floorVerts array
                    0,
                    // The number of triangles to draw
                    2);
            }
        }
    }
}
