using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;

namespace TextAnalysisTool.NET;

internal static class ArchiveSupport
{
	private static readonly HashSet<string> s_archiveExtensions = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
	{
		".zip", ".7z", ".gz"
	};

	public static bool IsArchivePath(string path)
	{
		return !string.IsNullOrEmpty(path) && s_archiveExtensions.Contains(Path.GetExtension(path));
	}

	public static bool IsGzipPath(string path)
	{
		return string.Equals(Path.GetExtension(path), ".gz", StringComparison.OrdinalIgnoreCase);
	}

	public static bool Is7zPath(string path)
	{
		return string.Equals(Path.GetExtension(path), ".7z", StringComparison.OrdinalIgnoreCase);
	}

	private static string m_cached7zExe;

	public static bool Is7zAvailable()
	{
		return Find7zExe() != null;
	}

	public static string Find7zExe()
	{
		if (m_cached7zExe != null)
		{
			return m_cached7zExe;
		}
		foreach (string candidate in Get7zCandidates())
		{
			if (File.Exists(candidate))
			{
				m_cached7zExe = candidate;
				return candidate;
			}
		}
		return null;
	}

	private static IEnumerable<string> Get7zCandidates()
	{
		string pathValue = Environment.GetEnvironmentVariable("PATH");
		if (!string.IsNullOrEmpty(pathValue))
		{
			foreach (string dir in pathValue.Split(Path.PathSeparator))
			{
				if (!string.IsNullOrEmpty(dir))
				{
					yield return Path.Combine(dir, "7z.exe");
				}
			}
		}
		string programFiles = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
		string programFilesX86 = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86);
		string[] roots = new[] { programFiles, programFilesX86 };
		foreach (string root in roots)
		{
			if (string.IsNullOrEmpty(root))
			{
				continue;
			}
			yield return Path.Combine(root, "7-Zip", "7z.exe");
			yield return Path.Combine(root, "7zip", "7z.exe");
			yield return Path.Combine(root, "7-Zip", "7za.exe");
		}
	}

	/// <summary>Returns the display-name (or null) used to identify a plain/archive file's single source.</summary>
	public static string GetSourceName(string filePath)
	{
		string name = Path.GetFileName(filePath);
		if (IsGzipPath(name))
		{
			name = Path.GetFileNameWithoutExtension(name);
		}
		return name;
	}

	/// <summary>Lists file members inside an archive (zip/7z). Returns null if the path is not a multi-member archive.</summary>
	public static List<string> ListMembers(string archivePath, out string error)
	{
		error = null;
		if (string.IsNullOrEmpty(archivePath) || !File.Exists(archivePath))
		{
			return null;
		}
		if (Is7zPath(archivePath))
		{
			return List7zMembers(archivePath, out error);
		}
		if (string.Equals(Path.GetExtension(archivePath), ".zip", StringComparison.OrdinalIgnoreCase))
		{
			try
			{
				using (ZipArchive zip = ZipFile.OpenRead(archivePath))
				{
					return zip.Entries.Where(e => !string.IsNullOrEmpty(e.Name) && !e.FullName.EndsWith("/")).Select(e => e.FullName).ToList();
				}
			}
			catch (Exception e)
			{
				error = e.Message;
				return null;
			}
		}
		return null;
	}

	/// <summary>Opens a member of an archive as a TextReader. 7z members are extracted to the provided temp directory.</summary>
	public static TextReader OpenMember(string archivePath, string memberName, string tempDir, List<string> tempFiles)
	{
		if (Is7zPath(archivePath))
		{
			// extract member to temp dir then open
			string dest = Extract7zMember(archivePath, memberName, tempDir);
			if (dest == null)
			{
				return null;
			}
			tempFiles?.Add(dest);
			return new StreamReader(dest, Encoding.UTF8, detectEncodingFromByteOrderMarks: true);
		}
		if (string.Equals(Path.GetExtension(archivePath), ".zip", StringComparison.OrdinalIgnoreCase))
		{
			ZipArchive zip = ZipFile.OpenRead(archivePath);
			ZipArchiveEntry entry = zip.GetEntry(memberName);
			if (entry == null)
			{
				zip.Dispose();
				return null;
			}
			return new ZipEntryTextReader(entry);
		}
		return null;
	}

	public static TextReader OpenGzip(string filePath)
	{
		return new StreamReader(new GZipStream(new FileStream(filePath, FileMode.Open, FileAccess.Read), CompressionMode.Decompress), Encoding.UTF8, detectEncodingFromByteOrderMarks: true);
	}

	/// <summary>Extracts an archive member (zip/7z) to a file in tempDir and returns its path.</summary>
	public static string ExtractMemberToFile(string archivePath, string memberName, string tempDir)
	{
		if (Is7zPath(archivePath))
		{
			return Extract7zMember(archivePath, memberName, tempDir);
		}
		if (string.Equals(Path.GetExtension(archivePath), ".zip", StringComparison.OrdinalIgnoreCase))
		{
			try
			{
				Directory.CreateDirectory(tempDir);
				if (!IsSafeSubPath(tempDir, Path.GetFileName(memberName), out string dest))
				{
					return null;
				}
				using (ZipArchive zip = ZipFile.OpenRead(archivePath))
				{
					ZipArchiveEntry entry = zip.GetEntry(memberName);
					if (entry == null)
					{
						return null;
					}
					string dir = Path.GetDirectoryName(dest);
					if (!string.IsNullOrEmpty(dir))
					{
						Directory.CreateDirectory(dir);
					}
					entry.ExtractToFile(dest, overwrite: true);
				}
				return dest;
			}
			catch (Exception)
			{
				return null;
			}
		}
		return null;
	}

	private sealed class ZipEntryTextReader : TextReader
	{
		private readonly ZipArchive m_zip;

		private readonly StreamReader m_reader;

		public ZipEntryTextReader(ZipArchiveEntry entry)
		{
			m_zip = entry.Archive;
			m_reader = new StreamReader(entry.Open(), Encoding.UTF8, detectEncodingFromByteOrderMarks: true);
		}

		public override int Read(char[] buffer, int index, int count)
		{
			return m_reader.Read(buffer, index, count);
		}

		public override string ReadLine()
		{
			return m_reader.ReadLine();
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				m_reader.Dispose();
				m_zip.Dispose();
			}
			base.Dispose(disposing);
		}
	}

	private static List<string> List7zMembers(string archivePath, out string error)
	{
		error = null;
		string exe = Find7zExe();
		if (exe == null)
		{
			error = "7-Zip is not installed.";
			return null;
		}
		try
		{
			ProcessStartInfo psi = new ProcessStartInfo(exe, string.Format("l -slt -sccUTF-8 \"{0}\"", archivePath))
			{
				UseShellExecute = false,
				RedirectStandardOutput = true,
				RedirectStandardError = true,
				CreateNoWindow = true,
				StandardOutputEncoding = Encoding.UTF8
			};
			using (Process p = Process.Start(psi))
			{
				string stdout = p.StandardOutput.ReadToEnd();
				p.WaitForExit();
				List<string> members = new List<string>();
				string path = null;
				bool isFolder = false;
				foreach (string rawLine in stdout.Replace("\r", "").Split('\n'))
				{
					string line = rawLine.Trim();
					if (line.Length == 0)
					{
						if (path != null && !isFolder)
						{
							members.Add(path);
						}
						path = null;
						isFolder = false;
					}
					else if (line.StartsWith("Path = ", StringComparison.Ordinal))
					{
						path = line.Substring("Path = ".Length);
					}
					else if (line.StartsWith("Folder = ", StringComparison.Ordinal) && line.EndsWith("+"))
					{
						isFolder = true;
					}
				}
				if (path != null && !isFolder)
				{
					members.Add(path);
				}
				string archiveFullPath = Path.GetFullPath(archivePath);
				members.RemoveAll(m => string.Equals(m, archiveFullPath, StringComparison.OrdinalIgnoreCase) || string.Equals(m, Path.GetFileName(archivePath), StringComparison.OrdinalIgnoreCase));
				return members;
			}
		}
		catch (Exception e)
		{
			error = e.Message;
			return null;
		}
	}

	private static bool IsSafeSubPath(string basePath, string relativePath, out string fullPath)
	{
		fullPath = null;
		if (string.IsNullOrEmpty(basePath) || string.IsNullOrEmpty(relativePath))
		{
			return false;
		}
		try
		{
			string fullBase = Path.GetFullPath(basePath).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar) + Path.DirectorySeparatorChar;
			string candidate = Path.GetFullPath(Path.Combine(basePath, relativePath));
			if (candidate.StartsWith(fullBase, StringComparison.OrdinalIgnoreCase))
			{
				fullPath = candidate;
				return true;
			}
		}
		catch (Exception)
		{
		}
		return false;
	}

	private static string EscapeCommandLineArg(string arg)
	{
		if (string.IsNullOrEmpty(arg))
		{
			return "\"\"";
		}
		return "\"" + arg.Replace("\"", "\\\"") + "\"";
	}

	private static string Extract7zMember(string archivePath, string memberName, string tempDir)
	{
		string exe = Find7zExe();
		if (exe == null)
		{
			return null;
		}
		if (!IsSafeSubPath(tempDir, memberName, out string dest))
		{
			return null;
		}
		Directory.CreateDirectory(tempDir);
		string safeOutDir = tempDir.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
		string args = string.Format("x -y -o{0} {1} {2}", EscapeCommandLineArg(safeOutDir), EscapeCommandLineArg(archivePath), EscapeCommandLineArg(memberName));
		ProcessStartInfo psi = new ProcessStartInfo(exe, args)
		{
			UseShellExecute = false,
			RedirectStandardOutput = true,
			RedirectStandardError = true,
			CreateNoWindow = true
		};
		using (Process p = Process.Start(psi))
		{
			p.StandardOutput.ReadToEnd();
			p.StandardError.ReadToEnd();
			p.WaitForExit();
		}
		return File.Exists(dest) ? dest : null;
	}
}