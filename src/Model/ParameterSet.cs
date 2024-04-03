// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.PowerShell.PlatyPS.Model
{
    /// <summary>
    /// Class to represent the properties of a parameter in PowerShell help.
    /// </summary>
    public class ParameterSet : IEquatable<ParameterSet>
    {
        public string Name { get; set;} = string.Empty;
        public string Position { get; set; } = string.Empty;
        public bool IsRequired { get; set; } = false;
        public bool ValueByPipeline { get; set; } = false;
        public bool ValueByPipelineByPropertyName { get; set; } = false;
        public bool ValueFromRemainingArguments { get; set; } = false;

        public ParameterSet()
        {

        }

        public ParameterSet(string name)
        {
            Name = name;
        }

        public bool Equals(ParameterSet other)
        {
            if (other is null)
            {
                return false;
            }

            return (
                string.Compare(Name, other.Name, StringComparison.CurrentCulture) == 0 &&
                string.Compare(Position, other.Position, StringComparison.CurrentCulture) == 0 &&
                IsRequired == other.IsRequired &&
                ValueByPipeline == other.ValueByPipeline &&
                ValueByPipelineByPropertyName == other.ValueByPipelineByPropertyName &&
                ValueFromRemainingArguments == other.ValueFromRemainingArguments);
        }

        public override bool Equals(object other)
        {
            if (other is null)
            {
                return false;
            }

            if (other is ParameterSet parameterSet2)
            {
                return Equals(parameterSet2);
            }

            return false;
        }

        public override int GetHashCode()
        {
            return (Name, Position, IsRequired, ValueByPipeline, ValueByPipelineByPropertyName, ValueFromRemainingArguments).GetHashCode();
        }

        public static bool operator ==(ParameterSet parameterSet1, ParameterSet parameterSet2)
        {
            if (parameterSet1 is not null && parameterSet2 is not null)
            {
                return parameterSet1.Equals(parameterSet2);
            }

            return false;
        }

        public static bool operator !=(ParameterSet parameterSet1, ParameterSet parameterSet2)
        {
            if (parameterSet1 is not null && parameterSet2 is not null)
            {
                return ! parameterSet1.Equals(parameterSet2);
            }

            return false;
        }
    }

    /*
    public class PipelineInputInfo : IEquatable<PipelineInputInfo>
    {
        public bool ByPropertyName { get; set; }
        public bool ByValue { get; set; }

        public PipelineInputInfo(bool byPropertyName, bool byValue)
        {
            ByPropertyName = byPropertyName;
            ByValue = byValue;
        }

        public PipelineInputInfo(bool byBoth)
        {
            ByPropertyName = byBoth;
            ByValue = byBoth;
        }

        override public string ToString()
        {
            return string.Format("ByName ({0}), ByValue ({1})", ByPropertyName, ByValue);
        }

        public override int GetHashCode()
        {
            return ByPropertyName.GetHashCode() ^ ByValue.GetHashCode();
        }

        public bool Equals(PipelineInputInfo other)
        {
            if (other is null)
            {
                return false;
            }

            return (ByPropertyName == other.ByPropertyName && ByValue == other.ByValue);
        }

        public override bool Equals(object other)
        {
            if (other is null)
            {
                return false;
            }

            if (other is PipelineInputInfo info2)
            {
                return Equals(info2);
            }

            return false;
        }

        public static bool operator == (PipelineInputInfo info1, PipelineInputInfo info2)
        {
            if (info1 is not null && info2 is not null)
            {
                return info1.Equals(info2);
            }

            return false;
        }

        public static bool operator !=(PipelineInputInfo info1, PipelineInputInfo info2)
        {
            if (info1 is not null && info2 is not null)
            {
                return ! info1.Equals(info2);
            }

            return false;
        }
    }
    */
}
