using System;

namespace Toadman.GameLauncher.Client
{
    public class ServerNotAvailableException : Exception
    {
        public ServerNotAvailableException()
            : base("Server is not available")
        {
        }

        public ServerNotAvailableException(string message)
            : base(message)
        {
        }
    }
}