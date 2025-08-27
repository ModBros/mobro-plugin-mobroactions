namespace MoBro.Plugin.MoBroActions;

internal static class Ids
{
  internal static class Action
  {
    internal const string StartProcess = "a_start_process";
    internal const string InvokeScript = "a_invoke_script";
    internal const string LockWorkstation = "a_lock_workstation";
    internal const string OpenUrl = "a_open_url";
  }

  internal static class ActionSettings
  {
    internal const string StarProcessExePath = "s_start_process_exe_path";
    internal const string InvokeScriptPath = "s_invoke_script_path";
    internal const string InvokeScriptShowShell = "s_invoke_script_show_shell";
    internal const string OpenUrl = "s_open_url";
  }

  public static string Label(string id) => id + "_label";
  public static string Description(string id) => id + "_desc";
}