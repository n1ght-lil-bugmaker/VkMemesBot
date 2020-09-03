using System;
using System.Collections.Generic;
using System.Text;
using VkNet.Model.RequestParams;

namespace VKBOT.Executors
{
    class DontKnowExecutor : Executor
    {
        public override void Execute()
        {
            Api.Messages.Send(new MessagesSendParams()
            {
                UserId = UserID,
                Message = "Не знаю такой команды, напиши \"хелп\", чтобы увидеть список команд",
                RandomId = GetRandomNullableInt()
            });
        }
    }
}
