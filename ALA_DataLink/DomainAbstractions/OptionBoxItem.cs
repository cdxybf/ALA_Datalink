﻿using ProgrammingParadigms;
using System.Windows;
using System.Windows.Controls;

namespace DomainAbstractions
{
    /// <summary>
    /// A specifc abstraction which supposed to only be wired to OptionBox. 
    /// When the item is selected, it generates an IEvent
    /// </summary>
    public class OptionBoxItem : IUI
    {
        // properties
        public bool Selected { get => comboBoxItem.IsSelected; set => comboBoxItem.IsSelected = value; }

        // outputs
        private IEvent eventItemSelectedOutput;

        // private fields
        private ComboBoxItem comboBoxItem;

        /// <summary>
        /// An IUI element which returns the dropdown list item, should be used only with OptionBox
        /// </summary>
        /// <param name="title">the text displayed on the item</param>
        public OptionBoxItem(string title)
        {
            comboBoxItem = new ComboBoxItem();
            comboBoxItem.Content = title;
            comboBoxItem.Selected += (object sender, RoutedEventArgs e) =>
            {
                eventItemSelectedOutput?.Execute();
            };
        }

        // IUI implementation ------------------------------------------------------
        UIElement IUI.GetWPFElement()
        {
            return comboBoxItem;
        }
    }
}
