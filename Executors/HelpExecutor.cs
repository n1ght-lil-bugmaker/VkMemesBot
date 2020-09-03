using System;
using System.Collections.Generic;
using System.Text;
using VkNet.Model.RequestParams;

namespace VKBOT
{
    class HelpExecutor : Executor
    {
        public override void Execute()
        {
            var answer = "Пока я знаю такие команды:\n";

            foreach (var command in MemesBot.Commands)
            {
                answer += command.Command + " " + command.Description + "\n";
            }

            Api.Messages.Send(new MessagesSendParams()
            {
                UserId = UserID,
                Message = answer,
                RandomId = GetRandomNullableInt()

            });
        }
    }
}
