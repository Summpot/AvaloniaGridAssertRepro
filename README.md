# Avalonia Grid MathUtilities.IsZero Assertion Repro

Minimal reproducible sample demonstrating a floating-point precision assertion failure (`Assertion failed: MathUtilities.IsZero(sizeToDistribute)`) in Avalonia's `Grid.EnsureMinSizeInDefinitionRange` under `Debug` mode.

## Project Structure

```
AvaloniaGridAssertRepro/
├── submodules/
│   └── Avalonia/            # Official Avalonia repository (Git Submodule)
├── AvaloniaGridAssertRepro.csproj
├── Program.cs                # Minimal reproduction entrypoint (~25 lines)
└── README.md
```

## How to Run

### 1. Debug Configuration (Assertion Triggered)
In `Debug` configuration, `Debug.Assert` calls are active, triggering the fatal assertion failure:

```powershell
dotnet run
```

**Output:**
```
=== Avalonia Grid MathUtilities.IsZero Assertion Repro ===
Setting up Grid with 7 Star rows, RowSpacing=12, and a child spanning rows 4..6...
Calling grid.Measure(new Size(1000, 1278))...
Process terminated. Assertion failed.
   at Avalonia.Controls.Grid.EnsureMinSizeInDefinitionRange(IReadOnlyList`1 definitions, Int32 start, Int32 count, Double spacing, Double desiredSize) in ...\Grid.cs:line 1339
   at Avalonia.Controls.Grid.MeasureCellsGroup(...)
   at Avalonia.Controls.Grid.MeasureOverride(...)
   at Avalonia.Layout.Layoutable.Measure(...)
   at Program.<Main>$(String[] args) in ...\Program.cs:line 27
```

### 2. Release Configuration (Succeeds)
In `Release` configuration, `Debug.Assert` calls are stripped by the compiler, allowing normal execution:

```powershell
dotnet run -c Release
```

**Output:**
```
=== Avalonia Grid MathUtilities.IsZero Assertion Repro ===
Setting up Grid with 7 Star rows, RowSpacing=12, and a child spanning rows 4..6...
Calling grid.Measure(new Size(1000, 1278))...
Measure completed successfully (Assertion did not fail).
```

---

## Technical Details

### Root Cause
1. **Scenario**:
   - A `Grid` has 7 `*` (Star) row definitions and `RowSpacing = 12`.
   - A child control spans across rows 4..6 (`RowSpan = 3`) with desired height `540.8571428571429`.
   - When available height is `1278`, spacing across 7 rows accounts for `12 * 6 = 72`, leaving `1206` for star resolution. Each star row receives `1206 / 7 = 172.28571428571428...`.
   - The child spanning 3 rows requests `540.8571428571429 - 12 * 2 = 516.8571428571429`.

2. **Floating-point Residual**:
   - `EnsureMinSizeInDefinitionRange` distributes `516.8571428571429` proportionally across the 3 rows.
   - Successive divisions and subtractions in IEEE 754 double precision leave a residual:
     $$\text{sizeToDistribute} \approx 2.842170943040401 \times 10^{-14}$$
   - For floating-point numbers of magnitude ~500, 1 ULP is approximately $1.13 \times 10^{-13}$. The leftover residual of $2.84 \times 10^{-14}$ is smaller than a single ULP, making it mathematically zero within physical layout limits.

3. **Assertion Invalidation**:
   - `MathUtilities.IsZero(double value)` checks:
     $$\text{Math.Abs}(value) < 10.0 \times \text{DoubleEpsilon} \approx 2.22 \times 10^{-15}$$
   - Because $2.84 \times 10^{-14} > 2.22 \times 10^{-15}$, `MathUtilities.IsZero` returns `false`, causing `Debug.Assert` to terminate the process.

