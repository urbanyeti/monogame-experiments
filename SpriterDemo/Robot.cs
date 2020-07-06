using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Text;

namespace SpriterDemo
{
    public class Robot : AnimatedObject
    {
        public Robot(string name, MonoGameDebugAnimator animator)
            : base(name, animator)
        {
        }

        public static new string Prefix { get; } = "robot-";
        public override string Key => Prefix + Name;

        public ContextMenu ContextMenu { get; set; } = new ContextMenu();
        public bool ShowContextMenu { get; set; } = false;
        public void AddContextMenuItem(MenuItem item, MenuPosition position)
        {
            var offset = position switch
            {
                MenuPosition.Left => new Vector2(-100, 0),
                MenuPosition.TopLeft => new Vector2(-100, -100),
                MenuPosition.Top => new Vector2(0, -100),
                MenuPosition.TopRight => new Vector2(100, -100),
                MenuPosition.Center => new Vector2(0, 0),
                MenuPosition.Right => new Vector2(100, 0),
                MenuPosition.BottomLeft => new Vector2(-100, 100),
                MenuPosition.Bottom => new Vector2(0, 100),
                MenuPosition.BottomRight => new Vector2(100, 100),
                _ => throw new NotImplementedException(),
            };

            item.Animator.Position = Animator.Position + offset;
            item.Animator.Scale = new Vector2(.75f, .75f);
            ContextMenu.AddItem(item, position);
        }

        public void GetContextMenuItem(MenuPosition position) => ContextMenu.GetItem(position);

        public override void Update(float deltaTime)
        {
            base.Update(deltaTime);
            if (ShowContextMenu)
            {
                ContextMenu.Items.ForEach(x => x.Update(deltaTime));
            }
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            base.Draw(spriteBatch);
            if (ShowContextMenu)
            {
                ContextMenu.Items.ForEach(x => x.Draw(spriteBatch));
            }
        }

        protected override void AnimationFinished(string animation)
        {
            switch (animation)
            {
                case "select_start":
                    PlaySafely("select_loop");
                    break;
                case "select_stop":
                    PlaySafely("idle");
                    break;
                default:
                    break;
            }
        }
    }
}
