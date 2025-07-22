using Microsoft.Extensions.Logging;
using MoBro.Plugin.SDK;
using MoBro.Plugin.SDK.Services;

namespace MoBro.Plugin.MoBroActions;

public sealed class Plugin : IMoBroPlugin
{
  private readonly IMoBroService _service;
  private readonly ILogger _logger;

  public Plugin(IMoBroService service, ILogger logger)
  {
    _service = service;
    _logger = logger;
  }

  public void Init()
  {
  }
}