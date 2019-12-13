using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Choices;
using mediaStreamingChatbot.Classes;
using mediaStreamingChatbot.Services;
using mediaStreamingChatbot.Dialogs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;


namespace mediaStreamingChatbot.Dialogs
{
    public class TroubleshootDialog : ComponentDialog
    {
        #region Variables
        private readonly BotStateService _botStateService;
        #endregion

        public TroubleshootDialog(string dialogId, BotStateService botStateService) : base(dialogId)
        {
            _botStateService = botStateService ?? throw new System.ArgumentNullException(nameof(botStateService));

            InitializeWaterfallDialog();
        }

        private void InitializeWaterfallDialog()
        {
            // Create Waterfall Steps
            var waterfallSteps = new WaterfallStep[]
            {
                DescriptionStepAsync,
                determineClient,
                moreDetail,
                lastDetail,
                helpClient,
                SummaryStepAsync
            };

            // Add Named Dialogs
            AddDialog(new WaterfallDialog($"{nameof(TroubleshootDialog)}.mainFlow", waterfallSteps));
            AddDialog(new TextPrompt($"{nameof(TroubleshootDialog)}.description", userOrProducerValidatorAsync));
            AddDialog(new TextPrompt($"{nameof(TroubleshootDialog)}.generic"));

            // Set the starting Dialog
            InitialDialogId = $"{nameof(TroubleshootDialog)}.mainFlow";
        }

        #region Waterfall Steps
        private async Task<DialogTurnResult> DescriptionStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            await stepContext.Context.SendActivityAsync(MessageFactory.Text($"COMMON QUESTIONS AND ANSWERS!!!!"), cancellationToken);

            await stepContext.Context.SendActivityAsync(MessageFactory.Text($"Q: Some common issue: Cant make a payment"), cancellationToken);
            await stepContext.Context.SendActivityAsync(MessageFactory.Text($"A: Check internet connection or clear cache"), cancellationToken);
            
            await stepContext.Context.SendActivityAsync(MessageFactory.Text($"Q: Video not loading"), cancellationToken);
            await stepContext.Context.SendActivityAsync(MessageFactory.Text($"A: reset your internet or check in with your ISP"), cancellationToken);

            await stepContext.Context.SendActivityAsync(MessageFactory.Text($"Q: Cant find a channel"), cancellationToken);
            await stepContext.Context.SendActivityAsync(MessageFactory.Text($"A: Sometimes creators change their channel names so be sure to subscribe for updates!"), cancellationToken);
            return await stepContext.PromptAsync($"{nameof(TroubleshootDialog)}.description",
                new PromptOptions
                {
                    Prompt = MessageFactory.Text("For users type 'user' \n\n For content producers type 'content producer'"),
                    RetryPrompt = MessageFactory.Text("Please type one of the two options above")
                }, cancellationToken);
        }

        private async Task<DialogTurnResult> determineClient(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            stepContext.Values["userOrProducer"] = (string)stepContext.Result;
            return await stepContext.PromptAsync($"{nameof(TroubleshootDialog)}.generic",
                new PromptOptions
                {
                    Prompt = MessageFactory.Text($"Thank you! What issue are you having"),
                    
                }, cancellationToken);

        }

        private async Task<DialogTurnResult> moreDetail(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            stepContext.Values["mainIssue"] = (string)stepContext.Result;

            return await stepContext.PromptAsync($"{nameof(TroubleshootDialog)}.generic",
                new PromptOptions
                {
                    Prompt = MessageFactory.Text($"Thank you again. In more detail can you explain the issue"),

                }, cancellationToken);
        }
        private async Task<DialogTurnResult> lastDetail(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            stepContext.Values["detailedIssue"] = (string)stepContext.Result;

            return await stepContext.PromptAsync($"{nameof(TroubleshootDialog)}.generic",
               new PromptOptions
               {
                   Prompt = MessageFactory.Text($"Are there any other problems?"),

               }, cancellationToken);
        }

