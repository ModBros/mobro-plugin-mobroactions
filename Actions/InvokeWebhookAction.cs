using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MoBro.Plugin.SDK.Builders;
using MoBro.Plugin.SDK.Enums;
using MoBro.Plugin.SDK.Models.Settings;
using MoBro.Plugin.SDK.Services;

namespace MoBro.Plugin.MoBroActions.Actions;

public class InvokeWebhookAction : AbstractAction
{
  // a webhook is triggered by a button press, so do not wait for the default of 100 seconds
  private static readonly HttpClient HttpClient = new() { Timeout = TimeSpan.FromSeconds(15) };

  // headers are separated by new lines or '|', or by ';' if followed by the next header name
  // (a ';' within a header value such as "application/json; charset=utf-8" is kept)
  private static readonly Regex HeaderSeparator = new(@"\r?\n|\||;(?=\s*[A-Za-z0-9-]+\s*:)", RegexOptions.Compiled);

  private const string UrlKey = Ids.ActionSettings.WebhookUrl;
  private const string MethodKey = Ids.ActionSettings.WebhookMethod;
  private const string BodyKey = Ids.ActionSettings.WebhookBody;
  private const string HeadersKey = Ids.ActionSettings.WebhookHeaders;

  protected override string Id => Ids.Action.InvokeWebhook;
  protected override string? Group() => Ids.Group.Web;

  protected override Func<SettingsBuilder.INameStage, SettingsFieldBase>[] Settings() =>
  [
    sb => sb
      .WithName(UrlKey)
      .WithLabel(Ids.Label(UrlKey), Ids.Description(UrlKey))
      .AsRequired()
      .OfTypeString()
      .WithRegex("^(https?):.+")
      .Build(),
    sb => sb
      .WithName(MethodKey)
      .WithLabel(Ids.Label(MethodKey), Ids.Description(MethodKey))
      .AsRequired()
      .OfTypeSelect()
      .WithOption("GET", "GET")
      .WithOption("POST", "POST")
      .WithOption("PUT", "PUT")
      .WithOption("PATCH", "PATCH")
      .WithOption("DELETE", "DELETE")
      .WithDefault("GET")
      .Build(),
    sb => sb
      .WithName(BodyKey)
      .WithLabel(Ids.Label(BodyKey), Ids.Description(BodyKey))
      .OfTypeString()
      .Build(),
    sb => sb
      .WithName(HeadersKey)
      .WithLabel(Ids.Label(HeadersKey), Ids.Description(HeadersKey))
      .OfTypeString()
      .Build()
  ];

  protected override (string, CoreMetricType)? Metric() => null;

  protected override Task Handler(IMoBroSettings settings)
  {
    var url = settings.GetValue(UrlKey, "").Trim();
    if (!Uri.TryCreate(url, UriKind.Absolute, out var uri) ||
        (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps))
    {
      logger.LogWarning("Invalid or unsupported webhook URL: {Url}", url);
      return Task.FromException(new ArgumentException("Only http and https URLs are supported"));
    }

    var method = settings.GetValue<string>(MethodKey, "GET").Trim().ToUpperInvariant();
    if (method is not ("GET" or "POST" or "PUT" or "PATCH" or "DELETE"))
    {
      logger.LogWarning("Unsupported HTTP method: {Method}", method);
      return Task.FromException(new ArgumentException($"Unsupported HTTP method: {method}"));
    }

    var request = new HttpRequestMessage(new HttpMethod(method), uri);

    // Attach optional body (the content type can be overridden by a Content-Type header)
    var body = settings.GetValue<string>(BodyKey, "").Trim();
    if (!string.IsNullOrWhiteSpace(body))
    {
      request.Content = new StringContent(body, Encoding.UTF8, IsJson(body) ? "application/json" : "text/plain");
    }

    // Parse optional headers (format: "Key: Value", separated by new lines, '|' or ';')
    var headersRaw = settings.GetValue<string>(HeadersKey, "").Trim();
    if (!string.IsNullOrWhiteSpace(headersRaw))
    {
      foreach (var line in HeaderSeparator.Split(headersRaw))
      {
        var trimmed = line.Trim();
        var separatorIndex = trimmed.IndexOf(':');
        if (separatorIndex <= 0) continue;
        var headerName = trimmed[..separatorIndex].Trim();
        var headerValue = trimmed[(separatorIndex + 1)..].Trim();
        if (!string.IsNullOrWhiteSpace(headerName))
        {
          AddHeader(request, headerName, headerValue);
        }
      }
    }

    // only the host is logged, as webhook URLs often contain secrets (e.g. Discord tokens)
    return SendAsync(request, method, uri.Host);
  }

  private void AddHeader(HttpRequestMessage request, string name, string value)
  {
    if (request.Headers.TryAddWithoutValidation(name, value)) return;

    // content headers (e.g. Content-Type) are not allowed on the request itself, only on its content
    if (request.Content != null)
    {
      request.Content.Headers.Remove(name);
      if (request.Content.Headers.TryAddWithoutValidation(name, value)) return;
    }

    logger.LogWarning("Ignoring webhook header {HeaderName} (content headers require a request body)", name);
  }

  private static bool IsJson(string body)
  {
    try
    {
      using var document = JsonDocument.Parse(body);
      return document.RootElement.ValueKind is JsonValueKind.Object or JsonValueKind.Array;
    }
    catch (JsonException)
    {
      return false;
    }
  }

  private async Task SendAsync(HttpRequestMessage request, string method, string host)
  {
    using var _ = request;
    HttpResponseMessage response;
    try
    {
      logger.LogInformation("Invoking webhook: {Method} {Host}", method, host);
      response = await HttpClient.SendAsync(request);
    }
    catch (Exception ex)
    {
      logger.LogError(ex, "Failed to invoke webhook: {Method} {Host}", method, host);
      throw;
    }

    using var __ = response;
    if (!response.IsSuccessStatusCode)
    {
      logger.LogWarning("Webhook returned non-success status: {StatusCode} for {Method} {Host}",
        (int)response.StatusCode, method, host);

      throw new HttpRequestException(
        $"Webhook request failed with status {(int)response.StatusCode} ({response.ReasonPhrase})");
    }

    logger.LogInformation("Webhook invoked successfully: {StatusCode}", (int)response.StatusCode);
  }
}