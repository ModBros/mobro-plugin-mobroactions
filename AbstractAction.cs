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
  protected ILogger logger = null!;

  public void SetLogger(ILogger logger)
  {
    this.logger = logger;
  }

  public IMoBroItem[] GetItems()
  {
    // built here instead of in the constructor, as it calls members overridden by the (not yet constructed) subclass
    var action = BuildAction();
    var metric = BuildMetric();
    if (metric == null)
    {
      return [action];
    }

    return [action, metric];
  }

  public virtual void Init()
  {
  }

  public virtual bool Enabled() => true;

  protected abstract string Id { get; }
  protected abstract Task Handler(IMoBroSettings settings);
  protected abstract (string, CoreMetricType)? Metric();
  protected abstract Func<SettingsBuilder.INameStage, SettingsFieldBase>[] Settings();
  protected virtual string Category() => Ids.Category.MoBroActions;
  protected virtual string? Group() => null;


  private Metric? BuildMetric()
  {
    var metricData = Metric();
    if (metricData == null) return null;
    var (id, metricType) = metricData.Value;
    var withMetricCategory = MoBroItem
      .CreateMetric()
      .WithId(id)
      .WithLabel(Ids.Label(id), Ids.Description(id))
      .OfType(metricType)
      .OfCategory(Category());
    return (Group() != null ? withMetricCategory.OfGroup(Group()!) : withMetricCategory.OfNoGroup())
      .AsDynamicValue()
      .Build();
  }

  private Action BuildAction()
  {
    var withActionCategory = MoBroItem
      .CreateAction()
      .WithId(Id)
      .WithLabel(Ids.Label(Id), Ids.Description(Id))
      .OfCategory(Category());
    var builder = (Group() != null ? withActionCategory.OfGroup(Group()!) : withActionCategory.OfNoGroup())
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