using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MoBro.Plugin.SDK.Builders;
using MoBro.Plugin.SDK.Enums;
using MoBro.Plugin.SDK.Models.Settings;
using MoBro.Plugin.SDK.Services;

namespace MoBro.Plugin.MoBroActions.Actions;

public class LockWorkstationAction : AbstractAction
{
  protected override string Id => Ids.Action.LockWorkstation;
  protected override string? Group() => Ids.Group.System;

  protected override Func<SettingsBuilder.INameStage, SettingsFieldBase>[] Settings() => [];

  protected override (string, CoreMetricType)? Metric() => null;

  protected override Task Handler(IMoBroSettings settings)
  {
    if (!LockWorkStation())
    {
      var error = Marshal.GetLastPInvokeError();
      logger.LogError("Failed to lock workstation. Win32Error={ErrorCode}", error);
      return Task.FromException(new Win32Exception(error));
    }

    logger.LogInformation("Workstation locked");
    return Task.CompletedTask;
  }

  [DllImport("user32.dll", SetLastError = true)]
  private static extern bool LockWorkStation();
}