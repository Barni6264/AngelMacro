﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace AngelMacro
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        App()
        {
            //if (CultureInfo.CurrentCulture.TwoLetterISOLanguageName.ToLower() == "hu")
            //{
            //    System.Threading.Thread.CurrentThread.CurrentUICulture = new CultureInfo("hu-HU");
            //}
        }
    }
}
