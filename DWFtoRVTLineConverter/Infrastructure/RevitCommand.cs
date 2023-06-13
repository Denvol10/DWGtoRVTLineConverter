﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using Autodesk.Revit.DB.Architecture;
using DWGtoRVTLineConverter.ViewModels;

namespace DWGtoRVTLineConverter.Infrastructure
{
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    internal class RevitCommand : IExternalCommand
    {
        public static MainWindow mainView = null;

        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIApplication uiapp = commandData.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;
            Application app = uiapp.Application;
            Document doc = uiapp.ActiveUIDocument.Document;

            try
            {
                var RevitModel = new RevitModelForfard(uiapp);
                MainWindowViewModel mainWindowViewModel = new MainWindowViewModel(RevitModel);

                System.Diagnostics.Process proc = System.Diagnostics.Process.GetCurrentProcess();

                using (MainWindow view = new MainWindow())
                {
                    System.Windows.Interop.WindowInteropHelper helper = new System.Windows.Interop.WindowInteropHelper(view);
                    helper.Owner = proc.MainWindowHandle;

                    view.DataContext = mainWindowViewModel;

                    mainView = view;

                    view.ShowDialog();
                }

                return Result.Succeeded;
            }
            catch (Exception ex)
            {
                message = ex.Message;
                return Result.Failed;
            }
        }
    }
}
