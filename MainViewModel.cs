using Lab3Interpolation.Commands;
using Lab3Interpolation.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using static System.Net.Mime.MediaTypeNames;

namespace Lab3Interpolation.ViewModels;

public class MainViewModel : INotifyPropertyChanged
{
    private const double CanvasWidth = 900;
    private const double CanvasHeight = 300;
    private const double Margin = 40;

    private double _h1 = Math.PI / 4;
    private double _h2 = Math.PI / 32;

    private bool _showReference = true;
    private bool _showSupport = true;
    private bool _showLinear = true;
    private bool _showHermite = true;

    private string _status = "Готово.";

    public PointCollection ReferencePoints { get; } = new();
    public PointCollection SupportPoints { get; } = new();
    public PointCollection LinearPoints { get; } = new();
    public PointCollection HermitePoints { get; } = new();
    public PointCollection LinearErrorPoints { get; } = new();
    public PointCollection HermiteErrorPoints { get; } = new();

    public double H1
    {
        get => _h1;
        set
        {
            if (SetField(ref _h1, value))
            {
                OnPropertyChanged(nameof(IsInputValid));
                ((RelayCommand)BuildCommand).RaiseCanExecuteChanged();
            }
        }
    }

   public double H2
{
    get => _h2;
    set
    {
        if (SetField(ref _h2, value))
        {
            OnPropertyChanged(nameof(IsInputValid));
            ((RelayCommand)BuildCommand).RaiseCanExecuteChanged();
        }
    }
}

    public bool ShowReference
    {
        get => _showReference;
        set => SetField(ref _showReference, value);
    }

    public bool ShowSupport
    {
        get => _showSupport;
        set => SetField(ref _showSupport, value);
    }

    public bool ShowLinear
    {
        get => _showLinear;
        set => SetField(ref _showLinear, value);
    }

    public bool ShowHermite
    {
        get => _showHermite;
        set => SetField(ref _showHermite, value);
    }

    public string Status
    {
        get => _status;
        private set => SetField(ref _status, value);
    }

    public bool IsInputValid =>
        H1 > 0 &&
        H2 > 0 &&
        H2 < H1;

    public ICommand BuildCommand { get; }
    public ICommand TestCommand { get; }

    public MainViewModel()
    {
        BuildCommand = new RelayCommand(
            Build,
            () => IsInputValid);

        TestCommand = new RelayCommand(Test);

        Build();
    }

    private void Test()
    {
        try
        {
            Interpolation[] interpolations =
            {
            new LinearInterpolation(),
            new HermiteInterpolation()
        };

            var equidistantPoints = new List<PointD>
        {
            new(0, 0),
            new(1, Math.Sin(1)),
            new(2, Math.Sin(2)),
            new(3, Math.Sin(3))
        };

            var nonEquidistantPoints = new List<PointD>
        {
            new(0, 0),
            new(0.5, Math.Sin(0.5)),
            new(1.7, Math.Sin(1.7)),
            new(3, Math.Sin(3))
        };

            const double tolerance = 1e-10;

            foreach (Interpolation interpolation in interpolations)
            {
                foreach (PointD point in equidistantPoints)
                {
                    double result =
                        interpolation.Calculate(
                            equidistantPoints,
                            point.X);

                    if (Math.Abs(result - point.Y) > tolerance)
                    {
                        throw new Exception(
                            "Помилка перевірки опорних точок.");
                    }
                }

                foreach (PointD point in nonEquidistantPoints)
                {
                    double result =
                        interpolation.Calculate(
                            nonEquidistantPoints,
                            point.X);

                    if (Math.Abs(result - point.Y) > tolerance)
                    {
                        throw new Exception(
                            "Помилка для нерівновіддалених точок.");
                    }
                }
            }

            var duplicateXPoints = new List<PointD>
        {
            new(0, 0),
            new(1, 1),
            new(1, 0)
        };

            bool duplicateRejected = false;

            try
            {
                interpolations[0].Calculate(
                    duplicateXPoints,
                    1);
            }
            catch (ArgumentException)
            {
                duplicateRejected = true;
            }

            if (!duplicateRejected)
            {
                throw new Exception(
                    "Однакові значення X не були відхилені.");
            }

            Status =
                "Перевірка пройдена: " +
                "рівновіддалені та нерівновіддалені точки " +
                "коректні, однакові X відхиляються.";
        }
        catch (Exception ex)
        {
            Status = "Помилка перевірки: " + ex.Message;
        }
    }

