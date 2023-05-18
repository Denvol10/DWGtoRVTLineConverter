using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using Autodesk.Revit.UI;
using DWGtoRVTLineConverter.Infrastructure;

namespace DWGtoRVTLineConverter
{
    internal class ApplicationRevit : IExternalApplication
    {
        public Result OnStartup(UIControlledApplication application)
        {
            string assemblyLocation = Assembly.GetExecutingAssembly().Location;
            string iconsDirectoryPath = Path.GetDirectoryName(assemblyLocation) + @"\icons\";
            string tabName = "Revit Infrastructure Tools";

            application.CreateRibbonTab(tabName);

            RibbonPanel panel = application.CreateRibbonPanel(tabName, "Line Tools");

            PushButtonData buttonData = new PushButtonData("Plugin for convert DWG to RVT",
                                                           "DWG to RVT",
                                                           assemblyLocation,
                                                           typeof(RevitCommand).FullName)
            {
                LargeImage = new BitmapImage(new Uri(iconsDirectoryPath + "dwgToRVT_icon.png")),
                LongDescription = "Команда для получения линий из вставленного DWG файла"
            };


            panel.AddItem(buttonData);

            return Result.Succeeded;

        }

        public Result OnShutdown(UIControlledApplication application)
        {
            return Result.Succeeded;
        }

    }
}
