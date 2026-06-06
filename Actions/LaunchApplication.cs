using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MoBro.Plugin.SDK.Builders;
using MoBro.Plugin.SDK.Enums;
using MoBro.Plugin.SDK.Models.Settings;
using MoBro.Plugin.SDK.Services;

namespace MoBro.Plugin.MoBroActions.Actions;

public class LaunchApplication : AbstractAction
{
  private const string PathKey = Ids.ActionSettings.StarProcessExePath;

  protected override string Id => Ids.Action.StartProcess;
  protected override string? Group() => Ids.Group.System;

  protected override Func<SettingsBuilder.INameStage, SettingsFieldBase>[] Settings() =>
  [
    sb => sb
      .WithName(PathKey)
      .WithLabel(
        Ids.Label(PathKey),
        Ids.Description(PathKey)
      )
      .AsRequired()
      .OfTypeString()
      .WithRegex("^.*\\.exe$")
      .Build()
  ];

  protected override (string, CoreMetricType)? Metric() => null;

  protected override Task Handler(IMoBroSettings settings)
  {
    var rawPath = settings.GetValue<string>(PathKey, "");
    if (string.IsNullOrWhiteSpace(rawPath))
    {
      Logger.LogWarning("Empty path to .exe provided");
      return Task.FromException(new ArgumentException("Path must not be empty"));
    }

    // Normalize and expand environment variables, strip quotes
    var path = Environment.ExpandEnvironmentVariables(rawPath.Trim().Trim('"'));

    // Validate extension defensively
    if (!string.Equals(Path.GetExtension(path), ".exe", StringComparison.OrdinalIgnoreCase))
    {
      Logger.LogWarning("Rejected non-executable path: {ExePath}", path);
      return Task.FromException(new ArgumentException("Path must be an executable"));
    }

    if (!File.Exists(path))
    {
      Logger.LogWarning("Executable not found: {ExePath}", path);
      return Task.FromException(new FileNotFoundException("Executable not found", path));
    }

    Logger.LogInformation("Starting exe: {ExePath}", path);
    try
    {
      var psi = new ProcessStartInfo
      {
        FileName = path,
        UseShellExecute = true, // let the shell resolve and handle associations
        WorkingDirectory = Path.GetDirectoryName(path) ?? Environment.CurrentDirectory,
        ErrorDialog = false
      };

      using var _ = Process.Start(psi);
    }
    catch (Exception e)
    {
      Logger.LogError(e, "Failed to start exe: {ExePath}", path);
      return Task.FromException(e);
    }

    return Task.CompletedTask;
  }
}