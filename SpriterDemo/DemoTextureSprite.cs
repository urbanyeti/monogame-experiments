using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SpriterDotNet.MonoGame;
using SpriterDotNet.MonoGame.Sprites;
using System;

namespace SpriterDemo
{
    public class DemoTextureSprite : ISprite
    {
        public float Width => Texture.Width;
        public float Height => Texture.Height;

        public Texture2D Texture { get; }

        public DemoTextureSprite(Texture2D texture)
        {
            Texture = texture;
        }

        public void Draw(SpriteBatch spriteBatch, Vector2 pivot, Vector2 position, Vector2 scale, float rotation, Color color, float depth)
        {
            SpriteEffects effects = SpriteEffects.None;

            float originX = pivot.X * Texture.Width;
            float originY = pivot.Y * Texture.Height;

            if (scale.X < 0)
            {
                effects |= SpriteEffects.FlipHorizontally;
                originX = Texture.Width - originX;
            }

            if (scale.Y < 0)
            {
                effects |= SpriteEffects.FlipVertically;
                originY = Texture.Height - originY;
            }

            scale = new Vector2(Math.Abs(scale.X), Math.Abs(scale.Y));
            Vector2 origin = new Vector2(originX, originY);

            spriteBatch.Draw(Texture, position, null, color, rotation, origin, scale, effects, depth);
        }
    }
}