# build NuGet package
Write-Output "Building NuGet package"
& .\NuGet.exe pack ..\BeYourMarket.nuspec

$x = $host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")