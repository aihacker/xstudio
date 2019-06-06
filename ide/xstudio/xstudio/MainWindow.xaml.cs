using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using HPSocketCS;
using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.CodeCompletion;
using ICSharpCode.AvalonEdit.Editing;
using Jint;
using Jint.Parser;
using Jint.Runtime;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Security;
using ServiceStack.Redis;
using xstudio.DataBase;
using xstudio.Httpd;
using xstudio.Model;

namespace xstudio
{
    /// <summary>
    ///     MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow
    {
        private readonly TaskFactory _factory = new TaskFactory();

        public readonly EventWaitHandle Wait = new AutoResetEvent(false);
        private CompletionWindow _completionWindow;
        private ObservableCollection<JavaScript> _scripts;

        private IRedisSubscription _subscription;

        public HttpListener Httpd;
        public Setting Setting = Setting.Load();

        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;
            SettingsFlyout.DataContext = Setting;
            Httpd = new HttpListener(HttpdCallback);
            Loaded += OnLoaded;
            ScripEditor.Drop += OnDrag;
            Closed += (sender, args) => { Httpd.Shutdown(); };
        }

        public ICommand SearchCommand
        {
            get
            {
                return new CommandBase(o =>
                {
                    var name = o as string;
                    if (!string.IsNullOrEmpty(name))
                    {
                        _factory.StartNew(s =>
                        {
                            try
                            {
                                var result = ClientManager.CallFunc(Setting.Package, Properties.Resources.JSReflect,
                                    name,
                                    string.Format("{0}@{1}:{2}", Setting.Auth, Setting.Host, Setting.Port));
                                var json = JToken.Parse(result);

                                if (json["code"].Value<int>() == 0)
                                {
                                    var reflect = json["data"].ToObject<Reflect>();
                                    DataBaseManager.SaveReflect(reflect);
                                }
                                else
                                {
                                    WriteLine(json["data"] + "");
                                }
                            }
                            catch (Exception exception)
                            {
                                WriteLine(exception.Message);
                            }

                            Dispatcher.Invoke(new Action(LoadAllReflect));
                        }, name);
                    }
                });
            }
        }

        public ICommand ButtonCommand
        {
            get
            {
                return new CommandBase(o =>
                {
                    if ("create" == (string) o)
                    {
                        var name = ShowInput("新建", "输入文件名：");
                        if (string.IsNullOrWhiteSpace(name) || name == "plugin")
                            return;
                        var code = new JavaScript {Name = name, Text = string.Empty};
                        DataBaseManager.CreateJavaScript(code);
                        LoadAllCode();
                    }

                    if ("delete" == (string) o)
                    {
                        var java = ScripEditor.Tag as JavaScript;
                        if (java != null)
                        {
                            DataBaseManager.DeleteJavaScript(java);
                            _scripts.Remove(java);
                            ScripEditor.Tag = null;
                            ScripEditor.Text = string.Empty;
                        }
                    }

                    if ("save" == (string) o)
                    {
                        var java = ScripEditor.Tag as JavaScript;
                        if (java != null)
                        {
                            DataBaseManager.SaveJavaScript(java);
                        }
                    }

                    if ("format" == (string) o)
                    {
                        var text = ScripEditor.Text;
                        if (!string.IsNullOrEmpty(text))
                        {
                            ScripEditor.Text = Utils.JsBeautify(text);
                        }
                    }

                    if ("setting" == (string) o)
                    {
                        SettingsFlyout.IsOpen = !SettingsFlyout.IsOpen;
                    }

                    if ("clear" == (string) o)
                    {
                        MessageEditor.Text = string.Empty;
                    }

                    if ("callback" == (string) o)
                    {
                        var java = ScripEditor.Tag as JavaScript;
                        if (java != null && java.Name == "plugin")
                        {
                            var item = ReflectTree.SelectedItem as TreeViewItem;
                            if (item != null && item.Parent is TreeViewItem)
                            {
                                Plugin(ScripEditor.Text, (string) ((TreeViewItem) item.Parent).Header, (string) item.Tag);
                            }
                            return;
                        }
                        CallFunc(ScripEditor.Text);
                    }

                    if ("inject" == (string) o)
                    {
                        HookFunc();
                    }
                });
            }
        }

