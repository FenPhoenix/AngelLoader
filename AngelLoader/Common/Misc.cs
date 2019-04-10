using System;
using System.Collections.Generic;

namespace AngelLoader.Common
{
    internal static class ByteSize
    {
        internal static int KB = 1024;
        internal static int MB = KB * 1024;
        internal static int GB = MB * 1024;
    }

    internal static class Defaults
    {
        internal const int MainWindowX = 50;
        internal const int MainWindowY = 50;

        internal const int ColumnWidth = 100;
        internal const int MinColumnWidth = 25;

        internal const float TopSplitterPercent = 0.741f;
        internal const float MainSplitterPercent = 0.4425f;

        internal const string WebSearchUrl = "https://www.google.com/search?q=\"$TITLE$\" site:ttlg.com";
    }

    internal class CacheData
    {
        internal List<string> Readmes = new List<string>();
    }

    internal enum ReadmeType
    {
        PlainText,
        RichText,
        HTML
    }

    #region DisableEvents

    /*
     Implement the interface on your form, and put guard clauses on all your event handlers that you want to be
     disableable:

     if (EventsDisabled) return;

     Then whenever you want to disable those event handlers, just make a using block:

     using (new DisableEvents(this))
     {
     }

     Inside this block, put any code that changes the state of the controls in such a way that would normally
     run their event handlers. The guard clauses will exit them before anything happens. Problem solved. And
     much better than a nasty wall of Control.Event1 -= Control_Event1; Control.Event1 += Control_Event1; etc.,
     and has the added bonus of guaranteeing a reset of the value due to the using block.
    */

    internal interface IEventDisabler
    {
        bool EventsDisabled { get; set; }
    }

    internal sealed class DisableEvents : IDisposable
    {
        private readonly IEventDisabler Obj;
        internal DisableEvents(IEventDisabler obj)
        {
            Obj = obj;
            Obj.EventsDisabled = true;
        }

        public void Dispose() => Obj.EventsDisabled = false;
    }

    #endregion

    #region DisableKeyPresses

    internal sealed class DisableKeyPresses : IDisposable
    {
        private readonly IKeyPressDisabler Obj;

        internal DisableKeyPresses(IKeyPressDisabler obj)
        {
            Obj = obj;
            Obj.KeyPressesDisabled = true;
        }

        public void Dispose() => Obj.KeyPressesDisabled = false;
    }

    internal interface IKeyPressDisabler
    {
        bool KeyPressesDisabled { get; set; }
    }

    #endregion

    internal interface ILocalizable
    {
        // Ugly optional bool... but needs must...
        void SetUITextToLocalized(bool suspendResume = true);
    }

    internal static class Regexes
    {
        // Uh, nothing in here at the moment.
    }
}
