// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

// Do not remove this, it is needed to retain calls to these conditional methods in release builds
#define DEBUG
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;

namespace System.Diagnostics
{
    public static class DebugDelegateWrapper
    {
        internal static readonly object critSec = new object();
        static DebugDelegateWrapper()
        {
            AutoFlushGet = () => true;
            AutoFlushSet = (value) => {};
            IndentLevelGet = () => DebugInternal.IndentLevelGet();
            IndentLevelSet = (value) => { DebugInternal.IndentLevelSet(value); };
            IndentSizeGet = () => DebugInternal.IndentSizeGet();
            IndentSizeSet = (value) => { DebugInternal.IndentSizeSet(value); };
            
            AssertOverload1 = DebugInternal.Assert;
            AssertOverload2 = DebugInternal.Assert;
            AssertOverload3 = DebugInternal.Assert;
            Close = DebugInternal.Close;
            FailOverload1 = DebugInternal.Fail;
            FailOverload2 = DebugInternal.Fail;
            Flush = DebugInternal.Flush;
            Indent = DebugInternal.Indent;
            Unindent = DebugInternal.Unindent;
            WriteOverload1 = DebugInternal.Write;
            WriteOverload2 = DebugInternal.Write;
            WriteOverload3 = DebugInternal.Write;
            WriteOverload4 = DebugInternal.Write;
            WriteLineOverload1 = DebugInternal.WriteLine;
            WriteLineOverload2 = DebugInternal.WriteLine;
            WriteLineOverload3 = DebugInternal.WriteLine;
            WriteLineOverload4 = DebugInternal.WriteLine;
        }

        internal static Func<bool> AutoFlushGet;
        internal static Action<bool> AutoFlushSet;
        internal static Func<int> IndentLevelGet;
        internal static Action<int> IndentLevelSet;
        internal static Func<int> IndentSizeGet;
        internal static Action<int> IndentSizeSet;
        internal static Action<bool> AssertOverload1;
        internal static Action<bool, string> AssertOverload2;
        internal static Action<bool, string, string> AssertOverload3;
        internal static Action Close;
        internal static Action<string> FailOverload1;
        internal static Action<string, string> FailOverload2;
        internal static Action Flush;
        internal static Action Indent;
        internal static Action Unindent;
        internal static Action<object> WriteOverload1;
        internal static Action<object, string> WriteOverload2;
        internal static Action<string> WriteOverload3;
        internal static Action<string, string> WriteOverload4;
        internal static Action<object> WriteLineOverload1;
        internal static Action<object, string> WriteLineOverload2;
        internal static Action<string> WriteLineOverload3;
        internal static Action<string, string> WriteLineOverload4;

        internal static class DebugInternal
        {
            private static readonly object s_lock = new object();
            private static bool s_needIndent;
            private static int s_indentSize = 4;
            [ThreadStatic]
            private static int s_indentLevel;

            internal static int IndentLevelGet()
            {
                return s_indentLevel;
            }

            internal static void IndentLevelSet(int value)
            {
                s_indentLevel = value < 0 ? 0 : value;
            }

            internal static int IndentSizeGet()
            {
                return s_indentSize;
            }

            internal static void IndentSizeSet(int value)
            {
                s_indentSize = value < 0 ? 0 : value;
            }

            internal static void Assert(bool condition)
            {
                Assert(condition, string.Empty, string.Empty);
            }

            internal static void Assert(bool condition, string message)
            {
                Assert(condition, message, string.Empty);
            }

            internal static void Assert(bool condition, string message, string detailMessage)
            {
                if (!condition)
                {
                    string stackTrace;
                    try
                    {
                        stackTrace = new StackTrace(0, true).ToString(System.Diagnostics.StackTrace.TraceFormat.Normal);
                    }
                    catch
                    {
                        stackTrace = "";
                    }
                    WriteLine(FormatAssert(stackTrace, message, detailMessage));
                    Debug.s_ShowDialog(stackTrace, message, detailMessage, "Assertion Failed");
                }
            }

            internal static void Close() { }

            internal static void Fail(string message)
            {
                Assert(false, message, string.Empty);
            }

            internal static void Fail(string message, string detailMessage)
            {
                Assert(false, message, detailMessage);
            }

            internal static void Flush() { }

            internal static void Indent()
            {
                Debug.IndentLevel++;
            }

            internal static void Unindent()
            {
                Debug.IndentLevel--;
            }

            internal static void Write(string message)
            {
                lock (s_lock)
                {
                    if (message == null)
                    {
                        Debug.s_WriteCore(string.Empty);
                        return;
                    }
                    if (s_needIndent)
                    {
                        message = GetIndentString() + message;
                        s_needIndent = false;
                    }
                    Debug.s_WriteCore(message);
                    if (message.EndsWith(Environment.NewLine))
                    {
                        s_needIndent = true;
                    }
                }
            }

