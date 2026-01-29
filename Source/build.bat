@echo off
chcp 936 >nul
setlocal enabledelayedexpansion

echo ========================================
echo Mech Commander MOD Build Tool
echo ========================================
echo.

REM Check dotnet
echo Checking .NET SDK...
where dotnet >nul 2>nul
if %ERRORLEVEL% NEQ 0 (
    echo Error: dotnet not found. Please install .NET SDK.
    echo https://dotnet.microsoft.com/download
    echo.
    pause
    exit /b 1
)

REM Check project file
if not exist "MechCommander.csproj" (
    echo Error: MechCommander.csproj not found.
    echo Please run this script in the Source folder.
    echo.
    pause
    exit /b 1
)

REM Clean old build outputs (Source folder)
echo Cleaning old build outputs...
if exist "bin" (
    echo Removing bin...
    rmdir /s /q "bin"
)
if exist "obj" (
    echo Removing obj...
    rmdir /s /q "obj"
)

REM Ensure Assemblies output folder
if not exist "..\1.6\Assemblies" (
    mkdir "..\1.6\Assemblies"
)
echo Clean done.
echo.

REM Build config (Release only)
set "CONFIG=Release"
echo Using configuration: %CONFIG%
echo.

REM Build
echo Building C# sources...
dotnet build "MechCommander.csproj" --configuration %CONFIG% --verbosity normal

set "BUILD_RESULT=%ERRORLEVEL%"
echo.
echo Build finished, dotnet exit code: %BUILD_RESULT%

if %BUILD_RESULT% EQU 0 (
    REM Clean bin/obj again
    if exist "bin" rmdir /s /q "bin"
    if exist "obj" rmdir /s /q "obj"

    REM Remove PDB files from output
    echo Removing PDB files...
    if exist "..\1.6\Assemblies\MechCommander.pdb" (
        del /q "..\1.6\Assemblies\MechCommander.pdb"
        echo Removed MechCommander.pdb
    )

    echo.
    echo ========================================
    echo Build succeeded!
    echo Expected output:
    echo   ..\1.6\Assemblies\MechCommander.dll
    echo ========================================
    echo.
    if exist "..\1.6\Assemblies\MechCommander.dll" (
        echo Found DLL:
        dir "..\1.6\Assemblies\MechCommander.dll" /b
    ) else (
        echo Warning: MechCommander.dll not found in output.
        echo Check dotnet output or errors above.
    )
    echo.
    echo Assemblies folder content:
    dir "..\1.6\Assemblies" /b
) else (
    echo.
    echo ========================================
    echo Build failed!
    echo Check the errors above.
    echo ========================================
)

echo.
pause
endlocal
