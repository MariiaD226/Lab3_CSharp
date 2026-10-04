using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lab3Interpolation.Models;

public sealed class HermiteInterpolation : Interpolation
{
    public override double Calculate(
        IReadOnlyList<PointD> points,
        double x)
    {
        ValidatePoints(points);

        if (x < points[0].X || x > points[^1].X)
        {
            throw new ArgumentOutOfRangeException(
                nameof(x),
                "X знаходиться за межами опорних точок.");
        }

        if (x == points[^1].X)
        {
            return points[^1].Y;
        }

        int i = FindInterval(points, x);

        PointD p0 = points[i];
        PointD p1 = points[i + 1];

        double dx = p1.X - p0.X;
        double t = (x - p0.X) / dx;

        double m0 = CalculateSlope(points, i);
        double m1 = CalculateSlope(points, i + 1);

        double h00 = 2 * Math.Pow(t, 3)
                     - 3 * Math.Pow(t, 2)
                     + 1;

        double h10 = Math.Pow(t, 3)
                     - 2 * Math.Pow(t, 2)
                     + t;

        double h01 = -2 * Math.Pow(t, 3)
                     + 3 * Math.Pow(t, 2);

        double h11 = Math.Pow(t, 3)
                     - Math.Pow(t, 2);

        return h00 * p0.Y
             + h10 * dx * m0
             + h01 * p1.Y
             + h11 * dx * m1;
    }

    private static double CalculateSlope(
        IReadOnlyList<PointD> points,
        int index)
    {
        if (index == 0)
        {
            return (points[1].Y - points[0].Y)
                   / (points[1].X - points[0].X);
        }

        if (index == points.Count - 1)
        {
            int last = points.Count - 1;

            return (points[last].Y - points[last - 1].Y)
                   / (points[last].X - points[last - 1].X);
        }

        return (points[index + 1].Y - points[index - 1].Y)
               / (points[index + 1].X - points[index - 1].X);
    }

    private static int FindInterval(
        IReadOnlyList<PointD> points,
        double x)
    {
        for (int i = 0; i < points.Count - 1; i++)
        {
            if (x >= points[i].X && x <= points[i + 1].X)
            {
                return i;
            }
        }

        throw new InvalidOperationException(
            "Не вдалося знайти інтервал.");
    }

    private static void ValidatePoints(
        IReadOnlyList<PointD> points)
    {
        if (points.Count < 2)
        {
            throw new ArgumentException(
                "Потрібно щонайменше дві опорні точки.");
        }

        for (int i = 1; i < points.Count; i++)
        {
            if (points[i].X <= points[i - 1].X)
            {
                throw new ArgumentException(
                    "X опорних точок повинні бути строго зростаючими.");
            }
        }
    }
}