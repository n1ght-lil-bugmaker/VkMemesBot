using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using VkNet.Model;
using VkNet.Model.Attachments;
using VkNet.Model.RequestParams;

namespace VKBOT
{
    class MemesExecutor : Executor
    {
        public override void Execute()
        {
            try
            {
                using (WebClient client = new WebClient())
                {
                    UploadServerInfo uploadServer = Api.Photo.GetMessagesUploadServer(PersonID);

                    string response = Encoding.ASCII.GetString(
                        client.UploadFile(uploadServer.UploadUrl,
                        GetPathToMeme(ChangeSymbols(requestMessage, ' ', '_'))));

                    var photo = Api.Photo.SaveMessagesPhoto(response);

                    Api.Messages.Send(new MessagesSendParams()
                    {
                        UserId = UserID,
                        Attachments = new List<MediaAttachment>()
                    {
                        photo.FirstOrDefault()
                    },
                        RandomId = GetRandomNullableInt()
                    });
                }
            }
            catch(HasNotFoundException ex)
            {
                string similars = GetSimilarMemes(ex.ToFindMeme, MemesBot.Memes);

                if (similars == "")
                {
                    Api.Messages.Send(new MessagesSendParams()
                    {
                        UserId = UserID,
                        Message = "Не нашел такой мем",
                        RandomId = GetRandomNullableInt()
                    });
                }
                else
                {
                    Api.Messages.Send(new MessagesSendParams()
                    {
                        UserId = UserID,
                        Message = $"Не нашел такой мем, но есть похожие: {similars}",
                        RandomId = GetRandomNullableInt()
                    });
                }
            }
            


        }
    }
}
