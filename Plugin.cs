using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using MoBro.Plugin.SDK;
using MoBro.Plugin.SDK.Services;

namespace MoBro.Plugin.MoBroActions;

public sealed class Plugin : IMoBroPlugin
{
  private readonly IMoBroService _service;
  private readonly ILogger _logger;

  private readonly List<AbstractAction> _actions;

  public Plugin(IMoBroService service, ILogger logger)
  {
    _service = service;
    _logger = logger;
    _actions = [];
  }

  public void Init()
  {
    _logger.LogInformation("Initializing MoBroActions Plugin...");

    // Scan for all classes implementing IAction 
    var actionTypes = typeof(Plugin).Assembly.GetTypes();
    foreach (var type in actionTypes)
    {
      if (!typeof(AbstractAction).IsAssignableFrom(type) || type.IsInterface || type.IsAbstract) continue;
      try
      {
        if (Activator.CreateInstance(type) is AbstractAction actionInstance && actionInstance.Enabled())
        {
          actionInstance.SetLogger(_logger);
          actionInstance.Init();
          _actions.Add(actionInstance);
        }
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "Failed to initialize action type: {TypeFullName}", type.FullName);
      }
    }

    // Register items
    var items = _actions
      .Select(action => action.GetItems())
      .SelectMany(items => items);
    foreach (var item in items)
    {
      _service.Register(item);
    }
  }
}