using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lab3Interpolation.Models;

public sealed class LinearInterpolation : Interpolation
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

        PointD p1 = points[i];
        PointD p2 = points[i + 1];

        double t = (x - p1.X) / (p2.X - p1.X);

        return p1.Y + t * (p2.Y - p1.Y);
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