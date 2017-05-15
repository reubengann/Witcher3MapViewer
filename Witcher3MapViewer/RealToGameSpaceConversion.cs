using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mapsui.Geometries;

namespace Witcher3MapViewer
{
    public class RealToGameSpaceConversion
    {
        private double Slope, InterceptX, InterceptY;

        public RealToGameSpaceConversion(double slope, double interceptx, double intercepty)
        {
            Slope = slope;
            InterceptX = interceptx;
            InterceptY = intercepty;
        }

        public Point ToGameSpace(Point realspace)
        {
            return new Point(Math.Round(Slope * realspace.X + InterceptX),
                Math.Round(Slope * realspace.Y + InterceptY));
        }

        public Point ToWorldSpace(Point gamespace)
        {
            return new Point((gamespace.X - InterceptX) / Slope,
                (gamespace.Y - InterceptY) / Slope);
        }
    }
}
