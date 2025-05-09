using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using SendMail.NET.Core.Pipeline;
using SendMail.NET.Core.Pipeline.Steps;
using SendMail.NET.Core.Providers;
using SendMail.NET.Core.Services;

namespace SendMail.NET.Core.Extensions
{
    /// <summary>
    /// Builder for configuring SendMail.NET services.
    /// </summary>
    public class SendMailBuilder
    {
        /// <summary>
        /// Gets the service collection.
        /// </summary>
        public IServiceCollection Services { get; }
        private readonly List<Type> _pipelineSteps;
        private readonly EmailProviderOptions _providerOptions;

        /// <summary>
        /// Initializes a new instance of the SendMailBuilder class.
        /// </summary>
        /// <param name="services">The service collection.</param>
        public SendMailBuilder(IServiceCollection services)
        {
            Services = services;
            _pipelineSteps = new List<Type>();
            _providerOptions = new EmailProviderOptions();
        }

        /// <summary>
        /// Adds a pipeline step to the email pipeline.
        /// </summary>
        /// <typeparam name="TStep">The type of the pipeline step.</typeparam>
        /// <returns>The builder instance.</returns>
        public SendMailBuilder AddPipelineStep<TStep>() where TStep : class, IEmailPipelineStep
        {
            Services.AddScoped<IEmailPipelineStep, TStep>();
            _pipelineSteps.Add(typeof(TStep));
            return this;
        }

        /// <summary>
        /// Adds a pipeline step instance to the email pipeline.
        /// </summary>
        /// <param name="step">The pipeline step instance.</param>
        /// <returns>The builder instance.</returns>
        public SendMailBuilder AddPipelineStep(IEmailPipelineStep step)
        {
            Services.AddSingleton(step);
            _pipelineSteps.Add(step.GetType());
            return this;
        }

        /// <summary>
        /// Configures the default pipeline with validation and sending steps.
        /// </summary>
        /// <returns>The builder instance.</returns>
        public SendMailBuilder UseDefaultPipeline()
        {
            // Add default pipeline steps in order
            AddPipelineStep<ValidationStep>();
            //AddPipelineStep<TemplateCompilationStep>();
            //AddPipelineStep<ProviderSelectionStep>();
            AddPipelineStep<SendingStep>();
            //AddPipelineStep<LoggingStep>();
            return this;
        }

        /// <summary>
        /// Adds an email provider to the service collection.
        /// </summary>
        /// <typeparam name="TProvider">The type of the provider.</typeparam>
        /// <param name="configure">The configuration action.</param>
        /// <returns>The builder instance.</returns>
        public SendMailBuilder AddProvider<TProvider>(Action<ProviderConfig> configure) 
            where TProvider : class, IEmailProvider
        {
            Services.AddScoped<IEmailProvider, TProvider>();
            
            var config = new ProviderConfig();
            configure(config);
            _providerOptions.Providers.Add(config);
            
            return this;
        }

        /// <summary>
        /// Configures the provider options.
        /// </summary>
        /// <param name="configure">The configuration action.</param>
        /// <returns>The builder instance.</returns>
        public SendMailBuilder ConfigureProviders(Action<EmailProviderOptions> configure)
        {
            configure(_providerOptions);
            return this;
        }

        /// <summary>
        /// Builds the SendMail.NET services.
        /// </summary>
        public void Build()
        {
            Services.Configure<EmailProviderOptions>(options =>
            {
                options.Providers = _providerOptions.Providers;
                options.EnableFallback = _providerOptions.EnableFallback;
                options.MaxRetries = _providerOptions.MaxRetries;
            });

            Services.AddScoped<IEmailProviderManager, EmailProviderManager>();
        }
    }

    /// <summary>
    /// Extension methods for configuring SendMail.NET services.
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Adds SendMail.NET services to the service collection.
        /// </summary>
        /// <param name="services">The service collection.</param>
        /// <param name="configure">The configuration action.</param>
        /// <returns>The builder instance.</returns>
        /// <exception cref="ArgumentNullException">Thrown when services or configure is null.</exception>
        public static SendMailBuilder AddSendMail(this IServiceCollection services, Action<SendMailBuilder> configure)
        {
            if (services == null)
                throw new ArgumentNullException(nameof(services));

            if (configure == null)
                throw new ArgumentNullException(nameof(configure));

            // Register core services
            services.AddScoped<EmailPipeline>();
            services.AddScoped<IEmailService, EmailService>();

            var builder = new SendMailBuilder(services);
            configure(builder);
            builder.Build();

            return builder;
        }
    }
} 