using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using VKBOT.Executors;
using VkNet;
using VkNet.Exception;
using VkNet.Model.Attachments;

namespace VKBOT
{
    abstract class Executor
    {
        static protected VkApi Api;
        static protected long? UserID;
        static protected Photo photo;
        static protected string requestMessage;
        static protected long PersonID;


        public static Executor GetExecutor(string message, long? userId, long personID, VkApi api, params object[] additional)
        {
            Api = api;
            UserID = userId;
            PersonID = personID;
            photo = additional[0] as Photo;
 

            var request = ParseCommand(message);
            requestMessage = request.Message;

            if(request.Command == "мем")
            {
                return new MemesExecutor();
            }
            else if(request.Command == "добавить" && MemesBot.IsAdmin(UserID))
            {
                return new AddMemeExecutor();
            }
            else if(request.Command == "+админ" && MemesBot.IsSuperAdmin(UserID))
            {
                return new AddAdminExecutor();
            }
            else if(request.Command == "-админ" && MemesBot.IsSuperAdmin(UserID))
            {
                return new RemoveAdminExecutor();
            }
            else if(request.Command == "хелп")
            {
                return new HelpExecutor();
            }
            else
            {
                return new DontKnowExecutor();
            }
        }
        public abstract void Execute();

        static protected (string Command, string Message) ParseCommand(string request)
        {
            bool wasEscape = false;
            string Command = "", Message = "";

            foreach (char letter in request)
            {
                if (letter == ' ' && !wasEscape)
                {
                    wasEscape = true;
                    continue;
                }
                else if (wasEscape)
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
        protected string GetPathToMeme(string request)
        {
            if (File.Exists(Directory.GetCurrentDirectory() + @"\memes\" + request + ".jpg"))
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
                throw new HasNotFoundException(request);
            }
        }
        protected static string GetPhotoUrl(Photo photo)
        {
            if (photo.Url != null)
            {
                return photo.Url.AbsoluteUri;
            }

            else if (photo.BigPhotoSrc != null)
            {
                return photo.BigPhotoSrc.AbsoluteUri;
            }
            else if (photo.Photo2560 != null)
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

            else if (photo.Photo100 != null)
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
        protected string GetFormatFromUrl(string request)
        {
            var mass = request.Split('.');

            return "." + mass[mass.Length - 1];
        }
        protected string ChangeSymbols(string toParse, char toChange, char changed)
        {
            string result = "";
            foreach (var symbol in toParse)
            {
                if (symbol == toChange)
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

        protected void AddAdmin(string id)
        {
            long parsedId;

            if (long.TryParse(id, out parsedId))
            {
                if(MemesBot.Admins.Contains(parsedId))
                {
                    throw new AddRemoveException("Такой админ уже есть");
                }
                File.AppendAllText(Directory.GetCurrentDirectory() + @"\admins.txt", '\n' + id);
                MemesBot.Admins.Add(parsedId);
            }
            else
            {
                throw new AddRemoveException("Неверный ID");
            }
        }
        protected void RemoveAdmins(string toRemove)
        {
            long idToRemove;

            if (!long.TryParse(toRemove, out idToRemove))
            {
                throw new AddRemoveException("Неверный id");
            }

            if(!MemesBot.Admins.Contains(idToRemove))
            {
                throw new AddRemoveException("Такого админа нет");
            }

            string[] info = File.ReadAllLines(Directory.GetCurrentDirectory() + @"\admins.txt");

            string[] result = info
                        .Where(x => x != toRemove)
                        .ToArray();

            using (StreamWriter writer = new StreamWriter(Directory.GetCurrentDirectory() + @"\admins.txt"))
            {
                for (int index = 0; index < result.Length - 1; index++)
                {
                    writer.WriteLine(result[index]);
                }

                writer.Write(result[result.Length - 1]);
            }

            MemesBot.Admins.Remove(idToRemove);
        }

        protected double CompareMemes(string first, string second)
        {
            double matches = 0;

            if (first.Length > second.Length)
            {
                for (int i = 0; i < second.Length; i++)
                {
                    if (first[i] == second[i])
                    {
                        matches++;
                    }
                }

                return (matches / first.Length) * 100;
            }
            else
            {
                for (int i = 0; i < first.Length; i++)
                {
                    if (first[i] == second[i])
                    {
                        matches++;
                    }
                }
                return (matches / second.Length) * 100;
            }

        }

        protected string GetSimilarMemes(string UnparsedRequest, HashSet<string> Memes)
        {
            string request = ChangeSymbols(UnparsedRequest, ' ', '_');
            string result = "";

            foreach (var meme in Memes)
            {
                if (CompareMemes(meme, request) >= 40)
                {
                    result += ChangeSymbols(meme, '_', ' ') + "; ";
                }
            }

            return result;
        }
        protected static int? GetRandomNullableInt()
        {
            int? result = 0;
            var tmp = new Random().Next(int.MinValue, int.MaxValue);

            return result + tmp;
        }


    }
}
