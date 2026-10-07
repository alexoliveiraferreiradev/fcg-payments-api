using Serilog;

namespace Fcg.Payments.API.Extensions
{
    public static class ObservabilityExtensions
    {
        public static WebApplicationBuilder AddObservabilityExtension(this WebApplicationBuilder builder )
        {
            builder.AddSerilogExtension();
            return builder;
        }
        private static WebApplicationBuilder AddSerilogExtension(this WebApplicationBuilder builder)
        {
            builder.Host.UseSerilog((context, configuration) =>
            {
                configuration.ReadFrom.Configuration(context.Configuration);
            });

            return builder;
        }
    }
}
