using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MoBro.Plugin.SDK.Builders;
using MoBro.Plugin.SDK.Enums;
using MoBro.Plugin.SDK.Models;
using MoBro.Plugin.SDK.Models.Metrics;
using MoBro.Plugin.SDK.Models.Settings;
using MoBro.Plugin.SDK.Services;
using Action = MoBro.Plugin.SDK.Models.Actions.Action;

namespace MoBro.Plugin.MoBroActions;

public abstract class AbstractAction
{
  protected ILogger Logger = null!;

  private readonly Action _action;
  private readonly Metric? _metric;

  protected AbstractAction()
  {
    _action = BuildAction();
    _metric = BuildMetric();
  }

  public void SetLogger(ILogger logger)
  {
    Logger = logger;
  }

  public IMoBroItem[] GetItems()
  {
    if (_metric == null)
    {
      return [_action];
    }

    return [_action, _metric];
  }

  public virtual void Init()
  {
  }
  
  public virtual bool Enabled() => true;

  protected abstract string Id { get; }
  protected abstract Task Handler(IMoBroSettings settings);
  protected abstract (string, CoreMetricType)? Metric();
  protected abstract Func<SettingsBuilder.INameStage, SettingsFieldBase>[] Settings();


  private Metric? BuildMetric()
  {
    var metricData = Metric();
    if (metricData == null) return null;
    var (id, metricType) = metricData.Value;
    return MoBroItem
      .CreateMetric()
      .WithId(id)
      .WithLabel(Ids.Label(id), Ids.Description(id))
      .OfType(metricType)
      .OfCategory(CoreCategory.Media)
      .OfNoGroup()
      .AsDynamicValue()
      .Build();
  }

  private Action BuildAction()
  {
    var builder = MoBroItem
      .CreateAction()
      .WithId(Id)
      .WithLabel(Ids.Label(Id), Ids.Description(Id))
      .OfCategory(CoreCategory.Media)
      .OfNoGroup()
      .WithMetric(Metric()?.Item1)
      .WithAsyncHandler(Handler);

    foreach (var setting in Settings())
    {
      builder.WithSetting(setting);
    }

    return builder.Build();
  }

  protected static MetricValue Value(string id, object? value) => new(id, value);
}