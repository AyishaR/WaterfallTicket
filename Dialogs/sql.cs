using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.BotBuilderSamples.Dialogs
{
    public class sql : Parent
    {
        public sql(UserState userState, IHttpClientFactory httpClientFactory, IConfiguration config)
            : base(nameof(sql), httpClientFactory, config)
        {
            
            
            AddDialog(new TextPrompt(nameof(TextPrompt)));
            AddDialog(new ChoicePrompt(nameof(ChoicePrompt)));
            AddDialog(new service_now_ticket(userState, httpClientFactory, config));
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

            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), new WaterfallStep[]
            {
                queryasync,
                replyasync
            }));


            InitialDialogId = nameof(WaterfallDialog);
        }

        private async Task<DialogTurnResult> queryasync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = MessageFactory.Text($"Query?") }, cancellationToken);
        }

        private async Task<DialogTurnResult> replyasync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            string q = stepContext.Result.ToString(), output="";
            string connetionString = "Data Source=localhost;Initial Catalog=ipprojects02;User ID=root;Password=root";
            SqlDataReader datareader;
            SqlConnection cnx = new SqlConnection(connetionString);
            cnx.Open();
            SqlCommand cmd = new SqlCommand(q, cnx);
            datareader = cmd.ExecuteReader();
            
            while(datareader.Read())
            {
                for(int i=0; i<datareader.FieldCount;i++)
                    output += (datareader.GetValue(i) + "\t");
                output += "\n";
            }

            await stepContext.Context.SendActivityAsync(output);
            return await stepContext.BeginDialogAsync(nameof(sql), cancellationToken);
        }
    }
}
