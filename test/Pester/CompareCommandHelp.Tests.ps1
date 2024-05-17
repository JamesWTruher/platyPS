# Copyright (c) Microsoft Corporation.
# Licensed under the MIT License.

Describe "Compare-CommandHelp can find differences" {
    BeforeAll {
        $ch1 = Import-MarkdownCommandHelp "${PSScriptRoot}/assets/get-date.md"
        $ch2 = Import-MarkdownCommandHelp "${PSScriptRoot}/assets/get-date.alt01.md"
        $result1 = Compare-CommandHelp $ch1 $ch2
        $result2 = Compare-CommandHelp $ch1 $ch2 -PropertyNamesToExclude Diagnostics
    }

    It "Should properly identify that the objects are different" {
        $result1[-1] | Should -Be "Comparison result: 'NOT OK'"
    }

    It "Should properly identify the number of differences" {
        $result1.where({$_ -match "are not the same|are different"}).Count | Should -Be 11
    }

    It "Should properly identify the elements which are different" {
        $expected = "CommandHelp.Syntax.ParameterSetName", "CommandHelp.Syntax.ParameterNames", "CommandHelp.Syntax.ParameterSetName",
            "CommandHelp.Syntax.ParameterNames", "CommandHelp.Syntax.ParameterSetName", "CommandHelp.Syntax.ParameterNames",
            "CommandHelp.Syntax.ParameterSetName", "CommandHelp.Syntax.ParameterNames", "CommandHelp.Examples.Title",
            "CommandHelp.Examples.Remarks", "CommandHelp.Diagnostics"
        $observed = $result1.split("`n").Where({$_ -match "are not the same|are different"}).foreach({$_.trim().split()[0]})
        $observed | Should -Be $expected
    }

    It "Should be possible to exclude an element from comparison" {
        $expected = "CommandHelp.Syntax.ParameterSetName", "CommandHelp.Syntax.ParameterNames", "CommandHelp.Syntax.ParameterSetName",
            "CommandHelp.Syntax.ParameterNames", "CommandHelp.Syntax.ParameterSetName", "CommandHelp.Syntax.ParameterNames",
            "CommandHelp.Syntax.ParameterSetName", "CommandHelp.Syntax.ParameterNames", "CommandHelp.Examples.Title",
            "CommandHelp.Examples.Remarks"
        $observed = $result2.split("`n").Where({$_ -match "are not the same|are different"}).foreach({$_.trim().split()[0]})
        $observed | Should -Be $expected

    }
}