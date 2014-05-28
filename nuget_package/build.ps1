Copy-Item ..\Deploy\Release\x86\packs\dx11\core\VVVV.DX11.Core.dll .\build\net40\lib\x86\VVVV.DX11.Core.dll
Copy-Item ..\Deploy\Release\x86\packs\dx11\core\VVVV.DX11.Lib.dll .\build\net40\lib\x86\VVVV.DX11.Lib.dll
Copy-Item ..\Deploy\Release\x64\packs\dx11\core\VVVV.DX11.Core.dll .\build\net40\lib\x64\VVVV.DX11.Core.dll
Copy-Item ..\Deploy\Release\x64\packs\dx11\core\VVVV.DX11.Lib.dll .\build\net40\lib\x64\VVVV.DX11.Lib.dll

nuget pack -NoPackageAnalysis