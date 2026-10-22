WPF Modern - dotnet run. Design matches web version exactly. Segoe UI Variable, cards 8px radius, shadow, accent #0078D4. DataGrid alternating #FBFBFB, selected #0078D4.

October 21 startup fix: App.xaml now specifies StartupUri="MainWindow.xaml".
Previously the application entry point ran without creating its main window.
Build with: dotnet build WorkOrderWPF.csproj -c Release
Run bin\Release\net8.0-windows\DVO_WORK_ORDER_V8_WPF.exe, keeping its DLL,
deps.json and runtimeconfig.json alongside it. Requires .NET 8 Desktop Runtime.
This is an independent sample UI with hardcoded data and mock actions, not the
production addin. No database access or real saving/printing is implemented.

The original output was locked by earlier invisible sample processes. A verified
corrected copy is in bin\Release\StartupFixed\DVO_WORK_ORDER_V8_WPF.exe.
Build that copy with:
dotnet build WorkOrderWPF.csproj -c Release -o bin\Release\StartupFixed
Its main window was verified to open. No old processes were forcibly closed.