    private void Build()
    {
        if (!IsInputValid)
        {
            Status = "Помилка: потрібно, щоб h₁ > 0, h₂ > 0 та h₂ < h₁.";
            return;
        }

        double start = 0;
        double end = 2 * Math.PI;

        List<PointD> support = BuildSupportPoints(
            start,
            end,
            H1);

        Interpolation[] interpolations =
        {
            new LinearInterpolation(),
            new HermiteInterpolation()
        };

        List<PointD> linear =
            BuildInterpolatedPoints(
                support,
                H2,
                interpolations[0]);

        List<PointD> hermite =
            BuildInterpolatedPoints(
                support,
                H2,
                interpolations[1]);

        List<PointD> reference =
            BuildReferencePoints(
                start,
                end,
                H2 / 4);

        List<PointD> linearError =
    BuildErrorPoints(
        linear,
        x => Math.Sin(x));

        List<PointD> hermiteError =
            BuildErrorPoints(
                hermite,
                x => Math.Sin(x));

        UpdateCollection(
            SupportPoints,
            ToScreenPoints(support));

        UpdateCollection(
            LinearPoints,
            ToScreenPoints(linear));

        UpdateCollection(
            HermitePoints,
            ToScreenPoints(hermite));

        UpdateCollection(
            ReferencePoints,
            ToScreenPoints(reference));

        UpdateCollection(
            LinearErrorPoints,
            ToErrorScreenPoints(linearError));

        UpdateCollection(
            HermiteErrorPoints,
            ToErrorScreenPoints(hermiteError));

        Status =
            $"Опорних точок: {support.Count}; " +
            $"точок інтерполяції: {linear.Count}; " +
            $"h₁ = {H1:F4}; h₂ = {H2:F4}";
    }

    private static List<PointD> BuildSupportPoints(
    double start,
    double end,
    double step)
    {
        var points = new List<PointD>();

        for (double x = start; x < end; x += step)
        {
            points.Add(
                new PointD(
                    x,
                    Math.Sin(x)));
        }

        if (points.Count == 0 ||
            Math.Abs(points[^1].X - end) > 1e-10)
        {
            points.Add(
                new PointD(
                    end,
                    Math.Sin(end)));
        }

        return points;
    }

    private static List<PointD> BuildReferencePoints(
        double start,
        double end,
        double step)
    {
        return BuildSupportPoints(start, end, step);
    }

    private static List<PointD> BuildInterpolatedPoints(
    IReadOnlyList<PointD> support,
    double step,
    Interpolation interpolation)
    {
        var result = new List<PointD>();

        double start = support[0].X;
        double end = support[^1].X;

        for (double x = start; x < end; x += step)
        {
            result.Add(
                new PointD(
                    x,
                    interpolation.Calculate(
                        support,
                        x)));
        }

        if (result.Count == 0 ||
            Math.Abs(result[^1].X - end) > 1e-10)
        {
            result.Add(
                new PointD(
                    end,
                    interpolation.Calculate(
                        support,
                        end)));
        }

        return result;
    }

    private static List<PointD> BuildErrorPoints(
    IReadOnlyList<PointD> points,
    Func<double, double> referenceFunction)
    {
        var result = new List<PointD>();

        foreach (PointD point in points)
        {
            double error =
                Math.Abs(
                    point.Y -
                    referenceFunction(point.X));

            result.Add(
                new PointD(
                    point.X,
                    error));
        }

        return result;
    }

    private static List<Point> ToScreenPoints(
        IReadOnlyList<PointD> points)
    {
        const double xMin = 0;
        const double xMax = 2 * Math.PI;
        const double yMin = -1.2;
        const double yMax = 1.2;

        return points
            .Select(p =>
            {
                double screenX =
                    Margin +
                    (p.X - xMin)
                    / (xMax - xMin)
                    * (CanvasWidth - 2 * Margin);

                double screenY =
                    CanvasHeight -
                    Margin -
                    (p.Y - yMin)
                    / (yMax - yMin)
                    * (CanvasHeight - 2 * Margin);

                return new Point(screenX, screenY);
            })
            .ToList();
    }

    private static List<Point> ToErrorScreenPoints(
    IReadOnlyList<PointD> points)
    {
        const double xMin = 0;
        const double xMax = 2 * Math.PI;

        const double yMin = 0;
        const double yMax = 0.15;

        const double errorCanvasWidth = 900;
        const double errorCanvasHeight = 180;
        const double errorMargin = 20;

        return points
            .Select(p =>
            {
                double screenX =
                    errorMargin +
                    (p.X - xMin)
                    / (xMax - xMin)
                    * (errorCanvasWidth - 2 * errorMargin);

                double screenY =
                    errorCanvasHeight -
                    errorMargin -
                    (p.Y - yMin)
                    / (yMax - yMin)
                    * (errorCanvasHeight - 2 * errorMargin);

                return new Point(screenX, screenY);
            })
            .ToList();
    }

    private static void UpdateCollection(
    PointCollection target,
    IEnumerable<Point> source)
    {
        target.Clear();

        foreach (Point point in source)
        {
            target.Add(point);
        }
    }

    private bool SetField<T>(
        ref T field,
        T value,
        [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(
                field,
                value))
        {
            return false;
        }

        field = value;

        OnPropertyChanged(propertyName);

        return true;
    }

    private void OnPropertyChanged(
        [CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(
            this,
            new PropertyChangedEventArgs(propertyName));
    }

    public event PropertyChangedEventHandler?
        PropertyChanged;
}