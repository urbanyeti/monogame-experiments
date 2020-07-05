using SpriterDotNet;
using System;
using System.Collections.Generic;
using System.Text;

namespace SpriterDemo
{
    public class SpriterDemoOptions
    {
        public const string SpriterDemo = "SpriterDemo";
        public List<string> ScmlFiles { get; set; }
        public string FontName { get; set; }
        public int WindowWidth { get; set; }
        public int WindowHeight { get; set; }
        public bool FullScreen { get; set; }
        public float ModelScale { get; set; }
        public static readonly Config Config = new Config
        {
            MetadataEnabled = true,
            EventsEnabled = true,
            PoolingEnabled = true,
            TagsEnabled = true,
            VarsEnabled = true,
            SoundsEnabled = false
        };
    }
}
