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
