using System;
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
using System.Collections.ObjectModel;

namespace DWGtoRVTLineConverter
{
    public class RevitModelForfard
    {
        private UIApplication Uiapp { get; set; } = null;
        private Application App { get; set; } = null;
        private UIDocument Uidoc { get; set; } = null;
        private Document Doc { get; set; } = null;

        public RevitModelForfard(UIApplication uiapp)
        {
            Uiapp = uiapp;
            App = uiapp.Application;
            Uidoc = uiapp.ActiveUIDocument;
            Doc = uiapp.ActiveUIDocument.Document;
        }

        public List<string> GetAllRooms()
        {
            var rooms = new FilteredElementCollector(Doc).OfCategory(BuiltInCategory.OST_Rooms)
                                                          .Cast<Room>()
                                                          .Select(r => r.Name)
                                                          .ToList();

            return rooms;
        }

        public string GetDWGFileName()
        {
            Selection sel = Uiapp.ActiveUIDocument.Selection;
            Reference picked = sel.PickObject(ObjectType.Element, "Select DWG File");
            Element elem = Doc.GetElement(picked);
            var param = elem.get_Parameter(BuiltInParameter.IMPORT_SYMBOL_NAME);

            return param.AsString();
        }
    }
}
