namespace ThoughtStuff.TeamsSamples.Http
{
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.WebApiCompatShim;
    using Microsoft.Graph;
    using Microsoft.Graph.Core.Telemetry;
    using System.Collections.Generic;
    using System.Net.Http;
    using System.Threading.Tasks;
    using ThoughtStuff.TeamsSamples.IVRBotSample;

    /// <summary>
    /// Entry point for handling call-related web hook requests from the stateful client
    /// </summary>
    public class PlatformCallController : Controller
    {
        private readonly IGraphLogger graphLogger;

        private readonly Bot bot;

        /// <summary>
        /// Initializes a new instance of the <see cref="PlatformCallController"/> class.
        /// </summary>
        /// <param name="bot">The bot</param>
        public PlatformCallController(Bot bot)
        {
            this.graphLogger = bot.Client.GraphLogger.CreateShim(nameof(PlatformCallController));

            this.bot = bot;
        }

        /// <summary>
        /// This is the entry point from the Teams services for a new call, updates to an existing call etc. Teams calls this API endpoint whenever there is new information to share.
        /// </summary>
        [HttpPost]
        [Route(HttpRouteConstants.OnIncomingRequestRoute)]
        public async Task<IActionResult> OnIncomingRequestAsync()
        {
            var requestMessage = this.Request.HttpContext.GetHttpRequestMessage();

            var requestUri = requestMessage.RequestUri;

            this.bot.AddCallbackLog($"Process incoming request {requestUri}");

            // Pass the incoming message to the Calling SDK, via the bot class.
            var response = await this.bot.Client.ProcessNotificationAsync(requestMessage).ConfigureAwait(false);

            // Convert the status code, content of HttpResponseMessage to IActionResult,
            // and copy the headers from response to HttpContext.Response.Headers.
            return await this.GetActionResultAsync(response).ConfigureAwait(false);
        }


    }
}