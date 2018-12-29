namespace ThoughtStuff.TeamsSamples.IVRBotSample
{
    using System;

    /// <summary>
    /// The bot options class
    /// </summary>
    public class BotOptions
    {
        /// <summary>
        /// Gets or sets the application id
        /// </summary>
        public string AppId { get; set; }

        /// <summary>
        /// Gets or sets the application secret
        /// </summary>
        public string AppSecret { get; set; }

        /// <summary>
        /// Gets or sets the calls uri of the application
        /// </summary>
        public Uri BotBaseUrl { get; set; }

        /// <summary>
        /// Gets or sets the comms platform endpoint uri
        /// </summary>
        public Uri PlaceCallEndpointUrl { get; set; }

        /// <summary>
        /// Gets or sets the auth token audience
        /// </summary>
        public string AuthTokenAudience { get; set; }
    }
}
