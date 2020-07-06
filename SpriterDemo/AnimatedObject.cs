using Microsoft.Xna.Framework.Graphics;
using System.Linq;

namespace SpriterDemo
{
    public class AnimatedObject
    {
        public AnimatedObject(string name, MonoGameDebugAnimator animator)
        {
            Animator = (MonoGameDebugAnimator)animator.Clone();
            Name = name;
            Animator.AnimationFinished += AnimationFinished;
        }

        public string Name { get; }
        public MonoGameDebugAnimator Animator { get; }
        public static string Prefix { get; } = "general-";
        public string Key => Prefix + Name;

        public virtual void Update(float deltaTime) => Animator.Update(deltaTime);
        public virtual void Draw(SpriteBatch spriteBatch) => Animator.Draw(spriteBatch);

        public virtual void PlaySafely(string name)
        {
            if (Animator.GetAnimations().Contains(name))
            {
                Animator.Play(name);
            }
        }

        protected virtual void AnimationFinished(string animation)
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
