using System;
using VkNet;
using VkNet.Model;
using VkNet.Enums.SafetyEnums;
using VkNet.Model.RequestParams;
using System.Threading.Tasks;
using System.IO;
using System.Net;
using System.Text;
using System.Collections.Generic;
using VkNet.Model.Attachments;
using System.Linq;

namespace VKBOT
{
    public class MemesBot : IBot
    {      
        public bool IsRunning { get; private set; }

        public static List<(string Command, string Description)> Commands = new List<(string, string)>()
        {
            ("Мем:", "скидывает мем по команде \"мем [название мема]\"")
        };
        public static HashSet<long?> Admins = new HashSet<long?>()
        {
            43841691
        };
        public static HashSet<string> Memes = new HashSet<string>();

        private const long _superAdminID = 43841691;

        private string _token;
        private ulong _groupID;
        private long _presonID;

        public MemesBot(string Token, ulong GroupID, long PersonID)
        {
            this._token = Token;
            this._groupID = GroupID;
            this._presonID = PersonID;
            IsRunning = false;

            InitializeMemes();
            InitializeAdmins();
        }

        public async void Run()
        {
            IsRunning = true;
            VkApi api = new VkApi();

            try
            {
                api.Authorize(new ApiAuthParams()
                {
                    AccessToken = _token
                });
            }
            catch (Exception ex)
            {
                throw ex;
            }

            await Task.Run(() =>
                {
                    while(IsRunning)
                    {                        
                        LongPollServerResponse server = api.Groups.GetLongPollServer(_groupID);
                        BotsLongPollHistoryResponse longpoll = api.Groups.GetBotsLongPollHistory(new BotsLongPollHistoryParams()
                        {
                            Key = server.Key,
                            Server = server.Server,
                            Ts = server.Ts,
                            Wait = 25
                        });

                        if(longpoll.Updates == null)
                        {
                            continue;
                        }

                        foreach(var update in longpoll.Updates)
                        {
                            if (update.Type == GroupUpdateType.MessageNew)
                            {
                                Photo photo = null;

                                foreach(var attachment in update.Message.Attachments)
                                {
                                    photo = attachment.Instance as Photo;
                                    break;
                                }

                                Executor executor = Executor.GetExecutor(
                                    update.Message.Body,
                                    update.Message.UserId,
                                    _presonID,
                                    api,
                                    photo);

                                executor.Execute();
                            }
                            
                        }     
                    }
            });
        }
        public void Stop()
        {
            IsRunning = false;
        }
        private void InitializeMemes()
        {
            foreach(var path in Directory.GetFiles(Directory.GetCurrentDirectory() + @"\memes\"))
            {
                var tmp = path.Split(@"\");

                Memes.Add(tmp[tmp.Length - 1].Split('.')[0]);
            }
        }
        private void InitializeAdmins()
        {
            var admins = File.ReadAllLines(Directory.GetCurrentDirectory() + @"\admins.txt");

            foreach(var admin in admins)
            {
                Admins.Add(long.Parse(admin));
            }

        }
        public static bool IsAdmin(long? UserID)
        {
            return Admins.Contains(UserID);
        }
        public static bool IsSuperAdmin(long? UserID)
        {
            return UserID == _superAdminID;
        }
    }
}
