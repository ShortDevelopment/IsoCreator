using System;

namespace IsoCreator
{
    internal class ProgramShim
    {
        [MTAThread]
        public static void Main(string[] args)
            => Program.Main(args);
    }
}
