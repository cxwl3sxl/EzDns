using System.Diagnostics;
using System.Reflection;

namespace EzDns.WebApi;

/// <summary>
/// Handles service management commands (install, uninstall, status, start, stop).
/// Windows: uses sc.exe
/// Linux: uses systemctl
/// </summary>
public static class ServiceCommands
{
    private const string ServiceName = "EzDns";
    private const string DisplayName = "EzDns Server";
    private const string Description = "EzDns DNS Management Server";

    /// <summary>
    /// Attempts to handle a service command from the given args.
    /// Returns true if a known command was handled (application should exit),
    /// false if no command matched (application should continue normal startup).
    /// </summary>
    public static bool HandleCommand(string[] args)
    {
        if (args.Length == 0) return false;

        var command = args[0].ToLowerInvariant();

        switch (command)
        {
            case "install":
                RunInstall();
                return true;

            case "uninstall":
                RunUninstall();
                return true;

            case "status":
                RunStatus();
                return true;

            case "start":
                RunStart();
                return true;

            case "stop":
                RunStop();
                return true;

            case "help":
            case "--help":
            case "-h":
                ShowHelp();
                return true;

            default:
                Console.Error.WriteLine($"Unknown command: '{args[0]}'");
                Console.Error.WriteLine($"Run '{GetExecutableName()} help' to see available commands.");
                return true;
        }
    }

    /// <summary>
    /// Displays available service commands.
    /// </summary>
    private static void ShowHelp()
    {
        var exeName = GetExecutableName();
        WriteLineColor("EzDns Web API - Service Management", ConsoleColor.Cyan);
        Console.WriteLine();
        Console.WriteLine("Usage:");
        Console.WriteLine($"  {exeName}             Start the application in console mode");
        Console.WriteLine($"  {exeName} install      Install as a system service");
        Console.WriteLine($"  {exeName} uninstall    Uninstall the system service");
        Console.WriteLine($"  {exeName} status       Show service status");
        Console.WriteLine($"  {exeName} start        Start the service");
        Console.WriteLine($"  {exeName} stop         Stop the service");
        Console.WriteLine($"  {exeName} help         Show this help message");
        Console.WriteLine();
        WriteLineColor("Platform Support:", ConsoleColor.Yellow);
        Console.WriteLine("  Windows  : Uses sc.exe (requires Administrator privileges)");
        Console.WriteLine("  Linux    : Uses systemd (requires sudo privileges)");
    }

    // ── Cross-platform dispatch ──────────────────────────────────────────────

    private static void RunInstall()
    {
        if (OperatingSystem.IsWindows())
            InstallWindows();
        else if (OperatingSystem.IsLinux())
            InstallLinux();
        else
            Console.Error.WriteLine("Unsupported operating system. Only Windows and Linux are supported.");
    }

    private static void RunUninstall()
    {
        if (OperatingSystem.IsWindows())
            UninstallWindows();
        else if (OperatingSystem.IsLinux())
            UninstallLinux();
        else
            Console.Error.WriteLine("Unsupported operating system. Only Windows and Linux are supported.");
    }

    private static void RunStatus()
    {
        if (OperatingSystem.IsWindows())
            QueryStatusWindows();
        else if (OperatingSystem.IsLinux())
            QueryStatusLinux();
        else
            Console.Error.WriteLine("Unsupported operating system. Only Windows and Linux are supported.");
    }

    private static void RunStart()
    {
        if (OperatingSystem.IsWindows())
            ControlServiceWindows("start");
        else if (OperatingSystem.IsLinux())
            ControlServiceLinux("start");
        else
            Console.Error.WriteLine("Unsupported operating system. Only Windows and Linux are supported.");
    }

    private static void RunStop()
    {
        if (OperatingSystem.IsWindows())
            ControlServiceWindows("stop");
        else if (OperatingSystem.IsLinux())
            ControlServiceLinux("stop");
        else
            Console.Error.WriteLine("Unsupported operating system. Only Windows and Linux are supported.");
    }

    // ── Windows implementation (sc.exe) ──────────────────────────────────────

