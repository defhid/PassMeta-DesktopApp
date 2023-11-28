rmdir /s /q .\windows

dotnet publish ..\PassMeta-DesktopApp.sln --runtime win-x64 --configuration Release --output .\windows

rename .\windows\PassMeta.DesktopApp.Ui.exe PassMeta.exe