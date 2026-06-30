namespace Carbon.Components;

/// <summary>
/// Central place for Carbon-related localized text of Core plugin and components.
/// </summary>
public struct Localisation
{
	internal static CorePlugin Core => Community.Runtime.Core;

	public static Dictionary<string, string> Phrases = new()
	{
		["cooldown_player"] = "You're cooled down. Please wait {0}.",
		["unknown_chat_cmd_1"] = "<color=orange>Unknown command:</color> {0}",
		["unknown_chat_cmd_2"] = "<color=orange>Unknown command:</color> {0}\n<size=12s>Suggesting: {1}</size>",
		["unknown_chat_cmd_separator_1"] = ", ",
		["unknown_chat_cmd_separator_2"] = " or ",
		["no_perm"] = "You don't have any of the required permissions to run this command.",
		["no_group"] = "You aren't in any of the required groups to run this command.",
		["no_auth"] = $"You don't have the minimum auth level [{{0}}] required to execute this command [your level: {{1}}]."
	};

	public static Dictionary<string, string> Phrases_zh_CN = new()
	{
		["cooldown_player"] = "你已被冷却，请等待 {0}。",
		["unknown_chat_cmd_1"] = "<color=orange>未知命令:</color> {0}",
		["unknown_chat_cmd_2"] = "<color=orange>未知命令:</color> {0}\n<size=12s>你是否想输入: {1}</size>",
		["unknown_chat_cmd_separator_1"] = "、",
		["unknown_chat_cmd_separator_2"] = " 或 ",
		["no_perm"] = "你没有运行此命令所需的任何权限。",
		["no_group"] = "你不在运行此命令所需的任何组中。",
		["no_auth"] = $"你的权限等级不足以执行此命令 [需要: {{0}}，你的等级: {{1}}]。"
	};

	public static string Get(string key, string playerId)
	{
		return Core.lang.GetMessage(key, Core, playerId, Core.lang.GetLanguage(playerId));
	}
	public static string Get(string key, string playerId, params object[] format)
	{
		return string.Format(Get(key, playerId), format);
	}
}
