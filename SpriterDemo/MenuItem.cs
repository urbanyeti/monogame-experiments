using System;
using System.Collections.Generic;
using System.Text;

namespace SpriterDemo
{
    public class MenuItem : AnimatedObject
    {
        public MenuItem(string name, MonoGameDebugAnimator animator)
            : base(name, animator)
        {
            PlaySafely("idle");
        }

        public static new string Prefix { get; } = "menu-";
        public override string Key => Prefix + Name;

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