        public string Plugin(string jscode, string clazz, string desc)
        {
            try
            {
                var engine = new Engine();
                engine.SetValue("log", new Action<object>(o => { WriteLine(o + string.Empty); }));

                var result = engine.Execute(jscode).GetValue("main").Invoke(clazz, desc);
                return result.Type == Types.String ? result.AsString() : null;
            }
            catch (Exception exception)
            {
                WriteLine("plugin: " + exception.Message);
            }

            return null;
        }
        private void HttpdCallback(HttpRequest req)
        {
            try
            {
                var json = JToken.Parse(Encoding.UTF8.GetString(req.RequestBody));
                var source = json["source"] + "";
                var param = json["param"];
                _factory.StartNew(() =>
                {
                    var result = ClientManager.CallFunc(Setting.Package, source, param,
                        string.Format("{0}@{1}:{2}", Setting.Auth, Setting.Host, Setting.Port));
                    req.Respond(HttpStatusCode.Ok, null, result);
                });
            }
            catch (Exception exception)
            {
                var code = -2;
                var data = exception.Message;
                req.Respond(HttpStatusCode.InternalServerError, null, JsonConvert.SerializeObject(new {code, data}));
            }
        }


        public static string RSAPublicKeyJava2DotNet(string publicKey)
        {
            RsaKeyParameters publicKeyParam = (RsaKeyParameters)PublicKeyFactory.CreateKey(Convert.FromBase64String(publicKey));
            return string.Format("<RSAKeyValue><Modulus>{0}</Modulus><Exponent>{1}</Exponent></RSAKeyValue>",
                Convert.ToBase64String(publicKeyParam.Modulus.ToByteArrayUnsigned()),
                Convert.ToBase64String(publicKeyParam.Exponent.ToByteArrayUnsigned()));
        }    
    

        private void OnLoaded(object sender, RoutedEventArgs routedEventArgs)
        {
            Debug.WriteLine(RSAPublicKeyJava2DotNet("MIGfMA0GCSqGSIb3DQEBAQUAA4GNADCBiQKBgQDENksAVqDoz5SMCZq0bsZwE+I3NjrANyTTwUVSf1+ec1PfPB4tiocEpYJFCYju9MIbawR8ivECbUWjpffZq5QllJg+19CB7V5rYGcEnb/M7CS3lFF2sNcRFJUtXUUAqyR3/l7PmpxTwObZ4DLG258dhE2vFlVGXjnuLs+FI2hg4QIDAQAB"));

            if (DataBaseManager.GetJavaScriptByName("plugin") == null)
            {
                DataBaseManager.CreateJavaScript(new JavaScript
                {
                    Name = "plugin",
                    Text = @"//代码生成插件，当视图选择项发生变化后触发该方法。返回的结果在信息里面被替换；
//调试时注意要选中视图里面的一个选项才会被触发；
var main = function (clazz, desc){
	log(desc);//调试输出
	return ''; //返回生成的代码；
}"
                });
            }

            var timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(2000)
            };

            timer.Tick += (o, args) => { Wait.Set(); };
            timer.Start();


            WriteLine("作者：12300735");
            WriteLine("群聊：1014726129");
            WriteLine("限制：" + "免费版本，API接口请求频率受限，解除限制请联系作者。");

            _scripts = new ObservableCollection<JavaScript>();
            ScripList.ItemsSource = _scripts;
            LoadAllCode();

            LoadAllReflect();

            ScripEditor.TextChanged += OnTextChanged;
            ScripEditor.TextArea.TextEntered += OnTextEntered;
            ScripEditor.TextArea.TextEntering += OnTextEntering;

