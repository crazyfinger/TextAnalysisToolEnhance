using System;
using System.Drawing;
using Microsoft.Win32;

namespace TATPlugin_AndroidLog;

public enum FilterColorMode
{
	Default = 0,    // Default black / system text color (Color.Empty)
	Random = 1,     // Distinguishable random / palette color
	Specified = 2   // User-specified custom color
}

public sealed class AndroidLogSettings
{
	private const string RegistryKeyPath = @"Software\DavidAns\TextAnalysisTool.NET\AndroidLogPlugin";

	public static readonly Color[] HighContrastPalette = new Color[]
	{
		Color.FromArgb(0, 114, 206),    // Vivid Blue
		Color.FromArgb(217, 83, 79),    // Crimson Red
		Color.FromArgb(46, 139, 87),    // Sea Green
		Color.FromArgb(153, 51, 204),   // Purple
		Color.FromArgb(230, 126, 34),   // Orange
		Color.FromArgb(0, 136, 145),    // Teal
		Color.FromArgb(178, 34, 34),    // Firebrick
		Color.FromArgb(70, 130, 180),   // Steel Blue
		Color.FromArgb(199, 21, 133),   // Medium Violet Red
		Color.FromArgb(139, 69, 19),    // Saddle Brown
		Color.FromArgb(30, 144, 255),   // Dodger Blue
		Color.FromArgb(34, 139, 34),    // Forest Green
		Color.FromArgb(148, 0, 211),    // Dark Violet
		Color.FromArgb(210, 105, 30),   // Chocolate
		Color.FromArgb(0, 128, 128),    // Dark Cyan
		Color.FromArgb(165, 42, 42),    // Brown
		Color.FromArgb(106, 90, 205),   // Slate Blue
		Color.FromArgb(47, 79, 79),     // Dark Slate Gray
		Color.FromArgb(184, 134, 11),   // Dark Goldenrod
		Color.FromArgb(138, 43, 226),   // Blue Violet
		Color.FromArgb(205, 92, 92),    // Indian Red
		Color.FromArgb(0, 100, 0),      // Dark Green
		Color.FromArgb(65, 105, 225),   // Royal Blue
		Color.FromArgb(192, 57, 43)     // Dark Red
	};

	private static readonly Random s_random = new Random();

	private static int s_lastRandomIndex = -1;

	public FilterColorMode TagColorMode { get; set; } = FilterColorMode.Specified;

	public Color TagForeColor { get; set; } = Color.FromArgb(46, 139, 87);

	public Color TagBackColor { get; set; } = Color.Empty;

	public FilterColorMode PidColorMode { get; set; } = FilterColorMode.Specified;

	public Color PidForeColor { get; set; } = Color.FromArgb(0, 114, 206);

	public Color PidBackColor { get; set; } = Color.Empty;

	public bool ConsistentRandomColor { get; set; } = true;

	public void Reset()
	{
		TagColorMode = FilterColorMode.Specified;
		TagForeColor = Color.FromArgb(46, 139, 87);
		TagBackColor = Color.Empty;
		PidColorMode = FilterColorMode.Specified;
		PidForeColor = Color.FromArgb(0, 114, 206);
		PidBackColor = Color.Empty;
		ConsistentRandomColor = true;
	}

	public void Load()
	{
		try
		{
			using RegistryKey key = Registry.CurrentUser.OpenSubKey(RegistryKeyPath);
			if (key == null)
			{
				return;
			}
			object tagMode = key.GetValue("TagColorMode");
			if (tagMode is int tagModeInt && Enum.IsDefined(typeof(FilterColorMode), tagModeInt))
			{
				TagColorMode = (FilterColorMode)tagModeInt;
			}
			object tagFore = key.GetValue("TagForeColor");
			if (tagFore is int tagForeInt)
			{
				TagForeColor = Color.FromArgb(tagForeInt);
			}
			object tagBack = key.GetValue("TagBackColor");
			if (tagBack is int tagBackInt)
			{
				TagBackColor = Color.FromArgb(tagBackInt);
			}
			object pidMode = key.GetValue("PidColorMode");
			if (pidMode is int pidModeInt && Enum.IsDefined(typeof(FilterColorMode), pidModeInt))
			{
				PidColorMode = (FilterColorMode)pidModeInt;
			}
			object pidFore = key.GetValue("PidForeColor");
			if (pidFore is int pidForeInt)
			{
				PidForeColor = Color.FromArgb(pidForeInt);
			}
			object pidBack = key.GetValue("PidBackColor");
			if (pidBack is int pidBackInt)
			{
				PidBackColor = Color.FromArgb(pidBackInt);
			}
			object consistent = key.GetValue("ConsistentRandomColor");
			if (consistent is int consistentInt)
			{
				ConsistentRandomColor = consistentInt != 0;
			}
		}
		catch
		{
		}
	}

