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
    var path = settings.GetValue<string>(PathKey, "");
    if (path.Length == 0 || !File.Exists(path)) return Task.CompletedTask;

    Logger.LogInformation("Starting exe: {ExePath}", path);
    try
    {
      var process = new Process();
      process.StartInfo.FileName = path;
      process.Start();
    }
    catch (Exception e)
    {
      Logger.LogError(e, "Failed to start exe: {ExePath}", path);
      throw;
    }

    return Task.CompletedTask;
  }
}