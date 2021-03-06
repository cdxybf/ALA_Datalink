﻿using ProgrammingParadigms;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace DomainAbstractions
{
    /// <summary>
    /// A UI element, just like a label with a text string on it. Two inputs:
    /// 1. IUI for getting the WPF element.
    /// 2. IDataFlow<string> for inputing the text.
    /// </summary>
    public class Text : IUI, IDataFlow<string>, IDataFlow<bool>
    {
        // properties
        public Brush Color = Brushes.Black;
        public double HeightRatio;
        public FontWeight FontWeight { set => textBlock.FontWeight = value; }
        public Thickness Margin;
        public Thickness Padding;
        public double FontSize = 14;
        public bool ShowBorder = false;

        // private fields
        private UIElement wpfElement;
        private TextBlock textBlock = new TextBlock();

        /// <summary>
        /// An IUI abstraction which disaplys a giving text.
        /// </summary>
        /// <param name="text">text displayed</param>
        /// <param name="visible">control the visibility of the text</param>
        public Text(string text = null, bool visible = true)
        {
            textBlock.Text = text;
            textBlock.VerticalAlignment = VerticalAlignment.Center;
            //textBlock.FontFamily = new FontFamily("Arial");
            textBlock.Visibility = visible ? Visibility.Visible : Visibility.Collapsed;
        }

        // IUI implementation ---------------------------------------------------------------
        UIElement IUI.GetWPFElement()
        {
            if (HeightRatio != 0)
            {
                textBlock.LayoutTransform = new ScaleTransform() { ScaleY = HeightRatio };
            }

            if (ShowBorder)
            {
                var border = new Border();
                border.Margin = Margin;
                border.Child = textBlock;
                border.BorderThickness = new Thickness(1);
                border.BorderBrush = new SolidColorBrush(System.Windows.Media.Color.FromRgb(180, 180, 180));
                textBlock.Padding = Padding;
                wpfElement = border;
            }
            else
            {
                textBlock.Margin = Margin;
                wpfElement = textBlock;
            }
            textBlock.Foreground = Color;
            textBlock.FontSize = FontSize;

            return wpfElement;
        }

        // IDataFlow<string> implementation ---------------------------------------------------------------
        string IDataFlow<string>.Data { set => textBlock.Text = value; }

        // IDataFlow<bool> implementation -----------------------------------------------------------------
        bool IDataFlow<bool>.Data { set => wpfElement.Visibility = value ? Visibility.Visible : Visibility.Collapsed; }
    }
}
