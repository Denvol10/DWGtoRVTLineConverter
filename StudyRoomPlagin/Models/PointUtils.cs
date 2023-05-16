using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DWGtoRVTLineConverter.Models
{
    public class PointUtils
    {
        public double X { get; set; }
        public double Y { get; set; }
        public double Z { get; set; }

        public PointUtils(XYZ point)
        {
            X = point.X;
            Y = point.Y;
            Z = point.Z;
        }
    }
}