            internal static void Write(object value)
            {
                Write(value?.ToString());
            }

            internal static void Write(string message, string category)
            {
                if (category == null)
                {
                    Write(message);
                }
                else
                {
                    Write(category + ":" + message);
                }
            }

            internal static void Write(object value, string category)
            {
                Write(value?.ToString(), category);
            }

            internal static void WriteLine(string message)
            {
                Write(message + Environment.NewLine);
            }

            internal static void WriteLine(object value)
            {
                WriteLine(value?.ToString());
            }

            internal static void WriteLine(object value, string category)
            {
                WriteLine(value?.ToString(), category);
            }

            internal static void WriteLine(string message, string category)
            {
                if (category == null)
                {
                    WriteLine(message);
                }
                else
                {
                    WriteLine(category + ":" + message);
                }
            }

            private static string s_indentString;

            internal static string GetIndentString()
            {
                int indentCount = IndentSizeGet() * IndentLevelGet();
                if (s_indentString?.Length == indentCount)
                {
                    return s_indentString;
                }
                return s_indentString = new string(' ', indentCount);
            }

            internal static string FormatAssert(string stackTrace, string message, string detailMessage)
            {
                string newLine = GetIndentString() + Environment.NewLine;
                return SR.DebugAssertBanner + newLine
                    + SR.DebugAssertShortMessage + newLine
                    + message + newLine
                    + SR.DebugAssertLongMessage + newLine
                    + detailMessage + newLine
                    + stackTrace;
            }
        }
    }

    /// <summary>
    /// Provides a set of properties and methods for debugging code.
    /// </summary>
    public static partial class Debug
    {
        static Debug()
        {
            DebugDelegateWrapper.AutoFlushGet(); // just to make sure we call static constructor asap
        }

        public static bool AutoFlush { get { return DebugDelegateWrapper.AutoFlushGet(); } set { DebugDelegateWrapper.AutoFlushSet(value); } }

        public static int IndentLevel { get { return DebugDelegateWrapper.IndentLevelGet(); } set { DebugDelegateWrapper.IndentLevelSet(value); } }

        public static int IndentSize { get { return DebugDelegateWrapper.IndentSizeGet(); } set { DebugDelegateWrapper.IndentSizeSet(value); } }

        [System.Diagnostics.Conditional("DEBUG")]
        public static void Close() { DebugDelegateWrapper.Close(); }

        [System.Diagnostics.Conditional("DEBUG")]
        public static void Flush() { DebugDelegateWrapper.Flush(); }

        [System.Diagnostics.Conditional("DEBUG")]
        public static void Indent() { DebugDelegateWrapper.Indent(); }

        [System.Diagnostics.Conditional("DEBUG")]
        public static void Unindent() { DebugDelegateWrapper.Unindent(); }

        [System.Diagnostics.Conditional("DEBUG")]
        public static void Print(string message)
        {
            DebugDelegateWrapper.WriteOverload3(message);
        }

        [System.Diagnostics.Conditional("DEBUG")]
        public static void Print(string format, params object[] args)
        {
            DebugDelegateWrapper.WriteOverload3(string.Format(null, format, args));
        }

        [System.Diagnostics.Conditional("DEBUG")]
        public static void Assert(bool condition)
        {
            DebugDelegateWrapper.AssertOverload1(condition);
        }

        [System.Diagnostics.Conditional("DEBUG")]
        public static void Assert(bool condition, string message)
        {
            DebugDelegateWrapper.AssertOverload2(condition, message);
        }

        [System.Diagnostics.Conditional("DEBUG")]
        public static void Assert(bool condition, string message, string detailMessage)
        {
            DebugDelegateWrapper.AssertOverload3(condition, message, detailMessage);
        }

        internal static void ContractFailure(bool condition, string message, string detailMessage, string failureKindMessage)
        {
            if (!condition)
            {
                string stackTrace;
                try
                {
                    stackTrace = new StackTrace(2, true).ToString(System.Diagnostics.StackTrace.TraceFormat.Normal);
                }
                catch
                {
                    stackTrace = "";
                }
                DebugDelegateWrapper.WriteLineOverload3(DebugDelegateWrapper.DebugInternal.FormatAssert(stackTrace, message, detailMessage));
                s_ShowDialog(stackTrace, message, detailMessage, SR.GetResourceString(failureKindMessage));
            }
        }

        [System.Diagnostics.Conditional("DEBUG")]
        public static void Fail(string message)
        {
            DebugDelegateWrapper.FailOverload1(message);
        }