	public void Save()
	{
		try
		{
			using RegistryKey key = Registry.CurrentUser.CreateSubKey(RegistryKeyPath);
			if (key == null)
			{
				return;
			}
			key.SetValue("TagColorMode", (int)TagColorMode, RegistryValueKind.DWord);
			key.SetValue("TagForeColor", TagForeColor.ToArgb(), RegistryValueKind.DWord);
			key.SetValue("TagBackColor", TagBackColor.ToArgb(), RegistryValueKind.DWord);
			key.SetValue("PidColorMode", (int)PidColorMode, RegistryValueKind.DWord);
			key.SetValue("PidForeColor", PidForeColor.ToArgb(), RegistryValueKind.DWord);
			key.SetValue("PidBackColor", PidBackColor.ToArgb(), RegistryValueKind.DWord);
			key.SetValue("ConsistentRandomColor", ConsistentRandomColor ? 1 : 0, RegistryValueKind.DWord);
		}
		catch
		{
		}
	}

	public AndroidLogSettings Clone()
	{
		return new AndroidLogSettings
		{
			TagColorMode = this.TagColorMode,
			TagForeColor = this.TagForeColor,
			TagBackColor = this.TagBackColor,
			PidColorMode = this.PidColorMode,
			PidForeColor = this.PidForeColor,
			PidBackColor = this.PidBackColor,
			ConsistentRandomColor = this.ConsistentRandomColor
		};
	}

	public void Apply(AndroidLogSettings other)
	{
		if (other == null)
		{
			return;
		}
		TagColorMode = other.TagColorMode;
		TagForeColor = other.TagForeColor;
		TagBackColor = other.TagBackColor;
		PidColorMode = other.PidColorMode;
		PidForeColor = other.PidForeColor;
		PidBackColor = other.PidBackColor;
		ConsistentRandomColor = other.ConsistentRandomColor;
	}

	public Color GetColorForTag(string tag, bool forceNewRandom = false)
	{
		switch (TagColorMode)
		{
		case FilterColorMode.Default:
			return Color.Empty;
		case FilterColorMode.Specified:
			return TagForeColor;
		case FilterColorMode.Random:
			return GetRandomColorForTag(tag, forceNewRandom);
		default:
			return Color.Empty;
		}
	}

	public Color GetColorForPid(int pid, bool forceNewRandom = false)
	{
		switch (PidColorMode)
		{
		case FilterColorMode.Default:
			return Color.Empty;
		case FilterColorMode.Specified:
			return PidForeColor;
		case FilterColorMode.Random:
			return GetRandomColorForPid(pid, forceNewRandom);
		default:
			return Color.Empty;
		}
	}

	public Color GetRandomColorForTag(string tag, bool forceNewRandom = false)
	{
		if (ConsistentRandomColor && !forceNewRandom && !string.IsNullOrEmpty(tag))
		{
			uint hash = 2166136261u;
			foreach (char c in tag)
			{
				hash = (hash ^ c) * 16777619u;
			}
			int idx = (int)(hash % (uint)HighContrastPalette.Length);
			return HighContrastPalette[idx];
		}
		return NextRandomPaletteColor();
	}

	public Color GetRandomColorForPid(int pid, bool forceNewRandom = false)
	{
		if (ConsistentRandomColor && !forceNewRandom && pid > 0)
		{
			uint hash = (uint)pid * 2654435761u;
			int idx = (int)(hash % (uint)HighContrastPalette.Length);
			return HighContrastPalette[idx];
		}
		return NextRandomPaletteColor();
	}

	private static Color NextRandomPaletteColor()
	{
		lock (s_random)
		{
			int idx = s_random.Next(HighContrastPalette.Length);
			if (idx == s_lastRandomIndex && HighContrastPalette.Length > 1)
			{
				idx = (idx + 1 + s_random.Next(HighContrastPalette.Length - 1)) % HighContrastPalette.Length;
			}
			s_lastRandomIndex = idx;
			return HighContrastPalette[idx];
		}
	}
}
