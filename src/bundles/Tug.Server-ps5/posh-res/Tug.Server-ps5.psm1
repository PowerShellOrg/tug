function Install-TugServer {
<#
.SYNOPSIS
Installs the Tug DSC Pull Server.

.PARAMETER InstallPath
Overrides the default target installation path (%PROGRAMFILES%\Tug\Server).

.PARAMETER Overwrite
If the InstallPath exists and is not empty and then the installation will fail unless
this switch is specified.  When specified, it will overwrite any files in the target
location except for configuration files.
#>
    [CmdletBinding()]
    param(
        [string]$InstallPath="$($env:ProgramFiles)\Tug\Server",
        [switch]$Overwrite
    )

    $fullPath = [System.IO.Path]::GetFullPath([System.IO.Path]::Combine($PWD, $InstallPath))
    Write-Verbose "Resolved installation path as [$fullPath]"

    if (Test-Path -PathType Container $fullPath) {
        if (-not $Overwrite -and (Get-ChildItem $fullPath).Length) {
            Write-Error "Target installation path is not empty; specify Overwrite switch to force install"
            return
        }
        Write-Verbose "Target installation path already exists"
    }
    else {
        Write-Verbose "Creating target installation path"
        mkdir $fullPath -Force | Out-Null
    }

    $tugModule = $MyInvocation.MyCommand.Module
    Write-Verbose "My Module is [$tugModule] is located at [$($tugModule.Path)]"

    $binSource = [System.IO.Path]::Combine($tugModule.ModuleBase, "bin")
    $smpSource = [System.IO.Path]::Combine($tugModule.ModuleBase, "samples")

    if (-not (Test-Path -PathType Container $binSource)) {
        Write-Error "Cannot resolve installation source files"
        return
    }

    if (-not (Test-Path -PathType Container $smpSource)) {
        Write-Error "Cannot resolve installation configuration files"
        return
    }

    Write-Verbose "Copying binary files over"
    Copy-Item $binSource -Destination "$fullPath\bin" -Recurse -Force

    foreach ($f in (Get-ChildItem -Path $smpSource)) {
        Write-Verbose "Installing [$f]:"

        $hashSource = Get-FileHash $f.FullName -Algorithm MD5
        $fileFullPath = [System.IO.Path]::Combine($fullPath, $f.Name)

        if (Test-Path $fileFullPath) {
            Write-Verbose "  * already found at [$fileFullPath]"
            $hashTarget = Get-FileHash $fileFullPath -Algorithm MD5
            
            if ($hashTarget.Hash -ne $hashSource.Hash) {
                $samplePath = [System.IO.Path]::Combine($fullPath, "sample")
                $fileFullPath = [System.IO.Path]::Combine($samplePath, $f.Name)

                ## Make sure there is a place to stash our samples
                if (-not (Test-Path -PathType Container $samplePath)) {
                    mkdir $samplePath -Force | Out-Null
                }

                $copyIndex = 0
                while (Test-Path $fileFullPath) {
                    $hashTarget = Get-FileHash $fileFullPath -Algorithm MD5

                    ## We only want to save samples if they're newer
                    if ($hashTarget.Hash -eq $hashSource.Hash) {
                        Write-Verbose "  * latest sample already found at [$fileFullPath]"
                        $fileFullPath = $null
                        break
                    }

                    ## Advance to the next candidate sample name
                    $fileFullPath = $fileFullPath -replace '_\d+$',''
                    $fileFullPath += "_$((++$copyIndex))"
                }

                if ($fileFullPath) {
                    Write-Verbose "  * saving as sample file to [$fileFullPath]"
                    Copy-Item $f.FullName -Destination $fileFullPath
                }
            }
        }
        else {
            Write-Verbose "  * creating initial at [$fileFullPath]"
            Copy-Item $f.FullName $fileFullPath
        }
    }
}
