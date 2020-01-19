using System.Windows;

namespace ProgrammingParadigms
{
    /// <summary>
    /// Hierarchical structure of the UI
    /// </summary>
    public interface IUI
    {
        UIElement GetWPFElement();
    }
}
