﻿// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.CommandLine.Parsing;
using System.Threading.Tasks;
using Microsoft.DotNet.Interactive.Parsing;

namespace Microsoft.DotNet.Interactive.Commands
{
    internal class DirectiveCommand : KernelCommandBase
    {
        public DirectiveCommand(
            ParseResult parseResult, 
            DirectiveNode directiveNode = null)
        {
            ParseResult = parseResult;
            DirectiveNode = directiveNode;
        }

        public ParseResult ParseResult { get; }

        public DirectiveNode DirectiveNode { get; }

        public override async Task InvokeAsync(KernelInvocationContext context)
        {
            await ParseResult.InvokeAsync();
        }

        public override string ToString()
        {
            return $"Directive: {ParseResult.CommandResult.Command.Name}";
        }
    }
}