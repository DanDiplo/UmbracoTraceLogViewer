xcopy ..\Umbraco.7.4\App_Plugins\DiploTraceLogViewer  ..\Umbraco\App_Plugins /e /i /h /s /y

Call nuget.exe restore ..\DiploTraceLogViewer.sln
Call "C:\Program Files (x86)\MSBuild\12.0\Bin\MsBuild.exe" Package.build.xml /p:Configuration=Release