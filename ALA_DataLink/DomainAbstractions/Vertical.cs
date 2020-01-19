﻿using ProgrammingParadigms;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace DomainAbstractions
{
    /// <summary>
    /// Arranges sub UI elements vertically.
    /// Automatically sizes the widths to be the same as Vertical, 
    /// but heights to be shared according to the property Layouts.
    /// Using 2 to averagely share the height, or 0 to auto size based on the sub-elements.
    /// </summary>
    public class Vertical : IUI
    {
        // properties
        public string InstanceName;

        /// <summary>
        /// Layout of it's sub elements, 0 for auto sizing, 2 for averagely sharing
        /// </summary>
        public int[] Layouts { get; set; } 
        public Thickness Margin { set => gridPanel.Margin = value; }

        // outputs
        private List<IUI> children = new List<IUI>();

        // private fields
        private System.Windows.Controls.Grid gridPanel = new System.Windows.Controls.Grid();

        /// <summary>
        /// A layout IUI which arranges it's sub-elements vertically.
        /// Layouts properties must be asigned, '0' represents Auto Sizing, '2' represents fill parent.
        /// An example of 3 sub-elements layout: [0, 0, 2]
        /// </summary>
        public Vertical()
        {
            gridPanel.ShowGridLines = false;
            gridPanel.ColumnDefinitions.Add(new ColumnDefinition() {
                Width = new GridLength(1, GridUnitType.Star)
            });
        }

        // IUI implementation -------------------------------------------------------
        UIElement IUI.GetWPFElement()
        {
            if (Layouts == null || Layouts.Length < children.Count)
                throw new System.Exception("Vertical Layouts incompatible with children.");

            for (var i = 0; i < children.Count; i++)
            {
                GridUnitType type = (GridUnitType)System.Convert.ToInt32(Layouts[i]);
                gridPanel.RowDefinitions.Add(new RowDefinition() {
                    Height = new GridLength(1, type)
                });
                var e = children[i].GetWPFElement();
                gridPanel.Children.Add(e);
                System.Windows.Controls.Grid.SetRow(e, i);
                System.Windows.Controls.Grid.SetColumn(e, 0);
            }

            return gridPanel;
        }
    }
}
