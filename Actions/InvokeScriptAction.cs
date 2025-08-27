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

public class InvokeScript : AbstractAction
{
  private const string PathKey = Ids.ActionSettings.InvokeScriptPath;
  private const string ShowShellKey = Ids.ActionSettings.InvokeScriptShowShell;

  protected override string Id => Ids.Action.InvokeScript;

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
      .WithRegex("^.*\\.(bat|cmd|ps1)$")
      .Build(),
    sb => sb
      .WithName(ShowShellKey)
      .WithLabel(
        Ids.Label(ShowShellKey),
        Ids.Description(ShowShellKey)
      )
      .OfTypeCheckbox()
      .WithDefault(false)
      .Build()
  ];

  protected override (string, CoreMetricType)? Metric() => null;

  protected override Task Handler(IMoBroSettings settings)
  {
    var rawPath = settings.GetValue<string>(PathKey, "");
    if (string.IsNullOrWhiteSpace(rawPath))
    {
      Logger.LogWarning("Empty path to script provided");
      return Task.FromException(new ArgumentException("Path must not be empty"));
    }

    var path = Environment.ExpandEnvironmentVariables(rawPath.Trim().Trim('"'));
    if (!File.Exists(path))
    {
      Logger.LogWarning("Script not found: {ScriptPath}", path);
      return Task.FromException(new FileNotFoundException("Script not found", path));
    }

    var showShell = settings.GetValue(ShowShellKey, false);

    var extension = Path.GetExtension(path).ToLowerInvariant();
    ProcessStartInfo startInfo;
    switch (extension)
    {
      case ".bat":
      case ".cmd":
        Logger.LogInformation("Invoking batch script: {ScriptPath}", path);
        startInfo = new ProcessStartInfo
        {
          FileName = "cmd.exe",
          Arguments = $"/c \"{path}\"",
          UseShellExecute = false,
          CreateNoWindow = !showShell,
          WorkingDirectory = Path.GetDirectoryName(path) ?? Environment.CurrentDirectory,
          WindowStyle = showShell ? ProcessWindowStyle.Normal : ProcessWindowStyle.Hidden
        };
        break;

      case ".ps1":
        Logger.LogInformation("Invoking PowerShell script: {ScriptPath}", path);
        startInfo = new ProcessStartInfo
        {
          FileName = "powershell.exe",
          Arguments = $"-NoProfile -ExecutionPolicy Bypass -File \"{path}\"",
          UseShellExecute = false,
          CreateNoWindow = !showShell,
          WorkingDirectory = Path.GetDirectoryName(path) ?? Environment.CurrentDirectory,
          WindowStyle = showShell ? ProcessWindowStyle.Normal : ProcessWindowStyle.Hidden
        };
        break;

      default:
        Logger.LogWarning("Unsupported script extension: {Extension}", extension);
        return Task.FromException(new ArgumentException("Unsupported script extension"));
    }

    try
    {
      using var process = new Process();
      process.StartInfo = startInfo;
      process.Start();
    }
    catch (Exception e)
    {
      Logger.LogError(e, "Failed to invoke script: {ScriptPath}", path);
      return Task.FromException(e);
    }

    return Task.CompletedTask;
  }
}