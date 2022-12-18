﻿using System;
using System.Windows.Forms;

namespace FenGen;

internal static class Program
{
    /// <summary>
    /// The main entry point for the application.
    /// </summary>
    [STAThread]
    internal static void Main()
    {
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);
        Application.Run(new AppContext());
    }
}

internal sealed class AppContext : ApplicationContext
{
    public AppContext() => Core.Init();
}