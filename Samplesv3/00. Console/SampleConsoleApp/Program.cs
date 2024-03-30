using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Text.Json;

internal class Program
{
    internal static readonly ActivitySource ActivitySource = new(typeof(Program).Namespace!);
    private static readonly JsonSerializerOptions MyJsonSerializerOptions = new(JsonSerializerOptions.Default) { ReadCommentHandling = JsonCommentHandling.Skip };

    private readonly ILogger logger;
    private readonly IConfiguration configuration;
    private readonly ILoggerFactory loggerFactory;
    private readonly IFileProvider fileProvider;

    public Program(ILogger<Program> logger, IConfiguration configuration, ILoggerFactory loggerFactory, IFileProvider fileProvider)
    {
        this.logger = logger;
        this.configuration = configuration;
        this.loggerFactory = loggerFactory;
        this.fileProvider = fileProvider;
    }

    private static async Task Main(string[] args)
    {
        //// See https://aka.ms/new-console-template for more information
        Console.WriteLine("Hello, World, from sample console app!");

        return;
    }
}