using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Text;

namespace xstudio.Httpd
{
    public class HttpUtility
    {
        public static string UrlEncode(string str, Encoding e)
        {
            var sb = new StringBuilder();
            var raw = e.GetBytes(str);
            for (var i = 0; i < raw.Length; i++)
            {
                if ((raw[i] >= 0x30 && raw[i] <= 0x39) || // 0-9
                    (raw[i] >= 0x41 && raw[i] <= 0x5a) || // A-Z
                    (raw[i] >= 0x61 && raw[i] <= 0x7a) || // a-z
                    (raw[i] == 0x2d) || // -
                    (raw[i] == 0x2e) || // .
                    (raw[i] == 0x5f) || // _
                    (raw[i] == 0x7e)) // ~
                {
                    sb.Append((char) raw[i]);
                }
                else
                {
                    sb.Append('%');
                    sb.Append(raw[i].ToString("X2"));
                }
            }
            return sb.ToString();
        }

        public static string UrlDecode(string str, Encoding e)
        {
            // 1st pass
            var len = 0;
            for (var i = 0; i < str.Length; i++, len++)
            {
                if (str[i] == '%')
                    i += 2;
            }

            // 2nd pass
            var raw = new byte[len];
            for (int i = 0, q = 0; i < str.Length; i++, q++)
            {
                if (str[i] == '%')
                {
                    raw[q] = FromHex(str[i + 1], str[i + 2]);
                    i += 2;
                }
                else if (str[i] == '+')
                {
                    raw[q] = (byte) ' ';
                }
                else
                {
                    raw[q] = (byte) str[i];
                }
            }

            return e.GetString(raw).TrimEnd('\0');
        }

        private static byte FromHex(char high, char low)
        {
            return (byte) ((Uri.FromHex(high) << 4) | Uri.FromHex(low));
        }

        public static void ParseUrlEncodedString(string query, Dictionary<string, string> dic, Encoding e)
        {
            var items = query.Split('&');
            foreach (var item in items)
            {
                var tmp = item.Split('=');
                if (tmp.Length == 2)
                    dic[UrlDecode(tmp[0], e)] = UrlDecode(tmp[1], e);
                else if (tmp.Length == 1)
                    dic[UrlDecode(tmp[0], e)] = "";
            }
        }

        public static Dictionary<string, string> ParseUrlEncodedStringToDictionary(byte[] ascii_query)
        {
            return ParseUrlEncodedStringToDictionary(ascii_query, Encoding.UTF8);
        }

        public static Dictionary<string, string> ParseUrlEncodedStringToDictionary(byte[] ascii_query, Encoding e)
        {
            return ParseUrlEncodedStringToDictionary(Encoding.ASCII.GetString(ascii_query), e);
        }

        public static Dictionary<string, string> ParseUrlEncodedStringToDictionary(string query, Encoding e)
        {
            var dic = new Dictionary<string, string>();
            ParseUrlEncodedString(query, dic, e);
            return dic;
        }

        public static void ParseUrlEncodedString(string body, NameValueCollection collection, Encoding e)
        {
            var items = body.Split('&');
            foreach (var item in items)
            {
                var tmp = item.Split('=');
                if (tmp.Length == 2)
                    collection.Add(UrlDecode(tmp[0], e), UrlDecode(tmp[1], e));
                else if (tmp.Length == 1)
                    collection.Add(UrlDecode(tmp[0], e), "");
            }
        }

        public static NameValueCollection ParseUrlEncodedStringToNameValueCollection(string query, Encoding e)
        {
            var c = new NameValueCollection();
            ParseUrlEncodedString(query, c, e);
            return c;
        }
    }
}