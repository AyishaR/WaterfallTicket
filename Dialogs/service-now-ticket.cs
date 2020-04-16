// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Choices;
using Microsoft.BotBuilderSamples;
using Microsoft.Bot.Schema;
using Newtonsoft.Json.Linq;
using System.Net;
using System.Text;
using System.IO;
using System.Net.Http;
using Microsoft.Extensions.Configuration;

namespace Microsoft.BotBuilderSamples.Dialogs
{
    public class service_now_ticket : Parent
    {
        private readonly IStatePropertyAccessor<TicketDetails> _ticketAccessor;

        public service_now_ticket(UserState userState, IHttpClientFactory httpClientFactory, IConfiguration c)
            : base(INTENTS.SERVICE_NOW_TICKET, httpClientFactory,c)
        {
            _ticketAccessor = userState.CreateProperty<TicketDetails>("TicketDetails");

            AddDialog(new TextPrompt(nameof(TextPrompt)));
            AddDialog(new ChoicePrompt(nameof(ChoicePrompt)));
            AddDialog(new ConfirmPrompt(nameof(ConfirmPrompt)));
            AddDialog(new WaterfallDialog(INTENTS.SERVICE_NOW_TICKET, new WaterfallStep[]
            {
                
                GetPriorityasync,
                GetCategoryasync,
                GetDescasync,
                ConfirmStepAsync,
                Summaryasync
            }
            ));

            InitialDialogId = INTENTS.SERVICE_NOW_TICKET;
        }

       

        private static async Task<DialogTurnResult> GetPriorityasync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var json = JObject.Parse(stepContext.Options.ToString());
            if(json["priority"] == null)
            {
                return await stepContext.PromptAsync(nameof(ChoicePrompt),
                new PromptOptions
                {
                    Prompt = MessageFactory.Text($"Please enter the level of priority...(1 is high, 3 is low) "),
                    Choices = ChoiceFactory.ToChoices(new List<string> { "1", "2", "3" }),
                }, cancellationToken);
               
            }
           else
            {
                return await stepContext.NextAsync(json["priority"].First.First.ToString());
            }
        }

        private static async Task<DialogTurnResult> GetCategoryasync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var json = JObject.Parse(stepContext.Options.ToString());
            stepContext.Values["priority"] = stepContext.Result.GetType() == "".GetType() ? stepContext.Result : ((FoundChoice)stepContext.Result).Value ;
            if (json["department"] == null)
            {
                return await stepContext.PromptAsync(nameof(ChoicePrompt),
                new PromptOptions
                {
                    Prompt = MessageFactory.Text($"Please enter the department. "),
                    Choices = ChoiceFactory.ToChoices(new List<string> { "IT", "HR", "Finance" }),
                }, cancellationToken);
            }
            else
            {
                return await stepContext.NextAsync(json["department"].First.First.ToString());
            }
        }

        private async Task<DialogTurnResult> GetDescasync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var json = JObject.Parse(stepContext.Options.ToString());
            stepContext.Values["category"] = stepContext.Result.GetType() == "".GetType() ? stepContext.Result : ((FoundChoice)stepContext.Result).Value;
            if (json["description"] == null)
            {
                return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = MessageFactory.Text($"Please enter your description.") }, cancellationToken);
            }
            else
            { 
                return await stepContext.NextAsync(json["description"].First.ToString());
            }
        }

        private async Task<DialogTurnResult> ConfirmStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            
            stepContext.Values["description"] = stepContext.Result;

            var msg = $"Here are the ticket details: \n Priority: {stepContext.Values["priority"]} \n Category: {stepContext.Values["category"]} " +
                $"\n Description: {stepContext.Values["description"]}.\n\nCan I confirm this?";
            return await stepContext.PromptAsync(nameof(ConfirmPrompt), new PromptOptions { Prompt = MessageFactory.Text(msg) }, cancellationToken);
        }

        private async Task<DialogTurnResult> Summaryasync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            if ((bool)stepContext.Result)
            {
                //{
                //    var ticket_details = await _ticketAccessor.GetAsync(stepContext.Context, () => new TicketDetails(), cancellationToken);

                //    ticket_details.priority = (string)stepContext.Values["priority"];
                //    ticket_details.category = (string)stepContext.Values["category"];
                //    ticket_details.description = (string)stepContext.Values["description"];

                //    var body = $"<![CDATA[<html>" +
                //        "<body>" +
                //            "<h3>New SNC ticket raised</h3>" +
                //            $"<b>Category:<\b> P{ticket_details.priority}<br>" +
                //            $"<b>Category:<\b> {ticket_details.category}<br>" +
                //            $"{ticket_details.description}" +
                //        "</body>" +
                //    "</html>";
                //    var key = "thisismytoken";
                //    var uri = "https://spapps.flex.com/NotificationWS/notificationWS.asmx";
                //    var httpbody = "<?xml version=\"1.0\" encoding=\"utf-8\"?>" +
                //         "<soap:Envelope xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instanc\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\" xmlns:soap=\"http://schemas.xmlsoap.org/soap/envelope/\">" +
                //         "<soap:Body>" +
                //         "<sendNotificationService xmlns=\"http://tempuri.org/\">" +
                //         $"<SendTo>ryku99@gmail.com</SendTo>" +
                //         $"<Subject>New SNC ticket</Subject>" +
                //         $"<Body>{body}</Body>" +
                //         $"<key>{key}</key>" +
                //         "</sendNotificationService>" +
                //         "</soap:Body>" +
                //         "</soap:Envelope>";
                //    WebRequest request = WebRequest.Create(uri);
                //    request.Method = "POST";
                //    request.ContentType = "text/xml; charset=utf-8";
                //    byte[] bytearray = Encoding.UTF8.GetBytes(httpbody);
                //    Stream datastream = request.GetRequestStream();
                //    datastream.Write(bytearray, 0, bytearray.Length);
                //    datastream.Close();
                //    WebResponse response = request.GetResponse();
                //    string msg = "";
                //    if (((HttpWebResponse)response).StatusDescription == "OK")
                //    {
                //        msg = "A mail with the ticket summary has been sent to you.";
                //    }
                //    else
                //    {
                //        msg = "Confirmation mail not sent.";
                //    }
                //}
                //var msg = $"The ticket details: \n EmailID: abc@flex.com \n A ticket of priority {ticket_details.priority} is raised in {ticket_details.category} " +
                //$"as follows:\n {ticket_details.description}.";
                string msg = "A mail will be sent.";
                await stepContext.Context.SendActivityAsync(MessageFactory.Text(msg), cancellationToken);
            }
            else
            {
                await stepContext.Context.SendActivityAsync(MessageFactory.Text("Thanks. Your ticket will not be raised."), cancellationToken);
            }

            return await stepContext.EndDialogAsync();
        }
    }
}