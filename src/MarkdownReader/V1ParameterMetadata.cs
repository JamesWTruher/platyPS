using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using YamlDotNet;
using YamlDotNet.Serialization;
using Microsoft.PowerShell.PlatyPS.MarkdownWriter;
using Microsoft.PowerShell.PlatyPS.Model;

namespace Microsoft.PowerShell.PlatyPS
{
    public class ParameterMetadataV1
    {
		public string Type { get; set; } = string.Empty;
		[YamlMember(Alias = "Parameter Sets")]
		public string ParameterSets { get; set; } = string.Empty;
		public string Aliases { get; set; } = string.Empty;
		public bool Required { get; set; }
		public string Position { get; set; } = string.Empty;
		[YamlMember(Alias = "Default value")]
		public string DefaultValue { get; set; } = string.Empty;
        [YamlMember(Alias = "Accept pipeline input")]
        public string AcceptPipelineInput { get; set; } = string.Empty;
		[YamlMember(Alias = "Accept wildcard characters")]
		public bool AcceptWildcardCharacters { get; set; }

        public string[] GetParameterSetList()
        {
            List<string> l = new List<string>();
			if (string.IsNullOrEmpty(ParameterSets))
			{
				return new string[]{ "(All)" };
			}

            foreach(var p in ParameterSets.Split(Constants.Comma, StringSplitOptions.RemoveEmptyEntries))
            {
                l.Add(p.Trim());
            }
            return l.ToArray();
        }

        public bool ParameterSetIncludes(string name)
        {
            foreach(var p in ParameterSets.Split(Constants.Comma, StringSplitOptions.RemoveEmptyEntries))
            {
                if (string.Compare(p.Trim(), name, true) == 0)
                {
                    return true;
                }
            }
            return false;
        }

		public string[] GetAliases()
		{
			if (string.IsNullOrEmpty(Aliases))
			{
				return new string[] { };
			}

            List<string> l = new List<string>();
            foreach(var p in Aliases.Split(Constants.Comma, StringSplitOptions.RemoveEmptyEntries))
            {
                l.Add(p.Trim());
            }
            return l.ToArray();
		}

        // We have a number of things to check because there's no consistency in the
        // documentation repositories.
        private Regex trueNameRegex = new Regex(@"^true \(.*ByProperty", RegexOptions.IgnoreCase);
        private Regex trueValueRegex = new Regex(@"^true \(.*ByValue", RegexOptions.IgnoreCase);
        private Regex valueRegex = new Regex(@"^ByValue \((?<value>\w+)\), ByName \((?<name>\w+)\)", RegexOptions.IgnoreCase);

		public bool GetByValue(string byValueAndProperty)
		{
            // This might be one of the following:
            // ByValue (False), ByName (False)
            // ByValue (False), ByName (True)
            // ByValue (System.Object[]), ByName (System.Object[])
            // ByValue (True), ByName (False)
            // False
            // True (ByPropertyName, ByValue)
            // True (ByPropertyName)
            // True (ByValue)

            if (string.IsNullOrEmpty(byValueAndProperty))
            {
                return false;
            }
    
            var stringValue = byValueAndProperty.Trim();

			if (string.Compare(stringValue, "true", true) == 0)
			{
				return true;
			}

			if (string.Compare(stringValue, "false", true) == 0)
			{
				return false;
			}

            Match mInfo;
            mInfo = trueValueRegex.Match(stringValue);
            if (mInfo.Success)
            {
                return true;
            }

            mInfo = valueRegex.Match(stringValue);
            if (mInfo.Success)
            {
                if(string.Compare(mInfo.Groups["value"].Value, "true", true) == 0)
                {
                    return true;
                }
            }

			return false;
		}

		public bool GetByProperty(string byValueAndProperty)
		{
            if (string.IsNullOrEmpty(byValueAndProperty))
            {
                return false;
            }
    
			var stringValue = byValueAndProperty.Trim();
            // If it just says "True"
			if (string.Compare(stringValue, "true", true) == 0)
			{
				return true;
			}

			if (string.Compare(stringValue, "false", true) == 0)
			{
				return false;
			}

            Match mInfo;
            mInfo = trueNameRegex.Match(stringValue);
            if (mInfo.Success)
            {
                return true;
            }

            mInfo = valueRegex.Match(stringValue);
            if (mInfo.Success)
            {
                if(string.Compare(mInfo.Groups["name"].Value, "true", true) == 0)
                {
                    return true;
                }
            }

			return false;
		}

        public static bool TryConvertToV1(string yaml, out ParameterMetadataV1? v1)
        {
            v1 = null;
            try
            {
                var result = new DeserializerBuilder().Build().Deserialize<ParameterMetadataV1>(yaml);
                v1 = result;
                return true;
            }
            catch
            {
                ; // do nothing we couldn't parse the yaml, and we'll return false.
            }

            return false;
        }

        public bool TryConvertMetadataToV2(out ParameterMetadataV2 v2)
        {
            var result = new ParameterMetadataV2();
			result.Type = Type;
			result.DefaultValue = DefaultValue;
			result.Globbing = AcceptWildcardCharacters;
			result.Aliases.AddRange(GetAliases());
			result.DontShow = false;
			foreach(var pSetName in GetParameterSetList())
			{
				var pSetV2 = new ParameterSetV2(pSetName, Position);
				pSetV2.ValueByPipeline = GetByValue(AcceptPipelineInput);
				pSetV2.ValueByPipelineByPropertyName = GetByProperty(AcceptPipelineInput);
				pSetV2.IsRequired = Required;
				result.ParameterSets.Add(pSetV2);
			}

            v2 = result;
            return true;
        }

        public string ToYamlString()
        {
			StringBuilder sb = new();
			sb.AppendLine("```yaml");
			sb.Append(new SerializerBuilder().Build().Serialize(this));
			sb.AppendLine("```");
			return sb.ToString();
        }
    }
}
