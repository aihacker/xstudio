using System;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using ServiceStack.Common.Extensions;

namespace xstudio
{
    public class InputExtensions
    {
        private readonly string _before = null;
        private readonly string _after = null;
        public InputExtensions(string after, string before)
        {
            _before = before;
            _after = after;
        }

        public bool IsQuote()
        {
            return _before.Count(c => c == '\"' || c == '\'') % 2 == 1 || _after.Count(c => c == '\"' || c == '\'') % 2 == 1;
        }

        public bool IsNotInsert()
        {
            return Regex.IsMatch(_after, @"^\w");
        }

        public string KeyWordInput()
        {
            return IsQuote() || IsNotInsert() ? string.Empty : Regex.Match(_before, @"(?:^|[^\w\.])([A-Za-z_])$").Groups[1].Value;
        }

        public string[] MethodWordInput()
        {
            if ( IsQuote() || IsNotInsert())
            {
                return null;
            }
            else
            {
                var match = Regex.Match(_before, @"([A-Za-z_]+)\.([A-Za-z_])$");
                string[] list = new string[match.Groups.Count];
                for (var i = 0; i < match.Groups.Count; i++)
                {
                    list[i] = match.Groups[i].Value;
                }
                return list;
            }
        }

    }
}