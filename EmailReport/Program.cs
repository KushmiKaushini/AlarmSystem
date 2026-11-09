using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using AlarmSystem.Configuration;
using AlarmSystem.Controllers;
using AlarmSystem.Services;
using AlarmSystem.Services.EmailReporting;
using AlarmSystem.Services.EmailSender;

namespace EmailReport
{
    public class Program
    {
        public static async Task<int> Main(string[] args)
        {
            Console.WriteLine("===============================================================");
            Console.WriteLine("    SLT NOC - Automated Email Reporting Console Application     ");
            Console.WriteLine("===============================================================");

            // 1. Build Configuration
            var env = Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT") ?? "Production";
            var basePath = AppContext.BaseDirectory;

            var configBuilder = new ConfigurationBuilder()
                .SetBasePath(basePath)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env}.json", optional: true, reloadOnChange: true);

            // Load .env if present in current directory or app base path
            var envPaths = new[]
            {
                Path.Combine(Directory.GetCurrentDirectory(), ".env"),
                Path.Combine(basePath, ".env")
            };

            foreach (var envFile in envPaths)
            {
                if (File.Exists(envFile))
                {
                    var envConfig = new System.Collections.Generic.Dictionary<string, string?>();
                    foreach (var line in File.ReadAllLines(envFile))
                    {
                        var trimmed = line.Trim();
                        if (string.IsNullOrWhiteSpace(trimmed) || trimmed.StartsWith("#")) continue;

                        var separatorIndex = trimmed.IndexOf('=');
                        if (separatorIndex > 0)
                        {
                            var key = trimmed.Substring(0, separatorIndex).Trim().Replace("__", ":");
                            var val = trimmed.Substring(separatorIndex + 1).Trim().Trim('"', '\'');
                            if (!string.IsNullOrEmpty(val))
                            {
                                envConfig[key] = val;
                            }
                        }
                    }
                    if (envConfig.Count > 0)
                    {
                        configBuilder.AddInMemoryCollection(envConfig);
                    }
                    break;
                }
            }

            configBuilder.AddEnvironmentVariables();
            var configuration = configBuilder.Build();

            // 2. Parse Command-Line Arguments
            string? command = null;
            string? reportTypeArg = null;
            string? outputDirArg = null;

            for (int i = 0; i < args.Length; i++)
            {
                var arg = args[i].ToLowerInvariant();
                if (arg == "--help" || arg == "-h")
                {
                    PrintHelp();
                    return 0;
                }
                if (arg == "--type" || arg == "-t")
                {
                    command = "send";
                    if (i + 1 < args.Length) reportTypeArg = args[++i];
                }
                else if (arg == "--preview" || arg == "-p")
                {
                    command = "preview";
                    if (i + 1 < args.Length) reportTypeArg = args[++i];
                }
                else if (arg == "--export" || arg == "-e")
                {
                    command = "export";
                    if (i + 1 < args.Length) reportTypeArg = args[++i];
                }
                else if (arg == "--generate-all" || arg == "--docd" || arg == "-g")
                {
                    command = "generate-all";
                }
                else if (arg == "--out" || arg == "-o")
                {
                    if (i + 1 < args.Length) outputDirArg = args[++i];
                }
                else if (arg == "--run-now" || arg == "-r")
                {
                    command = "send";
                    reportTypeArg = "all";
                }
            }

            // 3. Build Service Collection
            var services = new ServiceCollection();
            services.AddSingleton<IConfiguration>(configuration);
            services.AddLogging(builder =>
            {
                builder.AddConfiguration(configuration.GetSection("Logging"));
                builder.AddConsole();
            });

            // SQL Connection
            services.AddScoped<SqlConnection>(_ =>
            {
                var connStr = configuration.GetConnectionString("NewDB")
                           ?? configuration.GetConnectionString("TMS")
                           ?? configuration.GetConnectionString("DefaultConnection");
                return new SqlConnection(connStr);
            });

            // Email Reporting Configuration
            services.Configure<EmailReportingOptions>(configuration.GetSection("EmailReporting"));

            // Register Services
            services.AddScoped<FaultReportService>();
            services.AddScoped<DailyEmailReportController>();
            services.AddSingleton<IEmailSenderService, EmailSenderService>();

