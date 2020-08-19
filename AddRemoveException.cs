using System;

namespace VKBOT
{
    class AddRemoveException : Exception
    {
        private string _message;

        public AddRemoveException(string Message)
        {
            _message = Message;
        }
        public override string Message => _message;
    }
}
