namespace MoBro.Plugin.MoBroActions;

internal static class Ids
{
  internal static class Action
  {
    internal const string StartProcess = "a_start_process";
  }

  internal static class ActionSettings
  {
    internal const string StarProcessExePath = "s_start_process_exe_path";
  }

  public static string Label(string id) => id + "_label";
  public static string Description(string id) => id + "_desc";
}