            var serviceProvider = services.BuildServiceProvider();
            var logger = serviceProvider.GetRequiredService<ILogger<Program>>();

            // 4. Handle Direct CLI Execution (Manual / Task Scheduler Mode)

            // 4.1 Generate All (Both HTML & Excel for all 4 types into target folder)
            if (command == "generate-all")
            {
                var targetFolder = outputDirArg ?? Path.Combine(Directory.GetCurrentDirectory(), "docd");
                if (!Directory.Exists(targetFolder)) Directory.CreateDirectory(targetFolder);

                Console.WriteLine($"[i] Generating all 4 reports (HTML + Excel) into: {targetFolder}");
                using var scope = serviceProvider.CreateScope();
                var faultService = scope.ServiceProvider.GetRequiredService<FaultReportService>();
                var controller = scope.ServiceProvider.GetRequiredService<DailyEmailReportController>();

                for (int t = 1; t <= 4; t++)
                {
                    try
                    {
                        // Generate HTML
                        var html = await faultService.GetEmailPreviewHtmlAsync(t);
                        var htmlFileName = t switch
                        {
                            1 => "Report_Type1_Regional_Network_Faults.html",
                            2 => "Report_Type2_OPMC_Faults.html",
                            3 => "Report_Type3_SLT_NOC_Fault_Management.html",
                            _ => "Report_Type4_Comprehensive_Network_Failures.html"
                        };
                        var htmlPath = Path.Combine(targetFolder, htmlFileName);
                        await File.WriteAllTextAsync(htmlPath, html);
                        Console.WriteLine($"[+] Generated HTML: {htmlFileName}");

                        // Generate Excel
                        var excelPath = await controller.GenerateExcelFileAsync(t, outputDirectory: targetFolder);
                        Console.WriteLine($"[+] Generated Excel: {Path.GetFileName(excelPath)}");
                    }
                    catch (Exception ex)
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine($"[-] Error generating Type {t}: {ex.Message}");
                        Console.ResetColor();
                    }
                }

                Console.WriteLine($"\n[✓] All reports successfully generated in: {targetFolder}\n");
                return 0;
            }

            // 4.2 Preview HTML
            if (command == "preview")
            {
                int type = int.TryParse(reportTypeArg, out var t) ? t : 1;
                using var scope = serviceProvider.CreateScope();
                var faultService = scope.ServiceProvider.GetRequiredService<FaultReportService>();
                var html = await faultService.GetEmailPreviewHtmlAsync(type);
                var targetDir = outputDirArg ?? Directory.GetCurrentDirectory();
                if (!Directory.Exists(targetDir)) Directory.CreateDirectory(targetDir);
                var outPath = Path.Combine(targetDir, $"Report_Type{type}_Preview.html");
                await File.WriteAllTextAsync(outPath, html);
                Console.WriteLine($"[+] Successfully generated HTML preview file: {outPath}");
                return 0;
            }

            // 4.3 Export Excel
            if (command == "export")
            {
                using var scope = serviceProvider.CreateScope();
                var controller = scope.ServiceProvider.GetRequiredService<DailyEmailReportController>();
                var targetDir = outputDirArg ?? Directory.GetCurrentDirectory();

                int[] typesToExport = reportTypeArg?.ToLowerInvariant() switch
                {
                    "1" => new[] { 1 },
                    "2" => new[] { 2 },
                    "3" => new[] { 3 },
                    "4" => new[] { 4 },
                    _ => new[] { 1, 2, 3, 4 }
                };

                foreach (var t in typesToExport)
                {
                    var excelPath = await controller.GenerateExcelFileAsync(t, outputDirectory: targetDir);
                    Console.WriteLine($"[+] Generated Excel file: {excelPath}");
                }
                return 0;
            }

