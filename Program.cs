using System;
using MoBro.Plugin.MoBroActions;
using MoBro.Plugin.SDK;
using Serilog.Events;

using var plugin = MoBroPluginBuilder
  .Create<Plugin>()
  .WithLogLevel(LogEventLevel.Debug)
  .Build();

Console.ReadLine();
