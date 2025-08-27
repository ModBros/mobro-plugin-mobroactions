namespace MoBro.Plugin.MoBroActions;

internal static class Ids
{
  internal static class Action
  {
    internal const string StartProcess = "a_start_process";
    internal const string InvokeScript = "a_invoke_script";
  }

  internal static class ActionSettings
  {
    internal const string StarProcessExePath = "s_start_process_exe_path";
    internal const string InvokeScriptPath = "s_invoke_script_path";
    internal const string InvokeScriptShowShell = "s_invoke_script_show_shell";
  }

  public static string Label(string id) => id + "_label";
  public static string Description(string id) => id + "_desc";
}