    private static void InstallWindows()
    {
        Console.WriteLine($"Installing Windows service '{ServiceName}'...");

        var binPath = GetWindowsBinPath();
        var args = $"create {ServiceName} binPath={binPath} start=auto DisplayName=\"{DisplayName}\"";

        var result = RunProcess("sc.exe", args);

        if (result.ExitCode == 0)
        {
            // Set description
            RunProcess("sc.exe", $"description {ServiceName} \"{Description}\"");
            WriteLineColor($"Service '{ServiceName}' installed successfully.", ConsoleColor.Green);
            Console.WriteLine($"Start the service with: {GetExecutableName()} start");
        }
        else
        {
            Console.Error.WriteLine($"Failed to install service. Exit code: {result.ExitCode}");
            Console.Error.WriteLine(result.Stderr);
        }
    }

    /// <summary>
    /// Builds the correct binPath argument for sc.exe.
    /// Handles both framework-dependent (dotnet host) and self-contained deployments.
    /// sc.exe format: binPath= "escaped command line"
    ///   - Outer quotes wrapped by the binPath argument for sc.exe
    ///   - Inner quotes must be escaped as \" for the SCM command line
    ///
    /// For framework-dependent (dotnet host):
    ///     sc.exe expects: binPath= "\"C:\path\to\dotnet.exe\" \"C:\path\to\app.dll\""
    ///
    /// For self-contained with spaces in path:
    ///     sc.exe expects: binPath= "\"C:\Program Files\app.exe\""
    ///
    /// For self-contained without spaces:
    ///     sc.exe expects: binPath= "C:\path\to\app.exe"
    /// </summary>
    private static string GetWindowsBinPath()
    {
        var processPath = Environment.ProcessPath
            ?? throw new InvalidOperationException("Cannot determine process path.");
        var fileName = Path.GetFileName(processPath);

        var isDotnetHost = fileName.Equals("dotnet", StringComparison.OrdinalIgnoreCase) ||
                           fileName.Equals("dotnet.exe", StringComparison.OrdinalIgnoreCase);

        if (isDotnetHost)
        {
            // Framework-dependent: dotnet.exe "path\to\app.dll"
            var dllPath = Assembly.GetExecutingAssembly().Location;
            // C# string: $"\"\\\"{p}\\\" \\\"{dll}\\\"\"" produces:
            // "\"C:\path\to\dotnet.exe\" \"C:\path\to\app.dll\""
            return $"\"\\\"{processPath}\\\" \\\"{dllPath}\\\"\"";
        }

        // Self-contained: full path to executable
        // C# string: $"\"{processPath}\""  wraps path in quotes for sc.exe
        // For paths with spaces, also escape inner quotes:  $"\"\\\"{p}\\\"\""
        return processPath.Contains(' ')
            ? $"\"\\\"{processPath}\\\"\""
            : $"\"{processPath}\"";
    }

    private static void UninstallWindows()
    {
        Console.WriteLine($"Uninstalling Windows service '{ServiceName}'...");

        // Check if service exists before stopping
        var queryResult = RunProcess("sc.exe", $"query {ServiceName}");
        if (queryResult.ExitCode != 0)
        {
            if (queryResult.Stderr.Contains("1060"))
            {
                WriteLineColor($"Service '{ServiceName}' is not installed.", ConsoleColor.Yellow);
                return;
            }
        }

        // Stop the service first
        StopServiceSilentlyWindows();

        var result = RunProcess("sc.exe", $"delete {ServiceName}");
        if (result.ExitCode == 0)
        {
            WriteLineColor($"Service '{ServiceName}' uninstalled successfully.", ConsoleColor.Green);
        }
        else
        {
            Console.Error.WriteLine($"Failed to uninstall service. Exit code: {result.ExitCode}");
            Console.Error.WriteLine(result.Stderr);
        }
    }

    /// <summary>
    /// Stops the service without printing errors if it's not running or not installed.
    /// </summary>
    private static void StopServiceSilentlyWindows()
    {
        var result = RunProcess("sc.exe", $"stop {ServiceName}");
        if (result.ExitCode == 0)
        {
            Console.WriteLine($"Service '{ServiceName}' stopped.");
        }
        // Silently ignore if already stopped or not installed
    }

