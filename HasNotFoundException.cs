using System;
using System.Collections.Generic;
using System.Text;

namespace VKBOT
{
    class HasNotFoundException : Exception
    {
        public override string Message => "Мем не найден";
    }
}
