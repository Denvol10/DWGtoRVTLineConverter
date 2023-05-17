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
using Microsoft.Win32;
using System.Text.Json;
using System.IO;
using DWGtoRVTLineConverter.Models;

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

        public List<string> GetAllPolyLinesName()
        {
            Selection sel = Uiapp.ActiveUIDocument.Selection;
            Reference picked = sel.PickObject(ObjectType.Element, "Select DWG File");
            Element elem = Doc.GetElement(picked);

            Options options = new Options();
            var geometry = elem.get_Geometry(options);
            var geomInstance = geometry.OfType<GeometryInstance>().First();
            var lines = geomInstance.GetInstanceGeometry().OfType<PolyLine>();

            var lineHashCode = lines.Select(l => l.Id.ToString()).ToList();

            return lineHashCode;
        }

        public string GetDWGFileName()
        {
            Selection sel = Uiapp.ActiveUIDocument.Selection;
            Reference picked = sel.PickObject(ObjectType.Element, "Select DWG File");
            Element elem = Doc.GetElement(picked);
            var param = elem.get_Parameter(BuiltInParameter.IMPORT_SYMBOL_NAME);

            return param.AsString();
        }

        public List<PolylineUtils> GetAllPolyLinesFromDWG()
        {
            Selection sel = Uiapp.ActiveUIDocument.Selection;
            Reference picked = sel.PickObject(ObjectType.Element, "Select DWG File");
            Element elem = Doc.GetElement(picked);

            Options options = new Options();
            var geometry = elem.get_Geometry(options);
            var geomInstance = geometry.OfType<GeometryInstance>().First();
            var lines = geomInstance.GetInstanceGeometry().OfType<PolyLine>().ToList();
            var lineUtils = lines.Select(l => new PolylineUtils(l)).ToList();

            return lineUtils;
        }

        public void ExportPolyLines(List<PolylineUtils> lines)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "Json files (*.json)|*.json";

            if(saveFileDialog.ShowDialog() == true)
            {
                string filename = saveFileDialog.FileName;

                string jsonString = JsonSerializer.Serialize(lines);
                File.WriteAllText(filename, jsonString);
            }
        }

        public IEnumerable<PolylineUtils> ImportPolyLinesfromJson()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Json files (*.json)|*.json";

            if(openFileDialog.ShowDialog() == true)
            {
                string fileName = openFileDialog.FileName;
                string jsonString = File.ReadAllText(fileName);
                var lines = JsonSerializer.Deserialize<List<PolylineUtils>>(jsonString);

                return lines;
            }

            return null;
        }

        public void CreateAdaptivePoints(PolylineUtils line)
        {
            using(Transaction trans = new Transaction(Doc, "Create Points"))
            {
                trans.Start();
                foreach(var point in line.GetPoints())
                {
                    XYZ xyzPoint = new XYZ(point.X, point.Y, point.Z);
                    Doc.FamilyCreate.NewReferencePoint(xyzPoint);
                }
                trans.Commit();
            }
        }

        public void CreatePolylinesInFamily(IEnumerable<PolylineUtils> lines)
        {
            using(Transaction trans = new Transaction(Doc, "Polylines Created"))
            {
                trans.Start();

                foreach(var line in lines)
                {
                    var referencePoints = new ReferencePointArray();

                    foreach (var point in line.GetPoints())
                    {
                        XYZ xyzPoint = new XYZ(point.X, point.Y, point.Z);
                        var referencePoint = Doc.FamilyCreate.NewReferencePoint(xyzPoint);
                        referencePoints.Append(referencePoint);
                    }

                    var pointPairs = new List<ReferencePointArray>();
                    for (int i = 0; i < referencePoints.Size - 1; i++)
                    {
                        var poinArray = new ReferencePointArray();
                        poinArray.Append(referencePoints.get_Item(i));
                        poinArray.Append(referencePoints.get_Item(i + 1));
                        pointPairs.Add(poinArray);
                    }

                    foreach (var points in pointPairs)
                    {
                        Doc.FamilyCreate.NewCurveByPoints(points);
                    }
                }

                trans.Commit();
            }
        }
    }
}