    private static void QueryStatusWindows()
    {
        var result = RunProcess("sc.exe", $"query {ServiceName}");
        if (result.ExitCode == 0)
        {
            WriteLineColor("── Windows Service Status ──────────────────────", ConsoleColor.Cyan);
            foreach (var line in result.Stdout.Split(Environment.NewLine))
            {
                var trimmed = line.Trim();
                if (trimmed.StartsWith("SERVICE_NAME") ||
                    trimmed.StartsWith("DISPLAY_NAME") ||
                    trimmed.StartsWith("STATE") ||
                    trimmed.StartsWith("TYPE"))
                {
                    WriteLineColor(trimmed, ConsoleColor.White);
                }
            }
        }
        else if (result.Stderr.Contains("1060"))
        {
            WriteLineColor($"Service '{ServiceName}' is not installed.", ConsoleColor.Yellow);
        }
        else
        {
            Console.Error.WriteLine($"Failed to query service status. Exit code: {result.ExitCode}");
            Console.Error.WriteLine(result.Stderr);
        }
    }

    private static void ControlServiceWindows(string action)
    {
        Console.WriteLine($"'{action}' service '{ServiceName}'...");
        var result = RunProcess("sc.exe", $"{action} {ServiceName}");

        if (result.ExitCode == 0)
        {
            WriteLineColor($"Service '{ServiceName}' {action}ed successfully.", ConsoleColor.Green);
        }
        else
        {
            var stderr = result.Stderr;
            if (stderr.Contains("1060"))
                Console.Error.WriteLine($"Service '{ServiceName}' is not installed.");
            else if (stderr.Contains("1056") || stderr.Contains("1053"))
                Console.Error.WriteLine($"Service '{ServiceName}' is already transitioning.");
            else if (stderr.Contains("1062"))
                Console.Error.WriteLine($"Service '{ServiceName}' has already been {action}ed.");
            else
            {
                Console.Error.WriteLine($"Failed to {action} service. Exit code: {result.ExitCode}");
                Console.Error.WriteLine(stderr);
            }
        }
    }

    // ── Linux implementation (systemctl) ────────────────────────────────────

    private static void InstallLinux()
    {
        Console.WriteLine($"Installing systemd service '{ServiceName}'...");

        var execPath = GetLinuxExecStart();
        var unitContent = $@"[Unit]
Description={Description}
After=network.target

[Service]
Type=notify
ExecStart={execPath}
Restart=on-failure
RestartSec=5
Environment=ASPNETCORE_ENVIRONMENT=Production
Environment=DOTNET_ENVIRONMENT=Production

[Install]
WantedBy=multi-user.target
";

        var unitPath = $"/etc/systemd/system/{ServiceName}.service";
        var tempFile = Path.GetTempFileName();

        try
        {
            File.WriteAllText(tempFile, unitContent);

            var copyResult = RunProcess("sudo", $"cp \"{tempFile}\" \"{unitPath}\"");
            if (copyResult.ExitCode != 0)
            {
                Console.Error.WriteLine("Failed to copy systemd unit file. Do you have sudo privileges?");
                Console.Error.WriteLine(copyResult.Stderr);
                return;
            }

            RunProcess("sudo", "systemctl daemon-reload");
            var enableResult = RunProcess("sudo", $"systemctl enable {ServiceName}.service");

            if (enableResult.ExitCode == 0)
            {
                WriteLineColor($"Service '{ServiceName}' installed and enabled successfully.", ConsoleColor.Green);
                Console.WriteLine($"Start the service with: {GetExecutableName()} start");
            }
            else
            {
                Console.Error.WriteLine("Failed to enable service.");
                Console.Error.WriteLine(enableResult.Stderr);
            }
        }
        finally
        {
            if (File.Exists(tempFile))
                File.Delete(tempFile);
        }
    }

