using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using SendMail.NET.Core.Pipeline;
using SendMail.NET.Core.Pipeline.Steps;
using SendMail.NET.Core.Providers;
using SendMail.NET.Core.Services;

namespace SendMail.NET.Core.Extensions
{
    public class SendMailBuilder
    {
        public IServiceCollection Services { get; }
        private readonly List<Type> _pipelineSteps;
        private readonly EmailProviderOptions _providerOptions;

        public SendMailBuilder(IServiceCollection services)
        {
            Services = services;
            _pipelineSteps = new List<Type>();
            _providerOptions = new EmailProviderOptions();
        }

        public SendMailBuilder AddPipelineStep<TStep>() where TStep : class, IEmailPipelineStep
        {
            Services.AddScoped<IEmailPipelineStep, TStep>();
            _pipelineSteps.Add(typeof(TStep));
            return this;
        }

        public SendMailBuilder AddPipelineStep(IEmailPipelineStep step)
        {
            Services.AddSingleton(step);
            _pipelineSteps.Add(step.GetType());
            return this;
        }

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

        public SendMailBuilder AddProvider<TProvider>(Action<ProviderConfig> configure) 
            where TProvider : class, IEmailProvider
        {
            Services.AddScoped<IEmailProvider, TProvider>();
            
            var config = new ProviderConfig();
            configure(config);
            _providerOptions.Providers.Add(config);
            
            return this;
        }

        public SendMailBuilder ConfigureProviders(Action<EmailProviderOptions> configure)
        {
            configure(_providerOptions);
            return this;
        }

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

    public static class ServiceCollectionExtensions
    {
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