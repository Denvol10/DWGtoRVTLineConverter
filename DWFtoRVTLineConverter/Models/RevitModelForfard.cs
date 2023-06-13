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
using System.IO;
using DWGtoRVTLineConverter.Models;
using Newtonsoft.Json;

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

        public List<PolylineUtils> GetAllPolyLinesFromDWG(ref string dwgName)
        {
            Selection sel = Uiapp.ActiveUIDocument.Selection;
            Reference picked = sel.PickObject(ObjectType.Element, "Select DWG File");
            Element elem = Doc.GetElement(picked);
            dwgName = elem.get_Parameter(BuiltInParameter.IMPORT_SYMBOL_NAME).AsString();

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

            if (saveFileDialog.ShowDialog() == true)
            {
                string filename = saveFileDialog.FileName;

                string jsonString = JsonConvert.SerializeObject(lines, Formatting.Indented);
                File.WriteAllText(filename, jsonString);
            }
        }

        public IEnumerable<PolylineUtils> ImportPolyLinesfromJson()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Json files (*.json)|*.json";

            if (openFileDialog.ShowDialog() == true)
            {
                string fileName = openFileDialog.FileName;
                string jsonString = File.ReadAllText(fileName);
                var lines = JsonConvert.DeserializeObject<List<PolylineUtils>>(jsonString);

                return lines;
            }

            return null;
        }

        public void CreatePolylinesInFamily(IEnumerable<PolylineUtils> lines)
        {
            using (Transaction trans = new Transaction(Doc, "Polylines Created"))
            {
                trans.Start();

                foreach (var line in lines)
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

        public void CreateDirectShapeLinesInModel()
        {
            Selection sel = Uiapp.ActiveUIDocument.Selection;
            Reference picked = sel.PickObject(ObjectType.Element, "Select DWG File");
            Element elem = Doc.GetElement(picked);

            Options options = new Options();
            var geometry = elem.get_Geometry(options);
            var geomInstance = geometry.OfType<GeometryInstance>().First();
            var allcurves = new List<GeometryObject>();

            var polylines = geomInstance.GetInstanceGeometry().OfType<PolyLine>().ToList();
            var curves = geomInstance.GetInstanceGeometry().OfType<Curve>().ToList();

            allcurves.AddRange(polylines);
            allcurves.AddRange(curves);

            ElementId categoryId = new ElementId(BuiltInCategory.OST_Lines);

            foreach (var line in allcurves)
            {
                using (Transaction trans = new Transaction(Doc, "Create Curve"))
                {
                    trans.Start();
                    var lineList = new List<GeometryObject>() { line };
                    DirectShape directShape = DirectShape.CreateElement(Doc, categoryId);
                    if (directShape.IsValidShape(lineList))
                    {
                        directShape.SetShape(lineList);
                    }
                    trans.Commit();
                }
            }
        }

        public void SaveCurvesInFamilyFile()
        {
            var libraryPath = App.GetLibraryPaths().Values.First();
            string sourceDirectory = Path.GetFullPath(Path.Combine(libraryPath, @"..\..\"));
            string templatePath = Path.Combine(sourceDirectory, @"Family Templates\Russian\Метрическая система, адаптивная типовая модель.rft");

            Selection sel = Uiapp.ActiveUIDocument.Selection;
            Reference picked = sel.PickObject(ObjectType.Element, "Select DWG File");
            Element elem = Doc.GetElement(picked);

            Options options = new Options();
            var geometry = elem.get_Geometry(options);
            var geomInstance = geometry.OfType<GeometryInstance>().First();
            var allcurves = new List<GeometryObject>();

            var polylines = geomInstance.GetInstanceGeometry().OfType<PolyLine>().ToList();
            var curves = geomInstance.GetInstanceGeometry().OfType<Curve>().ToList();

            allcurves.AddRange(polylines);
            allcurves.AddRange(curves);

            ElementId categoryId = new ElementId(BuiltInCategory.OST_Lines);

            Document familyDocument = App.NewFamilyDocument(templatePath);

            using (Transaction trans = new Transaction(familyDocument, "Create Curves"))
            {
                trans.Start();
                foreach (var line in allcurves)
                {
                    var lineList = new List<GeometryObject>() { line };
                    DirectShape directShape = DirectShape.CreateElement(familyDocument, categoryId);
                    if (directShape.IsValidShape(lineList))
                    {
                        directShape.SetShape(lineList);
                    }
                }
                trans.Commit();
            }

            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "Файлы семейств (*.rfa)|*.rfa";
            SaveAsOptions saveOptions = new SaveAsOptions();
            saveOptions.OverwriteExistingFile = true;

            if (saveFileDialog.ShowDialog() == true)
            {
                string filePath = saveFileDialog.FileName;
                familyDocument.SaveAs(filePath, saveOptions);
                familyDocument.Close(false);
            }
        }
    }
}