            // 4.4 Send Emails
            if (command == "send")
            {
                using var scope = serviceProvider.CreateScope();
                var emailSender = scope.ServiceProvider.GetRequiredService<IEmailSenderService>();
                var recipientsConfig = configuration.GetSection("EmailReporting:Recipients");
                var defaultRecipient = configuration["EmailReporting:DefaultRecipient"]
                                     ?? recipientsConfig["NetworkEngineer"]
                                     ?? recipientsConfig["Default"]
                                     ?? "network.engineer@slt.com.lk";

                int[] typesToSend = reportTypeArg?.ToLowerInvariant() switch
                {
                    "1" => new[] { 1 },
                    "2" => new[] { 2 },
                    "3" => new[] { 3 },
                    "4" => new[] { 4 },
                    _ => new[] { 1, 2, 3, 4 }
                };

                Console.WriteLine($"[i] Manually dispatching Report Types: {string.Join(", ", typesToSend)}");
                var today = DateTime.Today;

                foreach (var type in typesToSend)
                {
                    var recipients = recipientsConfig[type.ToString()]
                                  ?? recipientsConfig[$"Type{type}"]
                                  ?? defaultRecipient;

                    if (string.IsNullOrWhiteSpace(recipients))
                    {
                        Console.WriteLine($"[!] Skipping Type {type}: No recipient configured.");
                        continue;
                    }

                    try
                    {
                        Console.WriteLine($"[>] Sending Report Type {type} to: {recipients}...");
                        await emailSender.SendReportEmailAsync(type, today, recipients);
                        Console.WriteLine($"[+] Successfully sent Report Type {type}!");
                    }
                    catch (Exception ex)
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine($"[-] Failed to send Report Type {type}: {ex.Message}");
                        Console.ResetColor();
                    }
                }

                return 0;
            }

            // 5. Default Mode: Run Hosted Background Scheduler Daemon
            Console.WriteLine("[i] No immediate action specified. Launching continuous background scheduler daemon...");
            Console.WriteLine("[i] Press Ctrl+C to stop the service.\n");

            var host = Host.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration((context, builder) =>
                {
                    builder.AddConfiguration(configuration);
                })
                .ConfigureServices((hostContext, hostServices) =>
                {
                    hostServices.AddSingleton<IConfiguration>(configuration);
                    hostServices.AddScoped<SqlConnection>(_ =>
                    {
                        var connStr = configuration.GetConnectionString("NewDB")
                                   ?? configuration.GetConnectionString("TMS")
                                   ?? configuration.GetConnectionString("DefaultConnection");
                        return new SqlConnection(connStr);
                    });
                    hostServices.Configure<EmailReportingOptions>(configuration.GetSection("EmailReporting"));
                    hostServices.AddScoped<FaultReportService>();
                    hostServices.AddScoped<DailyEmailReportController>();
                    hostServices.AddSingleton<IEmailSenderService, EmailSenderService>();
                    hostServices.AddHostedService<DailyEmailReportSchedulerService>();
                })
                .Build();

            await host.RunAsync();
            return 0;
        }

        private static void PrintHelp()
        {
            Console.WriteLine("Usage: dotnet run [options]");
            Console.WriteLine();
            Console.WriteLine("Options:");
            Console.WriteLine("  (no args)                 Run as continuous background scheduler daemon");
            Console.WriteLine("  -t, --type <1|2|3|4|all>  Immediately dispatch specific or all reports via SMTP");
            Console.WriteLine("  -r, --run-now             Alias for --type all (dispatch all 4 reports now)");
            Console.WriteLine("  -p, --preview <1..4>      Generate an HTML preview file of the specified report");
            Console.WriteLine("  -e, --export <1..4|all>   Generate Excel (.xlsx) file of the specified report");
            Console.WriteLine("  -g, --generate-all        Generate all 4 HTML & Excel reports into 'docd/'");
            Console.WriteLine("  -o, --out <directory>     Specify custom output directory for generated files");
            Console.WriteLine("  -h, --help                Show this help dialog");
            Console.WriteLine();
            Console.WriteLine("Examples:");
            Console.WriteLine("  dotnet run                    # Run background scheduler service");
            Console.WriteLine("  dotnet run -- --generate-all  # Generate all 4 HTML and Excel reports");
            Console.WriteLine("  dotnet run -- --export all    # Generate all Excel spreadsheets");
            Console.WriteLine("  dotnet run -- --type 1        # Dispatch Type 1 report immediately");
            Console.WriteLine("  dotnet run -- --preview 4     # Generate HTML preview for Type 4 report");
        }
    }
}
