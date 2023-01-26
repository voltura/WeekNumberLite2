"C:\Program Files\Microsoft Visual Studio\2022\Professional\MSBuild\Current\Bin\MSBuild.exe" WeekNumberLite2.sln /p:Platform=x64 /t:Clean,Build /property:Configuration=Release -maxcpucount
"C:\Program Files\Microsoft Visual Studio\2022\Professional\dotnet\runtime\dotnet.exe" publish -r win-x64 -c Release
