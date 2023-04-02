namespace Witcher3MapViewer.Core
{
    public class Point
    {
        public readonly double X;
        public readonly double Y;

        public Point(double x, double y)
        {
            this.X = x;
            this.Y = y;
        }
    }

    public static class RealToGameSpaceConversion
    {
        public static Point ToGameSpace(Point realspace, WorldSetting worldSetting)
        {
            return new Point(Math.Round(worldSetting.Slope * realspace.X + worldSetting.XIntercept),
                Math.Round(worldSetting.Slope * realspace.Y + worldSetting.YIntercept));
        }

        public static Point ToWorldSpace(Point gamespace, WorldSetting worldSetting)
        {
            return new Point((gamespace.X - worldSetting.XIntercept) / worldSetting.Slope,
                (gamespace.Y - worldSetting.YIntercept) / worldSetting.Slope);
        }
    }
}
