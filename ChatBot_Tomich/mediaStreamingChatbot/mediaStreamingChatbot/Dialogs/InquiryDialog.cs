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
    public class InquiryDialog : ComponentDialog
    {
        #region Variables
        private readonly BotStateService _botStateService;
        #endregion  
        public InquiryDialog(string dialogId, BotStateService botStateService) : base(dialogId)
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
                processQuestion,
                findCreator,
                showCreatorContent
            };

            // Add Named Dialogs
            AddDialog(new WaterfallDialog($"{nameof(InquiryDialog)}.mainFlow", waterfallSteps));
            AddDialog(new TextPrompt($"{nameof(InquiryDialog)}.genericText", creatorExistsValidatorAsync));
            AddDialog(new TextPrompt($"{nameof(InquiryDialog)}.generic"));

            // Set the starting Dialog
            InitialDialogId = $"{nameof(InquiryDialog)}.mainFlow";
        }

        private async Task<DialogTurnResult> InitialStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {

            return await stepContext.PromptAsync($"{nameof(InquiryDialog)}.generic",
                new PromptOptions
                {
                    Prompt = MessageFactory.Text("What are you inquiring about today?"),
                    
                }, cancellationToken);

        }
        private async Task<DialogTurnResult> processQuestion(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {

            if((string)stepContext.Result == "Do you have videos for kids?")
            {
                await stepContext.Context.SendActivityAsync(MessageFactory.Text($"Yes we have over 10,000 kids videos on our channel!"), cancellationToken);
                return await stepContext.EndDialogAsync(null, cancellationToken);
            }
            else if((string)stepContext.Result == "Do you have music videos?")
            {
                await stepContext.Context.SendActivityAsync(MessageFactory.Text($"Yes we have over 402,897 music videos"), cancellationToken);
                return await stepContext.EndDialogAsync(null, cancellationToken);
            }
            else if ((string)stepContext.Result == "Where can I find videos on cosplay?")
            {
                await stepContext.Context.SendActivityAsync(MessageFactory.Text($"Simply search cosplay in the search tab of our website"), cancellationToken);
                return await stepContext.EndDialogAsync(null, cancellationToken);
            }
            else if ((string)stepContext.Result == "What's the latest video by user: codingallday")
            {
                await stepContext.Context.SendActivityAsync(MessageFactory.Text($"Codingallday's latest video is 'scaling a react project'"), cancellationToken);
                return await stepContext.EndDialogAsync(null, cancellationToken);
            }
            else
            {
                await stepContext.Context.SendActivityAsync(MessageFactory.Text($"I dont understand but you can try searching for a creator."), cancellationToken);
                return await stepContext.NextAsync(null, cancellationToken);
            }

        }
        private async Task<DialogTurnResult> findCreator(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {

            return await stepContext.PromptAsync($"{nameof(InquiryDialog)}.genericText",
                new PromptOptions
                {
                    Prompt = MessageFactory.Text("Enter the name of the creator you are interested in"),
                    RetryPrompt = MessageFactory.Text("Sorry that channel does not exist." + "\n\n" + "Try typing in channels from 'harrysGotGame','HeyItsLarry', or 'TheEpicTraveler' "),
                }, cancellationToken);

        }

        private async Task<DialogTurnResult> showCreatorContent(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {

            if (stepContext.Result.ToString().ToLower() == "harrysgotgame")
            {
                await stepContext.Context.SendActivityAsync(MessageFactory.Text($"HarrysGotGame channels has: "), cancellationToken);
                await stepContext.Context.SendActivityAsync(MessageFactory.Text($"200,431 subscribers"), cancellationToken);
                await stepContext.Context.SendActivityAsync(MessageFactory.Text($"42 Videos"), cancellationToken);
                await stepContext.Context.SendActivityAsync(MessageFactory.Text($"Viewership is down by 43% this month"), cancellationToken);
            }
            else if (stepContext.Result.ToString().ToLower() == "heyitslarry")
            {
                await stepContext.Context.SendActivityAsync(MessageFactory.Text($"HeyItsLarry's channels has: "), cancellationToken);
                await stepContext.Context.SendActivityAsync(MessageFactory.Text($"10,031 subscribers"), cancellationToken);
                await stepContext.Context.SendActivityAsync(MessageFactory.Text($"5 Videos"), cancellationToken);
                await stepContext.Context.SendActivityAsync(MessageFactory.Text($"Viewership is up by 13% this month"), cancellationToken);

            }
            else if (stepContext.Result.ToString().ToLower() == "theepictraveler")
            {
                await stepContext.Context.SendActivityAsync(MessageFactory.Text($"TheEpicTraveler's channels has: "), cancellationToken);
                await stepContext.Context.SendActivityAsync(MessageFactory.Text($"300,831 subscribers"), cancellationToken);
                await stepContext.Context.SendActivityAsync(MessageFactory.Text($"28 Videos"), cancellationToken);
                await stepContext.Context.SendActivityAsync(MessageFactory.Text($"Viewership is up by 76% this month"), cancellationToken);
            }

            await stepContext.Context.SendActivityAsync(MessageFactory.Text($"To solve any other problems feel free to type 'hello' again."), cancellationToken);
            return await stepContext.EndDialogAsync(null, cancellationToken);

        }

        private Task<bool> creatorExistsValidatorAsync(PromptValidatorContext<string> promptContext, CancellationToken cancellationToken)
        {
            var valid = false;

            if (promptContext.Recognized.Succeeded)
            {
                if (promptContext.Recognized.Value.ToLower() == "harrysgotgame" || promptContext.Recognized.Value.ToLower() == "heyitslarry" || promptContext.Recognized.Value.ToLower() == "theepictraveler")
                {
                    valid = true;
                }
            }

            return Task.FromResult(valid);
        }
    }
}
