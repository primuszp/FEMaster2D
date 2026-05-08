# FEMaster 2D

[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)

A modern finite element analysis (FEA) and visualization tool for 2D constant-strain triangle elements. Built in C# with .NET and WinForms, FEMaster 2D provides an intuitive interface for analyzing structural mechanics problems in 2D space.

## Overview

FEMaster 2D is a modernized implementation of the classical finite element method for 2D static analysis. It features:

- **Direct Condensation Solver**: Efficient solver for linear systems using direct condensation method
- **Triangle Element Analysis**: Constant-strain triangle (CST) element formulation
- **Interactive Visualization**: Real-time mesh visualization with deformed shape overlays
- **Post-Processing**: Stress and strain contour plotting with customizable legends
- **Model Import/Export**: Support for custom FEM model file formats

## Features

- **Model Definition**
  - Load FEM models with node coordinates, triangular elements, boundary conditions, and applied loads
  - Visual inspection of node numbering, element connectivity, supports, and load distribution

- **Numerical Analysis**
  - Direct condensation solver for static linear analysis
  - Automatic handling of boundary conditions and support reactions
  - Efficient memory-optimized solver implementation

- **Visualization & Post-Processing**
  - Undeformed and deformed mesh visualization with scaling control
  - Contour plots for stress and strain distribution
  - Element and node selection with interactive highlighting
  - High-quality viewport export as image files (PNG, BMP, etc.)

## Example Result

![Model result](assets/model-result.png)

## Architecture

The project is organized into two main components:

| Component | Purpose |
|-----------|---------|
| **FEMaster.Core** | Core FEM solver, element mathematics, model structure, and matrix operations |
| **FEMaster.Form** | WinForms user interface, visualization layer, and graphics rendering |

## Building & Running

### Prerequisites
- .NET SDK 6.0 or higher
- Visual Studio 2022 (recommended) or any .NET-compatible IDE

### Build Instructions

```bash
# Clone the repository
git clone https://github.com/Primusz/FEMaster2D.git
cd FEMaster2D

# Build with .NET SDK
dotnet build FEMaster/FEMaster.sln

# Or open in Visual Studio 2022
start FEMaster/FEMaster.sln
```

## Technical Background

This project is a C# modernization of the original **btFEM** Visual Basic project, based on the finite element theory and algorithms described in the foundational CodeProject article:

> **"Finite Element Analysis in VB.NET"** — Original research and implementation guide  
> [View on CodeProject](https://www.codeproject.com/Articles/1207954/Finite-Element-Analysis-in-VB-Net)

The modernization includes:
- Contemporary .NET architecture and best practices
- Improved computational performance in the solver
- Enhanced WinForms UI with modern styling
- Cleaned codebase with better separation of concerns

## Documentation

Original research and theoretical foundation:
- `docs/Finite Element Analysis in VB.pdf` — Complete FEA theory and implementation guide
- `docs/btFEM-original-VB-source.zip` — Original VB.NET source code (archived from CodeProject)
- `docs/ORIGINAL_SOURCE.md` — Attribution and reference information

## Project Structure

```
FEMaster2D/
├── FEMaster/
│   ├── FEMaster.Core/       # FEM solver and element mathematics
│   └── FEMaster.Form/       # WinForms UI and graphics layer
├── assets/                   # Screenshots and visual assets
└── docs/                     # Original documentation and references
```

## Notes

- The legacy VB source code is archived separately, not included in this repository for clarity and maintainability
- Development and build artifacts are excluded via `.gitignore`

## License

This project is provided as-is for educational and research purposes. Please refer to the LICENSE file for details.

## Contributing

Contributions are welcome! Feel free to submit issues and pull requests to help improve FEMaster 2D.
