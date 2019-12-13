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
    public class paymentDialog : ComponentDialog
    {
        #region Variables
        private readonly BotStateService _botStateService;
        #endregion  
        public paymentDialog(string dialogId, BotStateService botStateService) : base(dialogId)
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
                askQuestionMain,
                askQuestionSecondary,
                askQuestionLast,
                processQuestion,
                nameOfCreator,
                paymentType,
                ManagePayments,
                CreateOrUnsub,
                FinalStepAsync
            };

            // Add Named Dialogs
            AddDialog(new WaterfallDialog($"{nameof(GreetingDialog)}.mainFlow", waterfallSteps));
            AddDialog(new TextPrompt($"{nameof(GreetingDialog)}.userOrCreator", userOrCreatorValidatorAsync));
            AddDialog(new TextPrompt($"{nameof(GreetingDialog)}.genericText"));
            AddDialog(new TextPrompt($"{nameof(GreetingDialog)}.OneTimeOrSub", OneTypeOrSubValidatorAsync));
            AddDialog(new TextPrompt($"{nameof(GreetingDialog)}.payments", PaymentsValidatorAsync));
            AddDialog(new TextPrompt($"{nameof(GreetingDialog)}.paymentType", PaymentTypeValidatorAsync));
            
            // Set the starting Dialog
            InitialDialogId = $"{nameof(GreetingDialog)}.mainFlow";
        }

        private async Task<DialogTurnResult> InitialStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
         
            return await stepContext.PromptAsync($"{nameof(GreetingDialog)}.userOrCreator",
                new PromptOptions
                {
                    Prompt = MessageFactory.Text("If you are a user type 'user' if you are a creator type 'creator'"),
                    RetryPrompt = MessageFactory.Text("Must enter 'user' or 'creator' "),
                }, cancellationToken);

        }

        private async Task<DialogTurnResult> askQuestionMain(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            stepContext.Values["userOrCreator"] = (string)stepContext.Result;

            return await stepContext.PromptAsync($"{nameof(GreetingDialog)}.genericText",
                new PromptOptions
                {
                    Prompt = MessageFactory.Text("How may I be of service?"),
                }, cancellationToken);
        }

        private async Task<DialogTurnResult> askQuestionSecondary(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            stepContext.Values["mainQuestion"] = (string)stepContext.Result;

            return await stepContext.PromptAsync($"{nameof(GreetingDialog)}.genericText",
                new PromptOptions
                {
                    Prompt = MessageFactory.Text("Can you provide more details?"),
                }, cancellationToken);
        }

        private async Task<DialogTurnResult> askQuestionLast(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            stepContext.Values["secondaryQuestion"] = (string)stepContext.Result;

            return await stepContext.PromptAsync($"{nameof(GreetingDialog)}.genericText",
                new PromptOptions
                {
                    Prompt = MessageFactory.Text("Thank you! Is there any last comments that you would like to add?"),
                }, cancellationToken);
        }

        private async Task<DialogTurnResult> processQuestion(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            stepContext.Values["lastQuestion"] = (string)stepContext.Result;

            if((string)stepContext.Values["userOrCreator"] == "user")
            {
                if((string)stepContext.Values["mainQuestion"] == "I want to pay for something but don't know how")
                {
                    await stepContext.Context.SendActivityAsync(MessageFactory.Text($"Payments can be made here directly so lets Start!"), cancellationToken);
                    return await stepContext.NextAsync(null, cancellationToken);
                }
                else if((string)stepContext.Values["mainQuestion"] == "I want to stop my subscription to a feed")
                {
                    await stepContext.Context.SendActivityAsync(MessageFactory.Text($"Subscriptions can be handleed here directly so lets start!"), cancellationToken);
                    return await stepContext.NextAsync(null, cancellationToken);
                }
                else if ((string)stepContext.Values["mainQuestion"] == "Why am I still getting charged?")
                {
                    if ((string) stepContext.Values["secondaryQuestion"] == "For a subscription to nownews")
                    {
                        if((string)stepContext.Values["lastQuestion"] == "I cancelled three weeks ago")
                        {
                            await stepContext.Context.SendActivityAsync(MessageFactory.Text($"Im sorry to hear this is an issue"), cancellationToken);
                            await stepContext.Context.SendActivityAsync(MessageFactory.Text($"It looks like you did not cancel your subscription after the 7 day free trial"), cancellationToken);
                            return await stepContext.EndDialogAsync(null, cancellationToken);
                        }
                        else
                        {
                            await stepContext.Context.SendActivityAsync(MessageFactory.Text($"Im not entirely sure"), cancellationToken);
                            await stepContext.Context.SendActivityAsync(MessageFactory.Text($"please be sure to start a support ticket under my troubleshoot option!"), cancellationToken);
                            return await stepContext.EndDialogAsync(null, cancellationToken);
                        }

                    }
                    else
                    {
                        await stepContext.Context.SendActivityAsync(MessageFactory.Text($"If you are still gettign charged please run through my troubleshoot moded to create a ticket"), cancellationToken);
                        return await stepContext.EndDialogAsync(null, cancellationToken);
                    }
                    
                }
                else if ((string)stepContext.Values["mainQuestion"] == "How much does this video cost?")
                {
                    await stepContext.Context.SendActivityAsync(MessageFactory.Text($"My records show that this video costs $24.99"), cancellationToken);
                    return await stepContext.EndDialogAsync(null, cancellationToken);
                }
                else if ((string)stepContext.Values["mainQuestion"] == "How much does the subscription for this user cost?")
                {
                    await stepContext.Context.SendActivityAsync(MessageFactory.Text($"The user in question charges a monthly fee of $ 7.99"), cancellationToken);
                    return await stepContext.EndDialogAsync(null, cancellationToken);
                }
                else
                {
                    await stepContext.Context.SendActivityAsync(MessageFactory.Text($"I'm not sure what you meant but you can find a creator after this"), cancellationToken);
                    return await stepContext.NextAsync(null, cancellationToken);
                }
            }
            else
            {
                if ((string)stepContext.Values["mainQuestion"] == "I want to charge $5.00 for this video per view")
                {
                    await stepContext.Context.SendActivityAsync(MessageFactory.Text($"Great your charge preferences have been update and should be live within the next 10 minutes!"), cancellationToken);
                    return await stepContext.EndDialogAsync(null, cancellationToken);
                }
                else if((string)stepContext.Values["mainQuestion"] == "I want to charge $10.00 for this video for unlimited viewing")
                {
                    await stepContext.Context.SendActivityAsync(MessageFactory.Text($"Many of our content creators have asked for this so we now offer this funtionality!"), cancellationToken);
                    await stepContext.Context.SendActivityAsync(MessageFactory.Text($"Your request has been approved and will be live in the next 10 minutes."), cancellationToken);
                    return await stepContext.EndDialogAsync(null, cancellationToken);
                }
                else if ((string)stepContext.Values["mainQuestion"] == "I want to setup a subscription to my channel or streams for $15.00")
                {
                    await stepContext.Context.SendActivityAsync(MessageFactory.Text($"Your request has been approved and will be live in 10 minutes!"), cancellationToken);
                    return await stepContext.EndDialogAsync(null, cancellationToken);
                }
                else if ((string)stepContext.Values["mainQuestion"] == "I'm not getting paid for my views/subscriptions")
                {
                    if ((string)stepContext.Values["secondaryQuestion"] == "What views have I gotten since last week?")
                    {
                        if ((string)stepContext.Values["lastQuestion"] == "Do I have any money that hasn't been paid out?")
                        {
                            await stepContext.Context.SendActivityAsync(MessageFactory.Text($"To answer your first question, payments are made every month. Your next mapment will be 11/22/19"), cancellationToken);
                            await stepContext.Context.SendActivityAsync(MessageFactory.Text($"To answer your second question, you have 20,043 views"), cancellationToken);
                            await stepContext.Context.SendActivityAsync(MessageFactory.Text($"To answer your third question, you have $300 that hasnt been paid out"), cancellationToken);
                            return await stepContext.EndDialogAsync(null, cancellationToken);
                        }
                        else
                        {
                            await stepContext.Context.SendActivityAsync(MessageFactory.Text($"Sorry I did not understand your second question. To answer your first question, Payments are made very month."), cancellationToken);
                            return await stepContext.EndDialogAsync(null, cancellationToken);
                        }
                    }
                    else
                    {
                        await stepContext.Context.SendActivityAsync(MessageFactory.Text($"Paymnets are made very month."), cancellationToken);
                        await stepContext.Context.SendActivityAsync(MessageFactory.Text($"You have 100 views as of 11/30/19"), cancellationToken);
                        return await stepContext.EndDialogAsync(null, cancellationToken);
                    }
                }
                else
                {
                    await stepContext.Context.SendActivityAsync(MessageFactory.Text($"Hmmm i'm unable to help you on this matter"), cancellationToken);
                    await stepContext.Context.SendActivityAsync(MessageFactory.Text($"Check in another time!"), cancellationToken);

                    return await stepContext.EndDialogAsync(null, cancellationToken);
                }
            }

           
        }

        private async Task<DialogTurnResult> nameOfCreator(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            User user = await _botStateService.UserProfileAccessor.GetAsync(stepContext.Context, () => new User());

        
           return await stepContext.PromptAsync($"{nameof(GreetingDialog)}.genericText",
               new PromptOptions
               {
                   Prompt = MessageFactory.Text("Enter the name of the creator that you would like to manage or subscribe to!")
               }, cancellationToken);
      
        }

        private async Task<DialogTurnResult> paymentType(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            stepContext.Values["creator"] = (string)stepContext.Result;

            return await stepContext.PromptAsync($"{nameof(GreetingDialog)}.OneTimeOrSub",
               new PromptOptions
               {
                   Prompt = MessageFactory.Text(String.Format("To make a one time payment to {0} type 'one time'" + "\n\n" + "To manage a recurring subscription with {0} type 'recurring' ", (string)stepContext.Values["creator"])),
                   RetryPrompt = MessageFactory.Text("Must enter 'one time' or 'recurring' "),
               }, cancellationToken);                 
        }

        private async Task<DialogTurnResult> ManagePayments(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            //User user = await _botStateService.UserProfileAccessor.GetAsync(stepContext.Context, () => new User());
            stepContext.Values["paymentType"] = (string)stepContext.Result;

            if ((string)stepContext.Result == "one time")
            {
                return await stepContext.PromptAsync($"{nameof(GreetingDialog)}.payments",
                    new PromptOptions
                    {
                        Prompt = MessageFactory.Text(String.Format("Type yes if you are ok with a one time payment of $15.99 to {0}'s channel \n Note: this will last for one month", (string)stepContext.Values["creator"])),
                        RetryPrompt = MessageFactory.Text("please type 'yes' or 'no'"),
                    }, cancellationToken);
            }
            else if ((string)stepContext.Result == "recurring")
            {
                return await stepContext.PromptAsync($"{nameof(GreetingDialog)}.paymentType",
                    new PromptOptions
                    {
                        Prompt = MessageFactory.Text("Type '1' to unsubscribe or '2' to start a reccurring subscription"),
                        RetryPrompt = MessageFactory.Text("You can only select '1' or '2' "),
                    }, cancellationToken);
            }
            else
            {
                return await stepContext.NextAsync(null, cancellationToken);
            }

        }

        private async Task<DialogTurnResult> CreateOrUnsub(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            //User user = await _botStateService.UserProfileAccessor.GetAsync(stepContext.Context, () => new User());
            stepContext.Values["managePaymentResult"] = (string)stepContext.Result;

            if ((string)stepContext.Result == "1")
            {
                await stepContext.Context.SendActivityAsync(MessageFactory.Text(String.Format("You have unsubscribed to {0}'s Channel", (string)stepContext.Values["creator"])), cancellationToken);
                await stepContext.Context.SendActivityAsync(MessageFactory.Text($"To solve any other problems feel free to type 'hello' again."), cancellationToken);
                return await stepContext.EndDialogAsync(null, cancellationToken);
            }
            else if ((string)stepContext.Result == "2")
            {
                return await stepContext.PromptAsync($"{nameof(GreetingDialog)}.payments",
                    new PromptOptions
                    {
                        Prompt = MessageFactory.Text(String.Format("Type 'yes' if you are ok with reccuring payment of $15.99 to {0}'s channel",(string)stepContext.Values["creator"])),
                        RetryPrompt = MessageFactory.Text("you can only type 'yes' or 'no'"),

                    }, cancellationToken);
            }
            else if ((string)stepContext.Result == "yes")
            {
                await stepContext.Context.SendActivityAsync(MessageFactory.Text(String.Format("Payment sucessful")), cancellationToken);
                await stepContext.Context.SendActivityAsync(MessageFactory.Text($"To solve any other problems feel free to type 'hello' again."), cancellationToken);
                return await stepContext.EndDialogAsync(null, cancellationToken);
            }
            else if ((string)stepContext.Result == "no")
            {
                await stepContext.Context.SendActivityAsync(MessageFactory.Text($"Payment cancelled!"), cancellationToken);
                await stepContext.Context.SendActivityAsync(MessageFactory.Text($"To solve any other problems feel free to type 'hello' again."), cancellationToken);
                return await stepContext.EndDialogAsync(null, cancellationToken);
            }
            else
            {
                return await stepContext.NextAsync(null, cancellationToken);
            }
        }

        private async Task<DialogTurnResult> FinalStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            if ((string)stepContext.Result == "yes")
            {
                await stepContext.Context.SendActivityAsync(MessageFactory.Text(String.Format("Payment sucessful")), cancellationToken);
            }
            else
            {
                await stepContext.Context.SendActivityAsync(MessageFactory.Text(String.Format("Payment cancelled")), cancellationToken);
            }
            await stepContext.Context.SendActivityAsync(MessageFactory.Text($"To solve any other problems feel free to type 'hello' again."), cancellationToken);
            return await stepContext.EndDialogAsync(null, cancellationToken);
        }

        private Task<bool> PaymentsValidatorAsync(PromptValidatorContext<string> promptContext, CancellationToken cancellationToken)
        {
            var valid = false;

            if(promptContext.Recognized.Succeeded)
            {
                if(promptContext.Recognized.Value.ToLower() == "yes" || promptContext.Recognized.Value.ToLower() == "no")
                {
                    valid = true;
                }
            }

            return Task.FromResult(valid);
        }

        private Task<bool> PaymentTypeValidatorAsync(PromptValidatorContext<string> promptContext, CancellationToken cancellationToken)
        {
            var valid = false;

            if (promptContext.Recognized.Succeeded)
            {
                if (promptContext.Recognized.Value.ToLower() == "1" || promptContext.Recognized.Value.ToLower() == "2")
                {
                    valid = true;
                }
            }

            return Task.FromResult(valid);
        }

        private Task<bool> OneTypeOrSubValidatorAsync(PromptValidatorContext<string> promptContext, CancellationToken cancellationToken)
        {
            var valid = false;

            if (promptContext.Recognized.Succeeded)
            {
                if (promptContext.Recognized.Value.ToLower() == "one time" || promptContext.Recognized.Value.ToLower() == "recurring")
                {
                    valid = true;
                }
            }

            return Task.FromResult(valid);
        }

        private Task<bool> userOrCreatorValidatorAsync(PromptValidatorContext<string> promptContext, CancellationToken cancellationToken)
        {
            var valid = false;

            if (promptContext.Recognized.Succeeded)
            {
                if (promptContext.Recognized.Value.ToLower() == "user" || promptContext.Recognized.Value.ToLower() == "creator")
                {
                    valid = true;
                }
            }

            return Task.FromResult(valid);
        }
    }
}