        private async Task<DialogTurnResult> helpClient(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            stepContext.Values["lastDetail"] = (string)stepContext.Result;

            
            if((string)stepContext.Values["userOrProducer"] == "user")
            {
                if ((string)stepContext.Values["mainIssue"] == "I'm having trouble")
                {
                    if ((string)stepContext.Values["detailedIssue"] == "I can't get access to content I've paid for")
                    {
                        await stepContext.Context.SendActivityAsync(MessageFactory.Text($"Im so sorry to hear that. :("), cancellationToken);
                        await stepContext.Context.SendActivityAsync(MessageFactory.Text($"We are aware of this issue and will be resolving it soon"), cancellationToken);
                        await stepContext.Context.SendActivityAsync(MessageFactory.Text($"In the mean time we will be creating a support ticket for you!"), cancellationToken);
                    }
                    else if ((string)stepContext.Values["detailedIssue"] == "I can't get videos to play on other devices")
                    {
                        await stepContext.Context.SendActivityAsync(MessageFactory.Text($"I'm sorry for this occurance."), cancellationToken);
                        await stepContext.Context.SendActivityAsync(MessageFactory.Text($"Some solutions may be resetting your device, checking your internet connection, or updating your device"), cancellationToken);
                        await stepContext.Context.SendActivityAsync(MessageFactory.Text($"To further our support we will create a support ticket"), cancellationToken);
                    }
                    else
                    {
                        await stepContext.Context.SendActivityAsync(MessageFactory.Text($"I'm unaware of this issue"), cancellationToken);
                        await stepContext.Context.SendActivityAsync(MessageFactory.Text($"To further our support we will create a support ticket"), cancellationToken);
                    }
                }
                else if ((string)stepContext.Values["mainIssue"] == "I need help")
                {
                    if ((string)stepContext.Values["detailedIssue"] == "I want to check on my support ticket #345")
                    {
                        await stepContext.Context.SendActivityAsync(MessageFactory.Text($"Thanks your ticket is still under review and should be resolved within the next couple of days"), cancellationToken);
                        await stepContext.Context.SendActivityAsync(MessageFactory.Text($"Here is the status of your ticket"), cancellationToken);
                    }
                    else
                    {
                        await stepContext.Context.SendActivityAsync(MessageFactory.Text($"I'm sorry there are no current soulutions as of yet but we are working hard for customer service!"), cancellationToken);
                        await stepContext.Context.SendActivityAsync(MessageFactory.Text($"A support ticket will be created for you shortly"), cancellationToken);
                    }
                }
                else
                {
                    await stepContext.Context.SendActivityAsync(MessageFactory.Text($"hmm it seems i dont have a solution as of yet"), cancellationToken);
                    await stepContext.Context.SendActivityAsync(MessageFactory.Text($"A support ticket will be created for you shortly"), cancellationToken);
                }
              
                
            }
            else
            {
                if ((string)stepContext.Values["mainIssue"] == "My latest video isn't online")
                {
                    if((string)stepContext.Values["lastDetail"] == "Please open a support ticket")
                    {
                        await stepContext.Context.SendActivityAsync(MessageFactory.Text($"I'm sorry to hear. We will gladly create a support ticket"), cancellationToken);
                    }
                    else
                    {
                        await stepContext.Context.SendActivityAsync(MessageFactory.Text($"Try uploading in a lower resolution"), cancellationToken);
                        return await stepContext.EndDialogAsync(cancellationToken: cancellationToken);
                    }
                    
                }
                else if ((string)stepContext.Values["mainIssue"] == "My subscription page is not showing my latest videos")
                {
                    if((string)stepContext.Values["lastDetail"] == "Please make this video first on my page")
                    {
                        await stepContext.Context.SendActivityAsync(MessageFactory.Text($"We have made this video first on your page"), cancellationToken);
                        return await stepContext.EndDialogAsync(cancellationToken: cancellationToken);
                    }
                    else
                    {
                        await stepContext.Context.SendActivityAsync(MessageFactory.Text($"We are creating a support ticket"), cancellationToken);
                    }
                }
                else if ((string)stepContext.Values["mainIssue"] == "Why was my video cut off?")
                {
                    if ((string)stepContext.Values["detailedIssue"] == "I didn't know there was a 10 minute max time")
                    {
                        if ((string)stepContext.Values["lastDetail"] == "Can I suggest a trim function be added on submission?")
                        {
                            await stepContext.Context.SendActivityAsync(MessageFactory.Text($"Yes we will look into creating a trim function!"), cancellationToken);
                            await stepContext.Context.SendActivityAsync(MessageFactory.Text($"To help speed this process we will be creating a support ticket"), cancellationToken);
                        }
                    }
                }
                else
                {
                    await stepContext.Context.SendActivityAsync(MessageFactory.Text($"We are unaware of this issue :("), cancellationToken);
                    await stepContext.Context.SendActivityAsync(MessageFactory.Text($"To help speed this process we will be creating a support ticket"), cancellationToken);
                }
            }

            

            return await stepContext.NextAsync(null, cancellationToken);
        }

        private async Task<DialogTurnResult> SummaryStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {

            // Get the current profile object from user state.
            var User = await _botStateService.UserProfileAccessor.GetAsync(stepContext.Context, () => new User(), cancellationToken);

            // Save all of the data inside the troubleshoot class
            User.description = (string)stepContext.Values["detailedIssue"];
         

            // show summary of report to user 
           
            await stepContext.Context.SendActivityAsync(MessageFactory.Text($"Here is a summary of your ticket: "), cancellationToken);
            await stepContext.Context.SendActivityAsync(MessageFactory.Text(String.Format("Troubleshooting issue: {0} ", (string)stepContext.Values["detailedIssue"])), cancellationToken);
            await stepContext.Context.SendActivityAsync(MessageFactory.Text(String.Format("Detail: {0} ", (string)stepContext.Values["lastDetail"])), cancellationToken);
            await stepContext.Context.SendActivityAsync(MessageFactory.Text($"Status: Under Review"), cancellationToken);
            await stepContext.Context.SendActivityAsync(MessageFactory.Text($"Urgency: Medium"), cancellationToken);
            await stepContext.Context.SendActivityAsync(MessageFactory.Text($"To solve any other problems feel free to type 'hello' again."), cancellationToken);

            // Save data in userstate
            await _botStateService.UserProfileAccessor.SetAsync(stepContext.Context, User);

            // WaterfallStep always finishes with the end of the Waterfall or with another dialog, here it is the end.
            return await stepContext.EndDialogAsync(cancellationToken: cancellationToken);
        }
        #endregion

        private Task<bool> userOrProducerValidatorAsync(PromptValidatorContext<string> promptContext, CancellationToken cancellationToken)
        {
            var valid = false;

            if (promptContext.Recognized.Succeeded)
            {
                if (promptContext.Recognized.Value.ToLower() == "user" || promptContext.Recognized.Value.ToLower() == "content producer")
                {
                    valid = true;
                }
            }

            return Task.FromResult(valid);
        }

    }
}