    /// <summary>
    /// Returns the full ExecStart path for the systemd unit file.
    /// Uses Environment.ProcessPath directly (already the full path on both
    /// framework-dependent and self-contained deployments).
    /// </summary>
    private static string GetLinuxExecStart()
    {
        var processPath = Environment.ProcessPath
            ?? throw new InvalidOperationException("Cannot determine process path.");
        var fileName = Path.GetFileName(processPath);

        var isDotnetHost = fileName.Equals("dotnet", StringComparison.OrdinalIgnoreCase) ||
                           fileName.Equals("dotnet.exe", StringComparison.OrdinalIgnoreCase);

        if (isDotnetHost)
        {
            // Framework-dependent: Environment.ProcessPath is the full path to dotnet
            var dllPath = Assembly.GetExecutingAssembly().Location;
            return $"{processPath} \"{dllPath}\"";
        }

        // Self-contained: the native binary path
        return processPath;
    }

    private static void UninstallLinux()
    {
        Console.WriteLine($"Uninstalling systemd service '{ServiceName}'...");

        ControlServiceLinux("stop");
        RunProcess("sudo", $"systemctl disable {ServiceName}.service");

        var unitPath = $"/etc/systemd/system/{ServiceName}.service";
        var removeResult = RunProcess("sudo", $"rm -f \"{unitPath}\"");

        if (removeResult.ExitCode == 0)
        {
            RunProcess("sudo", "systemctl daemon-reload");
            WriteLineColor($"Service '{ServiceName}' uninstalled successfully.", ConsoleColor.Green);
        }
        else
        {
            Console.Error.WriteLine("Failed to remove systemd unit file.");
            Console.Error.WriteLine(removeResult.Stderr);
        }
    }

    private static void QueryStatusLinux()
    {
        var result = RunProcess("systemctl", $"status {ServiceName}.service --no-pager");

        if (result.ExitCode == 0 || result.ExitCode == 3) // 3 = inactive/dead
        {
            WriteLineColor("── Linux Service Status (systemctl) ─────────────", ConsoleColor.Cyan);
            Console.WriteLine(result.Stdout.Trim());
        }
        else if (result.Stderr.Contains("could not be found"))
        {
            WriteLineColor($"Service '{ServiceName}' is not installed.", ConsoleColor.Yellow);
        }
        else
        {
            Console.Error.WriteLine($"Failed to query service status. Exit code: {result.ExitCode}");
            if (!string.IsNullOrEmpty(result.Stderr))
                Console.Error.WriteLine(result.Stderr);
        }
    }

    private static void ControlServiceLinux(string action)
    {
        Console.WriteLine($"'{action}' service '{ServiceName}'...");
        var result = RunProcess("sudo", $"systemctl {action} {ServiceName}.service");

        if (result.ExitCode == 0)
        {
            WriteLineColor($"Service '{ServiceName}' {action}ed successfully.", ConsoleColor.Green);
        }
        else
        {
            var stderr = result.Stderr;
            if (stderr.Contains("could not be found") || stderr.Contains("not-found"))
                Console.Error.WriteLine($"Service '{ServiceName}' is not installed.");
            else
            {
                Console.Error.WriteLine($"Failed to {action} service. Exit code: {result.ExitCode}");
                Console.Error.WriteLine(stderr);
            }
        }
    }

    // ── Utilities ────────────────────────────────────────────────────────────

    /// <summary>
    /// Returns just the executable name (without path) for display in help messages.
    /// </summary>
    private static string GetExecutableName()
    {
        var path = Environment.ProcessPath;
        if (string.IsNullOrEmpty(path))
            return "EzDns.WebApi";

        var name = Path.GetFileName(path);
        return name.Equals("dotnet", StringComparison.OrdinalIgnoreCase) ||
               name.Equals("dotnet.exe", StringComparison.OrdinalIgnoreCase)
               ? "dotnet run --"
               : name;
    }

    private static (int ExitCode, string Stdout, string Stderr) RunProcess(string fileName, string arguments)
    {
        var startInfo = new ProcessStartInfo
        {
            FileName = fileName,
            Arguments = arguments,
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true
        };

        using var process = new Process { StartInfo = startInfo };
        process.Start();

        var stdout = process.StandardOutput.ReadToEnd();
        var stderr = process.StandardError.ReadToEnd();
        process.WaitForExit();

        return (process.ExitCode, stdout, stderr);
    }

    private static void WriteLineColor(string message, ConsoleColor color)
    {
        var original = Console.ForegroundColor;
        Console.ForegroundColor = color;
        Console.WriteLine(message);
        Console.ForegroundColor = original;
    }
}
