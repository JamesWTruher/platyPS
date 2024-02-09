// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Linq;

namespace Microsoft.PowerShell.PlatyPS.Model
{
    /// <summary>
    /// Class to represent the properties of a link in PowerShell help.
    /// </summary>
    internal class Links : IEquatable<Links>
    {
        public Links(string uri, string linkText)
        {
            Uri = uri;
            LinkText = linkText;
        }

        internal string Uri { get; set;}
        internal string LinkText { get; set;}

        internal string ToRelatedLinksString(string fmt)
        {
            // return $"[{LinkText}]({Uri})";
            return string.Format(fmt, LinkText, Uri);
        }

        public bool Equals(Links other)
        {
            if (other is null)
            {
                return false;
            }

            return (string.Compare(Uri, other.Uri) == 0 && string.Compare(LinkText, other.LinkText) == 0);
        }

        public override bool Equals(object other)
        {
            if (other is null)
            {
                return false;
            }

            if (other is Links link2)
            {
                return Equals(link2);
            }

            return false;
        }

        public override int GetHashCode()
        {
            return (Uri, LinkText).GetHashCode();
        }   

        public static bool operator == (Links link1, Links link2)
        {
            if (link1 is not null && link2 is not null)
            {
                return link1.Equals(link2);
            }

            return false;
        }

        public static bool operator != (Links link1, Links link2)
        {
            if (link1 is not null && link2 is not null)
            {
                return ! link1.Equals(link2);
            }

            return false;
        }
    }
}
