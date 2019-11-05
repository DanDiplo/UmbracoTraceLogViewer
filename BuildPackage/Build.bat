xcopy ..\Umbraco.7.4\App_Plugins\DiploTraceLogViewer  ..\Umbraco\App_Plugins /e /i /h /s /y

Call nuget.exe restore ..\DiploTraceLogViewer.sln
Call "%programfiles(x86)%\Microsoft Visual Studio\2019\Community\MSBuild\Current\Bin\MSBuild.exe" package.build.xml /bl /p:Configuration=Release