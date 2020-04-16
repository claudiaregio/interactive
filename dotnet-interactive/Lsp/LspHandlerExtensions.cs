﻿// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Microsoft.DotNet.Interactive.Commands;
using Microsoft.DotNet.Interactive.Events;
using Newtonsoft.Json.Linq;

namespace Microsoft.DotNet.Interactive.App.Lsp
{
    internal static class LspHandlerExtensions
    {
        private static readonly HashSet<Type> DefaultAcceptableEventTypes;

        static LspHandlerExtensions()
        {
            // these event types are acceptable for all LSP requests
            DefaultAcceptableEventTypes = new HashSet<Type>()
            {
                typeof(CommandHandled),
                typeof(CommandFailed),
                typeof(LanguageServiceNoResultProduced),
            };
        }

        public static async Task<JObject> HandleLspMethod(this KernelBase kernel, string methodName, JObject request)
        {
            JObject response = methodName switch
            {
                "textDocument/hover" => await kernel.GetLanguageServiceResultOrDefault<HoverParams, RequestHoverTextCommand, LanguageServiceHoverResponseProduced, HoverResponse>(request, hp => hp.ToCommand(), HoverResponse.FromLanguageServiceEvent),
                _ => null,
            };

            return response;
        }

        /// <summary>
        /// Completes the path from <see cref="JObject" /> -&gt; <typeparamref name="TRequest"/> -&gt; <typeparamref name="TRequestCommand"/> -&gt;
        /// <see cref="KernelBase.SendAsync(IKernelCommand, System.Threading.CancellationToken)"/> -&gt; <typeparamref name="TResultEvent"/> -&gt;
        /// <typeparamref name="TResult"/> -&gt; <see cref="JObject"/>.
        /// </summary>
        /// <typeparam name="TRequest">Type type that <see cref="JObject"/> should be deserialized into.</typeparam>
        /// <typeparam name="TRequestCommand">The command type that will be submitted to <see cref="KernelBase.SendAsync(IKernelCommand, System.Threading.CancellationToken)"/>.</typeparam>
        /// <typeparam name="TResultEvent">The expected resultant event produced by the kernel.  May not be returned if the specified kernel doesn't support the given LSP method.</typeparam>
        /// <typeparam name="TResult">The type that will be serialized back into <see cref="JObject"/></typeparam>
        /// <param name="kernel">The kernel that the <typeparamref name="TRequestCommand"/> is submitted to.</param>
        /// <param name="request">The JSON-parsed request.</param>
        /// <param name="commandCtor">Function to construct an appropriate <typeparamref name="TRequestCommand"/> from the <typeparamref name="TRequest"/>.</param>
        /// <param name="resultDtor">Function to deconstruct the <typeparamref name="TResultEvent"/> into the final <typeparamref name="TResult"/>.</param>
        private static async Task<JObject> GetLanguageServiceResultOrDefault<TRequest, TRequestCommand, TResultEvent, TResult>(
            this KernelBase kernel,
            JObject request,
            Func<TRequest, TRequestCommand> commandCtor,
            Func<TResultEvent, TResult> resultDtor)
            where TRequestCommand : IKernelCommand
        {
            // JObject -> TRequest
            if (!request.TryToObject<TRequest>(out var requestParams))
            {
                // couldn't deserialize, shouldn't be fatal
                return null;
            }

            // TRequest -> TRequestCommand
            var requestCommand = commandCtor(requestParams);

            // kernel handling
            var kernelCommandResult = await kernel.SendAsync(requestCommand);
            var resultEvent = await kernelCommandResult.KernelEvents
                .FirstAsync(kernelEvent => DefaultAcceptableEventTypes.Contains(kernelEvent.GetType()) || kernelEvent is TResultEvent);

            // TResultEvent -> TResult
            TResult resultObject = default;
            if (resultEvent is TResultEvent typedResultEvent)
            {
                resultObject = resultDtor(typedResultEvent);
            }

            // TResult -> JObject
            JObject responseJson = null;
            if (resultObject != null)
            {
                responseJson = JObject.FromObject(resultObject, LspSerializer.JsonSerializer);
            }

            return responseJson;
        }
    }
}