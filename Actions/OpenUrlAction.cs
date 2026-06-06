using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MoBro.Plugin.SDK.Builders;
using MoBro.Plugin.SDK.Enums;
using MoBro.Plugin.SDK.Models.Settings;
using MoBro.Plugin.SDK.Services;

namespace MoBro.Plugin.MoBroActions.Actions;

public class OpenUrlAction : AbstractAction
{
  private const string UrlKey = Ids.ActionSettings.OpenUrl;

  protected override string Id => Ids.Action.OpenUrl;
  protected override string? Group() => Ids.Group.Web;

  protected override Func<SettingsBuilder.INameStage, SettingsFieldBase>[] Settings() =>
  [
    sb => sb
      .WithName(UrlKey)
      .WithLabel(Ids.Label(UrlKey), Ids.Description(UrlKey))
      .AsRequired()
      .OfTypeString()
      .WithRegex("^(https?):.+")
      .Build()
  ];

  protected override (string, CoreMetricType)? Metric() => null;

  protected override Task Handler(IMoBroSettings settings)
  {
    var url = settings.GetValue<string>(UrlKey, "")?.Trim();
    if (string.IsNullOrWhiteSpace(url))
    {
      Logger.LogWarning("Empty URL provided");
      return Task.FromException(new ArgumentException("URL must not be empty"));
    }

    if (!Uri.TryCreate(url, UriKind.Absolute, out var uri) ||
        (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps))
    {
      Logger.LogWarning("Invalid or unsupported URL: {Url}", url);
      return Task.FromException(new ArgumentException("Only http and https URLs are supported"));
    }

    try
    {
      Logger.LogInformation("Opening URL: {Url}", url);
      var psi = new ProcessStartInfo
      {
        FileName = url,
        UseShellExecute = true
      };
      using var _ = Process.Start(psi);
    }
    catch (Exception ex)
    {
      Logger.LogError(ex, "Failed to open URL: {Url}", url);
      return Task.FromException(ex);
    }

    return Task.CompletedTask;
  }
}