using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Jint;
using xstudio.Properties;

namespace xstudio
{
    public class Utils
    {
        //public static [Ljava.lang.String; android.text.TextUtils.split(java.lang.String,java.util.regex.Pattern)

        public static Dictionary<string, string> BaseType = new Dictionary<string, string>
        {
            {"boolean", "java.lang.Boolean"},
            {"byte", "java.lang.Byte"},
            {"char", "java.lang.Character"},
            {"short", "java.lang.Short"},
            {"int", "java.lang.Integer"},
            {"long", "java.lang.Long"},
            {"float", "java.lang.Float"},
            {"double", "java.lang.Double"}
        };

        public static string JsBeautify(string js)
        {
            try
            {
                var engine = new Engine();
                return engine.Execute(Resources.JSBeautify).Invoke("js_beautify", js).AsString();
            }
            catch (Exception)
            {
                return js;
            }
        }



        public static string GenericSimpleness(string clazz, string desc)
        {
            //public static java.lang.String android.util.Base64.encodeToString(byte[],int,int,int)
            var sb = new StringBuilder();

            sb.AppendLine(desc);

            sb.AppendLine("");
            sb.AppendLine("//call");
            sb.AppendLine("var ref = org.joor.Reflect");
            sb.AppendLine("var main = function(lpparm, ctx, param) {");


            var param = Regex.Matches(desc, @"[\w\.\[\]\$]+(?=[,\)])");

            for (var i = 0; i < param.Count; i++)
            {
                var type = param[i] + string.Empty;

                if (type.EndsWith("[]"))
                {
                    var tmp = type.Replace("[]", null);
                    if (BaseType.ContainsKey(tmp))
                    {
                        sb.AppendLine(
                            string.Format("    var param_{0} = java.lang.reflect.Array.newInstance({1}.TYPE, 0);", i,
                                BaseType[tmp]));
                        if (tmp == "byte")
                        {
                            sb.AppendLine(string.Format("    //var param_{0} = android.util.Base64.decode('',2);", i));
                        }
                    }
                    else
                    {
                        sb.AppendLine(string.Format("    var clazz_{0} = ref.on('{1}',ctx.getClassLoader()).type();", i,
                            tmp));
                        sb.AppendLine(
                            string.Format("    var param_{0} = java.lang.reflect.Array.newInstance(clazz_{0}, 0);", i));
                    }
                }
                else
                {
                    if (BaseType.ContainsKey(type))
                    {
                        sb.AppendLine(string.Format(@"    var param_{0} = new {1}('');", i, BaseType[type]));
                    }
                    else
                    {
                        if (type == "java.lang.String")
                        {
                            sb.AppendLine(string.Format(@"    var param_{0} = ''; //{1}", i, type));
                        }
                        else
                        {
                            sb.AppendLine(
                                string.Format(
                                    @"    var param_{0} = ref.on('{1}',ctx.getClassLoader()).create().get();", i, type));
                        }
                    }
                }
            }

            var args = new string[param.Count + 1];
            for (var j = param.Count ; j >= 1; j--)
            {
                args[j] = "param_" + (j-1);
            }
            var name = Regex.Match(desc, @"\w+(?=\()").Value;
            args[0] = string.Format("'{0}'", name);

            sb.AppendLine("    ");
            sb.AppendLine(string.Format("    var result = ref.on('{0}', ctx.getClassLoader()).call({1}).get();", clazz,
                string.Join(" ,", args)));
            sb.AppendLine("    return result;");

            sb.Append("}");
            sb.AppendLine("    ");
            sb.AppendLine("    ");

            var func = Regex.Match(desc, @"[\w\$]+\(.*?\)").Value;

            sb.Append(@"//hook
var find = function(lpparm, ctx, param) {
    var methods = org.joor.Reflect.on('#clazz', ctx.getClassLoader()).type().getDeclaredMethods();
    var func = '#func';
    for (var i = 0; i < methods.length; i++) {
        if (methods[i].toString().contains(func)){
            return methods[i];
        }
    }
    return null;
}
var before_func = function(param) {
    //todo
    
}

var after_func = function(param) {
    //todo
    
    
}".Replace("#clazz", clazz).Replace("#desc", desc).Replace("#func", func));

            return sb.ToString();
        }
    }
}