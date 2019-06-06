using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using xstudio.Model;

namespace xstudio
{
    public class KeyWord
    {
        public static readonly string[] Keyword = { "abstract", "boolean", "break", "byte", "case", "catch", "char", "class", "const", "continue", "debugger", "default", "delete", "do", "double", "else", "enum", "export", "extends", "false", "final", "finally", "float", "for", "function", "goto", "if", "implements", "import", "in", "instanceof", "int", "interface", "long", "native", "new", "null", "package", "private", "protected", "public", "return", "short", "static", "super", "switch", "synchronized", "this", "throw", "throws", "transient", "true", "try", "typeof", "var", "void", "volatile", "while", "with", "console" };
        public static List<MethodWord> Methodword = new List<MethodWord>(); 
        
        public static List<Tuple<string,string>> GetKeyword(string input)
        {
            return Keyword.Where((s => s.StartsWith(input))).Select((s => new Tuple<string,string>(s,""))).ToList();
        }

        public static void UpdateWord()
        {
            Methodword.Clear();
            Methodword.AddRange(DataBase.DataBaseManager.AllMethod().ToList());
        }

        public static List<Tuple<string, string>> GetMethodword(string input, string token)
        {
            return
                Methodword.Where((word => word.Text.StartsWith(input)))
                    .Select((word => new Tuple<string, string>(word.Text, word.Id)))
                    .ToList();
        }
    }
}