        [System.Diagnostics.Conditional("DEBUG")]
        public static void Fail(string message, string detailMessage)
        {
            DebugDelegateWrapper.FailOverload2(message, detailMessage);
        }

        [System.Diagnostics.Conditional("DEBUG")]
        public static void Assert(bool condition, string message, string detailMessageFormat, params object[] args)
        {
            DebugDelegateWrapper.AssertOverload3(condition, message, string.Format(detailMessageFormat, args));
        }

        [System.Diagnostics.Conditional("DEBUG")]
        public static void WriteLine(string message)
        {
            DebugDelegateWrapper.WriteLineOverload3(message);
        }

        [System.Diagnostics.Conditional("DEBUG")]
        public static void Write(string message)
        {
            DebugDelegateWrapper.WriteOverload3(message);
        }

        [System.Diagnostics.Conditional("DEBUG")]
        public static void WriteLine(object value)
        {
            DebugDelegateWrapper.WriteLineOverload1(value);
        }

        [System.Diagnostics.Conditional("DEBUG")]
        public static void WriteLine(object value, string category)
        {
            DebugDelegateWrapper.WriteLineOverload2(value, category);
        }

        [System.Diagnostics.Conditional("DEBUG")]
        public static void WriteLine(string format, params object[] args)
        {
            DebugDelegateWrapper.WriteOverload1(string.Format(null, format, args));
        }

        [System.Diagnostics.Conditional("DEBUG")]
        public static void WriteLine(string message, string category)
        {
            DebugDelegateWrapper.WriteLineOverload4(message, category);
        }

        [System.Diagnostics.Conditional("DEBUG")]
        public static void Write(object value)
        {
            DebugDelegateWrapper.WriteOverload1(value);
        }

        [System.Diagnostics.Conditional("DEBUG")]
        public static void Write(string message, string category)
        {
            DebugDelegateWrapper.WriteOverload4(message, category);
        }

        [System.Diagnostics.Conditional("DEBUG")]
        public static void Write(object value, string category)
        {
            DebugDelegateWrapper.WriteOverload2(value, category);
        }

        [System.Diagnostics.Conditional("DEBUG")]
        public static void WriteIf(bool condition, string message)
        {
            if (condition)
            {
                DebugDelegateWrapper.WriteOverload3(message);
            }
        }

        [System.Diagnostics.Conditional("DEBUG")]
        public static void WriteIf(bool condition, object value)
        {
            if (condition)
            {
                DebugDelegateWrapper.WriteOverload1(value);
            }
        }

        [System.Diagnostics.Conditional("DEBUG")]
        public static void WriteIf(bool condition, string message, string category)
        {
            if (condition)
            {
                DebugDelegateWrapper.WriteOverload4(message, category);
            }
        }

        [System.Diagnostics.Conditional("DEBUG")]
        public static void WriteIf(bool condition, object value, string category)
        {
            if (condition)
            {
                DebugDelegateWrapper.WriteOverload2(value, category);
            }
        }

        [System.Diagnostics.Conditional("DEBUG")]
        public static void WriteLineIf(bool condition, object value)
        {
            if (condition)
            {
                DebugDelegateWrapper.WriteLineOverload1(value);
            }
        }

        [System.Diagnostics.Conditional("DEBUG")]
        public static void WriteLineIf(bool condition, object value, string category)
        {
            if (condition)
            {
                DebugDelegateWrapper.WriteLineOverload2(value, category);
            }
        }

        [System.Diagnostics.Conditional("DEBUG")]
        public static void WriteLineIf(bool condition, string message)
        {
            if (condition)
            {
                DebugDelegateWrapper.WriteLineOverload3(message);
            }
        }

        [System.Diagnostics.Conditional("DEBUG")]
        public static void WriteLineIf(bool condition, string message, string category)
        {
            if (condition)
            {
                DebugDelegateWrapper.WriteLineOverload4(message, category);
            }
        }

        private sealed class DebugAssertException : Exception
        {
            internal DebugAssertException(string stackTrace) :
                base(Environment.NewLine + stackTrace)
            {
            }

            internal DebugAssertException(string message, string stackTrace) :
                base(message + Environment.NewLine + Environment.NewLine + stackTrace)
            {
            }

            internal DebugAssertException(string message, string detailMessage, string stackTrace) :
                base(message + Environment.NewLine + detailMessage + Environment.NewLine + Environment.NewLine + stackTrace)
            {
            }
        }

        // internal and not readonly so that the tests can swap this out.
        internal static Action<string, string, string, string> s_ShowDialog = ShowDialog;

        internal static Action<string> s_WriteCore = WriteCore;
    }
}
