# APK Analyzer App

C# ASP.NET Core application to upload APK files, analyze permissions, and generate a basic security report.

## Features
- Upload APK files
- Extract basic app metadata
- Detect common risky permissions
- Generate HTML report
- Risk score calculation

## Tech stack
- ASP.NET Core Web API
- C#
- HTML report generation

## Project structure
- `Controllers/` – API endpoints
- `Models/` – request and report models
- `Services/` – analysis and report logic
- `Utilities/` – helper commands
- `Uploads/` – uploaded APKs
- `Reports/` – generated HTML reports

## Run locally

```bash
dotnet restore
dotnet build
dotnet run
```

Then open Swagger at:

```text
https://localhost:5001/swagger
```

You can upload an APK using the `POST /api/ApkUpload/upload` endpoint.

## Notes
This starter project uses `aapt` when available to extract package data and permissions. It is a good foundation for building a fuller APK security scanner.
