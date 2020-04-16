using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Configuration;
using Microsoft.BotBuilderSamples.Dialogs;
using Microsoft.Bot.Builder.AI.Luis;
using Microsoft.Bot.Builder.AI.QnA;
using System.Net.Http;
using Microsoft.Bot.Builder.Dialogs.Choices;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.IO;
using System;

namespace Microsoft.BotBuilderSamples.Dialogs
{
    public class Parent : ComponentDialog
    {
        readonly HttpClient httpClient;
        readonly IConfiguration configuration;
        public Parent(string id, IHttpClientFactory httpClientFactory, IConfiguration config)
            : base(id)
        {
            configuration = config;
            httpClient = httpClientFactory.CreateClient();
        }

        protected override async Task<DialogTurnResult> OnBeginDialogAsync(DialogContext innerDc, object options, CancellationToken cancellationToken = default(CancellationToken))
        {
            var result = await InterruptAsync(innerDc, cancellationToken);
            if (result != null)
            {
                return result;
            }

            return await base.OnBeginDialogAsync(innerDc, options, cancellationToken);
        }

        protected override async Task<DialogTurnResult> OnContinueDialogAsync(DialogContext innerDc, CancellationToken cancellationToken = default(CancellationToken))
        {
            var result = await InterruptAsync(innerDc, cancellationToken);
            if (result != null)
            {
                return result;
            }

            return await base.OnContinueDialogAsync(innerDc, cancellationToken);
        }

        private async Task<DialogTurnResult> InterruptAsync(DialogContext innerDc, CancellationToken cancellationToken = default(CancellationToken))

        {
            if (innerDc.Context.Activity.Value != null)
                await innerDc.Context.SendActivityAsync(innerDc.Context.Activity.Value.ToString());
            if(innerDc.Context.Activity.Text!=null)
            {
                var tc = innerDc.Context;
                var luisApplication = new LuisApplication(
                    configuration["LuisAppId"],
                    configuration["LuisAPIKey"],
                    configuration["LuisAPIHostName"]
                );

                var recognizer = new LuisRecognizer(luisApplication);
                var recognizerResult = await recognizer.RecognizeAsync(tc, cancellationToken);
                //await innerDc.Context.SendActivityAsync(MessageFactory.Text(recognizerResult.Intents.ToString()), cancellationToken);
                //    //await Task.WaitAll(recognizerResult, response);
                var (intent, score) = recognizerResult.GetTopScoringIntent();

                if (score >= 0.8 && !intent.Equals("None"))
                {
                    switch (intent)
                    {
                        case "cancel":
                            await innerDc.Context.SendActivityAsync($"Cancelling", cancellationToken: cancellationToken);
                            return await innerDc.EndDialogAsync();
                        case "logout":
                            // The bot adapter encapsulates the authentication processes.
                            //var botAdapter = (BotFrameworkAdapter)innerDc.Context.Adapter;
                            //await botAdapter.SignOutUserAsync(innerDc.Context, "login_all", null, cancellationToken);
                            await innerDc.Context.SendActivityAsync(MessageFactory.Text("You have been signed out."), cancellationToken);
                            return await innerDc.CancelAllDialogsAsync(cancellationToken);
                    }
                }
                
            }
            return null;
        }

        
    }
}