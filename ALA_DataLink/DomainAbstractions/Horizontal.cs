using ProgrammingParadigms;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace DomainAbstractions
{
    /// <summary>
    /// The container of the Horizontal is a Grid which can organize the sub-elements in a 
    /// table-style and here the Horizontal is a specific Grid which has only one Row. 
    /// The sub-elements will be sized automatically by default and could also be layouted
    /// and sized with the Ratio given with which the Horiontal is more flexible and configurable.
    /// </summary>
    public class Horizontal : IUI
    {
        // properties
        public string InstanceName;
        public int[] Ratios { get; set; }
        public Thickness Margin { set => gridPanel.Margin = value; }

        // outputs
        private List<IUI> children = new List<IUI>();

        // private fields
        private System.Windows.Controls.Grid gridPanel = new System.Windows.Controls.Grid();

        /// <summary>
        /// A layout IUI which arranges it's sub-elements horizontally.
        /// </summary>
        public Horizontal()
        {
            gridPanel.ShowGridLines = false;
            gridPanel.RowDefinitions.Add(new RowDefinition() {
                Height = new GridLength(1, GridUnitType.Star)
            });
        }

        // IUI implmentation -----------------------------------------------------------
        UIElement IUI.GetWPFElement()
        {
            for (var i = 0; i < children.Count; i++)
            {
                var r = (Ratios != null && i < Ratios.Length) ? Ratios[i] : 100;
                gridPanel.ColumnDefinitions.Add(new ColumnDefinition() {
                    Width = new GridLength(r, GridUnitType.Star)
                });

                var e = children[i].GetWPFElement();
                gridPanel.Children.Add(e);
                System.Windows.Controls.Grid.SetColumn(e, i);
                System.Windows.Controls.Grid.SetRow(e, 0);
            }

            return gridPanel;
        }
    }
}
