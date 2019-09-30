    function Remove-SubFoldersAndFiles 
    {
        Param ([string]$folderPath)
        Get-ChildItem -Path $folderPath -Recurse | Remove-Item -force -recurse
    }
    
    $user =  [Environment]::GetFolderPath("UserProfile")
    $sourceLocation = (Get-Location).Path
    
    $Destination =  $sourceLocation+'\NugetPackages'
    Remove-SubFoldersAndFiles -folderPath $Destination

    $Destination =  $user+ '\.nuget\packages'
    
    $nugetFolders = (Get-ChildItem $Destination -recurse | Where-Object {$_.PSIsContainer -eq $True -and $_.Name -like "liquid*"} | Sort-Object)
    
    foreach ($curfolder in $nugetFolders)
    {
        Remove-SubFoldersAndFiles($Destination + '\' + $curfolder.Name)
    }

    dotnet pack $sourceLocation\Liquid.All.sln --include-symbols --include-source
    
    $nugetFolders = (Get-ChildItem $sourceLocation -recurse | Where-Object {$_.PSIsContainer -eq $True -and $_.Name -like "liquid*"} | Sort-Object)

    foreach ($curfolder in $nugetFolders)
    {
        Copy-Item ($sourceLocation + '\' + $curfolder.Name + '\bin\Debug\*.nupkg') -Destination ($sourceLocation+'\NugetPackages') -Recurse
    }
