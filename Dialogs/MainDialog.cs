using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Choices;
using Microsoft.Bot.Builder.AI.Luis;
using Microsoft.Bot.Builder.AI.QnA;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using System;
using Microsoft.Extensions.Configuration;
using System.Net.Http;
using Microsoft.BotBuilderSamples;
using Microsoft.Bot.Schema;

namespace Microsoft.BotBuilderSamples.Dialogs
{
    public class MainDialog : Parent
    {
        protected readonly IConfiguration configuration;
        
        readonly IHttpClientFactory _httpClientFactory;
        public MainDialog(UserState userState, IHttpClientFactory httpClientFactory, IConfiguration config)
            : base(nameof(MainDialog), httpClientFactory,config)
        {
            configuration = config;
            
            _httpClientFactory = httpClientFactory;
            AddDialog(new TextPrompt(nameof(TextPrompt)));
            AddDialog(new ChoicePrompt(nameof(ChoicePrompt)));
            AddDialog(new service_now_ticket(userState, httpClientFactory,config));
            AddDialog(new QnA(userState, httpClientFactory, config));
            
            AddDialog(new OAuthPrompt(
                nameof(OAuthPrompt),
                new OAuthPromptSettings
                {
                    ConnectionName = "login_all",
                    Text = "Please sign in to continue...",
                    Title = "SIGN IN!",
                    Timeout = 60000, // User has 5 minutes to login (1000 * 60 * 5)
                }));

            AddDialog(new WaterfallDialog(nameof(MainDialog), new WaterfallStep[]
            {
                //SignInasync,
                WhatToDoasync,
                WhatToDoPromptasync,
                RestartStepasync
            }));


            InitialDialogId = nameof(MainDialog);
        }

        //private async Task<DialogTurnResult> SignInasync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        //{
        //    return await stepContext.BeginDialogAsync(nameof(OAuthPrompt), null, cancellationToken);           
        //}

        private async Task<DialogTurnResult> WhatToDoPromptasync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            if (stepContext.Result == null)
                return await stepContext.PromptAsync(nameof(TextPrompt),
                    new PromptOptions
                    {
                        Prompt = new Activity
                        {
                            Type = ActivityTypes.Message,
                            Text = null,
                            SuggestedActions = new SuggestedActions()
                            {
                                Actions = new List<CardAction>()
                                {
                                    new CardAction() { Title = "Raise a ticket", Type = ActionTypes.ImBack, Value = "Raise a ticket" },
                                    new CardAction() { Title = "Colaboratory", Type = ActionTypes.ImBack, Value = "Colaboratory" },
                                    new CardAction() { Title = "pimple", Type = ActionTypes.ImBack, Value = "pimple" }
                                },
                            },
                        }
                    }, cancellationToken);
            else
                return await stepContext.NextAsync(cancellationToken);
        }


        private async Task<DialogTurnResult> WhatToDoasync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var tc = stepContext.Context;
            
            var luisApplication = new LuisApplication(
                configuration["LuisAppId"],
                configuration["LuisAPIKey"],
                configuration["LuisAPIHostName"]
            );

            HttpClient httpClient = _httpClientFactory.CreateClient();

            var qnaMaker = new QnAMaker(new QnAMakerEndpoint
            {
                KnowledgeBaseId = configuration["KnowledgeBaseId"],
                EndpointKey = configuration["EndpointKey"],
                Host = configuration["Host"]
            },
            null,
            httpClient);
            
            var recognizer = new LuisRecognizer(luisApplication);
            var recognizerResult = await recognizer.RecognizeAsync(tc, cancellationToken);
                        
            var luis_score = recognizerResult.GetTopScoringIntent().intent.Equals("None")?0:recognizerResult.GetTopScoringIntent().score;
            if (luis_score >= 0.8)
            {
                return await stepContext.BeginDialogAsync(recognizerResult.GetTopScoringIntent().intent, recognizerResult.Entities, cancellationToken);
            }
            else
            {
                var response = await qnaMaker.GetAnswersAsync(tc);
                return await stepContext.BeginDialogAsync(nameof(QnA), new List<Object> { recognizerResult, response }, cancellationToken);
            }
        }

        private async Task<DialogTurnResult> RestartStepasync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {            
            return await stepContext.BeginDialogAsync(nameof(MainDialog), cancellationToken);
        }

    }
}
