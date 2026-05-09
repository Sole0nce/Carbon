#if !MINIMAL

namespace Carbon.Modules;

public partial class AdminModule
{
	public class EnvironmentTab
	{
		private static int LastWeatherPresetSelectedIndex;
		private static string[] Options;

		public static Tab Get()
		{
			var tab = (Tab)null;

			tab = new Tab("env", "环境", Community.Runtime.Core, access: "environment.use",
				onChange: (ap, tab) =>
				{
					Options ??= SingletonComponent<Climate>.Instance?.WeatherPresets.Select(x => x.name).ToArray();
					Draw(tab);
				});

			return tab;
		}

		static void Draw(Tab tab)
		{
			var presets = SingletonComponent<Climate>.Instance.WeatherPresets;
			var overrides = SingletonComponent<Climate>.Instance.WeatherOverrides;

			tab.AddColumn(0, true);
			tab.AddColumn(1, true);

			tab.AddName(0, "时间");
			{
				tab.AddInputButton(0, "日期", 0.3f,
					new Tab.OptionInput(null, ap => TOD_Sky.Instance.Cycle.DateTime.ToString(), 0,
						true, null), new Tab.OptionButton("更改", ap =>
					{
						Singleton.DatePicker.Open(ap.Player, date =>
						{
							var hour = TOD_Sky.Instance.Cycle.Hour;

							TOD_Sky.Instance.Cycle.DateTime = date;
							TOD_Sky.Instance.Cycle.Hour = hour;

							Draw(tab);
							Singleton.Draw(ap.Player);
						});
					}));

				tab.AddToggle(0, "时间流逝",
					ap => TOD_Sky.Instance.Components.Time.ProgressTime =
						!TOD_Sky.Instance.Components.Time.ProgressTime,
					ap => TOD_Sky.Instance.Components.Time.ProgressTime);
				tab.AddRange(0, "时间", 0, 24, ap => TOD_Sky.Instance.Cycle.Hour,
					(ap, value) => { TOD_Sky.Instance.Cycle.Hour = value; },
					ap => $"{TOD_Sky.Instance.Cycle.Hour:0.0}");

				tab.AddName(0, "海洋");
				tab.AddRange(0, "缩放", -100f, 500f, ap => overrides.OceanScale * 100f,
					(ap, value) =>
					{
						overrides.OceanScale = value * .01f;
						ServerMgr.SendReplicatedVars("weather.");
					}, ap => $"{overrides.OceanScale:0.0}");
				tab.AddRange(0, "水位", 0, 500f, ap => WaterSystem.OceanLevel,
					(ap, value) =>
					{
						WaterSystem.OceanLevel = value;
						ServerMgr.SendReplicatedVars("env.");
					}, ap => $"{WaterSystem.OceanLevel:0.0}");
			}

			tab.AddDropdown(1, "天气预设", ap => LastWeatherPresetSelectedIndex, (ap, index) =>
			{
				overrides.Set(presets[LastWeatherPresetSelectedIndex = index]);
				ServerMgr.SendReplicatedVars("weather.");
			}, Options);

			tab.AddName(1, "环境");
			{
				tab.AddRange(1, "风速", -100f, 100f, ap => overrides.Wind * 100f,
					(ap, value) =>
					{
						overrides.Wind = value * .01f;
						ServerMgr.SendReplicatedVars("weather.");
					}, ap => $"{overrides.Wind:0.0}");
				tab.AddRange(1, "降雨", -100f, 100f, ap => overrides.Rain * 100f,
					(ap, value) =>
					{
						overrides.Rain = value * .01f;
						ServerMgr.SendReplicatedVars("weather.");
					}, ap => $"{overrides.Rain:0.0}");
				tab.AddRange(1, "雷暴", -100f, 100f, ap => overrides.Thunder * 100f,
					(ap, value) =>
					{
						overrides.Thunder = value * .01f;
						ServerMgr.SendReplicatedVars("weather.");
					}, ap => $"{overrides.Thunder:0.0}");
				tab.AddRange(1, "彩虹", -100f, 100f, ap => overrides.Rainbow * 100f,
					(ap, value) =>
					{
						overrides.Rainbow = value * .01f;
						ServerMgr.SendReplicatedVars("weather.");
					}, ap => $"{overrides.Rainbow:0.0}");

				tab.AddName(1, "大气");
				tab.AddRange(1, "瑞利散射", -100f, 500f, ap => overrides.Atmosphere.RayleighMultiplier * 100f,
					(ap, value) =>
					{
						overrides.Atmosphere.RayleighMultiplier = value * .01f;
						ServerMgr.SendReplicatedVars("weather.");
					}, ap => $"{overrides.Atmosphere.RayleighMultiplier:0.0}");
				tab.AddRange(1, "米氏散射", -100f, 500f, ap => overrides.Atmosphere.MieMultiplier * 100f,
					(ap, value) =>
					{
						overrides.Atmosphere.MieMultiplier = value * .01f;
						ServerMgr.SendReplicatedVars("weather.");
					}, ap => $"{overrides.Atmosphere.MieMultiplier:0.0}");
				tab.AddRange(1, "亮度", -100f, 500f, ap => overrides.Atmosphere.Brightness * 100f,
					(ap, value) =>
					{
						overrides.Atmosphere.Brightness = value * .01f;
						ServerMgr.SendReplicatedVars("weather.");
					}, ap => $"{overrides.Atmosphere.Brightness:0.0}");
				tab.AddRange(1, "对比度", -100f, 500f, ap => overrides.Atmosphere.Contrast * 100f,
					(ap, value) =>
					{
						overrides.Atmosphere.Contrast = value * .01f;
						ServerMgr.SendReplicatedVars("weather.");
					}, ap => $"{overrides.Atmosphere.Contrast:0.0}");
				tab.AddRange(1, "方向性", -100f, 500f, ap => overrides.Atmosphere.Directionality * 100f,
					(ap, value) =>
					{
						overrides.Atmosphere.Directionality = value * .01f;
						ServerMgr.SendReplicatedVars("weather.");
					}, ap => $"{overrides.Atmosphere.Directionality:0.0}");
				tab.AddRange(1, "雾气", -100f, 500f, ap => overrides.Atmosphere.Fogginess * 100f,
					(ap, value) =>
					{
						overrides.Atmosphere.Fogginess = value * .01f;
						ServerMgr.SendReplicatedVars("weather.");
					}, ap => $"{overrides.Atmosphere.Fogginess:0.0}");

				tab.AddName(1, "云层");
				tab.AddRange(1, "大小", -100f, 500f, ap => overrides.Clouds.Size * 100f,
					(ap, value) =>
					{
						overrides.Clouds.Size = value * .01f;
						ServerMgr.SendReplicatedVars("weather.");
					}, ap => $"{overrides.Clouds.Size:0.0}");
				tab.AddRange(1, "不透明度", -100f, 500f, ap => overrides.Clouds.Opacity * 100f,
					(ap, value) =>
					{
						overrides.Clouds.Opacity = value * .01f;
						ServerMgr.SendReplicatedVars("weather.");
					}, ap => $"{overrides.Clouds.Opacity:0.0}");
				tab.AddRange(1, "覆盖度", -100f, 500f, ap => overrides.Clouds.Coverage * 100f,
					(ap, value) =>
					{
						overrides.Clouds.Coverage = value * .01f;
						ServerMgr.SendReplicatedVars("weather.");
					}, ap => $"{overrides.Clouds.Coverage:0.0}");
				tab.AddRange(1, "锐度", -100f, 500f, ap => overrides.Clouds.Sharpness * 100f,
					(ap, value) =>
					{
						overrides.Clouds.Sharpness = value * .01f;
						ServerMgr.SendReplicatedVars("weather.");
					}, ap => $"{overrides.Clouds.Sharpness:0.0}");
				tab.AddRange(1, "着色", -100f, 500f, ap => overrides.Clouds.Coloring * 100f,
					(ap, value) =>
					{
						overrides.Clouds.Coloring = value * .01f;
						ServerMgr.SendReplicatedVars("weather.");
					}, ap => $"{overrides.Clouds.Coloring:0.0}");
				tab.AddRange(1, "衰减", -100f, 500f, ap => overrides.Clouds.Attenuation * 100f,
					(ap, value) =>
					{
						overrides.Clouds.Attenuation = value * .01f;
						ServerMgr.SendReplicatedVars("weather.");
					}, ap => $"{overrides.Clouds.Attenuation:0.0}");
				tab.AddRange(1, "饱和度", -100f, 500f, ap => overrides.Clouds.Saturation * 100f,
					(ap, value) =>
					{
						overrides.Clouds.Saturation = value * .01f;
						ServerMgr.SendReplicatedVars("weather.");
					}, ap => $"{overrides.Clouds.Saturation:0.0}");
				tab.AddRange(1, "散射", -100f, 500f, ap => overrides.Clouds.Scattering * 100f,
					(ap, value) =>
					{
						overrides.Clouds.Scattering = value * .01f;
						ServerMgr.SendReplicatedVars("weather.");
					}, ap => $"{overrides.Clouds.Scattering:0.0}");
				tab.AddRange(1, "亮度", -100f, 500f, ap => overrides.Clouds.Brightness * 100f,
					(ap, value) =>
					{
						overrides.Clouds.Brightness = value * .01f;
						ServerMgr.SendReplicatedVars("weather.");
					}, ap => $"{overrides.Clouds.Brightness:0.0}");

			}
		}
	}
}

#endif