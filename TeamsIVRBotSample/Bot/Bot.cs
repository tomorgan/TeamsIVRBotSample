namespace ThoughtStuff.TeamsSamples.IVRBotSample
{
    using Microsoft.ApplicationInsights;
    using Microsoft.Extensions.Logging;
    using Microsoft.Graph;
    using Microsoft.Graph.Calls;
    using Microsoft.Graph.Core;
    using Microsoft.Graph.Core.Telemetry;
    using Microsoft.Graph.StatefulClient;
    using Sample.Common.Authentication;
    using Sample.Common.Meetings;
    using Sample.Common.OnlineMeetings;
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using ThoughtStuff.TeamsSamples.Data;



    /// <summary>
    /// The core bot logic.
    /// </summary>
    public class Bot
    {

        private readonly IGraphLogger graphLogger;

        private ConcurrentDictionary<string, CallHandler> callHandlers = new ConcurrentDictionary<string, CallHandler>();

        private LinkedList<string> callbackLogs = new LinkedList<string>();

        private AuthenticationProvider authProvider;
        private TelemetryClient telemetry = new TelemetryClient();

        public Uri baseURL { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Bot"/> class.
        /// </summary>
        /// <param name="options">The bot options</param>
        /// <param name="loggerFactory">The logger factory</param>
        public Bot(BotOptions options, IGraphLogger graphLogger)
        {
            var instanceNotificationUri = CallAffinityMiddleware.GetWebInstanceCallbackUri(new Uri(options.BotBaseUrl, HttpRouteConstants.OnIncomingRequestRoute));

            this.graphLogger = graphLogger;
            this.baseURL = options.BotBaseUrl;

            var authProvider = new AuthenticationProvider(
                options.AppId,
                options.AppSecret,
                this.graphLogger);

            var builder = new StatefulClientBuilder("TeamsIVRBotSample", options.AppId, this.graphLogger);
            builder.SetAuthenticationProvider(authProvider);
            builder.SetNotificationUrl(instanceNotificationUri);
            builder.SetServiceBaseUrl(options.PlaceCallEndpointUrl);

            this.Client = builder.Build();


            this.Client.Calls().OnIncoming += this.CallsOnIncoming;
            this.Client.Calls().OnUpdated += this.CallsOnUpdated;

        }


        /// <summary>
        /// Gets the client.
        /// </summary>
        /// <value>
        /// The client.
        /// </value>
        public IStatefulClient Client { get; }



        /// <summary>
        /// add callback log for diagnostics
        /// </summary>
        /// <param name="message">the message</param>
        public void AddCallbackLog(string message)
        {
            this.callbackLogs.AddFirst(message);
        }


        /// <summary>
        /// Incoming call handler
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="args">The <see cref="CollectionEventArgs{ICall}"/> instance containing the event data.</param>
        private void CallsOnIncoming(ICallCollection sender, CollectionEventArgs<ICall> args)
        {
            Task answerTask;

            var call = args.AddedResources.First();

            var callee = call.Resource.Targets.First();
            var callType = callee?.Identity?.GetApplicationInstance() == null ? CallType.BotIncoming : CallType.BotEndpointIncoming;


            var callHandler = new IncomingCallHandler(this, call);
            this.callHandlers[call.Resource.Id] = callHandler;
            var mediaConfig = new List<MediaInfo>();

            answerTask = call.AnswerAsync(mediaConfig, new[] { Modality.Audio });

            Task.Run(async () =>
            {
                try
                {
                    await answerTask.ConfigureAwait(false);
                    this.graphLogger.Info("Started answering incoming call");
                }
                catch (Exception ex)
                {
                    this.graphLogger.Error(ex, $"Exception happened when answering the call.");
                }
            });
        }

        /// <summary>
        /// Updated call handler.
        /// </summary>
        /// <param name="sender">The <see cref="ICallCollection"/> sender.</param>
        /// <param name="args">The <see cref="CollectionEventArgs{ICall}"/> instance containing the event data.</param>
        private void CallsOnUpdated(ICallCollection sender, CollectionEventArgs<ICall> args)
        {
            foreach (var call in args.RemovedResources)
            {
                if (this.callHandlers.TryRemove(call.Id, out CallHandler handler))
                {
                    handler.Dispose();
                }
            }
        }



    }
}
