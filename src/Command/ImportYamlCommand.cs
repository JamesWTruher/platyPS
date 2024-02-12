// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Management.Automation;
using YamlDotNet;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;
using Microsoft.PowerShell.PlatyPS.Model;

namespace Microsoft.PowerShell.PlatyPS
{
    /// <summary>
    /// Import a yaml command help file.
    /// </summary>
    [Cmdlet(VerbsData.Import, "YamlCommandHelp", HelpUri = "", DefaultParameterSetName = "FromPath")]
    [OutputType(typeof(object))]
    public sealed class ImportYamlMetadataCommand : PSCmdlet
    {
        #region cmdlet parameters

        /// <summary>
        /// An array of paths to get the markdown metadata from.
        /// </summary>
        [Parameter(Mandatory = true, ParameterSetName = "FromPath", ValueFromPipelineByPropertyName = true, ValueFromPipeline = true, Position = 0)]
        [SupportsWildcards]
        public string[] Path { get; set; } = Array.Empty<string>();

        /// <summary>
        /// Yaml content provided as a string.
        /// </summary>
        [Parameter(Mandatory = true, ParameterSetName = "FromYamlString")]
        [ValidateNotNullOrEmpty()]
        public string? Yaml { get; set; }

        private IDeserializer? yamlDeserializer;
        #endregion

        protected override void BeginProcessing()
        {
            yamlDeserializer = new DeserializerBuilder().Build();
        }

        protected override void ProcessRecord()
        {
            if (string.Equals(this.ParameterSetName, "FromYamlString", StringComparison.OrdinalIgnoreCase))
            {
                if (Yaml is not null)
                {
                    var result = yamlDeserializer?.Deserialize<Dictionary<object,object>>(Yaml);
                    WriteObject(result);
                }
            }
            else if (string.Equals(this.ParameterSetName, "FromPath", StringComparison.OrdinalIgnoreCase))
            {
                foreach (string filePath in Path)
                {
                    Collection<PathInfo> resolvedPaths = this.SessionState.Path.GetResolvedPSPathFromPSPath(filePath);

                    foreach (var resolvedPath in resolvedPaths)
                    {
                        var result = yamlDeserializer?.Deserialize<IDictionary<object,object>>(File.ReadAllText(resolvedPath.Path));
                        if (result is not null)
                        {
                            // WriteObject(result);
                            WriteObject(ConvertDictionaryToCommandHelp(result));
                        }
                        else
                        {
                            WriteWarning($"The file {resolvedPath.Path} is not a valid yaml file.");
                        }
                    }
                }
            }
        }

        string[] requiredKeys = new string[] { "metadata", "title", "synopsis", "syntaxes", "aliases", "description", "examples", "parameters", "inputs", "outputs", "notes", "links" };

        internal CommandHelp ConvertDictionaryToCommandHelp(IDictionary<object, object> commandHelp)
        {
            CommandHelp help = GetCommandHelp(commandHelp);
            return help;

        }

        private CommandHelp GetCommandHelp(IDictionary<object, object> commandHelp)
        {
            bool ok = true;
            foreach (var key in requiredKeys)
            {
                if (!commandHelp.ContainsKey(key))
                {
                    WriteWarning($"The yaml file does not contain the required key {key}.");
                    ok = false;
                }
            }   

            if (! ok)
            {
                throw new ArgumentException("The yaml file does not contain all the required keys.");
            }

            if (commandHelp["metadata"] is not IDictionary<object, object> metadata)
            {
                throw new ArgumentException("The yaml file does not contain a metadata key.");
            }

            if (! (metadata.ContainsKey("Locale") && metadata.ContainsKey("title") && metadata.ContainsKey("Module Name")))
            {
                throw new ArgumentException("The yaml file does not contain a locale key.");
            }

            System.Globalization.CultureInfo cultureInfo = System.Globalization.CultureInfo.GetCultureInfo(metadata["Locale"].ToString());
            CommandHelp help = new CommandHelp(metadata["title"].ToString(), metadata["Module Name"].ToString(), cultureInfo);

            if (commandHelp.ContainsKey("synopsis"))
            {
                help.Synopsis = commandHelp["synopsis"].ToString();
            }

            if (commandHelp.ContainsKey("description"))
            {
                help.Description = commandHelp["description"].ToString();
            }

            if (commandHelp.ContainsKey("notes"))
            {
                help.Notes = commandHelp["notes"].ToString();
            }

            return help;
        }   
    }

}
