using System.Windows.Media;
using System.Windows;
using System.Data;

using DomainAbstractions;
using ProgrammingParadigms;
using Wiring;
using System;

namespace Application
{
    public class Application
    {
        private MainWindow mainWindow = new MainWindow("ALA---Datalink");

        [STAThread]
        public static void Main()
        {
            new Application().Initialize().mainWindow.Run();
        }

        private Application Initialize()
        {
            Wiring.Wiring.PostWiringInitialize();
            return this;
        }

        private Application()
        {
            Wizard putInfoWizard;
            Wizard getInfoWizard;
            Wizard deleteInfoWizard;

            DataFlowConnector<bool> XR3000ConnectedConnector;
            DataFlowConnector<bool> XRSConnectedConnector;

            mainWindow
            // UI
            .WireTo(new Vertical() { Layouts = new int[] { 0, 0, 2, 0 } }
                .WireTo(new Menubar()
                    .WireTo(new Menu("File")
                        .WireTo(new MenuItem("Get information off device", false) { IConName = "3000Import.png" }
                            .WireFrom(XR3000ConnectedConnector = new DataFlowConnector<bool>())
                            .WireTo((getInfoWizard = new Wizard("Get information off device") { SecondTitle = "What information do you want to get off the device?" })
                                .WireTo(new WizardItem("Get selected session files") { ImageName = "Icon_Session.png", Checked = true }
                                    .WireTo(new Wizard("Get information off device") { SecondTitle = "What do you want to do with the session files?", ShowBackButton = true }
                                        .WireTo(new WizardItem("Save selected session files as files on the PC") { ImageName = "Icon_Session.png", Checked = true })
                                        .WireTo(new WizardItem("Send records to NAIT") { ImageName = "NAIT.png" })
                                        .WireTo(new WizardItem("Send sessions to MiHub Livestock") { ImageName = "MiHub40x40_ltblue_cloud.png" })
                                        .WireTo(new WizardItem("Send to \"remote system\" using Animal Data Interface") { ImageName = "ttlogo76x32.png" })
                                        .WireTo(new WizardItem("Send to LIC") { ImageName = "LicLogo.png" })
                                        .WireTo(getInfoWizard)
                                    )
                                )
                                .WireTo(new WizardItem("Get all animal lifetime information") { ImageName = "Icon_Animal.png" }
                                    .WireTo(new Wizard("Get information off device") { SecondTitle = "What do you want to do with the animal lifetime information?", ShowBackButton = true }
                                        .WireTo(new WizardItem("Save all animal lifetime information as a file on the PC") { ImageName = "Icon_Animal.png", Checked = true })
                                        .WireTo(new WizardItem("MiHub Life Data") { ImageName = "MiHub40x40_ltblue_cloud.png" })
                                        .WireTo(new WizardItem("Send to \"remote system\" using Animal Data Interface") { ImageName = "ttlogo76x32.png" })
                                        .WireTo(getInfoWizard)
                                    )
                                )
                                .WireTo(new WizardItem("Generate a report") { ImageName = "reporticon.png" })
                            )
                        )
                        .WireTo(new MenuItem("Put information onto device", false) { IConName = "3000Export.png" }
                            .WireFrom(XR3000ConnectedConnector)
                            .WireTo((putInfoWizard = new Wizard("Put information onto device") { SecondTitle = "What informatioin do you want to put onto the device?" })
                                .WireTo(new WizardItem("Session files") { ImageName = "Icon_Session.png", Checked = true }
                                    .WireTo(new Wizard() { SecondTitle = "Where are the session files?", ShowBackButton = true }
                                        .WireTo(putInfoWizard)
                                        .WireTo(new WizardItem("On the local PC") { ImageName = "Icon_Session.png", Checked = true })
                                        .WireTo(new WizardItem("On the \"remote system\"") { ImageName = "ttlogo76x32.png" })
                                    )
                                )
                                .WireTo(new WizardItem() { ContentText = "Animal lifetime information", ImageName = "Icon_Animal.png" }
                                    .WireTo(new Wizard() { SecondTitle = "Where is the file containing animal lifetime information?", ShowBackButton = true }
                                        .WireTo(putInfoWizard)
                                        .WireTo(new WizardItem("On the local PC") { ImageName = "Icon_Session.png", Checked = true }
                                            .WireTo(new OpenFileBrowser("Put information onto device"))
                                        )
                                        .WireTo(new WizardItem("On the \"remote system\"") { ImageName = "ttlogo76x32.png" })
                                    )
                                )
                            )
                        )
                        .WireTo(new MenuItem("Delete information off device", false) { IConName = "3000Delete.png" }
                            .WireFrom(XR3000ConnectedConnector)
                            .WireTo((deleteInfoWizard = new Wizard("Delete information off device") { SecondTitle = "What informatioin do you want to delete off the device?" })
                                .WireTo(new WizardItem("Selected sessions") { ImageName = "Icon_Session_Delete.png", Checked = true })
                                .WireTo(new WizardItem() { ContentText = "All information on device\n(all session files and animal lifetime information)", ImageName = "Icon_Clear3000.png" })
                            )
                        )
                        .WireTo(new MenuItem("Get information off device", false) { IConName = "XRSImport.png" }
                            .WireFrom(XRSConnectedConnector = new DataFlowConnector<bool>())
                            .WireTo(getInfoWizard)
                        )
                        .WireTo(new MenuItem("Put information onto device", false) { IConName = "XRSExport.png" }
                            .WireFrom(XRSConnectedConnector)
                            .WireTo(putInfoWizard)
                        )
                        .WireTo(new MenuItem("Delete information off device", false) { IConName = "XRSDelete.png" }
                            .WireFrom(XRSConnectedConnector)
                            .WireTo(deleteInfoWizard)
                        )
                        .WireTo(new MenuItem("Exit") { IConName = "exit.png" }
                            .WireTo(mainWindow)
                        )
                    )
                    .WireTo(new Menu("Tools")
                        .WireTo(new MenuItem("Data Link options...") { IConName = "Setup.png" })
                        .WireTo(new MenuItem("Updates..."))
                        .WireTo(new MenuItem("History") { IConName = "clock.png" })
                    )
                    .WireTo(new Menu("Help")
                        .WireTo(new MenuItem("Datamars website") { IConName = "ttlogo76x32.png" })
                        .WireTo(new MenuItem("About"))
                        .WireTo(new MenuItem("Teamviewer"))
                        .WireTo(new MenuItem("Help") { IConName = "gnome_help.png" })
                    )
                )
                .WireTo(new Toolbar()
                    // XR3000
                    .WireTo(new Tool("3000Import.png", false) { ToolTip = "Get information off device" }
                        .WireFrom(XR3000ConnectedConnector)
                        .WireTo(getInfoWizard)
                    )
                    .WireTo(new Tool("3000Export.png", false) { ToolTip = "Put information onto device" }
                        .WireFrom(XR3000ConnectedConnector)
                        .WireTo(putInfoWizard)
                    )
                    .WireTo(new Tool("3000Delete.png", false) { ToolTip = "Delete information off device" }
                        .WireFrom(XR3000ConnectedConnector)
                        .WireTo(deleteInfoWizard)
                    )
                    // XRS
                    .WireTo(new Tool("XRSImport.png", false) { ToolTip = "Get information off device" }
                        .WireFrom(XRSConnectedConnector)
                        .WireTo(getInfoWizard)
                    )
                    .WireTo(new Tool("XRSExport.png", false) { ToolTip = "Put information onto device" }
                        .WireFrom(XRSConnectedConnector)
                        .WireTo(putInfoWizard)
                    )
                    .WireTo(new Tool("XRSDelete.png", false) { ToolTip = "Delete information off device" }
                        .WireFrom(XRSConnectedConnector)
                        .WireTo(deleteInfoWizard)
                    )
                    // functional tools
                    .WireTo(new RightJustify()
                        .WireTo(new Tool("clock.png") { ToolTip = "History" })
                        .WireTo(new Tool("Setup.png") { ToolTip = "Settings for the Data Link application" })
                        .WireTo(new Tool("gnome_help.png") { ToolTip = "Help" })
                    )
                )
                .WireTo(new Horizontal() { InstanceName = "DataPanels", Ratios = new int[2] { 1, 2 } }
                    .WireTo(new Vertical() { Layouts = new int[] { 0, 2 } }
                        .WireTo(new Panel("Animal information")
                            .WireTo(new RowButton() { Margin = new Thickness(5, 0, 5, 0), Height = 50 }
                                .WireFrom(new StringFormat(" Animal lifetime information ({0})"))
                                .WireFrom(new LiteralString(""))
                            )
                        )
                        .WireTo(new Panel() { InstanceName = "SessionsPanel" }
                            .WireTo(new Horizontal() { Margin = new Thickness(5) }
                                .WireTo(new Text("Session files") { FontSize = 15 }
                                    .WireFrom(new StringFormat("Session files ({0})"))
                                    .WireFrom(new LiteralString("Session files"))
                                )
                                .WireTo(new RightJustify()
                                    .WireTo(new OptionBox() { DefaultTitle = "Select" }
                                        .WireTo(new OptionBoxItem("None"))
                                        .WireTo(new OptionBoxItem("All"))
                                        .WireTo(new OptionBoxItem("Today"))
                                        .WireTo(new Map() { Column = "checkbox", MapDelegate = (DataRow r, string s) => { if (s.Equals("Today") && DateTime.Now.ToString("dd/MM/yyyy").Equals(r["date"]) || s.Equals("All")) return true; return false; } })
                                    )
                                )
                            )
                            .WireTo(new Grid() { ShowHeader = false, RowHeight = 30, Margin = new Thickness(5, 0, 5, 0), PrimaryKey = "DeviceId" })
                            .WireTo(new Grid() { ShowHeader = false, RowHeight = 50, Margin = new Thickness(5, 0, 5, 0), PrimaryKey = "index" })
                        )
                    )
                    .WireTo(new Panel() { InstanceName = "MainDataPanel" }
                        .WireTo(new Text("0 record") { FontSize = 15, Margin = new Thickness(5) }
                            .WireFrom(new LiteralString("0 record"))
                            .WireFrom(new StringFormat("{0} of {1} records for \"{2}\""))
                            .WireFrom(new StringFormat("{0} of {1} animals"))
                        )
                        .WireTo(new Text("Downloading...", false) { FontSize = 15, Margin = new Thickness(5), FontWeight = FontWeights.Bold })
                        .WireTo(new Grid())
                    )
                )
                .WireTo(new Statusbar()
                    .WireTo(new Text("Searching on all ports...") { Color = Brushes.Red, FontSize = 14, FontWeight = FontWeights.Bold, Margin = new Thickness(3, 0, 0, 1) })
                    .WireTo(new Text(null, false) { Color = Brushes.Green, FontSize = 14, FontWeight = FontWeights.Bold, Margin = new Thickness(0, 0, 0, 1) }
                        .WireFrom(new LiteralString("Connected to XR3000"))
                        .WireFrom(new LiteralString("Connected to XRS"))
                    )
                )
            )
            //event and dataflow. AppStartsRun
            .WireTo(new EventConnector()
            // According to the confidential agreement of Datamars, the following code is not provided. 
            // Such code is correlated with how the abstractions are wired to interact with a connected device.

            // start to transact session list
            // .WireTo(new Transact() { AutoLoadNextBatch = true, ClearDestination = true, InstanceName = "transact session list" })
            );
        }
    }
}
