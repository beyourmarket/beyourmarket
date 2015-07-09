# build NuGet package
Write-Output "Packing NuGet package"
..\src\.nuget\NuGet.exe Pack .\BeYourMarket.nuspec -Version $env:appveyor_build_version