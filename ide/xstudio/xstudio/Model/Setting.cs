using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using LiteDB;
using Newtonsoft.Json;
using xstudio.Annotations;


namespace xstudio.Model
{
    public class Setting : INotifyPropertyChanged
    {
        public int Id { get; set; }
        public string Host { set; get; }
        public string Port { set; get; }
        public string Auth { set; get; }
        public string Package { set; get; }
        public string Listen { set; get; }
        public bool Start { set; get; }
      

        public event PropertyChangedEventHandler PropertyChanged;


        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged(string propertyName)
        {
            Debug.WriteLine(propertyName);
            var handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }

        public static Setting Load()
        {
            var dbpath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "android.db");
            using (var db = new LiteDatabase(dbpath))
            {
                var col = db.GetCollection<Setting>("setting");
                var set = col.FindById(1);
                if (set == null)
                {
                    col.Insert(1, new Setting() { Host = "127.0.0.1", Port = "6379", Auth = "android", Listen = "8000", Start = false });
                    return col.FindById(1);
                }

                return set;
            }
        }

        public void Upsert()
        {
            var dbpath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "android.db");
            using (var db = new LiteDatabase(dbpath))
            {
                var col = db.GetCollection<Setting>("setting");
                col.Upsert(1, this);
            }
        }

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}