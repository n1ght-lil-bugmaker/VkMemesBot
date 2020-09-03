using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using VkNet.Model.RequestParams;

namespace VKBOT
{
    class AddMemeExecutor : Executor
    {
        public override void Execute()
        {
            using (WebClient client = new WebClient())
            {
                client.DownloadFile(GetPhotoUrl(photo), Directory.GetCurrentDirectory()
                    + @"\memes\" + ChangeSymbols(requestMessage, ' ', '_')
                    + GetFormatFromUrl(GetPhotoUrl(photo)));
            }


            Api.Messages.Send(new MessagesSendParams()
            {
                UserId = UserID,
                Message = $"Мем {requestMessage} добавлен",
                RandomId = GetRandomNullableInt()
            });
        }
    }
}
