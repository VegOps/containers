#:property PublishAot=true
#:property InvariantGlobalization=true
#:property StripSymbols=true
#:property OptimizationPreference=Size
#:property IlcOptimizationPreference=Size
#:property StackTraceSupport=false
#:property UseSystemResourceKeys=true
#:property IlcTrimMetadata=true
#:property AssemblyName=entrypoint

using System.Runtime.InteropServices;

const string AppBin = "/usr/bin/garage";
const string ConfigPath = "/garage/etc/garage.toml";
const string DefaultConfigPath = "/usr/share/garage/garage.toml";

try
{
    if (!File.Exists(ConfigPath))
    {
        var rpcSecret = Secret();
        var adminToken = Secret();
        var config = File.ReadAllText(DefaultConfigPath)
            .Replace("CHANGEME_RPC_SECRET", rpcSecret, StringComparison.Ordinal)
            .Replace("CHANGEME_ADMIN_TOKEN", adminToken, StringComparison.Ordinal);

        File.WriteAllText(ConfigPath, config);
        File.SetUnixFileMode(ConfigPath, UnixFileMode.UserRead | UnixFileMode.UserWrite);
        Console.WriteLine($"Garage RPC secret: {rpcSecret}");
        Console.WriteLine($"Garage admin token: {adminToken}");
    }

    Exec();
}
catch (Exception ex)
{
    Console.Error.WriteLine(ex.Message);
    Environment.Exit(1);
}

static string Secret()
{
    var bytes = new byte[32];
    using var random = File.OpenRead("/dev/urandom");
    random.ReadExactly(bytes);
    return Convert.ToHexString(bytes).ToLowerInvariant();
}

static void Exec()
{
    var args = Environment.GetCommandLineArgs();
    var appArgs = args.Length > 1 ? args[1..] : ["server", "--single-node"];
    var argv = new string?[appArgs.Length + 2];
    argv[0] = AppBin;
    Array.Copy(appArgs, 0, argv, 1, appArgs.Length);

    var result = NativeMethods.execv(AppBin, argv);
    throw new Exception($"execv({AppBin}) failed: errno {Marshal.GetLastPInvokeError()}, result {result}");
}

static class NativeMethods
{
    [DllImport("libc", SetLastError = true)]
    internal static extern int execv(string filename, string?[] argv);
}