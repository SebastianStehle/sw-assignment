
using MongoDB.Driver;
using Pipeline.Services;
using Pipeline.Services.Middlewares.ErrorHandling;
using Pipeline.Services.Middlewares.Metrics;
using Pipeline.Services.Middlewares.Store;
using Pipeline.Services.Sources.FileSystem;
using Pipeline.Services.Steps.ArchiveStep;
using Pipeline.Services.Steps.Cleanup;
using Pipeline.Services.Steps.CopyFile;
using Pipeline.Services.Steps.Deletion;
using Pipeline.Services.Steps.ExtractMetadata;
using Pipeline.Services.Steps.OptimizeGlb;
using Pipeline.Services.Steps.Save;

namespace Pipeline
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            ConfigureServices(builder.Services, builder.Configuration);

            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseAuthorization();
            app.MapControllers();
            app.Run();
        }

        private static void ConfigureServices(IServiceCollection services, IConfiguration config)
        {
            services.Configure<SaveToDiskOptions>(
                config.GetSection("SaveToDisk"));

            services.Configure<FileSystemOptions>(
                config.GetSection("Source"));

            services.AddSingleton<IMetadataStore, MongoDbMetadataStore>();

            services.AddSingleton<IMongoClient>(c => new MongoClient(config.GetValue<string>("MongoDB:Configuration")));
            services.AddSingleton<IMongoDatabase>(c => c.GetRequiredService<IMongoClient>().GetDatabase(config.GetValue<string>("MongoDB:DatabaseName")));
            services.AddSingleton<PipelineRunner>();
            services.AddSingleton<IHostedService>(c => c.GetRequiredService<PipelineRunner>());

            services.AddSingleton<IDataSource, FileSystemDataSource>();

            services.AddSingleton<IPipelineMiddleware>(c => (IPipelineMiddleware)c.GetRequiredService<IMetadataStore>());
            services.AddSingleton<IPipelineMiddleware, ErrorHandlingMiddleware>();
            services.AddSingleton<IPipelineMiddleware, MetricsMiddleware>();

            services.AddSingleton<IPipelineStep, CopyFileStep>();
            services.AddSingleton<IPipelineStep, ExtractMetadataStep>();
            services.AddSingleton<IPipelineStep, ArchiveStep>();
            services.AddSingleton<IPipelineStep, OptimizeGlbStep>();
            services.AddSingleton<IPipelineStep, CalculateCompressionRateStep>();
            services.AddSingleton<IPipelineStep, SaveToDiskStep>();
            services.AddSingleton<IPipelineStep, DeleteStep>();
            services.AddSingleton<IPipelineStep, CleanupStep>();

            services.AddSingleton<IPipelineRunner>(c => c.GetRequiredService<PipelineRunner>());
        }
    }
}
