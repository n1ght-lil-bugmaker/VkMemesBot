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

        private List<(string Command, string Description)> _commands = new List<(string, string)>()
        {
            ("Мем:", "скидывает мем по команде \"мем [название мема]\"")
        };
        private HashSet<long?> _admins = new HashSet<long?>()
        {
            43841691
        };
        private HashSet<string> _memes = new HashSet<string>();

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
                        long? _UserID = 0;
                        string toFindMeme = "";
                        try
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
                                if(update.Type == GroupUpdateType.MessageNew)
                                {
                                    _UserID = update.Message.UserId;
                                    var request = ParseCommand(update.Message.Body.ToLower());

                                    

                                    if(request.Command == "мем")
                                    {
                                        if(!_memes.Contains(ChangeSymbols(request.Message, ' ', '_')))
                                        {
                                            toFindMeme = request.Message;
                                            throw new HasNotFoundException();
                                        }

                                        WebClient wc = new WebClient();

                                        UploadServerInfo uploadServer = api.Photo.GetMessagesUploadServer(_presonID); 

                                        string response = Encoding.ASCII.GetString(
                                            wc.UploadFile(uploadServer.UploadUrl,
                                            GetPathToMeme(ChangeSymbols(request.Message, ' ', '_'))));

                                        var photo = api.Photo.SaveMessagesPhoto(response);

                                        api.Messages.Send(new MessagesSendParams()
                                        {
                                            UserId = _UserID, 
                                            Attachments = new List<MediaAttachment>()
                                            {
                                                photo.FirstOrDefault()
                                            },
                                            RandomId = GetRandomNullableInt()
                                        });
                                    }
                                    else if(request.Command == "добавить" && _admins.Contains(_UserID))
                                    {
                                        if(_memes.Contains(ChangeSymbols(request.Message, ' ', '_')))
                                        {
                                            api.Messages.Send(new MessagesSendParams()
                                            {
                                                UserId = update.Message.UserId,
                                                Message = $"Мем {request.Message} уже есть",
                                                RandomId = GetRandomNullableInt()
                                            });
                                            continue;
                                        }
                                        foreach(var photo in update.Message.Attachments)
                                        {
                                            if(photo.Instance is Photo)
                                            {
                                                Photo toDownload = photo.Instance as Photo;

                                                using(WebClient client = new WebClient())
                                                {
                                                    client.DownloadFile(GetPhotoUrl(toDownload), Directory.GetCurrentDirectory() + @"\memes\" 
                                                    + ChangeSymbols(request.Message, ' ', '_')
                                                    + GetFormatFromUrl(GetPhotoUrl(toDownload)));
                                                }

                                                api.Messages.Send(new MessagesSendParams()
                                                {
                                                    UserId = update.Message.UserId,
                                                    Message = $"Мем {request.Message} добавлен",
                                                    RandomId = GetRandomNullableInt()
                                                });

                                                _memes.Add(ChangeSymbols(request.Message, ' ', '_'));
                                            }
                                        }
                                    }
                                    else if(request.Command == "+админ" && _UserID == _superAdminID)
                                    {
                                        foreach(var id in request.Message.Split(' '))
                                        {
                                            AddAdmin(id);
                                        }

                                        api.Messages.Send(new MessagesSendParams()
                                        {
                                            UserId = _UserID,
                                            Message = "Админ(ы) добавлен(ы)", 
                                            RandomId = GetRandomNullableInt()
                                        });
                                    }
                                    else if(request.Command == "-админ" && _UserID == _superAdminID)
                                    {
                                        RemoveAdmins(request.Message);

                                        api.Messages.Send(new MessagesSendParams()
                                        {
                                            UserId = _UserID,
                                            Message = "Админ(ы) удален(ы)",
                                            RandomId = GetRandomNullableInt()
                                        });

                                    }
                                    else if(request.Command == "хелп")
                                    {
                                        var answer = "Пока я знаю такие команды:\n";

                                        foreach(var command in _commands)
                                        {
                                            answer += command.Command + " " + command.Description + "\n";
                                        }

                                        api.Messages.Send(new MessagesSendParams()
                                        {
                                            UserId = _UserID,
                                            Message = answer,
                                            RandomId = GetRandomNullableInt()

                                        }) ;
                                    }
                                    else
                                    {
                                        api.Messages.Send(new MessagesSendParams()
                                        {
                                            UserId = update.Message.UserId,
                                            Message = "Не знаю такой команды, напиши \"хелп\", чтобы увидеть список команд",
                                            RandomId = GetRandomNullableInt()
                                        });
                                    }

                                    
                                }
                            }
                        }
                        catch (HasNotFoundException)
                        {
                            string similars = GetSimilarMemes(toFindMeme);

                            if(similars == "")
                            {
                                api.Messages.Send(new MessagesSendParams()
                                {
                                    UserId = _UserID,
                                    Message = "Не нашел такой мем",
                                    RandomId = GetRandomNullableInt()
                                });
                            }
                            else
                            {
                                api.Messages.Send(new MessagesSendParams()
                                {
                                    UserId = _UserID,
                                    Message = $"Не нашел такой мем, но есть похожие: {similars}",
                                    RandomId = GetRandomNullableInt()
                                });
                            }
                            
                        }
                        catch (AddRemoveException ex)
                        {
                            api.Messages.Send(new MessagesSendParams()
                            {
                                UserId = _UserID,
                                Message = ex.Message,
                                RandomId = GetRandomNullableInt()
                            });
                        }
                    }
            });
            

        }
        public void Stop()
        {
            IsRunning = false;
        }
        private string GetPathToMeme(string request)
        {
            if(File.Exists(Directory.GetCurrentDirectory() + @"\memes\" +  request  + ".jpg" ))
            {
                return Directory.GetCurrentDirectory() + @"\memes\" + request + ".jpg";
            }

            else if (File.Exists(Directory.GetCurrentDirectory() + @"\memes\" + request + ".png"))
            {
                return Directory.GetCurrentDirectory() + @"\memes\" + request + ".png";
            }

            else if (File.Exists(Directory.GetCurrentDirectory() + @"\memes\" + request + ".jpeg"))
            {
                return Directory.GetCurrentDirectory() + @"\memes\" + request + ".jpeg";
            }

            else
            {
                throw new HasNotFoundException();
            }
        }
        private (string Command, string Message) ParseCommand(string request)
        {
            bool wasEscape = false;
            string Command = "", Message = "";

            foreach(char letter in request)
            {
                if(letter == ' ' && !wasEscape)
                {
                    wasEscape = true;
                    continue;
                }
                else if(wasEscape)
                {
                    Message += letter;
                }
                else
                {
                    Command += letter;
                }
            }

            return (Command, Message);
        }
        private static int? GetRandomNullableInt()
        {
            int? result = 0;
            var tmp = new Random().Next(int.MinValue, int.MaxValue);

            return result + tmp;
        }
        private static string GetPhotoUrl(Photo photo)
        {
            if (photo.Url != null)
            {
                return photo.Url.AbsoluteUri;
            }

            else if (photo.BigPhotoSrc != null)
            {
                return photo.BigPhotoSrc.AbsoluteUri;
            }
            else if(photo.Photo2560 != null)
            {
                return photo.Photo2560.AbsoluteUri;
            }
            
            else if (photo.Photo1280 != null)
            {
                return photo.Photo1280.AbsoluteUri;
            }

            else if (photo.Photo807 != null)
            {
                return photo.Photo807.AbsoluteUri;
            }

            else if (photo.Photo604 != null)
            {
                return photo.Photo604.AbsoluteUri;
            }

            else if (photo.Photo200 != null)
            {
                return photo.Photo200.AbsoluteUri;
            }

            else if (photo.Photo130 != null)
            {
                return photo.Photo130.AbsoluteUri;
            }

            else if(photo.Photo100 != null)
            {
                return photo.Photo100.AbsoluteUri;
            }

            else if (photo.Photo75 != null)
            {
                return photo.Photo75.AbsoluteUri;
            }

            else if (photo.Photo50 != null)
            {
                return photo.Photo50.AbsoluteUri;
            }
               
            else if (photo.SmallPhotoSrc != null)
            {
                return photo.SmallPhotoSrc.AbsoluteUri;
            }
             
            else
            {
                throw new Exception();
            }

        }
        private string GetFormatFromUrl(string request)
        {
            var mass = request.Split('.');

            return "." + mass[mass.Length - 1];
        }
        private string ChangeSymbols(string toParse, char toChange, char changed)
        {
            string result = "";
            foreach(var symbol in toParse)
            {
                if(symbol == toChange)
                {
                    result += changed;
                }
                else
                {
                    result += symbol;
                }
            }

            return result;
        }
        private void InitializeMemes()
        {
            foreach(var path in Directory.GetFiles(Directory.GetCurrentDirectory() + @"\memes\"))
            {
                var tmp = path.Split(@"\");

                _memes.Add(tmp[tmp.Length - 1].Split('.')[0]);
            }
        }
        private void InitializeAdmins()
        {
            var admins = File.ReadAllLines(Directory.GetCurrentDirectory() + @"\admins.txt");

            foreach(var admin in admins)
            {
                _admins.Add(long.Parse(admin));
            }

        }
        private void AddAdmin(string id)
        {
            long parsedId;

            if(long.TryParse(id, out parsedId))
            {
                if(_admins.Contains(parsedId))
                {
                    throw new AddRemoveException("Такой админ уже есть");
                }
                _admins.Add(parsedId);
                File.AppendAllText(Directory.GetCurrentDirectory() + @"\admins.txt", '\n' + id);

            }
            else
            {
                throw new AddRemoveException("Неверный ID");
            }
        }
        private void RemoveAdmins(string toRemove)
        {
            long idToRemove;

            if(!long.TryParse(toRemove, out idToRemove))
            {
                throw new AddRemoveException("Неверный id");
            }

            if(!_admins.Contains(idToRemove))
            {
                throw new AddRemoveException("Такого админа нет");
            }

            string[] info = File.ReadAllLines(Directory.GetCurrentDirectory() + @"\admins.txt");

            string[] result = info
                        .Where(x => x!= toRemove)
                        .ToArray();

            using(StreamWriter writer = new StreamWriter(Directory.GetCurrentDirectory() + @"\admins.txt"))
            {
                for(int index = 0; index < result.Length -1; index++)
                {
                    writer.WriteLine(result[index]);
                }

                writer.Write(result[result.Length - 1]);
            }

            _admins.Remove(idToRemove);
        }

        private double CompareMemes(string first, string second)
        {
            double matches = 0;

            if(first.Length > second.Length)
            {
                for(int i = 0; i < second.Length; i++)
                {
                    if(first[i] == second[i])
                    {
                        matches++;
                    }
                }

                return (matches / first.Length) * 100;
            }
            else
            {
                for(int i = 0; i < first.Length; i++)
                {
                    if (first[i] == second[i])
                    {
                        matches++;
                    }
                }
                return (matches / second.Length) * 100;
            }

        }

        private string GetSimilarMemes(string UnparsedRequest)
        {
            string request = ChangeSymbols(UnparsedRequest, ' ', '_');
            string result = "";

            foreach(var meme in _memes)
            {
                if(CompareMemes(meme, request) >= 40)
                {
                    result += ChangeSymbols(meme, '_', ' ') + "; ";
                }
            }

            return result;
        }
    }
}
