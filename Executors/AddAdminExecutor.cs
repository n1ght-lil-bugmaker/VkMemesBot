using System;
using System.Collections.Generic;
using System.Text;
using VkNet.Model.RequestParams;

namespace VKBOT
{
    class AddAdminExecutor : Executor
    {
        public override void Execute()
        {
            try
            {
                foreach (var id in requestMessage.Split(' '))
                {
                    AddAdmin(id);
                }

                Api.Messages.Send(new MessagesSendParams()
                {
                    UserId = UserID,
                    Message = "Админ(ы) добавлен(ы)",
                    RandomId = GetRandomNullableInt()
                });
            }
            catch(AddRemoveException ex)
            {
                Api.Messages.Send(new MessagesSendParams()
                {
                    UserId = UserID,
                    Message = ex.Message,
                    RandomId = GetRandomNullableInt()
                });
            }
        }
    }
}