            SettingsFlyout.ClosingFinished += OnClosingFinished;
        }

        public void Subscription()
        {
            _factory.StartNew(() =>
            {
                using (
                    var client =
                        RedisManager.GetRedisClient(string.Format("{0}@{1}:{2}", Setting.Auth, Setting.Host,
                            Setting.Port)))
                {
                    _subscription = client.CreateSubscription();
                    _subscription.OnMessage += OnMessage;

                    _subscription.SubscribeToChannels("console");
                }
            });
        }

        private void OnMessage(string s, string s1)
        {
            WriteLine(string.Format("{0}: {1}", s, s1));
        }

        public void WriteLine(string msg)
        {
            Dispatcher.BeginInvoke(new Action<string>(s =>
            {
                MessageEditor.Document.Insert(MessageEditor.Document.TextLength, msg + "\r\n");
                MessageEditor.ScrollToEnd();
            }), msg);
        }

        private void HookFunc()
        {
            try
            {
                if (_subscription == null)
                {
                    Subscription();
                }

                var scripts =
                    _scripts.Where(script => script.IsImport && script.Name != "plugin").Select(script => script.Text);

                var json = JsonConvert.SerializeObject(scripts);
                var result = ClientManager.HookFunc(json,
                    string.Format("{0}@{1}:{2}", Setting.Auth, Setting.Host, Setting.Port));

                WriteLine("broadcast: " + result);
            }
            catch (Exception exception)
            {
                WriteLine(exception.Message);
            }
        }

        private void CallFunc(string jscode)
        {
            try
            {
                if (_subscription == null)
                {
                    Subscription();
                }

                new JavaScriptParser().Parse(jscode);
                var result = ClientManager.CallFunc(Setting.Package, jscode, null,
                    string.Format("{0}@{1}:{2}", Setting.Auth, Setting.Host, Setting.Port));

                if (!string.IsNullOrEmpty(result))
                {
                    WriteLine(result);
                }
            }
            catch (Exception exception)
            {
                WriteLine(exception.Message);
            }
        }

        private void LoadAllCode()
        {
            if (_scripts.Count > 0)
                _scripts.Clear();
            foreach (var code in DataBaseManager.AllJavaScript())
            {
                _scripts.Add(code);
            }
            KeyWord.UpdateWord();
        }

        private void LoadAllReflect()
        {
            KeyWord.UpdateWord();
            ReflectTree.Items.Clear();

            foreach (var reflect in DataBaseManager.AllReflect())
            {
                var root = new TreeViewItem
                {
                    Header = reflect.Id
                };
                root.KeyUp += (sender, args) =>
                {
                    if (args.Key == Key.Delete)
                    {
                        DataBaseManager.DeleteReflect((string) ((TreeViewItem) sender).Header);
                        LoadAllReflect();
                    }
                };

                foreach (var property in reflect.Propertys)
                {
                    var color = Colors.Black;

                    switch (property.Type)
                    {
                        case "field":
                            color = Colors.Red;
                            break;
                        case "method":
                            color = Colors.Blue;
                            break;
                        case "constructor":
                            color = Colors.Green;
                            break;
                    }

                    var node = new TreeViewItem
                    {
                        Header = property.Name,
                        Tag = property.Desc,
                        ToolTip = property.Desc,
                        Foreground = new SolidColorBrush(color)
                    };

                    node.Selected += (sender, args) =>
                    {
                        if (((SolidColorBrush) ((TreeViewItem) sender).Foreground).Color == Colors.Blue)
                        {
                            var clazz = (string) ((TreeViewItem) ((TreeViewItem) sender).Parent).Header;
                            var desc = (string) ((TreeViewItem) sender).Tag;

                            var source = DataBaseManager.GetJavaScriptByName("plugin").Text;
                            if (!string.IsNullOrEmpty(source))
                            {
                                var rv = Plugin(source, clazz, desc);
                                if (! string.IsNullOrEmpty(rv))
                                {
                                    InfoEditor.Text = rv;
                                    return;
                                }
                            }

                            InfoEditor.Text =
                                Utils.GenericSimpleness(clazz, desc);
                        }
                        else
                        {
                            InfoEditor.Text = (string) ((TreeViewItem) sender).Tag;
                        }
                    };

                    root.Items.Add(node);
                }
                ReflectTree.Items.Add(root);
            }
        }

        private string ShowInput(string title, string message)
        {
            var settings = new MetroDialogSettings {AffirmativeButtonText = "确认", NegativeButtonText = "取消"};
            return this.ShowModalInputExternal(title, message, settings);
        }

        private void ShowMessage(string title, string message)
        {
            var settings = new MetroDialogSettings {AffirmativeButtonText = "确认", NegativeButtonText = "取消"};
            this.ShowModalMessageExternal(title, message, settings: settings);
        }

        #region event

        private void OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count > 0)
            {
                ScripEditor.Tag = (JavaScript) e.AddedItems[0];
                ScripEditor.Text = ((JavaScript) e.AddedItems[0]).Text;
            }
        }

        private void OnTextChanged(object sender, EventArgs eventArgs)
        {
            var text = ((TextEditor) sender).Text;
            var java = ((TextEditor) sender).Tag as JavaScript;

            if (java != null)
            {
                if (!string.Equals(java.Text, text))
                {
                    java.Text = text;
                }
            }
        }

        private void OnTextEntering(object sender, TextCompositionEventArgs ags)
        {
            if (ScripEditor.Tag == null)
            {
                ags.Handled = true;
            }

            if (ags.Text.Length > 0 && _completionWindow != null)
            {
                if (!char.IsLetterOrDigit(ags.Text[0]))
                {
                    _completionWindow.CompletionList.RequestInsertion(ags);
                }
            }
        }

        private void OnTextEntered(object sender, TextCompositionEventArgs ags)
        {
            var input = ags.Text;
            var document = ((TextArea) sender).Document;
            var caret = ((TextArea) sender).Caret;
            var line = document.GetLineByOffset(caret.Offset);

            var after = document.GetText(caret.Offset, line.EndOffset - caret.Offset);
            var before = document.GetText(line.Offset, caret.Offset - line.Offset);

            var extensions = new InputExtensions(after, before);
            if (!extensions.IsNotInsert())
            {
                if (input == "'")
                {
                    document.Insert(caret.Offset, "'");
                    caret.Offset--;
                }
                if (input == "\"")
                {
                    document.Insert(caret.Offset, "\"");
                    caret.Offset--;
                }
                if (input == "{")
                {
                    document.Insert(caret.Offset, "}");
                    caret.Offset--;
                }
                if (input == "(")
                {
                    document.Insert(caret.Offset, ")");
                    caret.Offset--;
                }
                if (input == "[")
                {
                    document.Insert(caret.Offset, "]");
                    caret.Offset--;
                }
            }

            var text = extensions.KeyWordInput();
            if (!string.IsNullOrEmpty(text))
            {
                StartCompletionWindow((TextArea) sender, text);
                return;
            }

            var list = extensions.MethodWordInput();
            if (list != null && list.Length == 3)
            {
                StartCompletionWindow((TextArea) sender, list[2], list[1]);
            }
        }

        private void StartCompletionWindow(TextArea area, string text, string token = null)
        {
            if (_completionWindow != null)
                return;
            List<Tuple<string, string>> showKeys = null;
            if (string.IsNullOrEmpty(token))
            {
                showKeys = KeyWord.GetKeyword(text);
            }
            else
            {
                showKeys = KeyWord.GetMethodword(text, token);
            }

            if (showKeys == null || showKeys.Count == 0)
            {
                return;
            }


            _completionWindow = new CompletionWindow(area) {ResizeMode = ResizeMode.NoResize};
            _completionWindow.Closed += (sender, args) => _completionWindow = null;


            _completionWindow.CloseWhenCaretAtBeginning = false;

            var data = _completionWindow.CompletionList.CompletionData;
            foreach (var key in showKeys)
            {
                data.Add(new CompletionData(key.Item1, key.Item2));
            }
            _completionWindow.Show();
        }

        private void OnClosingFinished(object sender, RoutedEventArgs routedEventArgs)
        {
            Setting.Upsert();
        }

        private void OnIsCheckedChanged(object sender, EventArgs e)
        {
            if (((ToggleSwitch) sender).IsChecked == true)
            {
                Httpd.Start("0.0.0.0", Convert.ToUInt16(Setting.Listen));
            }
            else
            {
                Httpd.Stop();
            }
        }


        private void OnDrag(object sender, DragEventArgs dragEventArgs)
        {
            var list = (string[]) dragEventArgs.Data.GetData(DataFormats.FileDrop);
            foreach (var name in list)
            {
            }
        }

        #endregion
    }
}