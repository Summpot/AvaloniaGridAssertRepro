using System;
using Avalonia;
using Avalonia.Controls;

Console.WriteLine("=== Avalonia Grid MathUtilities.IsZero Assertion Repro ===");
Console.WriteLine("Setting up Grid with 7 Star rows, RowSpacing=12, and a child spanning rows 4..6...");

var grid = new Grid
{
    RowSpacing = 12,
    ColumnDefinitions = new ColumnDefinitions("*"),
    RowDefinitions = new RowDefinitions("*,*,*,*,*,*,*"),
};

// Child spans rows 4..6 (RowSpan = 3) with Desired Height = 540.8571428571429
// When available height is 1278 (inner available height = 1278 - 72 = 1206):
// 7 rows of 1* receive 1206 / 7 = 172.28571428571428... each.
// Rows 4..6 sum to 516.8571428571429 (requested size without spacing).
// When EnsureMinSizeInDefinitionRange distributes 516.8571428571429 across 3 rows,
// IEEE 754 division leaves a residual of ~2.84e-14, which exceeds 10 * DoubleEpsilon (2.22e-15).
var child = new Border { Height = 540.8571428571429 };
Grid.SetRow(child, 4);
Grid.SetRowSpan(child, 3);
grid.Children.Add(child);

Console.WriteLine("Calling grid.Measure(new Size(1000, 1278))...");
grid.Measure(new Size(1000, 1278));

Console.WriteLine("Measure completed successfully (Assertion did not fail).");
