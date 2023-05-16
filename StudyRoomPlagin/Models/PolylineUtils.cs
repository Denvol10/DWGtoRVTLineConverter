using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Win32;

namespace DWGtoRVTLineConverter.Models
{
    public class PolylineUtils
    {
        public List<PointUtils> Points { get; set; }

        public PolylineUtils(PolyLine line)
        {
            Points = line.GetCoordinates().Select(p => new PointUtils(p)).ToList();
        }
    }
}
