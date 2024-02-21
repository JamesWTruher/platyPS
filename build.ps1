[CmdletBinding(DefaultParameterSetName = 'Build')]
param(
    [Parameter(ParameterSetName = "Build")]
    [switch] $Clean,

    [Parameter(ParameterSetName = "Build")]
    [ValidateSet("Debug", "Release")]
    [string] $Configuration = "Debug",

    [string] $OutputDir = "$PSScriptRoot/out",

    [Parameter(ParameterSetName = "Test", Mandatory)]
    [switch] $Test,

    [Parameter(ParameterSetName = "Test")]
    [string] $XUnitLogPath = "$PSScriptRoot/xunit.tests.xml",

    [Parameter(ParameterSetName = "Test")]
    [string] $PesterLogPath = "$PSScriptRoot/pester.tests.xml"
)

if ($PSCmdlet.ParameterSetName -eq 'Build') {
    try {

        if ($Clean) {
            if (Test-Path $OutputDir) {
                Write-Verbose -Verbose "Cleaning output directory: $OutputDir"
                Remove-Item -Recurse -path $OutputDir -Force
            }

            $binFolder = "$PSScriptRoot/src/bin"
            if (Test-Path $binFolder) {
                Write-Verbose -Verbose "Cleaning output directory: $binFolder"
                Remove-Item -Recurse -path $binFolder -Force
            }

            $objFolder = "$PSScriptRoot/src/obj"
            if (Test-Path $objFolder) {
                Write-Verbose -Verbose "Cleaning output directory: $objFolder"
                Remove-Item -Recurse -path $objFolder -Force
            }
        }

        Push-Location "$PSScriptRoot/src"
        dotnet build --configuration $Configuration
        if ($LASTEXITCODE -ne 0) {
            throw "Build failure."
        }

        $expectedBuildPath = "./bin/$Configuration/net462/"
        $expectedDllPath = "$expectedBuildPath/Microsoft.PowerShell.PlatyPS.dll"
        $psd1Path = Join-Path $PSScriptRoot src platyPS.psd1
        $moduleFiles = @($expectedDllPath, $psd1Path)

        # in the case of a debug build, include the pdb
        if ($Configuration -eq "Debug") {
            $moduleFiles += "$expectedBuildPath/Microsoft.PowerShell.PlatyPS.pdb"
        }

        if (-not (Test-Path $moduleFiles)) {
            throw "Build did not succeed."
        }

        $moduleRoot = New-Item -Item Directory -Path "$OutputDir/platyPS" -Force
        $depsFolder = New-Item -Item Directory -Path "$moduleRoot/Dependencies" -Force
        $dependentAssemblies = @("$expectedBuildPath/Markdig.Signed.dll", "$expectedBuildPath/YamlDotNet.dll")

        Copy-Item -Path $dependentAssemblies -Destination $depsFolder -Verbose
        Copy-Item -Path $moduleFiles -Destination $moduleRoot -Verbose
    }
    finally {
        Pop-Location
    }
}
elseif ($PSCmdlet.ParameterSetName -eq 'Test') {
    $pesterTestRoot = "$PSScriptRoot/test/Pester"
    Write-Verbose "Executing Pester tests under $pesterTestRoot" -Verbose

    $sb = "Import-Module -Max 4.99 Pester
        Import-Module -Name '$OutputDir/platyPS' -Force
        Push-Location $pesterTestRoot
        Invoke-Pester -Outputformat nunitxml -outputfile $PesterLogPath"

    write-verbose -verbose -message "$sb"

    pwsh -noprofile -c "$sb"

    $results = [xml](Get-Content $PesterLogPath)
    if ($results."test-results".failures -ne 0) {
        throw "Pester Tests failed."
    }
}
