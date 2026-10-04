using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lab3Interpolation.Models;

public abstract class Interpolation
{
    public abstract double Calculate(
        IReadOnlyList<PointD> points,
        double x);
}