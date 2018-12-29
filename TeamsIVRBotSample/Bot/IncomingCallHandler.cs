namespace ThoughtStuff.TeamsSamples.IVRBotSample
{
    using Microsoft.Graph;
    using Microsoft.Graph.Calls;
    using Microsoft.Graph.Core.Telemetry;
    using Microsoft.Graph.Core.Transport;
    using Microsoft.Graph.StatefulClient;
    using Newtonsoft.Json.Linq;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Net;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using ThoughtStuff.TeamsSamples.Data;

    /// <summary>
    /// The call handler for incoming calls.
    /// </summary>
    public class IncomingCallHandler : CallHandler
    {


        private int promptTimes;
        private int numberToGuess;



        /// <summary>
        /// Initializes a new instance of the <see cref="IncomingCallHandler"/> class.
        /// </summary>
        /// <param name="bot">The bot.</param>
        /// <param name="call">The call.</param>
        /// <param name="endpointId">The bot endpoint id.</param>
        public IncomingCallHandler(Bot bot, ICall call)
            : base(bot, call)
        { }


        protected async override void CallOnUpdated(ICall sender, ResourceEventArgs<Call> args)
        {
            if (sender.Resource.State == CallState.Established)
            {
                var currentPromptTimes = Interlocked.Increment(ref this.promptTimes);

                //first time call gets established, play the welcome message. Also, pick a random number and wait for DTMF tones
                if (currentPromptTimes == 1)
                {
                    Random random = new Random();
                    this.numberToGuess = random.Next(1, 10);

                    this.SubscribeToTone();

                    await this.PlayNotificationPrompt("SimpleIVRWelcome.wav");
                }

                //subsequent times, deal with the DTMF tones
                if (sender.Resource.ToneInfo?.Tone != null)
                {
                    var toneValue = ConvertToneToNumber(sender.Resource.ToneInfo.Tone.Value);

                    if (toneValue > 0)
                    {
                        if (toneValue > numberToGuess)
                        {
                            await this.PlayNotificationPrompt("Lower.wav");
                        }
                        if (toneValue < numberToGuess)
                        {
                            await this.PlayNotificationPrompt("Higher.wav");
                        }
                        if (toneValue == numberToGuess)
                        {
                            await this.PlayNotificationPrompt("Finish.wav");
                            this.Call.Client.TerminateAsync();
                        }
                    }

                }
            }
        }


        private int ConvertToneToNumber(Tone tone)
        {
            switch (tone)
            {
                case Tone.Tone1:
                    return 1;
                case Tone.Tone2:
                    return 2;
                case Tone.Tone3:
                    return 3;
                case Tone.Tone4:
                    return 4;
                case Tone.Tone5:
                    return 5;
                case Tone.Tone6:
                    return 6;
                case Tone.Tone7:
                    return 7;
                case Tone.Tone8:
                    return 8;
                case Tone.Tone9:
                    return 9;
                default:
                    return -1;
            }
        }

        private async Task PlayNotificationPrompt(string filename)
        {
            try
            {

                var sb = new StringBuilder();

                var ttsMedia = new MediaInfo
                {
                    Uri = new Uri(this.Bot.baseURL, filename).ToString(),
                    ResourceId = Guid.NewGuid().ToString(),
                };
                var ttsMediaPrompt = new MediaPrompt() { MediaInfo = ttsMedia, Loop = 1 };

                await this.Call.PlayPromptAsync(new List<MediaPrompt> { ttsMediaPrompt }).ConfigureAwait(false);


                this.Logger.Info("Started playing prompt:" + filename);
            }
            catch (Exception ex)
            {
                this.Logger.Error(ex, $"Failed to play notification prompt: " + filename);
                throw;
            }

        }
    }
}
