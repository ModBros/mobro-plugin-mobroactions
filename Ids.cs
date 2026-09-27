namespace MoBro.Plugin.MoBroActions;

internal static class Ids
{
  internal static class Category
  {
    internal const string MoBroActions = "c_mobroactions";
  }

  internal static class Group
  {
    internal const string System = "g_system";
    internal const string Web = "g_web";
  }

  internal static class Action
  {
    internal const string StartProcess = "a_start_process";
    internal const string InvokeScript = "a_invoke_script";
    internal const string LockWorkstation = "a_lock_workstation";
    internal const string OpenUrl = "a_open_url";
    internal const string InvokeWebhook = "a_invoke_webhook";
  }

  internal static class ActionSettings
  {
    internal const string StarProcessExePath = "s_start_process_exe_path";
    internal const string InvokeScriptPath = "s_invoke_script_path";
    internal const string InvokeScriptShowShell = "s_invoke_script_show_shell";
    internal const string OpenUrl = "s_open_url";
    internal const string WebhookUrl = "s_webhook_url";
    internal const string WebhookMethod = "s_webhook_method";
    internal const string WebhookBody = "s_webhook_body";
    internal const string WebhookHeaders = "s_webhook_headers";
  }

  public static string Label(string id) => id + "_label";
  public static string Description(string id) => id + "_desc";
}