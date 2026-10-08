#if VRC_SDK_VRCSDK3
using System;
using System.Collections.Generic;
using System.Linq;
using BlackStartX.GestureManager.Editor.Data;
using BlackStartX.GestureManager.Editor.Library;
using BlackStartX.GestureManager.Editor.Modules.Vrc3.OpenSoundControl.VisualElements;
using BlackStartX.GestureManager.Editor.Modules.Vrc3.Params;
using BlackStartX.GestureManager.Editor.Modules.Vrc3.Vrc3Debug.Avatar;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace BlackStartX.GestureManager.Editor.Modules.Vrc3.OpenSoundControl
{
    public class OscModule
    {
        private readonly ModuleVrc3 _module;
        private const int SquareSize = VisualEpStyles.SquareSize;

        private const string VrcReceiverAddress = "127.0.0.1";
        private const int VrcListenerPort = 9000;
        private const int VrcSenderPort = 9001;

        internal GmgLayoutHelper.Toolbar ToolBar;
        private readonly OscSettings _settings;

        private UdpSender _sender;
        private UdpListener _listener;
        private readonly List<byte[]> _queue = new();

        private bool _customSelection;
        private string _customAddress = VrcReceiverAddress;
        private int _customListener = VrcSenderPort;
        private int _customSender = VrcListenerPort;
        private (string address, int listenerPort, int senderPort)? _forgotData;

        private IEnumerable<OscPacket.Message> Messages => _messages.Select(tuple => new OscPacket.Message(tuple.address, tuple.data));
        private readonly List<(string address, List<object> data)> _messages = new();
        private bool _sendingBundle;
        private int _addIndex = -1;
        private ulong _timeTag = 1;
        private string _dateTag = OscPacket.TimeTagToDateTime(1).ToString("F");

        private static string VrcReceiverPortUsed => $"VRChat 默认端口 {VrcListenerPort} 已被占用。";
        internal bool Enabled => _listener != null && _sender != null;

        private readonly Dictionary<string, EndpointControl> _dataDictionary = new();
        private readonly List<EndpointControl> _chronological = new();

        public OscModule(ModuleVrc3 module)
        {
            _module = module;
            _settings = new OscSettings(_module);
        }

        public void Update()
        {
            lock (_queue)
            {
                if (_queue.Count == 0) return;
                foreach (var bytes in _queue) OnListenerBytes(bytes);
                _queue.Clear();
            }
        }

        public void ControlPanel()
        {
            if ((_listener != null || _sender != null) && !Enabled) Stop();

            GUILayout.Label("OSC 调试", GestureManagerStyles.GuiHandTitle);
            if (_module.DummyMode != null)
            {
                GUILayout.Space(28);
                GUILayout.Label($"在 {_module.DummyMode.ModeName} 模式下无法使用 OSC 调试！", GestureManagerStyles.Centered);
                GUILayout.Space(28);
            }
            else if (!Enabled)
            {
                GUILayout.Label("OSC 调试当前未运行，请点击下方按钮启动！", GestureManagerStyles.Centered);

                GUILayout.Space(15);
                var isCustomMode = _customSelection && _settings.Stable;

                using (new GUILayout.HorizontalScope())
                using (new GmgLayoutHelper.FlexibleScope())
                {
                    if (isCustomMode)
                    {
                        using (new GUILayout.VerticalScope(EditorStyles.helpBox))
                        {
                            GUILayout.Label("Listening", GestureManagerStyles.GuiDebugTitle);
                            _customListener = EditorGUILayout.IntField(_customListener);
                        }

                        using (new GmgLayoutHelper.FlexibleScope())
                        using (new GUILayout.VerticalScope(EditorStyles.helpBox))
                        {
                            GUILayout.Label("Sending", GestureManagerStyles.GuiDebugTitle);
                            using (new GUILayout.HorizontalScope())
                            {
                                _customAddress = EditorGUILayout.TextField(_customAddress);
                                _customSender = EditorGUILayout.IntField(_customSender);
                            }
                        }

                        using (new GUILayout.VerticalScope())
                        {
                            var option = GUILayout.Height(20);
                            if (GUILayout.Button("返回！", option)) _customSelection = false;
                            using (new GmgLayoutHelper.GuiEnabled(_customListener >= 0 && _customSender >= 0))
                                if (GmgLayoutHelper.Button("启动！", Color.green, option))
                                    Start(_customListener, _customAddress, _customSender);
                        }
                    }
                    else
                    {
                        using (new GmgLayoutHelper.GuiEnabled(_settings.Stable))
                        {
                            if (GmgLayoutHelper.DebugButton("使用 VRChat 端口启动")) Start(VrcListenerPort, VrcReceiverAddress, VrcSenderPort);
                            GUILayout.FlexibleSpace();
                            if (GmgLayoutHelper.DebugButton("使用自定义端口启动")) _customSelection = true;
                        }
                    }
                }

                GUILayout.Space(isCustomMode ? -2 : 11);
            }
            else
            {
                GUILayout.Label("OSC 调试正在运行，你可以点击橙色按钮停止它！", GestureManagerStyles.Centered);

                GUILayout.Space(4);
                using (new GUILayout.HorizontalScope())
                using (new GmgLayoutHelper.FlexibleScope())
                {
                    using (new GUILayout.VerticalScope(EditorStyles.helpBox))
                    {
                        GUILayout.Label("监听端口", GestureManagerStyles.GuiDebugTitle);
                        GUILayout.Label($"{_listener?.Port}", GestureManagerStyles.Centered);
                    }

                    using (new GmgLayoutHelper.FlexibleScope())
                    using (new GUILayout.VerticalScope())
                    {
                        GUILayout.Space(8);
                        if (GmgLayoutHelper.Button("Stop", RadialMenuUtility.Colors.RestartButton, GUILayout.Height(30), GUILayout.Width(60))) Stop();
                    }

                    using (new GUILayout.VerticalScope(EditorStyles.helpBox))
                    {
                        GUILayout.Label("发送端口", GestureManagerStyles.GuiDebugTitle);
                        GUILayout.Label($"{_sender?.Port}", GestureManagerStyles.Centered);
                    }
                }

                GUILayout.Space(10);
            }

            GUILayout.Space(9);

            using (new GUILayout.HorizontalScope())
            using (new GmgLayoutHelper.FlexibleScope())
                if (GmgLayoutHelper.DebugButton(!_module.DebugOscWindow ? Vrc3AvatarDebugWindow.Text.D.Button : Vrc3AvatarDebugWindow.Text.W.Button))
                    _module.SwitchDebugOscView();

            GUILayout.Space(6);
        }

        private void Start(int listenerPort, string sendAddress, int senderPort)
        {
            if (!_settings.Setup()) return;

            try
            {
                _listener = new UdpListener(listenerPort, AddQueue);
                _sender = new UdpSender(sendAddress, senderPort);
                _customSelection = false;
            }
            catch (Exception)
            {
                var messageString = listenerPort == VrcListenerPort ? VrcReceiverPortUsed : $"端口 {listenerPort} 已被占用。";
                EditorUtility.DisplayDialog("手势管理器", messageString, ":c");
            }
        }

        internal void Stop(bool clear = true)
        {
            _listener?.Close();
            _listener = null;
            _sender?.Close();
            _sender = null;
            if (!clear) return;
            lock (_queue) _queue.Clear();
            ClearElements();
        }

        private void ClearElements()
        {
            foreach (var endpointControl in _chronological) endpointControl.Clear();
            _dataDictionary.Clear();
            _chronological.Clear();
        }

        public void Forget()
        {
            _forgotData = (_sender.Address, _listener.Port, _sender.Port);
            Stop(false);
        }

        public void Resume()
        {
            if (_forgotData == null) return;
            Start(_forgotData.Value.listenerPort, _forgotData.Value.address, _forgotData.Value.senderPort);
            _forgotData = null;
        }

        private void AddQueue(byte[] bytes)
        {
            lock (_queue) _queue.Add(bytes);
        }

        private void OnListenerBytes(byte[] bytes)
        {
            foreach (var message in OscPacket.GetMessages(bytes)) OnMessage(message);
        }

        public void DebugLayout(VisualElement root, VisualEpContainer holder, float width)
        {
            if (_listener == null || _sender == null) SettingsLayout();
            else Layout(root, holder, width);
        }

        private void SettingsLayout()
        {
            if (!_settings.Loaded)
            {
                GUILayout.Label("Settings", GestureManagerStyles.Header);
                GUILayout.Space(5);
                GUILayout.Label("你可以立即启动 OSC 调试，它将使用通用设置！", GestureManagerStyles.Centered);
                GUILayout.Space(10);
                GUILayout.Label("或者你也可以加载自己的 OSC 配置文件！", GestureManagerStyles.Centered);
                GUILayout.Space(30);
                using (new GUILayout.HorizontalScope())
                using (new GmgLayoutHelper.FlexibleScope())
                    if (GmgLayoutHelper.DebugButton("加载配置"))
                        _settings.Load();

                GUILayout.Space(20);
            }
            else if (_settings.Layout()) _settings.Clean();
        }

        private void Layout(VisualElement root, VisualEpContainer holder, float width)
        {
            GmgLayoutHelper.MyToolbar(ref ToolBar, new (string, Action)[]
            {
                ("Receive", () => ReceiveLayout(root, holder, width)),
                ("Send", SendLayout)
            });
        }

        private void ReceiveLayout(VisualElement root, VisualEpContainer holder, float width)
        {
            holder.Render(root);
            if (holder.parent != root) root.Add(holder);
            GUILayout.Label("已接收的 OSC 数据包消息", GestureManagerStyles.Header);

            if (_chronological.Count != 0) GmgLayoutHelper.HorizontalGrid(holder, width, SquareSize, SquareSize, 5, _chronological, ShowData);
            else NoDataLayout();
        }

        private static void ShowData(VisualEpContainer holder, Rect rect, EndpointControl element)
        {
            if (Event.current.type == EventType.Layout || Event.current.type == EventType.Used) return;

            holder.RenderMessage(rect, element);
        }

        private void SendLayout()
        {
            if (GmgLayoutHelper.TitleButton($"发送自定义 {(_sendingBundle ? "数据包" : "消息")}", _sendingBundle ? "消息" : "数据包")) _sendingBundle = !_sendingBundle;

            if (_messages.Count == 0) AddMessage();
            if (_sendingBundle) BundleLayout();
            else SingleMessageLayout();

            GUILayout.Space(15);
            using (new GUILayout.HorizontalScope())
            using (new GmgLayoutHelper.FlexibleScope())
                if (GmgLayoutHelper.DebugButton("Send"))
                    _sender.Send(_sendingBundle ? new OscPacket(_timeTag, Messages.ToList()).GetBytes() : Messages.First().GetBytes());
        }

        private void AddMessage() => _messages.Add(("/avatar/parameters/", new List<object>()));

        private void SingleMessageLayout()
        {
            using (new GUILayout.VerticalScope(EditorStyles.helpBox)) MessageLayout(0);
        }

        private void BundleLayout()
        {
            if (_timeTag != (_timeTag = GmgLayoutHelper.ULongField("时间戳: ", _timeTag))) OnNewTimeTag();
            GUILayout.Label(_dateTag, EditorStyles.helpBox);
            for (var i = 0; i < _messages.Count; i++) MessageBundleLayout(ref i);
            using (new GUILayout.VerticalScope(EditorStyles.helpBox))
                if (GUILayout.Button("新建 OSC 消息"))
                    AddMessage();
        }

        private void OnNewTimeTag() => _dateTag = OscPacket.TimeTagToDateTime(_timeTag).ToString("F");

        private void MessageBundleLayout(ref int i)
        {
            using (new GUILayout.VerticalScope(EditorStyles.helpBox))
            {
                if (GmgLayoutHelper.TitleButton($"OSC 消息 [{i}]", "-", 20)) _messages.RemoveAt(i--);
                else MessageLayout(i);
            }
        }

        private void MessageLayout(int mId)
        {
            var (address, data) = _messages[mId];

            address = EditorGUILayout.TextField("地址: ", address);
            for (var i = 0; i < data.Count; i++)
            {
                using (new GUILayout.HorizontalScope())
                {
                    switch (data[i])
                    {
                        case bool boolObject:
                            data[i] = EditorGUILayout.Toggle("布尔: ", boolObject);
                            break;
                        case float floatObject:
                            data[i] = EditorGUILayout.FloatField("浮点: ", floatObject);
                            break;
                        case char charObject:
                            data[i] = (char)EditorGUILayout.IntField("字符: ", charObject);
                            GUILayout.Label(charObject.ToString(), GUILayout.Width(50));
                            break;
                        case string stringObject:
                            data[i] = EditorGUILayout.TextField("字符串: ", stringObject);
                            break;
                        case int intObject:
                            data[i] = EditorGUILayout.IntField("整数: ", intObject);
                            break;
                        default:
                            data.RemoveAt(i);
                            i--;
                            continue;
                    }

                    if (!GUILayout.Button("-", GUILayout.Width(20))) continue;
                    data.RemoveAt(i);
                    i--;
                }
            }

            using (new GUILayout.HorizontalScope())
            {
                if (_addIndex != mId)
                {
                    EditorGUILayout.LabelField("", "");
                    if (GUILayout.Button("+", GUILayout.Width(20))) _addIndex = mId;
                }
                else
                {
                    if (GUILayout.Button("添加布尔值")) Add(data, false);
                    if (GUILayout.Button("添加整数")) Add(data, new int());
                    if (GUILayout.Button("添加浮点数")) Add(data, new float());
                    if (GUILayout.Button("添加字符串")) Add(data, "");
                    if (GUILayout.Button("添加字符")) Add(data, 'a');
                    if (GUILayout.Button("x", GUILayout.Width(20))) _addIndex = -1;
                }
            }

            _messages[mId] = (address, data);
        }

        private void Add(ICollection<object> data, object value)
        {
            data.Add(value);
            _addIndex = -1;
        }

        private void NoDataLayout() => ErrorLayout("尚未收到任何数据！", $"你的应用程序是否在向端口 {_listener.Port} 发送数据？");

        private static void ErrorLayout(string red, string white)
        {
            using (new GmgLayoutHelper.FlexibleScope())
            {
                GUILayout.Space(100);
                GUILayout.Label(red, GestureManagerStyles.TextError);
                GUILayout.Space(20);
                GUILayout.Label(white, GestureManagerStyles.Centered);
                GUILayout.Space(100);
            }
        }

        private void OnMessage(OscPacket.Message message)
        {
            _settings.OnMessage(message);
            if (!_dataDictionary.TryGetValue(message.Address, out var control)) _dataDictionary[message.Address] = control = new EndpointControl(message.Address, _chronological);
            control.OnMessage(message);
        }

        public void OnParameterChange(Vrc3Param param, float value)
        {
            if (_sender == null) return;
            var message = _settings.OnParameterChange(param, value);
            if (message == null) return;
            _sender.Send(message.GetBytes());
        }
    }
}
#endif