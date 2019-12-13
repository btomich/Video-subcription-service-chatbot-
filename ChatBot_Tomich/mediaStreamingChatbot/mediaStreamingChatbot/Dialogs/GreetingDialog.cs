using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using mediaStreamingChatbot.Dialogs;
using mediaStreamingChatbot.Classes;
using mediaStreamingChatbot.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;


namespace mediaStreamingChatbot.Dialogs
{
    public class GreetingDialog : ComponentDialog
    {
        #region Variables
        private readonly BotStateService _botStateService;
        #endregion  
        public GreetingDialog(string dialogId, BotStateService botStateService) : base(dialogId)
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
                FinalStepAsync,
            };

            // Add Named Dialogs
            AddDialog(new WaterfallDialog($"{nameof(GreetingDialog)}.mainFlow", waterfallSteps));
            AddDialog(new TextPrompt($"{nameof(GreetingDialog)}.name"));
            AddDialog(new TextPrompt($"{nameof(GreetingDialog)}.options", optionsValidatorAsync));

            // Set the starting Dialog
            InitialDialogId = $"{nameof(GreetingDialog)}.mainFlow";
        }

        private async Task<DialogTurnResult> InitialStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            User user = await _botStateService.UserProfileAccessor.GetAsync(stepContext.Context, () => new User());

            if (string.IsNullOrEmpty(user.first))
            {
                return await stepContext.PromptAsync($"{nameof(GreetingDialog)}.name",
                    new PromptOptions
                    {
                        Prompt = MessageFactory.Text("What is your name?")
                    }, cancellationToken);
            }
            else
            {
                return await stepContext.NextAsync(null, cancellationToken);
            }
        }

        private async Task<DialogTurnResult> FinalStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            User user = await _botStateService.UserProfileAccessor.GetAsync(stepContext.Context, () => new User());
            if (string.IsNullOrEmpty(user.first))
            {
                // Set the name
                user.first = (string)stepContext.Result;

                // Save any state changes that might have occured during the turn.
                await _botStateService.UserProfileAccessor.SetAsync(stepContext.Context, user);
            }

            await stepContext.Context.SendActivityAsync(MessageFactory.Text(String.Format("Hello {0}. Some of the main services I offer are: ", user.first)), cancellationToken);
            await stepContext.Context.SendActivityAsync(MessageFactory.Text(String.Format($"To manage payments type 'payment'", user.first)), cancellationToken);
            await stepContext.Context.SendActivityAsync(MessageFactory.Text(String.Format($"To inquiry about available content type 'inquiry' ", user.first)), cancellationToken);
            await stepContext.Context.SendActivityAsync(MessageFactory.Text(String.Format($"To troubleshoot an issue type 'troubleshoot' ", user.first)), cancellationToken);


            //return await stepContext.EndDialogAsync(null, cancellationToken);


            await stepContext.PromptAsync($"{nameof(GreetingDialog)}.options",
                    new PromptOptions
                    {
                        Prompt = MessageFactory.Text("Make sure to choose one of the three options above"),
                        RetryPrompt = MessageFactory.Text("Please type one of the three options above"),
                    }, cancellationToken);

            return await stepContext.EndDialogAsync(null, cancellationToken);
        }


        private Task<bool> optionsValidatorAsync(PromptValidatorContext<string> promptContext, CancellationToken cancellationToken)
        {
            var valid = false;

            if (promptContext.Recognized.Succeeded)
            {
                if (promptContext.Recognized.Value.ToLower() == "payment" || promptContext.Recognized.Value.ToLower() == "inquiry" || promptContext.Recognized.Value.ToLower() == "troubleshoot")
                {
                    valid = true;
                }
            }

            return Task.FromResult(valid);
        }
    }
}
