using Microsoft.Bot.Builder;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using mediaStreamingChatbot.Classes;
using Microsoft.Bot.Builder.Dialogs;

//may have to change back to mediaStreamingChatbot.services
namespace mediaStreamingChatbot.Services
{
    public class BotStateService
    {
        #region Variables
        //state variables
        public ConversationState ConversationState { get;}
        public UserState UserState { get; }

        public static string UserProfileID { get; } = $"{nameof(BotStateService)}.User";
        public static string ConversationDataId { get; } = $"{nameof(BotStateService)}.ConversationData";
        public static string DialogStateId { get; } = $"{nameof(BotStateService)}.DialogState";

        public IStatePropertyAccessor<User> UserProfileAccessor { get; set; }
        public IStatePropertyAccessor<ConversationData> ConversationDataAccessor { get; set; }
        public IStatePropertyAccessor<DialogState> DialogStateAccessor { get; set; }
        public IStatePropertyAccessor<Creator> creatorAccessor { get; set; }
        #endregion   

        public BotStateService(ConversationState conversationState, UserState userState)
        {
            UserState = userState ?? throw new ArgumentNullException(nameof(userState));
            ConversationState = conversationState ?? throw new ArgumentNullException(nameof(conversationState));
            InitializeAccessors();
        }

        public void InitializeAccessors()

        {
            //initialize user state
            UserProfileAccessor = UserState.CreateProperty<User>(UserProfileID);

            //initialize Conversation State Accessors
            ConversationDataAccessor = ConversationState.CreateProperty<ConversationData>(ConversationDataId);
            DialogStateAccessor = ConversationState.CreateProperty<DialogState>(DialogStateId);
            
        }
    }
}
