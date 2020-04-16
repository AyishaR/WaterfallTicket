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
using System.IO;
using Microsoft.Bot.Schema;
using Newtonsoft.Json;


namespace Microsoft.BotBuilderSamples.Dialogs
{
    public class QnA : Parent
    {
        LogContent content;
        //static JObject obj;
        IConfiguration configuration;
        HttpClient httpClient;
        public QnA(UserState userState, IHttpClientFactory httpClientFactory, IConfiguration config)
            : base(nameof(QnA), httpClientFactory,config)
        {
            configuration = config;
            httpClient = httpClientFactory.CreateClient();
            content = new LogContent();
            AddDialog(new TextPrompt(nameof(TextPrompt)));
            AddDialog(new ChoicePrompt(nameof(ChoicePrompt)));
            
            AddDialog(new WaterfallDialog(nameof(QnA), new WaterfallStep[]
            {
                
               Answerasync,
               Responseasync
            }));

            InitialDialogId = nameof(QnA);
        }
        
        private async Task<DialogTurnResult> Answerasync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            
            IList<object> x = (IList<object>)stepContext.Options;
            RecognizerResult rr = (RecognizerResult)x[0];
            QueryResult[] response = (QueryResult[])x[1];
            //string adaptiveCardJson = File.ReadAllText(@"Dialogs\Cards\feedback.json");
            //obj = (JObject)JsonConvert.DeserializeObject(adaptiveCardJson);

            bool reply = response.Length == 0 ? false : true;
            {
                //obj["actions"][0]["data"]["question"] = obj["actions"][1]["data"]["question"] = stepContext.Context.Activity.Text;
                //obj["body"][0]["text"] = obj["actions"][0]["data"]["qna answer"] = obj["actions"][1]["data"]["qna answer"] = reply?response[0].Answer:"";
                //obj["actions"][0]["data"]["luis intent"] = obj["actions"][1]["data"]["luis intent"] = rr.GetTopScoringIntent().intent;
                //obj["actions"][0]["data"]["luis score"] = obj["actions"][1]["data"]["luis score"] = rr.GetTopScoringIntent().score;
                //obj["actions"][0]["data"]["qna score"] = obj["actions"][1]["data"]["qna score"] = reply?response[0].Score:0;
                //obj["actions"][0]["data"]["user"] = obj["actions"][1]["data"]["user"] = stepContext.Context.Activity.From.Name;
                //obj["actions"][0]["data"]["date time"] = obj["actions"][1]["data"]["date time"] = DateTime.Now;
                //obj["actions"][0]["data"]["channel"] = obj["actions"][1]["data"]["channel"] = stepContext.Context.Activity.ChannelId;
            }

            content.Question = stepContext.Context.Activity.Text;
            content.LuisIntent = rr.GetTopScoringIntent().intent;
            content.LuisScore = rr.GetTopScoringIntent().score;
            content.QnAanswer = reply ? response[0].Answer : null;
            content.QnAscore = reply ? response[0].Score : 0;
            content.User = stepContext.Context.Activity.From.Name;
            content.Channel = stepContext.Context.Activity.ChannelId;
            content.Date_Time = DateTime.Now.ToString();
            // ENTER INTO DATABASE WITH LIKE = NULL and isAnswered = reply           

            if (response != null && response.Length > 0 && response[0].Score >= 0.75)
            {
                // isAnswered set as True
                
                return await stepContext.PromptAsync(nameof(TextPrompt),
                new PromptOptions
                {
                    Prompt = new Activity
                    {
                        Type = ActivityTypes.Message,
                        Text = response[0].Answer,
                        SuggestedActions = new SuggestedActions()
                        {
                            Actions = new List<CardAction>()
                            {
                                new CardAction() { Title = "Like", Type = ActionTypes.ImBack, Value = "Like" },
                                new CardAction() { Title = "Dislike", Type = ActionTypes.ImBack, Value = "Dislike" },
                                new CardAction() { Title = "Raise a ticket", Type = ActionTypes.ImBack, Value = "Raise a ticket" },
                                new CardAction() { Title = "Colaboratory", Type = ActionTypes.ImBack, Value = "Colaboratory" }
                            },
                        },
                    }
                }, cancellationToken);

            }
            else
            {
                // isAnswered is false
                await stepContext.Context.SendActivityAsync(MessageFactory.Text("Sorry, we will get you an answer for '" +
                    stepContext.Context.Activity.Text + "' soon."), cancellationToken);
                return await stepContext.EndDialogAsync();
            }        
        }

        private async Task<DialogTurnResult> Responseasync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            string r = stepContext.Result.ToString();
            switch(r)
            {
                case "Like":
                    {
                        // like column set as true
                        await stepContext.Context.SendActivityAsync(MessageFactory.Text("Liked"), cancellationToken);
                        break;
                    }
                case "Dislike":
                    {
                        // like column set as false
                        await stepContext.Context.SendActivityAsync(MessageFactory.Text("Disliked"), cancellationToken);
                        break;
                    }
                default:
                    {
                        // like column set as null
                        //await stepContext.Context.SendActivityAsync(MessageFactory.Text("No view"), cancellationToken);
                        return await stepContext.EndDialogAsync(r, cancellationToken);
                    }
            }
            //await stepContext.Context.SendActivityAsync(MessageFactory.Text(content.QnAscore.ToString()), cancellationToken);
            return await stepContext.EndDialogAsync();
        }
    }
}
