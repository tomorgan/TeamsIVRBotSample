namespace Microsoft.AspNetCore.Authentication
{
    /// <summary>
    /// The Azure AD options class
    /// </summary>
    public class AzureAdOptions
    {
        /// <summary>
        /// Gets or sets the application id as auth client id.
        /// </summary>
        public string AppId { get; set; }

        /// <summary>
        /// Gets or sets the application secret as auth client secret.
        /// </summary>
        public string AppSecret { get; set; }

        /// <summary>
        /// Gets or sets the instance
        /// </summary>
        public string Instance { get; set; }

        /// <summary>
        /// Gets or sets the domain
        /// </summary>
        public string Domain { get; set; }

        /// <summary>
        /// Gets or sets the tenant id
        /// </summary>
        public string TenantId { get; set; }
    }
}
