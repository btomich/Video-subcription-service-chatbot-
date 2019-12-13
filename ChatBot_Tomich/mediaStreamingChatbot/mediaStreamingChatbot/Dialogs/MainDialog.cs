using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using mediaStreamingChatbot.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace mediaStreamingChatbot.Dialogs
{

    public class MainDialog : ComponentDialog
    {
        #region Variables
        private readonly BotStateService _botStateService;
        #endregion  


        public MainDialog(BotStateService botStateService) : base(nameof(MainDialog))
        {
            _botStateService = botStateService ?? throw new System.ArgumentNullException(nameof(botStateService));
            

            InitializeWaterfallDialog();
        }

        private void InitializeWaterfallDialog()
        {
            // Create Waterfall Steps
            var waterfallSteps = new WaterfallStep[]
            {
                InitialStepAsync,
                FinalStepAsync
            };

            // Add Named Dialogs
            AddDialog(new GreetingDialog($"{nameof(MainDialog)}.greeting", _botStateService));
            AddDialog(new TroubleshootDialog($"{nameof(MainDialog)}.bugReport", _botStateService));
            AddDialog(new paymentDialog($"{nameof(MainDialog)}.paymentDialog", _botStateService));
            AddDialog(new InquiryDialog($"{nameof(MainDialog)}.inquiry", _botStateService));
            AddDialog(new WaterfallDialog($"{nameof(MainDialog)}.mainFlow", waterfallSteps));

            // Set the starting Dialog
            InitialDialogId = $"{nameof(MainDialog)}.mainFlow";
        }

        private async Task<DialogTurnResult> InitialStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
           
            if (Regex.Match(stepContext.Context.Activity.Text.ToLower(), "hello").Success)
            {
                return await stepContext.BeginDialogAsync($"{nameof(MainDialog)}.greeting", null, cancellationToken);
            }
            else if (Regex.Match(stepContext.Context.Activity.Text.ToLower(), "troubleshoot").Success)
            {
                return await stepContext.BeginDialogAsync($"{nameof(MainDialog)}.bugReport", null, cancellationToken);
            }
            else if (Regex.Match(stepContext.Context.Activity.Text.ToLower(), "payment").Success)
            {
                return await stepContext.BeginDialogAsync($"{nameof(MainDialog)}.paymentDialog", null, cancellationToken);
            }
            else if (Regex.Match(stepContext.Context.Activity.Text.ToLower(), "inquiry").Success)
            {
                return await stepContext.BeginDialogAsync($"{nameof(MainDialog)}.inquiry", null, cancellationToken);
            }
            else
            {
                return await stepContext.NextAsync(null, cancellationToken);
            }
        }

        private async Task<DialogTurnResult> FinalStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            return await stepContext.EndDialogAsync(null, cancellationToken);
        }
    }
}
