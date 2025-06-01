using System.Diagnostics;

namespace Toadman.GameLauncher.Core
{
    public class EstimatedTimer
    {
        private Stopwatch sw = new Stopwatch();
        private int delay = 5 * 1000;
        private long lastElapsedMilliseconds;
        private bool isStarted;

        public float? GetETA(long processedSize, long totalSize)
        {
            if (!isStarted)
            {
                sw.Start();
                isStarted = true;
                lastElapsedMilliseconds = 0;
                return 0;
            }

            if (sw.ElapsedMilliseconds - lastElapsedMilliseconds > delay)
            {
                lastElapsedMilliseconds = sw.ElapsedMilliseconds;
                return (totalSize - processedSize) * sw.ElapsedMilliseconds / processedSize;
            }
            else
                return null;
        }
    }
}