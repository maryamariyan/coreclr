// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

// Do not remove this, it is needed to retain calls to these conditional methods in release builds
#define DEBUG

namespace System.Diagnostics
{
    /// <summary>
    /// Provides a default implementation for an internal portion of Debug class
    /// that can be overriden to help link Debug with Trace Listeners
    /// </summary>
    public class DebugInternal
    {
        public virtual void Write(string message)
        {
            lock (s_lock)
            {
                if (message == null)
                {
                    Debug.Provider.Write(string.Empty);
                    return;
                }
                if (_needIndent)
                {
                    message = GetIndentString() + message;
                    _needIndent = false;
                }
                Debug.Provider.Write(message);
                if (message.EndsWith(Environment.NewLine))
                {
                    _needIndent = true;
                }
            }
        }
        
        public virtual void WriteLine(string message)
        {
            Write(message + Environment.NewLine);
        }

        public virtual void OnIndentLevelChanged(int indentLevel) { }

        public virtual void OnIndentSizeChanged(int indentSize) { }

        private static readonly object s_lock = new object();

        private bool _needIndent = true;

        private string _indentString;

        private string GetIndentString()
        {
            int indentCount = Debug.IndentSize * Debug.IndentLevel;
            if (_indentString?.Length == indentCount)
            {
                return _indentString;
            }
            return _indentString = new string(' ', indentCount);
        }
    }
}
