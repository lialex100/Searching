﻿using System.Reflection;
using System.Windows.Forms;

namespace Helper.Helper
{
    public static class ControlExtensions
    {
        public static void DoubleBuffer(this Control control)
        {
            // http://stackoverflow.com/questions/76993/how-to-double-buffer-net-controls-on-a-form/77233#77233
            // Taxes: Remote Desktop Connection and painting: http://blogs.msdn.com/oldnewthing/archive/2006/01/03/508694.aspx

            if (SystemInformation.TerminalServerSession) return;
            PropertyInfo dbProp = typeof(Control).GetProperty("DoubleBuffered", BindingFlags.NonPublic | BindingFlags.Instance);
            dbProp.SetValue(control, true, null);
        }
    }
}