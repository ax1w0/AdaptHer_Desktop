using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Text;
using System.Threading.Tasks;

namespace AdaptHER.Class
{
    public static class EmojiResourceHelper
    {
        private static Dictionary<string, string> _emojiDictionary;

        public static Dictionary<string, string> LoadEmojiDictionary()
        {
            if (_emojiDictionary != null)
                return _emojiDictionary;

            _emojiDictionary = new Dictionary<string, string>();

            if (Application.Current.Resources["EmojiList"] is string[] emojiList)
            {
                foreach (string item in emojiList)
                {
                    string[] parts = item.Split(',');
                    if (parts.Length == 2)
                    {
                        _emojiDictionary[parts[0]] = parts[1];
                    }
                }
            }

            return _emojiDictionary;
        }

        public static List<string> GetAllEmojiSymbols()
        {
            if (Application.Current.Resources["EmojiSymbols"] is string[] symbols)
            {
                return symbols.ToList();
            }

            return LoadEmojiDictionary().Keys.ToList();
        }

        public static string GetEmojiName(string emoji)
        {
            var dict = LoadEmojiDictionary();
            return dict.ContainsKey(emoji) ? dict[emoji] : "неизвестно";
        }

        public static List<string> GetRandomEmojis(int count)
        {
            var allEmojis = GetAllEmojiSymbols();
            var random = new System.Random();
            return allEmojis.OrderBy(x => random.Next()).Take(count).ToList();
        }
    }
}