using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Win32;

namespace DWGtoRVTLineConverter.Models
{
    public class PolylineUtils
    {
        public List<double> PointsX { get; set; }
        public List<double> PointsY { get; set; }
        public List<double> PointsZ { get; set; }

        public PolylineUtils(PolyLine line)
        {
            PointsX = line.GetCoordinates().Select(p => p.X).ToList();
            PointsY = line.GetCoordinates().Select(p => p.Y).ToList();
            PointsZ = line.GetCoordinates().Select(p => p.Z).ToList();
        }

        public PolylineUtils(List<double> pointsX, List<double> pointsY, List<double> pointsZ)
        {
            PointsX = new List<double>(pointsX);
            PointsY = new List<double>(pointsY);
            PointsZ = new List<double>(pointsZ);
        }

        public PolylineUtils()
        { }

        public List<XYZ> GetPoints()
        {
            var allCoordinate = PointsX.Zip(PointsY, (x, y) => new { x, y }).Zip(PointsZ, (xy, z) => new { X = xy.x, Y = xy.y, Z = z });

            var xyzPoints = allCoordinate.Select(p => new XYZ(p.X, p.Y, p.Z)).ToList();

            return xyzPoints;
        }
    }
}
