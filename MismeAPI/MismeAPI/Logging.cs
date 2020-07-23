namespace MismeAPI
{
    public static class Logging
    {
        //public static void SetupAWSLogging(WebHostBuilderContext hostingContext, LoggerConfiguration loggerConfiguration)
        //{
        //    var config = hostingContext.Configuration;
        //    var settings = config.GetSection("LoggingSettings").Get<LoggingSettings>();

        // loggerConfiguration .MinimumLevel.Information() .Enrich.FromLogContext() .WriteTo.Console();

        // if (!string.IsNullOrEmpty(settings.CloudWatchLogGroup)) { var options = new
        // CloudWatchSinkOptions { LogGroupName = settings.CloudWatchLogGroup, CreateLogGroup =
        // true, MinimumLogEventLevel = LogEventLevel.Information, TextFormatter = new
        // CompactJsonFormatter() }; var awsOptions = config.GetAWSOptions(); var cloudwatchClient =
        // awsOptions.CreateServiceClient<IAmazonCloudWatchLogs>(); loggerConfiguration
        // .WriteTo.AmazonCloudWatch(options, cloudwatchClient); }

        //}
    }
}