# Copyright (c) Microsoft Corporation.
# Licensed under the MIT License.

$ErrorActionPreference = 'Stop'
. $PSScriptRoot/CommonFunction.ps1

Describe 'Export-YamlCommandHelp' {

    Context 'errors' {
        It 'throw when OutputFolder is not a folder' {
            $null = New-Item -ItemType File -Path "$TestDrive/somefile.txt"
            $ch = Import-MarkdownCommandHelp -Path (Join-Path $PSScriptRoot assets get-date.md)
            { $ch | Export-YamlCommandHelp -OutputFolder "$TestDrive/somefile.txt" } |
            Should -Throw -ErrorId 'PathIsNotFolder,Microsoft.PowerShell.PlatyPS.ExportYamlCommandHelpCommand'
        }
    }

    Context 'metadata' {
        BeforeAll {
            $markdown = Join-Path $PSScriptRoot assets get-date.md 
            $ch = Import-MarkdownCommandHelp -Path $markdown
            $outputFI = $ch | Export-YamlCommandHelp -OutputFolder $TestDrive
            $outputPath = $outputFI.FullName
            import-module (Join-Path $PSScriptRoot PlatyPS.Test.psm1)
            $yaml = Import-CommandYaml -fullname $outputPath
        }

        It "Metadata should contain the '<Name>' key" -pending:$false -testCases @(
            @{ Name = "external help file" }
            @{ Name = "Module Name" }
            @{ Name = "online version" }
            @{ Name = "aliases" }
            @{ Name = "schema" }
        ) {
            param ($Name)
            $yaml.Metadata.Keys | Should -Contain $Name
        }

        It "Metadata will write new metadata if added to command help object" {
            $ch.Metadata.Add("newKey", "newValue")
            $outputFI = $ch | Export-YamlCommandHelp -OutputFolder $TestDrive -Force
            $yaml = Import-CommandYaml -fullname $outputFI.FullName
            $yaml.Metadata['newKey'] | Should -Be "newValue"
        }
    }

    Context 'encoding' {
        It 'writes appropriate encoding' {
            $outputFI = $ch | Export-YamlCommandHelp -OutputFolder $TestDrive -Encoding ([System.Text.Encoding]::UTF32)
            $bytes = Get-Content -Path $outputFI.FullName -asbyteStream | Select-Object -First 2
            $bytes | Should -Be 255,254
        }
    }

    Context 'Yaml Content' {
        BeforeAll {
            $markdown = Join-Path $PSScriptRoot assets get-date.md
            $ch = Import-MarkdownCommandHelp -Path $markdown
            $outputFI = $ch | Export-YamlCommandHelp -OutputFolder $TestDrive
            $yamlFile = $outputFI.FullName
        }

        It "Should contain the header '<line>'" -pending:$false -TestCases @(
            @{ line = "synopsis:" }
            @{ line = "syntaxes:" }
            @{ line = "aliases:" }
            @{ line = "description:" }
            @{ line = "examples:" }
            @{ line = "parameters:" }
            @{ line = "inputs:" }
            @{ line = "outputs:" }
            @{ line = "notes:" }
            @{ line = "links:" }
        ) {
            param ($line)
            $yamlFile | Should -FileContentMatch "^$line"
        }

        It "The order of the header lines should be correct" -pending:$false {
            $correctOrder = "synopsis:",
                "syntaxes:",
                "aliases:",
                "description:",
                "examples:",
                "parameters:",
                "inputs:",
                "outputs:",
                "notes:",
                "links:"
            $searchPattern = $correctOrder | %{ "^$_" }
            $observedOrder = Select-String $searchPattern $yamlFile | Sort-Object -Property LineNumber | ForEach-Object { $_.Line -replace ': .*',':' }
            $observedOrder | Should -Be $correctOrder
        }

        It "The alias section should contain the proper boiler-plate" -pending {
            $expectedMessage = "This cmdlet has the following aliases:"
            $observedLine = ""
            for($i = 0; $i -lt $lines.count; $i++) {
                if ($lines[$i] -eq "## ALIASES") {
                    $observedLine = $lines[$i+2]
                    break
                }
            }
            $observedLine | Should -BeExactly $expectedMessage
        }
    }
}
