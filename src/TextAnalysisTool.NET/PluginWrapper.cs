using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Windows.Forms;
using TextAnalysisTool.NET.Plugin;
using ToolBox;

namespace TextAnalysisTool.NET;

internal sealed class PluginWrapper : IComparable, ITextAnalysisToolPlugin
{
	private readonly Type m_type;

	private readonly object m_pluginInstance;

	private readonly MethodInfo m_GetFileTypeDescription;

	private readonly MethodInfo m_GetFileTypePattern;

	private readonly MethodInfo m_IsFileTypeSupported;

	private readonly MethodInfo m_GetReaderForFile;

	private readonly MethodInfo m_GetSettingsDialog;

	private readonly MethodInfo m_GetLineContextMenuItems;

	private bool m_enabled;

	public string Name => m_type.Name;

	public string AssemblyLocation => m_type.Assembly.Location;

	public Version AssemblyVersion => m_type.Assembly.GetName().Version;

	public PluginWrapper(Type t)
	{
		m_type = t;
		m_pluginInstance = m_type.GetConstructor(Type.EmptyTypes).Invoke(null);
		m_GetFileTypeDescription = m_type.GetMethod("GetFileTypeDescription", Type.EmptyTypes);
		m_GetFileTypePattern = m_type.GetMethod("GetFileTypePattern", Type.EmptyTypes);
		m_IsFileTypeSupported = m_type.GetMethod("IsFileTypeSupported", new Type[1] { typeof(string) });
		m_GetReaderForFile = m_type.GetMethod("GetReaderForFile", new Type[1] { typeof(string) });
		Utility.ThrowExceptionIf(m_pluginInstance == null || null == m_GetFileTypeDescription || null == m_GetFileTypePattern || null == m_IsFileTypeSupported || null == m_GetReaderForFile, null);
		m_GetSettingsDialog = m_type.GetMethod("GetSettingsDialog", Type.EmptyTypes);
		m_GetLineContextMenuItems = m_type.GetMethod("GetLineContextMenuItems", new Type[1] { typeof(string) });
		m_enabled = true;
	}

	public string GetFileTypeDescription()
	{
		return (string)CallPluginSafely(typeof(string), m_GetFileTypeDescription, null, null);
	}

	public string GetFileTypePattern()
	{
		return (string)CallPluginSafely(typeof(string), m_GetFileTypePattern, null, null);
	}

	public bool IsFileTypeSupported(string fileName)
	{
		return (bool)CallPluginSafely(typeof(bool), m_IsFileTypeSupported, new object[1] { fileName }, false);
	}

	public TextReader GetReaderForFile(string fileName)
	{
		return (TextReader)CallPluginSafely(typeof(TextReader), m_GetReaderForFile, new object[1] { fileName }, null);
	}

	public Form GetSettingsDialog()
	{
		if (m_GetSettingsDialog == null)
		{
			return null;
		}
		return (Form)CallPluginSafely(typeof(Form), m_GetSettingsDialog, null, null);
	}

	public IList<PluginContextMenuItem> GetLineContextMenuItems(string lineText)
	{
		if (m_GetLineContextMenuItems == null)
		{
			return null;
		}
		return (IList<PluginContextMenuItem>)CallPluginSafely(typeof(IList<PluginContextMenuItem>), m_GetLineContextMenuItems, new object[1] { lineText }, null);
	}

	private object CallPluginSafely(Type t, MethodInfo mi, object[] oa, object failedReturn)
	{
		object obj = failedReturn;
		if (m_enabled)
		{
			try
			{
				try
				{
					obj = mi.Invoke(m_pluginInstance, oa);
				}
				catch (TargetInvocationException ex)
				{
					throw ex.GetBaseException();
				}
				Utility.ThrowExceptionIf(obj != null && !t.IsAssignableFrom(obj.GetType()), null);
			}
			catch (FileLoadException)
			{
				throw;
			}
			catch (Exception)
			{
				CTextAnalysisTool.ShowMessageBox(string.Format("An error occurred while attempting to use the {0} plug-in (from {1}).  The plug-in has been disabled for this instance of {2}.", Name, AssemblyLocation, "TextAnalysisTool.NET"), MessageBoxIcon.Asterisk);
				m_enabled = false;
				obj = failedReturn;
			}
		}
		return obj;
	}

	public int CompareTo(object obj)
	{
		PluginWrapper pluginWrapper = (PluginWrapper)obj;
		return Name.CompareTo(pluginWrapper.Name);
	}
}
