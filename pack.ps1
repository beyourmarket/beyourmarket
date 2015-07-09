# build NuGet package
Write-Output "Building NuGet package"
& .\src\.nuget\NuGet.exe pack .\src\BeYourMarket.nuspec -Version $env:appveyor_build_version