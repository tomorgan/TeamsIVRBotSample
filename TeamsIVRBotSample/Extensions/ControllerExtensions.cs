namespace Microsoft.AspNetCore.Mvc
{
    using Microsoft.Extensions.Primitives;
    using Microsoft.Graph.CoreSDK.Exceptions;
    using System;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Threading.Tasks;

    /// <summary>
    /// The controller exceptions
    /// </summary>
    public static class ControllerExtensions
    {
        /// <summary>
        /// Convert the status code, content of HttpResponseMessage to IActionResult,
        /// and copy the headers from response to HttpContext.Response.Headers.
        /// </summary>
        /// <param name="controller">The HttpResponse instance.</param>
        /// <param name="responseMessage">The HTtpResponseMessage instance.</param>
        /// <returns>The action result</returns>
        public static async Task<IActionResult> GetActionResultAsync(this Controller controller, HttpResponseMessage responseMessage)
        {
            var response = controller.HttpContext.Response;

            controller.CopyResponseHeaders(responseMessage.Headers);

            var statusCode = (int)responseMessage.StatusCode;

            if (responseMessage.Content == null)
            {
                return controller.StatusCode(statusCode);
            }
            else
            {
                var responseBody = await responseMessage.Content.ReadAsStringAsync().ConfigureAwait(false);

                return controller.StatusCode(statusCode, responseBody);
            }
        }


        /// <summary>
        /// Copy the response headers to controller.HttpContext.Response.
        /// </summary>
        /// <param name="controller">The controller</param>
        /// <param name="responseHeaders">The response headers</param>
        private static void CopyResponseHeaders(this Controller controller, HttpResponseHeaders responseHeaders)
        {
            if (responseHeaders == null)
            {
                // do nothing as the source headers are null.
                return;
            }

            foreach (var header in responseHeaders)
            {
                controller.HttpContext.Response.Headers.Add(header.Key, new StringValues(header.Value?.ToArray()));
            }
        }
    }
}
