using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace TextAnalysisTool.NET;

[DesignerCategory("")]
public sealed class ColorComboBox : ComboBox
{
	private readonly SolidBrush m_brush;

	private Color[] m_colorsArray;

	private int m_customColorIndex;

	private bool m_haveIndeterminateColor;

	private static readonly Color[] m_foreColors = new Color[35]
	{
		Color.Empty,
		Color.Maroon,
		Color.DarkGreen,
		Color.Olive,
		Color.DarkBlue,
		Color.Purple,
		Color.Aquamarine,
		Color.Red,
		Color.Green,
		Color.Yellow,
		Color.Blue,
		Color.Magenta,
		Color.Cyan,
		Color.CadetBlue,
		Color.Chartreuse,
		Color.Chocolate,
		Color.CornflowerBlue,
		Color.Crimson,
		Color.DarkCyan,
		Color.DarkMagenta,
		Color.DeepPink,
		Color.Firebrick,
		Color.Goldenrod,
		Color.HotPink,
		Color.Indigo,
		Color.Lime,
		Color.Orange,
		Color.Salmon,
		Color.SeaGreen,
		Color.SlateBlue,
		Color.Teal,
		Color.Violet,
		Color.Black,
		Color.White,
		Color.Empty
	};

	private static readonly Color[] m_backColors = new Color[23]
	{
		Color.Empty,
		Color.LightPink,
		Color.LightSalmon,
		Color.LightCoral,
		Color.Plum,
		Color.Gainsboro,
		Color.LightGray,
		Color.LightSlateGray,
		Color.PaleTurquoise,
		Color.LightBlue,
		Color.LightSkyBlue,
		Color.RoyalBlue,
		Color.Aquamarine,
		Color.LightGreen,
		Color.Green,
		Color.Khaki,
		Color.Yellow,
		Color.Orange,
		Color.Red,
		Color.DarkViolet,
		Color.Black,
		Color.White,
		Color.Empty
	};

	private bool m_chooseBackgroundColors;

	[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
	public bool Initialized { get; set; }

	[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
	public Func<bool, int[]> GetCustomColors { get; set; } = (bool _) => new int[0];


	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public new ObjectCollection Items => base.Items;

	[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
	public bool BackgroundColorComboBox
	{
		get
		{
			return m_chooseBackgroundColors;
		}
		set
		{
			if (m_chooseBackgroundColors != value)
			{
				m_chooseBackgroundColors = value;
				m_colorsArray = (m_chooseBackgroundColors ? m_backColors : m_foreColors);
				m_customColorIndex = m_colorsArray.Length - 1;
				Items.Clear();
				Color[] colorsArray = m_colorsArray;
				foreach (Color color in colorsArray)
				{
					Items.Add(color);
				}
			}
		}
	}

	[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
	public bool IsIndeterminateColor
	{
		get
		{
			if (m_haveIndeterminateColor)
			{
				return SelectedIndex == 0;
			}
			return false;
		}
	}

	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public Color SelectedColor
	{
		get
		{
			if (SelectedIndex < 0 || SelectedIndex >= m_colorsArray.Length)
			{
				SelectedIndex = 0;
			}
			return m_colorsArray[SelectedIndex];
		}
		set
		{
			SelectedIndex = 0;
			if (!(Color.Empty != value))
			{
				return;
			}
			for (int i = 1; i < m_colorsArray.Length; i++)
			{
				Color color = m_colorsArray[i];
				if (value.R == color.R && value.G == color.G && value.B == color.B)
				{
					SelectedIndex = i;
					break;
				}
			}
			if (SelectedIndex == 0)
			{
				m_colorsArray[m_customColorIndex] = value;
				SelectedIndex = m_customColorIndex;
			}
		}
	}

	public ColorComboBox()
		: this(chooseBackgroundColor: false)
	{
	}

	public ColorComboBox(bool chooseBackgroundColor = false)
	{
		m_chooseBackgroundColors = chooseBackgroundColor;
		m_brush = new SolidBrush(Color.Empty);
		base.DrawMode = DrawMode.OwnerDrawFixed;
		base.DropDownStyle = ComboBoxStyle.DropDownList;
		base.SelectedIndexChanged += ColorComboBox_SelectedIndexChanged;
		m_colorsArray = (m_chooseBackgroundColors ? m_backColors : m_foreColors);
		m_customColorIndex = m_colorsArray.Length - 1;
		Items.Clear();
		Color[] colorsArray = m_colorsArray;
		foreach (Color color in colorsArray)
		{
			Items.Add(color);
		}
		SelectedIndex = 0;
	}

	public void AddIndeterminateColor()
	{
		m_haveIndeterminateColor = true;
		Color[] array = new Color[m_colorsArray.Length + 1];
		array[0] = Color.Empty;
		for (int i = 0; i < m_colorsArray.Length; i++)
		{
			array[i + 1] = m_colorsArray[i];
		}
		m_colorsArray = array;
		m_customColorIndex = m_colorsArray.Length - 1;
		Items.Clear();
		Color[] colorsArray = m_colorsArray;
		foreach (Color color in colorsArray)
		{
			Items.Add(color);
		}
	}

	private void ColorComboBox_SelectedIndexChanged(object sender, EventArgs e)
	{
		if (Initialized && SelectedIndex == m_customColorIndex)
		{
			ColorDialog colorDialog = new ColorDialog
			{
				AllowFullOpen = true,
				Color = m_colorsArray[m_customColorIndex],
				CustomColors = GetCustomColors(m_chooseBackgroundColors)
			};
			if (colorDialog.ShowDialog() == DialogResult.OK)
			{
				m_colorsArray[m_customColorIndex] = colorDialog.Color;
			}
		}
	}

	protected override void OnDrawItem(DrawItemEventArgs e)
	{
		e.DrawBackground();
		m_brush.Color = e.ForeColor;
		int num = e.Index;
		if (num < 0 || num >= m_colorsArray.Length)
		{
			num = 0;
		}
		string s;
		if (num == 0)
		{
			s = ((!m_haveIndeterminateColor) ? "[Default]" : "[multiple]");
		}
		else if (m_customColorIndex == num)
		{
			s = "[Custom]";
		}
		else
		{
			Color color = m_colorsArray[num];
			s = color.Name;
			if ((e.State & DrawItemState.Selected) != DrawItemState.Selected)
			{
				if (m_chooseBackgroundColors)
				{
					e.Graphics.FillRectangle(new SolidBrush(color), e.Bounds);
					float colorBrightness = GetColorBrightness(color);
					m_brush.Color = (((double)colorBrightness >= 0.5) ? Color.Black : Color.White);
				}
				else
				{
					if (color == Color.White)
					{
						e.Graphics.FillRectangle(new SolidBrush(Color.Black), e.Bounds);
					}
					m_brush.Color = color;
				}
			}
			else
			{
				m_brush.Color = Color.White;
			}
			if (color == Color.Empty)
			{
				s = "[Default]";
				m_brush.Color = e.ForeColor;
			}
		}
		e.DrawFocusRectangle();
		if (!base.Enabled)
		{
			m_brush.Color = SystemColors.GrayText;
		}
		e.Graphics.DrawString(s, e.Font, m_brush, e.Bounds);
	}

	private static float GetColorBrightness(Color color)
	{
		return (float)Math.Sqrt((double)(color.R * color.R) * 0.241 + (double)(color.G * color.G) * 0.691 + (double)(color.B * color.B) * 0.068) / 255f;
	}
}
