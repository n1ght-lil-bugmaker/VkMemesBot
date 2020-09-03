using System;
using System.Collections.Generic;
using System.Text;
using VkNet.Model.RequestParams;

namespace VKBOT
{
    class RemoveAdminExecutor : Executor
    {
        public override void Execute()
        {
            try
            {
                RemoveAdmins(requestMessage);

                Api.Messages.Send(new MessagesSendParams()
                {
                    UserId = UserID,
                    Message = "Админ(ы) удален(ы)",
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
