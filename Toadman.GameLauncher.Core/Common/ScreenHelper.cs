using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Toadman.GameLauncher.Core
{
    public class VideoModeComparer : IEqualityComparer<VideoMode>
    {
        public bool Equals(VideoMode x, VideoMode y)
        {
            // Two items are equal if their Width and Height are equal
            return x.Width == y.Width && x.Height == y.Height;
        }

        public int GetHashCode(VideoMode obj)
        {
            return ((obj.Width & 0xFFFF) << 16) | (obj.Height & 0xFFFF);
        }
    }

    public class VideoMode
    {
        public int Width { get; set; }
        public int Height { get; set; }
        public int BitsPerPixel { get; set; }
        public int DisplayFrequency { get; set; }
    }

    public static class ScreenHelper
    {
        public static List<VideoMode> GetVideoModes()
        {
            var result = new List<VideoMode>();
            var vDevMode = new WinAPI.DEVMODE();
            int index = 0;

            while (WinAPI.EnumDisplaySettings(null, index, ref vDevMode))
            {
                result.Add(new VideoMode {
                    Width = vDevMode.dmPelsWidth,
                    Height = vDevMode.dmPelsHeight,
                    BitsPerPixel = vDevMode.dmBitsPerPel,
                    DisplayFrequency = vDevMode.dmDisplayFrequency
                });

                index++;
            }

            return result;
        }
    }
}
