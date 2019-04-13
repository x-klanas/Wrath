using System;
using System.Diagnostics;

namespace GameController
{
    public class MyStopwatch : Stopwatch
    {
        public DateTime StartOffset { get; private set; }
        
        public new long ElapsedMilliseconds => base.ElapsedMilliseconds + StartOffset.Millisecond;

        public MyStopwatch(DateTime startOffset)
        {
            StartOffset = startOffset;
        }
        
    }
}