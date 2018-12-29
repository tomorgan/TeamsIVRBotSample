// <copyright file="Startup.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>

namespace ThoughtStuff.TeamsSamples
{
    using Microsoft.AspNetCore.Authentication;
    using Microsoft.AspNetCore.Authentication.JwtBearer;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using Microsoft.Graph.Core.Telemetry;
    using ThoughtStuff.TeamsSamples.IVRBotSample;

    /// <summary>
    /// Startup class
    /// </summary>
    public class Startup
    {
        private IGraphLogger graphLogger;

        /// <summary>
        /// Initializes a new instance of the <see cref="Startup"/> class.
        /// </summary>
        /// <param name="configuration">The configuration interface.</param>
        public Startup(IConfiguration configuration)
        {
            this.graphLogger = new GraphLogger(
                component: nameof(Startup),
                redirectToTrace: false);

            this.Configuration = configuration;
        }

        /// <summary>
        /// Gets the configuration.
        /// </summary>
        public IConfiguration Configuration { get; }

        /// <summary>
        /// This method gets called by the runtime. Use this method to add services to the container.
        /// </summary>
        /// <param name="services">The service collection</param>
        public void ConfigureServices(IServiceCollection services)
        {
         
            services.AddSingleton<IGraphLogger>(this.graphLogger);

            services.AddAuthentication(sharedOptions =>
            {
                sharedOptions.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddAzureAdBearer(options => this.Configuration.Bind("AzureAd", options));

            services.AddBot(options => this.Configuration.Bind("Bot", options));

            services.AddCors(options =>
            {
                options.AddPolicy("AllowAll",
                    builder =>
                    {
                        builder
                        .AllowAnyOrigin()
                        .AllowAnyMethod()
                        .AllowAnyHeader()
                        .AllowCredentials();
                    });
            });


            services.AddMvc();
        }

        /// <summary>
        /// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        /// </summary>
        /// <param name="app">The application builder</param>
        /// <param name="env">The hosting environment</param>
        /// <param name="loggerFactory">The logger of ILogger instance</param>
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            // TODO: This should be fixed
            // Casting to concrete type is not ideal
            // We can't ensure that this will always be the right type, so binding may not happen
            (this.graphLogger as GraphLogger)?.BindToILoggerFactory(loggerFactory);

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseStaticFiles();

            app.UseMiddleware<CallAffinityMiddleware>();

            // bypass the user-auth middleware for the incoming request of calls.
            app.UseWhen(
                context => !context.Request.Path.StartsWithSegments(new Microsoft.AspNetCore.Http.PathString(HttpRouteConstants.OnIncomingRequestRoute)),
                appBuilder => appBuilder.UseAuthentication());

            app.UseCors("AllowAll");

            app.UseMvc();
        }
    }
}
