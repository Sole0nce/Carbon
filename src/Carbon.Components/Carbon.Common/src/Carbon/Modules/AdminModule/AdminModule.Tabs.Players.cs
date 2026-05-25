#if !MINIMAL

using ProtoBuf;

namespace Carbon.Modules;

public partial class AdminModule
{
	readonly int[] _backpacks = new[]
	{
		-907422733,
		2068884361
	};

	public class PlayersTab
	{
		public static readonly List<BasePlayer> BlindedPlayers = [];

		public static Tab Get()
		{
			var players = new Tab("players", "玩家", Community.Runtime.Core, (instance, tab) =>
			{
				tab.ClearColumn(1);
				RefreshPlayers(tab, instance);
			}, "players.use");

			players.AddColumn(0);
			players.AddColumn(1);

			return players;
		}

		public static void RefreshPlayers(Tab tab, PlayerSession ap)
		{
			tab.ClearColumn(0);

			tab.AddInput(0, "搜索", ap => ap?.GetStorage<string>(tab, "playerfilter"), (ap2, args) =>
			{
				ap2.SetStorage(tab, "playerfilter", args.Select(x => x as string).ToString(" "));
				RefreshPlayers(tab, ap2);
			});

			var onlinePlayers = BasePlayer.allPlayerList.Distinct().Where(x => x.userID.IsSteamId() && x.IsConnected)
				.OrderBy(x => x.Connection?.connectionTime);
			tab.AddName(0, $"在线 ({onlinePlayers.Count():n0})");
			foreach (var player in onlinePlayers)
			{
				AddPlayer(tab, ap, player);
			}

			if (onlinePlayers.Count() == 0) tab.AddText(0, "未找到在线玩家。", 10, "1 1 1 0.4");

			var offlinePlayers = BasePlayer.allPlayerList.Distinct().Where(x => x.userID.IsSteamId() && !x.IsConnected);
			tab.AddName(0, $"离线 ({offlinePlayers.Count():n0})");
			foreach (var player in offlinePlayers)
			{
				AddPlayer(tab, ap, player);
			}

			if (offlinePlayers.Count() == 0) tab.AddText(0, "未找到离线玩家。", 10, "1 1 1 0.4");
		}

