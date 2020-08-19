using System;
using System.Collections.Generic;
using System.Text;

namespace VKBOT
{
    public interface IBot
    {
        public bool IsRunning { get; }
        public void Run();
        public void Stop();

    }
}
