Param (
    $parameters = @{},
    $srcFolder,
    $projectName,
    $projectVersion
)

# update package version in nuspec file
Write-Output "Updating version in nuspec file"
$nuspecPath = ".\src\BeYourMarket.nuspec"
[xml]$xml = Get-Content $nuspecPath
$xml.package.metadata.version = $projectVersion
$xml.Save($nuspecPath)

# build NuGet package
Write-Output "Building NuGet package"
& .\src\.nuget\NuGet.exe pack .\src\BeYourMarket.nuspec