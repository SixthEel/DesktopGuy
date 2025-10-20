@echo off
echo Building GIF Overlay Windows Forms Application...

REM Compile the C# code
echo Compiling C# code...
C:\Windows\Microsoft.NET\Framework64\v4.0.30319\csc.exe /target:winexe /out:GifOverlayApp.exe /reference:System.Windows.Forms.dll,System.Drawing.dll GifOverlayForms.cs

echo Build completed!
if exist GifOverlayApp.exe (
    echo Executable created successfully: GifOverlayApp.exe
) else (
    echo Failed to create executable.
)

pause