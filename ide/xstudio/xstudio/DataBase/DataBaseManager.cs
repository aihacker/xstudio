using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using LiteDB;
using xstudio.Model;

namespace xstudio.DataBase
{
    public class DataBaseManager
    {
        private static string ConnectionString
        {
            get
            {
                var filename = Path.Combine(Environment.CurrentDirectory, "studio.db");
                return string.Format("filename={0};password={1};", filename, "a384f42a655303cb");
            }
        }
#region 代码管理
        public static void CreateJavaScript(JavaScript javaScript)
        {
            using (var db = new LiteDatabase(ConnectionString))
            {
                var col = db.GetCollection<JavaScript>("js");
                col.Insert(javaScript);
            }
        }

        public static void SaveJavaScript(JavaScript javaScript)
        {
            using (var db = new LiteDatabase(ConnectionString))
            {
                var col = db.GetCollection<JavaScript>("js");
                col.Update(javaScript);
            }
        }

        public static IEnumerable<JavaScript> AllJavaScript()
        {
            using (var db = new LiteDatabase(ConnectionString))
            {
               var col = db.GetCollection<JavaScript>("js");
               return col.FindAll();
            }
        }

        public static JavaScript GetJavaScriptByName(string name)
        {
            using (var db = new LiteDatabase(ConnectionString))
            {
                var col = db.GetCollection<JavaScript>("js");
                return col.FindOne(Query.EQ("Name", name));
            }
        }

        public static void DeleteJavaScript(JavaScript javaScript)
        {
            using (var db = new LiteDatabase(ConnectionString))
            {
                var col = db.GetCollection<JavaScript>("js");
                col.Delete(javaScript.Id);
            }
        }

        public static void DeleteJavaScript(BsonValue id)
        {
            using (var db = new LiteDatabase(ConnectionString))
            {
                var col = db.GetCollection<JavaScript>("js");
                col.Delete(id);
            }
        }

#endregion

#region
        public static void SaveReflect(Reflect reflect)
        {
            new Task((() =>
            {
                foreach (var property in reflect.Propertys)
                {
                    if (reflect.Id.StartsWith("android") || reflect.Id.StartsWith("java") || reflect.Id.StartsWith("com.squareup") || reflect.Id.StartsWith("de.robv") || reflect.Id.StartsWith("okio") || reflect.Id.StartsWith("com.alibaba"))
                    {
                        if (property.Type == "method")
                        {
                            var desc = property.Desc;
                            var name = property.Name;

                            if (desc.StartsWith("public") || ! name.Contains("$"))
                            {
                                MethodWord mw = new MethodWord()
                                {
                                    Id = desc,
                                    Text = name
                                };
                                SaveMethod(mw);
                            }
                        }

                        if (property.Type == "field")
                        {
                            var desc = property.Desc;
                            var name = property.Name;

                            if (desc.StartsWith("public") || !name.Contains("$") || Char.IsLower(name.ToCharArray()[0]))
                            {
                                MethodWord mw = new MethodWord()
                                {
                                    Id = desc,
                                    Text = name
                                };
                                SaveMethod(mw);
                            }
                        }
                    }
                }
            })).Start();

            using (var db = new LiteDatabase(ConnectionString))
            {
                var col = db.GetCollection<Reflect>("reflect");
                col.Upsert(reflect);
            }
        }

        public static IEnumerable<Reflect> AllReflect()
        {
            using (var db = new LiteDatabase(ConnectionString))
            {
               var col = db.GetCollection<Reflect>("reflect");
               return col.FindAll();
            }
        }

        public static void DeleteReflect(BsonValue id)
        {
            using (var db = new LiteDatabase(ConnectionString))
            {
                var col = db.GetCollection<Reflect>("reflect");
                col.Delete(id);
            }
        }
#endregion

#region 
        public static void UploadLicense(string filename)
        {
            using (var db = new LiteDatabase(ConnectionString))
            {
                db.FileStorage.Upload("license", filename);
            }
        }

        public static byte[] GetLicense()
        {
            using (var db = new LiteDatabase(ConnectionString))
            {
                try
                {
                    var stream = db.FileStorage.OpenRead("license");
                    var memory = new MemoryStream();
                    stream.CopyTo(memory);
                    return memory.ToArray();
                }
                catch (Exception)
                {
                    return null;
                }
        
            }
        }

#endregion

#region
        public static IEnumerable<MethodWord> AllMethod()
        {
            using (var db = new LiteDatabase(ConnectionString))
            {
                var col = db.GetCollection<MethodWord>("method");
                return col.FindAll();
            }
        }

        public static void SaveMethod(MethodWord methodWord)
        {
            using (var db = new LiteDatabase(ConnectionString))
            {
                var col = db.GetCollection<MethodWord>("method");
                 col.Upsert(methodWord);
            }
        }


#endregion
    }
}