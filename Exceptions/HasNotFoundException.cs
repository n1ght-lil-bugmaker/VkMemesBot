using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace VKBOT
{
    class HasNotFoundException : Exception
    {
        public override string Message => "Мем не найден";
        public string ToFindMeme;

        public HasNotFoundException() { }

        public HasNotFoundException(string toFindMeme)
        {
            this.ToFindMeme = toFindMeme;
        }
    }
}
