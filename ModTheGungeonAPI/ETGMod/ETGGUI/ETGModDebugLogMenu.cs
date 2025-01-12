﻿#pragma warning disable 0626
#pragma warning disable 0649

using UnityEngine;
using System.Collections.Generic;
using SGUI;

public class ETGModDebugLogMenu : ETGModMenu {

    public static ETGModDebugLogMenu Instance { get; protected set; }
    public ETGModDebugLogMenu() {
        Instance = this;
    }

    /// <summary>
    /// All debug logged text lines. Feel free to add your lines here!
    /// </summary>
    protected static List<LoggedText> _AllLoggedText = new List<LoggedText>();
    protected static int _LoggedTextAddIndex = 0;

    public static Dictionary<LogType, Color> Colors = new Dictionary<LogType, Color>() {
        { LogType.Log,       new Color(1f, 1f, 1f, 0.8f) },
        { LogType.Assert,    new Color(1f, 0.2f, 0.2f, 1f) },
        { LogType.Error,     new Color(1f, 0.4f, 0.4f, 1f) },
        { LogType.Exception, new Color(1f, 0.1f, 0.1f, 1f) },
        { LogType.Warning,   new Color(1f, 1f, 0.2f, 1f) }
    };

    public override void Start() {
        GUI = new SGroup {
            Visible = false,
            Border = 20f,
            OnUpdateStyle = (SElement elem) => elem.Fill(),
            AutoLayout = (SGroup g) => g.AutoLayoutVertical,
            ScrollDirection = SGroup.EDirection.Vertical,
            Children = {
                new SLabel("Press the \"F\" key to clear all messages but errors") { Foreground = Color.green },
                new SLabel("Press the \"B\" key to move to the bottom of the log") { Foreground = Color.green },
                new SLabel("Press the \"T\" key to move to the top of the log") { Foreground = Color.green },
            }
        };
    }

    public override void Update() {
        if (_LoggedTextAddIndex < _AllLoggedText.Count) {
            _AllLoggedText[_LoggedTextAddIndex].Start();
            ++_LoggedTextAddIndex;
        }
        if (Input.GetKeyDown(KeyCode.F))
        {
            var filteredLoggedText = new List<LoggedText>();
            var contentSize = 0f;
            foreach (var msg in _AllLoggedText)
            {
                if (msg.LogType == LogType.Error || msg.LogType == LogType.Exception)
                {
                    filteredLoggedText.Add(msg);
                    contentSize += msg.GUIMessage.Size.y;
                }
                else
                {
                    GUI.Children.Remove(msg.GUIMessage);
                    GUI.Children.Remove(msg.GUIStacktrace);
                }
            }
            _AllLoggedText = filteredLoggedText;
            Instance.GUI.UpdateStyle();
            GUI.Size.y = Mathf.Max(contentSize, 1020);
            GUI.ContentSize.y = Mathf.Max(contentSize, 1020);
            _LoggedTextAddIndex = _AllLoggedText.Count;
            GUI.ScrollPosition.y = 0f;
        }
        if (Input.GetKeyDown(KeyCode.B))
        {
            GUI.ScrollPosition.y = float.MaxValue;
        }
        if (Input.GetKeyDown(KeyCode.T))
        {
            GUI.ScrollPosition.y = 0f;
        }
    }

    public static void Log(string log) {
        Logger(log, LogType.Log);
    }

    public static void LogWarning(string log) {
        Logger(log, LogType.Warning);
    }

    public static void LogError(string log) {
        Logger(log, LogType.Error);
    }

    protected static string GetStackTrace() {
        string stack = System.Environment.StackTrace;
        int index = stack.LastIndexOf("at UnityEngine.Debug.Log", System.StringComparison.InvariantCulture);
        if (index == -1) {
            index = stack.IndexOf("at UnityEngine.Application.CallLogCallback", System.StringComparison.InvariantCulture);
        }
        if (index == -1) {
            index = stack.IndexOf("at ETGModDebugLogMenu.Logger", System.StringComparison.InvariantCulture);
        }
        return stack.Substring(1 + stack.IndexOf('\n', index));
    }

    public static void Logger(string text, LogType type) { Logger(text, null, type); }
    public static void Logger(string text, string stackTrace, LogType type) {
        if (string.IsNullOrEmpty(stackTrace)) {
            stackTrace = GetStackTrace();
        }
        LoggedText entry;

        if (_AllLoggedText.Count != 0) {
            entry = _AllLoggedText[_AllLoggedText.Count - 1];
            if (entry.LogMessage == text &&
                entry.Stacktace == stackTrace &&
                entry.LogType == type) {
                entry.LogCount++;
                return;
            }
        }

        entry = new LoggedText(text, stackTrace, type);
        _AllLoggedText.Add(entry);
    }

    protected class LoggedText {

        public string LogMessage;
        public string Stacktace;
        public LogType LogType;

        public bool IsStacktraceShown;

        protected int _LogCount = 1;
        public int LogCount {
            get {
                return _LogCount;
            }
            set {
                _LogCount = value;
                if (GUIMessage != null) {
                    GUIMessage.Text = LogMessageFormatted;
                }
            }
        }

        public string LogMessageFormatted {
            get {
                if (LogCount == 1) {
                    return LogMessage;
                }
                return "(" + LogCount + ") " + LogMessage;
            }
        }

        public SButton GUIMessage;
        public SLabel GUIStacktrace;

        public LoggedText(string logMessage, string stackTrace, LogType type) {
            LogMessage = logMessage;
            Stacktace = stackTrace;
            LogType = type;
        }

        public void Start() {
            if (Instance?.GUI == null) {
                return;
            }

            Color color = Colors[LogType];

            GUIMessage = new SButton(LogMessageFormatted) {
                Parent = Instance.GUI,
                Border = Vector2.zero,
                Background = new Color(0, 0, 0, 0),
                Foreground = color,
                OnClick = delegate (SButton button) {
                    IsStacktraceShown = !IsStacktraceShown;
                    Instance.GUI.UpdateStyle();
                },
                With = { new SFadeInAnimation() }
            };
            GUIStacktrace = new SLabel(Stacktace) {
                Parent = Instance.GUI,
                Foreground = color,
                OnUpdateStyle = delegate (SElement elem) {
                    elem.Size.y = IsStacktraceShown ? elem.Size.y : 0f;
                    elem.Visible = IsStacktraceShown;
                }
            };
        }

    }

}
