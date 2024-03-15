// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Text;
using System.Collections.Generic;

namespace platyPS.Parser
{
    public class CommandSyntax {
        public string CommandName { get; }
        public string ParseSource { get; set; } = string.Empty;
        public bool HasCmdletBinding { get; set; }
        public List<Parameter> Parameters { get; }

        internal CommandSyntax(string cmdName)
        {
            CommandName = cmdName;
            Parameters = new();
        } 

        // this takes a string from get-command -syntax
        public static CommandSyntax ParseGetCommandSyntax(string commandSyntax)
        {
            if (commandSyntax is null)
            {
                throw new ArgumentException("String must have length greater than 0.");
            }

            int position = 0;
            string[] elements = commandSyntax.Split(new char[]{' ','\n','\r'}, StringSplitOptions.RemoveEmptyEntries);

            if (elements.Length < 2 && elements[0] == string.Empty)
            {
                throw new ArgumentException("String must have length greater than 0.");
            }

            var syntax = new CommandSyntax(elements[0]);
            syntax.ParseSource = commandSyntax;

            for(int i = 1; i < elements.Length; i++)
            {
                string parameter = elements[i];
                if (parameter.IndexOf("CommonParameters") != -1)
                {
                    syntax.HasCmdletBinding = true;
                    continue;
                }
                else if (i+1 < elements.Length && elements[i+1].StartsWith("<"))
                {
					i++;
					var pType = elements[i];

					if ( parameter.StartsWith("[[") && parameter.EndsWith("]")) { // [[-parm] <type>] optional, positional parameter
						syntax.Parameters.Add(
							new Parameter(
								parameter.TrimStart('[').TrimEnd(']').TrimStart('-'),
								pType.TrimEnd(']').TrimStart('<').TrimEnd('>'),
								position.ToString(),
								false
							)
						);
                        position++;
					}
					else if (parameter.StartsWith("[") && parameter.EndsWith("]")) { // [-parm] <type>
						syntax.Parameters.Add(
							new Parameter(
								parameter.TrimStart('[').TrimEnd(']').TrimStart('-'),
								pType.TrimEnd(']').TrimStart('<').TrimEnd('>'),
								position.ToString(),
								true
							)
						);
                        position++;
					}
					else if (parameter.StartsWith("[") && pType.EndsWith("]")) { // optional parameter and argument
						parameter = parameter.TrimStart('[').TrimStart('-');
						pType = pType.TrimEnd(']').TrimStart('<').TrimEnd('>');
						syntax.Parameters.Add(new Parameter(parameter, pType, "named", false));
					}
					else if (pType.StartsWith("<") && pType.EndsWith(">")) {
						pType = pType.TrimStart('<').TrimEnd('>');
						syntax.Parameters.Add(new Parameter(parameter.TrimStart('-'), pType, "named", true));
					}
					else {
						throw new ArgumentException($"{pType} is malformed.");
					}
				}
				else { // [-parm] or -parm without argument
					// switch
					if (parameter.StartsWith("-")) { // '-param' is a mandatory switch parameter
						syntax.Parameters.Add(new Parameter(parameter.TrimStart('-'), "SwitchParameter", "named", true));
					}
					else if (parameter.StartsWith("[") && parameter.EndsWith("]")) {
						// optional
						parameter = parameter.TrimStart('[').TrimEnd(']').TrimStart('-');
						syntax.Parameters.Add(new Parameter(parameter, "SwitchParameter", "named", false));
					}
					else {
						throw new ArgumentException($"{parameter} malformed");
					}
				}
			}

            return syntax;
		}
    }

    public class Parameter
    {
        public string ParameterName { get; set; }
        public string ParameterType { get; set; }
        public bool IsMandatory { get; set; }
        public bool IsPositional { get; set; }
        public string Position { get; set; } = string.Empty;

        public Parameter(string parameterName, string parameterType)
        {
            ParameterName = parameterName;
            ParameterType = parameterType;
        }

        public Parameter(string parameterName, string parameterType, string position, bool mandatory)
        {
            ParameterName = parameterName;
            ParameterType = parameterType;
            IsMandatory = mandatory;
            Position = position;
            IsPositional = int.TryParse(position, out int zz);
        }
    }
}
