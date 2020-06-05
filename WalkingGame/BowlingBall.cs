using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WalkingGame
{
    public class BowlingBall
    {
        private Model _model;
        private float _angle;

        public void Initialize(ContentManager contentManager)
        {
            _model = contentManager.Load<Model>("MonoCube");
        }

        public void Update(GameTime gameTime)
        {
            _angle += (float)gameTime.ElapsedGameTime.TotalSeconds;
        }

        public void Draw(Camera camera)
        {
            foreach (var mesh in _model.Meshes)
            {
                foreach (BasicEffect effect in mesh.Effects)
                {
                    effect.EnableDefaultLighting();
                    effect.PreferPerPixelLighting = true;

                    effect.World = GetWorldMatrix();
                    effect.View = camera.ViewMatrix;
                    effect.Projection = camera.ProjectionMatrix;
                }

                mesh.Draw();
            }
        }

        private Matrix GetWorldMatrix()
        {
            const float circleRadius = 8;
            const float heightOffGround = 3;


            var scaleMatrix = Matrix.CreateScale(1);
            Matrix rotateX = Matrix.CreateRotationX(MathHelper.PiOver2);
            Matrix rotateZ = Matrix.CreateRotationZ(MathHelper.PiOver2);

            // this matrix moves the model "out" from the origin
            Matrix translationMatrix = Matrix.CreateTranslation(
                circleRadius, 0, heightOffGround);

            // this matrix rotates everything around the origin
            Matrix rotationMatrix = Matrix.CreateRotationZ(_angle);

            // We combine the two to have the model move in a circle:
            Matrix combined = rotateX * rotateZ * scaleMatrix * translationMatrix * rotationMatrix;

            return combined;
        }
    }
}
