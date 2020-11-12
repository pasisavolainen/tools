using System;
using Terminal.Gui;

namespace dtail.Extensions
{
    public static class GuicsExtensions
    {
        public static Button? OnClick(this Button? button, Action action)
        {
            if (button != null)
                button.Clicked += action;
            return button;
        }
    }
}
