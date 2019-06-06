// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using System.Linq;
using Microsoft.Bot.Builder.AI.Luis;

namespace HotelBot1
{
    public class EmptyBot : ActivityHandler
    {
        public const string RestaurantIntent = "BookRestaurant";
        public const string SpaIntent = "BookSPA";
        public const string TouristIntent = "TouristInfo";
        private LuisRecognizer _recognizer { get; } = null;

        public EmptyBot(LuisRecognizer recognizer) : base()
        {
            _recognizer = recognizer ?? throw new System.ArgumentNullException(nameof(recognizer));
        }

        protected override async Task OnMembersAddedAsync(IList<ChannelAccount> membersAdded, ITurnContext<IConversationUpdateActivity> turnContext, CancellationToken cancellationToken)
        {
            foreach (var member in membersAdded)
            {
                if (member.Id != turnContext.Activity.Recipient.Id)
                {
                    await turnContext.SendActivityAsync(MessageFactory.Text("Hello, how can I help you today?"));
                }
            }
        }

        protected override async Task OnMessageActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            var recognizerResult = await _recognizer.RecognizeAsync(turnContext, cancellationToken);
            var (intent, score) = recognizerResult.GetTopScoringIntent();
            var entities = recognizerResult.Entities;

            string message = "";
            var people = entities["number"]?.FirstOrDefault()?.ToString() ?? "1";
            var date = entities["datetime"]?.FirstOrDefault()["timex"]?.FirstOrDefault()?.ToString() ?? "today";
            var topic = entities["TouristAttraction"]?.FirstOrDefault()?.FirstOrDefault()?.ToString() ?? "the city";

            switch (intent)
            {
                case RestaurantIntent:
                    message = $"We will book a table for {people} people for {date}.";
                    break;
                case SpaIntent:
                    message = $"We will book an appointment for the Spa for {people} people for {date}.";
                    break;
                case TouristIntent:
                    message = $"We will provide you with information about {topic}";
                    break;
                default:
                    message = "Sorry, I did not understand you, how may I help you?";
                    break;
            }

            await turnContext.SendActivityAsync(message);
        }

        private static async Task SendSuggestedActionsAsync(ITurnContext turnContext, CancellationToken cancellationToken)
        {
            var reply = turnContext.Activity.CreateReply("Hello, what can I help you with today?");
            reply.SuggestedActions = new SuggestedActions()
            {
                Actions = new List<CardAction>()
                {
                    new CardAction() { Title = "SPA", Type = ActionTypes.ImBack, Value = "SPA" },
                    new CardAction() { Title = "Restaurant", Type = ActionTypes.ImBack, Value = "Restaurant" },
                    new CardAction() { Title = "Tourist information", Type = ActionTypes.ImBack, Value = "Tourist information" }
                }
            };
            
            await turnContext.SendActivityAsync(reply, cancellationToken);
        }
    }
}
