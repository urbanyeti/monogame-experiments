using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WalkingGame
{
    public class Camera
    {
        GraphicsDevice graphicsDevice;
        Vector3 position = new Vector3(0, 20, 10);
        float angle;

        public Matrix ViewMatrix
        {
            get
            {
                var lookAtVector = new Vector3(0, -1, -.5f);
                var rotationMatrix = Matrix.CreateRotationZ(angle);
                lookAtVector = Vector3.Transform(lookAtVector, rotationMatrix);
                lookAtVector += position;

                var upVector = Vector3.UnitZ;

                return Matrix.CreateLookAt(
                    position, lookAtVector, upVector);
            }
        }

        public Matrix ProjectionMatrix
        {
            get
            {
                float fieldOfView = Microsoft.Xna.Framework.MathHelper.PiOver4;
                float nearClipPlane = 1;
                float farClipPlane = 200;
                float aspectRatio = graphicsDevice.Viewport.Width / (float)graphicsDevice.Viewport.Height;

                return Matrix.CreatePerspectiveFieldOfView(
                    fieldOfView, aspectRatio, nearClipPlane, farClipPlane);
            }
        }

        public Camera(GraphicsDevice graphicsDevice)
        {
            this.graphicsDevice = graphicsDevice;
        }

        public void Update(GameTime gameTime)
        {
            KeyboardState keyboardState = Keyboard.GetState();

            if (keyboardState.IsKeyDown(Keys.Left))
            {
                angle += (float)gameTime.ElapsedGameTime.TotalSeconds;
            }

            if (keyboardState.IsKeyDown(Keys.Right))
            {
                angle -= (float)gameTime.ElapsedGameTime.TotalSeconds;
            }

            if (keyboardState.IsKeyDown(Keys.Up))
            {
                var forwardVector = new Vector3(0, -5, 0);

                var rotationMatrix = Matrix.CreateRotationZ(angle);
                forwardVector = Vector3.Transform(forwardVector, rotationMatrix);

                const float unitsPerSecond = 3;

                this.position += forwardVector * unitsPerSecond *
                    (float)gameTime.ElapsedGameTime.TotalSeconds;
            }

            if (keyboardState.IsKeyDown(Keys.Down))
            {
                var forwardVector = new Vector3(0, 5, 0);

                var rotationMatrix = Matrix.CreateRotationZ(angle);
                forwardVector = Vector3.Transform(forwardVector, rotationMatrix);

                const float unitsPerSecond = 3;

                this.position += forwardVector * unitsPerSecond *
                    (float)gameTime.ElapsedGameTime.TotalSeconds;
            }


        }
    }
}