		public static void AddPlayer(Tab tab, PlayerSession ap, BasePlayer player)
		{
			if (ap != null)
			{
				var filter = ap.GetStorage<string>(tab, "playerfilter");

				if (!string.IsNullOrEmpty(filter) && !(player.displayName.ToLower().Contains(filter.ToLower()) || player.UserIDString.Contains(filter))) return;
			}

			tab.AddButton(0, $"{player.displayName}", _ =>
			{
				ap.SetStorage(tab, "playerfilterpl", player);
				ShowInfo(1, tab, ap, player);
			}, aap => aap == null || !(aap.GetStorage<BasePlayer>(tab, "playerfilterpl", null) == player) ? Tab.OptionButton.Types.None : Tab.OptionButton.Types.Selected);
		}
		public static void ShowInfo(int column, Tab tab, PlayerSession aap, BasePlayer player)
		{
			tab.ClearColumn(column);

			if (column != 1)
			{
				tab.AddButton(column, "<", ap =>
				{
					RefreshPlayers(tab, ap);
					ShowInfo(1, tab, ap, player);
				});
			}

			tab.AddName(column, $"玩家信息", TextAnchor.MiddleLeft);
			tab.AddInput(column, "名称", _ => player.displayName, (_, args) =>
			{
				player.AsIPlayer().Rename(args.Select(x => x as string).ToString(" "));
			});
			tab.AddInput(column, "Steam ID", _ => player.UserIDString, null);
			tab.AddInput(column, "网络 ID", _ => $"{player.net?.ID}", null);
			if (Singleton.HasAccess(aap.Player, "players.see_ips"))
			{
				tab.AddInput(column, "IP", _ => $"{player.net?.connection?.ipaddress}", null, hidden: true);
			}
			try
			{
				var position = player.transform.position;
				tab.AddInput(column, "位置", _ => $"{position} [{MapHelper.PositionToGrid(position)}]", null);
			}
			catch { }

			tab.AddButton(column, "玩家标志", ap =>
			{
				ShowInfo(0, tab, ap, player);
				PlayerFlags(1, tab, player);
			});

			if (Singleton.HasAccess(aap.Player, "permissions.use"))
			{
				tab.AddName(column, $"权限", TextAnchor.MiddleLeft);
				{
					tab.AddButton(column, "查看权限", ap =>
					{
						var perms = Singleton.FindTab("permissions");
						var permission = Community.Runtime.Core.permission;
						Singleton.SetTab(ap.Player, "permissions");

						ap.SetStorage(tab, "player", player.UserIDString);
						PermissionsTab.GeneratePlayers(perms, permission, ap);
						PermissionsTab.GenerateHookables(perms, ap, permission, permission.FindUser(player.UserIDString), null, PermissionsTab.HookableTypes.Plugin);
					}, _ => Tab.OptionButton.Types.Important);
				}
			}

			if (aap.Player.IsAdmin || Singleton.Permissions.UserHasPermission(aap.Player.UserIDString, "carbon.cmod"))
			{
				tab.AddButtonArray(column, new Tab.OptionButton("踢出", _ =>
				{
					Singleton.Modal.Open(aap.Player, $"踢出 {player.displayName}", new Dictionary<string, ModalModule.Modal.Field>
					{
						["reason"] = ModalModule.Modal.Field.Make("原因", ModalModule.Modal.Field.FieldTypes.String, @default: "别这么做。")
					}, onConfirm: (_, m) =>
					{
						player.Kick(m.Get<string>("reason"));
					});
				}), new Tab.OptionButton("封禁", ap =>
				{
					Singleton.Modal.Open(aap.Player, $"封禁 {player.displayName}", new Dictionary<string, ModalModule.Modal.Field>
					{
						["reason"] = ModalModule.Modal.Field.Make("原因", ModalModule.Modal.Field.FieldTypes.String, @default: "别这么做。"),
						["until"] = ModalModule.Modal.ButtonField.MakeButton("有效期至", "选择日期", _ =>
						{
							Core.NextTick(() => Singleton.DatePicker.Draw(ap.Player, date => ap.SetStorage(tab, "date", date)));
						})
					}, onConfirm: (_, m) =>
					{
						var date = ap.GetStorage(tab, "date", DateTime.UtcNow.AddYears(100));
						var now = DateTime.UtcNow;
						date = new DateTime(date.Year, date.Month, date.Day, now.Hour, now.Minute, now.Second, DateTimeKind.Utc);
						var then = now - date;

						player.AsIPlayer().Ban(m.Get<string>("reason"), then);
					});
				}), new Tab.OptionButton(player.IsSleeping() ? "结束睡眠" : "睡眠", ap =>
				{
					if (player.IsSleeping())
					{
						player.EndSleeping();
					}
					else
					{
						player.StartSleeping();
					}

					ShowInfo(column, tab, ap, player);
				}), new Tab.OptionButton("敌对状态", ap =>
				{
					var fields = new Dictionary<string, ModalModule.Modal.Field>
					{
						["duration"] = ModalModule.Modal.Field.Make("持续时间",
							ModalModule.Modal.Field.FieldTypes.Float, true, 60f)
					};

					Singleton.Modal.Open(ap.Player, "玩家敌对", fields, (ap, modal) =>
					{
						var duration = modal.Get<float>("duration").Clamp(0f, float.MaxValue);
						player.State.unHostileTimestamp = Network.TimeEx.currentTimestamp + duration;
						player.DirtyPlayerState();
						player.ClientRPC(RpcTarget.Player("SetHostileLength", player), duration);
						fields.Clear();
						fields = null;
						ShowInfo(column, tab, aap, player);
						Singleton.Draw(aap.Player);
					}, () =>
					{
						fields.Clear();
						fields = null;
					});
				}));
			}
			else tab.AddText(column, $"你需要 'carbon.cmod' 权限才能踢出、封禁、睡眠或更改玩家敌对状态",
				10, "1 1 1 0.4");

			tab.AddName(column, $"操作", TextAnchor.MiddleLeft);

			if (Singleton.HasAccess(aap.Player, "entities.tp_entity"))
			{
				tab.AddButtonArray(column,
					new Tab.OptionButton("传送到", ap => { ap.Player.Teleport(player.transform.position); }),
					new Tab.OptionButton("传送到我", _ =>
					{
						tab.CreateDialog($"你确定要这样做吗？", ap =>
						{
							player.Teleport(ap.Player.transform.position);
						}, null);
					}),
					new Tab.OptionButton("传送到所属物",
						ap =>
						{
							var entities = BaseEntity.Util.FindTargetsOwnedBy(player.userID, string.Empty);

							if (entities.Length > 0)
							{
								var randomEntity = entities[RandomEx.GetRandomInteger(0, entities.Length)];
								ap.Player.Teleport(randomEntity.transform.position);
							}
							else
							{
								Logger.Warn($" 未找到属于 {player} 的实体，无法传送。");
							}
						}));
			}

			if (Singleton.HasAccess(aap.Player, "entities.loot_players"))
			{
				tab.AddButtonArray(column,
					new Tab.OptionButton("搜刮", ap =>
					{
						OpenPlayerContainer(ap, player, tab);
					}),
					new Tab.OptionButton("扒光", ap =>
					{
						player.inventory.Strip();
					}),
					new Tab.OptionButton("重生", _ =>
					{
						tab.CreateDialog($"你确定要这样做吗？", _ =>
						{
							player.Hurt(player.MaxHealth());
							player.Respawn();
							player.EndSleeping();
						}, null);
					}));
				tab.AddText(column, "要搜刮背包，请在搜刮玩家时将背包物品拖到任意快捷栏", 10, "1 1 1 0.4");
			}

			if (Singleton.HasAccess(aap.Player, "players.inventory_management"))
			{
				tab.AddName(column, "背包锁定");
				tab.AddButtonArray(column,
					new Tab.OptionButton("主背包", _ =>
						{
							LockPlayerContainer(aap.Player, player, player.inventory.containerMain, !player.inventory.containerMain.IsLocked());
						},
						_ => player.inventory.containerMain.IsLocked()
							? Tab.OptionButton.Types.Important
							: Tab.OptionButton.Types.None),
					new Tab.OptionButton("快捷栏", _ =>
						{
							LockPlayerContainer(aap.Player, player, player.inventory.containerBelt, !player.inventory.containerBelt.IsLocked());
						},
						_ => player.inventory.containerBelt.IsLocked()
							? Tab.OptionButton.Types.Important
							: Tab.OptionButton.Types.None),
					new Tab.OptionButton("装备", _ =>
						{
							LockPlayerContainer(aap.Player, player, player.inventory.containerWear, !player.inventory.containerWear.IsLocked());
						},
						_ => player.inventory.containerWear.IsLocked()
							? Tab.OptionButton.Types.Important
							: Tab.OptionButton.Types.None));
			}

			if (Singleton.HasTab("entities"))
			{
				tab.AddButton(column, "选择实体", ap2 =>
				{
					Singleton.SetTab(ap2.Player, "entities");
					var tab = Singleton.GetTab(ap2.Player);
					EntitiesTab.SelectEntity(tab, ap2, player);
					EntitiesTab.DrawEntities(tab, ap2);
					EntitiesTab.DrawEntitySettings(tab, 1, ap2);
				});
			}

			if (Singleton.HasAccess(aap.Player, "entities.blind_players"))
			{
				if (!BlindedPlayers.Contains(player))
				{
					tab.AddButton(column, "致盲玩家", _ =>
					{
						tab.CreateDialog("你确定要致盲该玩家吗？", ap =>
						{
							BlindPlayer(aap.Player, player);
							ShowInfo(column, tab, ap, player);

							if (ap.Player == player) Core.timer.In(1, () => { Singleton.Close(player); });
						}, null);
					});
				}
				else
				{
					tab.AddButton(column, "解除致盲", ap =>
					{
						UnblindPlayer(aap.Player, player);
						ShowInfo(column, tab, ap, player);
					}, _ => Tab.OptionButton.Types.Selected);
				}
			}

			tab.AddName(column, "属性");
			tab.AddName(column, "战斗");
			tab.AddRange(column, "生命值", 0, player.MaxHealth(), _ => player.health, (_, value) => player.SetHealth(value), _ => $"{player.health:0}");

			tab.AddRange(column, "水分", 0, player.metabolism.hydration.max, _ => player.metabolism.hydration.value, (_, value) => player.metabolism.hydration.SetValue(value), _ => $"{player.metabolism.hydration.value:0}");
			tab.AddRange(column, "饱食度", 0, player.metabolism.calories.max, _ => player.metabolism.calories.value, (_, value) => player.metabolism.calories.SetValue(value), _ => $"{player.metabolism.calories.value:0}");
			tab.AddRange(column, "辐射", 0, player.metabolism.radiation_poison.max, _ => player.metabolism.radiation_poison.value, (_, value) => player.metabolism.radiation_poison.SetValue(value), _ => $"{player.metabolism.radiation_poison.value:0}");
			tab.AddRange(column, "流血", 0, player.metabolism.bleeding.max, _ => player.metabolism.bleeding.value, (_, value) => player.metabolism.bleeding.SetValue(value), _ => $"{player.metabolism.bleeding.value:0}");
			tab.AddRange(column, "湿度", 0, player.metabolism.wetness.max * 10f, ap => player.metabolism.wetness.value * 10f, (_, value) => player.metabolism.wetness.SetValue(value * 0.1f), _ => $"{player.metabolism.wetness.value * 100f:0}%");
			tab.AddButton(column, "强化属性", _ =>
			{
				EmpowerPlayerStats(aap.Player, player);
			});

			if (Singleton.HasAccess(aap.Player, "players.craft_queue"))
			{
				tab.AddName(column, "制作队列");

				var queue = player.inventory.crafting.queue.Where(x => !x.cancelled);
				foreach (var craft in queue)
				{
					tab.AddInputButton(column,
						$"{craft.blueprint.targetItem.displayName.english} (x{craft.amount}, {TimeEx.Format(craft.endTime - UnityEngine.Time.realtimeSinceStartup)})",
						0.1f,
						new Tab.OptionInput(null,
							_ =>
								$"<size=8>{craft.takenItems.Select(x => $"{x.info.displayName.english} x {x.amount}").ToString(", ")}</size>",
							0, true, null),
						new Tab.OptionButton("取消", TextAnchor.MiddleCenter, ap =>
						{
							player.inventory.crafting.CancelTask(craft.taskUID);
							ShowInfo(column, tab, ap, player);
						}, _ => Tab.OptionButton.Types.Important));
				}

				if (!queue.Any())
				{
					tab.AddText(column, "无制作项。", 8, "1 1 1 0.5");
				}
			}
		}
		public static void PlayerFlags(int column, Tab tab, BasePlayer player)
		{
			tab.ClearColumn(column);

			var counter = 0;
			var currentButtons = Facepunch.Pool.Get<List<Tab.OptionButton>>();

			tab.ClearColumn(column);

			tab.AddName(column, "玩家标志", TextAnchor.MiddleLeft);
			foreach (var flag in Enum.GetNames(typeof(BasePlayer.PlayerFlags)).OrderBy(x => x))
			{
				var flagValue = (BasePlayer.PlayerFlags)Enum.Parse(typeof(BasePlayer.PlayerFlags), flag);
				var hasFlag = player.HasPlayerFlag(flagValue);

				currentButtons.Add(new Tab.OptionButton(flag, ap =>
				{
					player.SetPlayerFlag(flagValue, !hasFlag);
					ShowInfo(0, tab, ap, player);
					PlayerFlags(column, tab, player);
				}, ap => hasFlag ? Tab.OptionButton.Types.Selected : Tab.OptionButton.Types.None));
				counter++;

				if (counter >= 5)
				{
					tab.AddButtonArray(column, currentButtons.ToArray());
					currentButtons.Clear();
					counter = 0;
				}
			}

			Facepunch.Pool.FreeUnmanaged(ref currentButtons);
		}
	}
}

#endif