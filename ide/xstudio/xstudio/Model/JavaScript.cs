using System.ComponentModel;
using LiteDB;
using xstudio.Annotations;

namespace xstudio.Model
{
    public class JavaScript : INotifyPropertyChanged
    {
        public ObjectId Id { get; set; }
        public string Name { get; set; }
        public string Text { get; set; }
        public bool IsImport { get; set; }


        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged(string propertyName)
        {
            var handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));

        }
    }
}