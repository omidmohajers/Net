using System;

namespace PA.Net.Core
{
    public class PackageEventArgs : EventArgs
    {
        public PackageEventArgs(Package pac)
        {
            Package = pac;
        }

        public Package Package { get; private set; }
    }
}