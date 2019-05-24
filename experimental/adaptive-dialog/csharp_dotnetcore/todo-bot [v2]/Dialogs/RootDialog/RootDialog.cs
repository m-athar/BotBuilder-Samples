﻿using System;
using System.Collections.Generic;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Adaptive;
using Microsoft.Bot.Builder.AI.Luis;
using Microsoft.Extensions.Configuration;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Rules;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Steps;

namespace Microsoft.BotBuilderSamples
{
    public class RootDialog : ComponentDialog
    {
        private static IConfiguration Configuration;

        public RootDialog(IConfiguration configuration)
            : base(nameof(RootDialog))
        {
            Configuration = configuration;
            // Create instance of adaptive dialog. 
            var rootDialog = new AdaptiveDialog(nameof(AdaptiveDialog))
            {
                // Create a LUIS recognizer.
                // The recognizer is built using the intents, utterances, patterns and entities defined in ./RootDialog.lu file
                Recognizer = CreateRecognizer(),
                Rules = new List<IRule>()
                {
                    // Intent rules for the LUIS model. Each intent here corresponds to an intent defined in ./Dialogs/Resources/ToDoBot.lu file
                    new IntentRule("Greeting")         { Steps = new List<IDialog>() { new SendActivity("[Help-Root-Dialog]") } },
                    new IntentRule("Add_todo")    { Steps = new List<IDialog>() { new BeginDialog(nameof(AddToDoDialog)) } },
                    new IntentRule("DeleteToDoDialog") { Steps = new List<IDialog>() { new BeginDialog(nameof(DeleteToDoDialog)) } },
                    new IntentRule("ViewToDoDialog")   { Steps = new List<IDialog>() { new BeginDialog(nameof(ViewToDoDialog)) } },
                    // Come back with LG template based readback for global help
                    new IntentRule("Help")             { Steps = new List<IDialog>() { new SendActivity("[Help-Root-Dialog]") } },
                    new IntentRule("Cancel")           { Steps = new List<IDialog>() {
                            // This is the global cancel in case a child dialog did not explicit handle cancel.
                            new SendActivity("Cancelling all dialogs.."),
                            new SendActivity("[Welcome-Actions]"),
                            new CancelAllDialogs(),
                        }
                    }
                }
            };

            // Add named dialogs to the DialogSet. These names are saved in the dialog state.
            AddDialog(rootDialog);

            // Add all child dialogS
            AddDialog(new AddToDoDialog(configuration));
            AddDialog(new DeleteToDoDialog(configuration));
            AddDialog(new ViewToDoDialog());

            // The initial child Dialog to run.
            InitialDialogId = nameof(AdaptiveDialog);
        }

        private static IRecognizer CreateRecognizer()
        {
            if (string.IsNullOrEmpty(Configuration["RootDialog_en-us_lu:Luis-host-name"]) || string.IsNullOrEmpty(Configuration["RootDialog_en-us_lu:Luis-endpoint-key"]) || string.IsNullOrEmpty(Configuration["RootDialog_en-us_lu:Luis-app-id"]))
            {
                throw new Exception("Your LUIS application is not configured. Please see README.MD to set up a LUIS application.");
            }
            return new LuisRecognizer(new LuisApplication()
            {
                Endpoint = Configuration["RootDialog_en-us_lu:Luis-host-name"],
                EndpointKey = Configuration["RootDialog_en-us_lu:Luis-endpoint-key"],
                ApplicationId = Configuration["RootDialog_en-us_lu:Luis-app-id"]
            });
        }
    }
}