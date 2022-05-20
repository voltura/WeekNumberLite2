"C:\Program Files\Microsoft Visual Studio\2022\Community\MSBuild\Current\Bin\MSBuild.exe" WeekNumberLite2.sln /p:Platform=x86 /t:Clean,Build /property:Configuration=Release -maxcpucount
"C:\Program Files\Microsoft Visual Studio\2022\Community\dotnet\runtime\dotnet.exe" publish -p:PublishProfile=FolderProfile
