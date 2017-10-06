using System;
using System.Collections.Generic;
using System.Linq;

namespace PointCloudViewer.Engine.Logic
{
    /// <summary>
    /// For debugging
    /// </summary>
    public sealed class AppConsole
    {
        private static readonly Lazy<AppConsole> Lazy =
        new Lazy<AppConsole>(() => new AppConsole());

        public static AppConsole Instance { get { return Lazy.Value; } }

        public List<string> MessagesList = new List<string>();

        private readonly List<string> _last=new List<string>();
        private int _messagesCount = 0;

        public List<String> GetLastMessages()
        {
            if (_messagesCount != MessagesList.Count)
            {
                _last.Clear();
                List<String> copy = new List<String>(MessagesList);
                const int displayLast = 3;
                for (int i = 0; i < displayLast; i++)
                {
                    if (copy.Count > 0 && copy.Last() != null)
                    {
                        _last.Add(copy.Last());
                        copy.Remove(copy.Last());
                    }
                }
                _messagesCount = MessagesList.Count;
            }
            return _last;
        }

        private AppConsole()
        {
        }

        public void WriteLine(string text)
        {
            if (MessagesList.Count > 100) MessagesList.Clear();
            MessagesList.Add(text);
        }
    }
}
