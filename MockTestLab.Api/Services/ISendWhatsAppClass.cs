using MessageBird;
using MessageBird.Exceptions;
using MessageBird.Objects.Conversations;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MockTestLab.Api.Services
{
    public class ISendWhatsAppClass
    {

        public void SendWhatsApp()
        {
            string YOUR_ACCESS_KEY = "YOUR_ACCESS_KEY";
            var client = Client.CreateDefault(YOUR_ACCESS_KEY);

            try
            {
                var transcriptionResponse = client.CreateTranscription("CALL ID", "LEG ID", "RECORDING ID", "LANGUAGE");
                var transcription = transcriptionResponse.Data.FirstOrDefault();

                Console.WriteLine("The Transcription Id is: {0}", transcription.Id);
                Console.WriteLine("The Transcription Recording Id is: {0}", transcription.RecordingId);
                Console.WriteLine("The Transcription Status is: {0}", transcription.Status);
            }
            catch (ErrorException e)
            {
                // Either the request fails with error descriptions from the endpoint.
                if (e.HasErrors)
                {
                    foreach (var error in e.Errors)
                    {
                        Console.WriteLine("code: {0} description: '{1}' parameter: '{2}'", error.Code, error.Description, error.Parameter);
                    }
                }
                // or fails without error information from the endpoint, in which case the reason contains a 'best effort' description.
                if (e.HasReason)
                {
                    Console.WriteLine(e.Reason);
                }
            }
        }

        public static void UserVarificationVaiWhatsApp()
        {
            const string YourAccessKey = "ADwfTzUQwSEyWogqLDrMjjJSr";

            Client client = Client.CreateDefault(YourAccessKey);

            // client with whatsapp sandbox enabled
            //Client client = Client.CreateDefault(YourAccessKey, features: new Client.Features[] { Client.Features.EnableWhatsAppSandboxConversations });

            HsmLanguage language = new HsmLanguage();
            language.Code = "en";
            language.Policy = HsmLanguagePolicy.Deterministic;

            List<HsmLocalizableParameter> hsmLocalizableParameters = new List<HsmLocalizableParameter>();

            HsmLocalizableParameter hsmParamName = new HsmLocalizableParameter();
            hsmParamName.Default = "Bob";

            HsmLocalizableParameter hsmParamWhen = new HsmLocalizableParameter();
            hsmParamName.Default = "Tomorrow";

            hsmLocalizableParameters.Add(hsmParamName);
            hsmLocalizableParameters.Add(hsmParamWhen);

            HsmContent hsmContent = new HsmContent();
            hsmContent.Namespace = "db6dbed0_9765_404e_98f7_d839fb6abd6b";
            hsmContent.TemplateName = "verification";
            hsmContent.Language = language;
            hsmContent.Params = hsmLocalizableParameters;

            Content content = new Content();
            content.Hsm = hsmContent;

            ConversationStartRequest request = new ConversationStartRequest();
            request.To = "919910186797";
            request.Type = ContentType.Hsm;
            request.Content = content;
            request.ChannelId = "aeebaf1eaca64299af373b604b08ed86";

            //MessageBird.Objects.Message message = client.StartConversation(request);
        }
    }
}
