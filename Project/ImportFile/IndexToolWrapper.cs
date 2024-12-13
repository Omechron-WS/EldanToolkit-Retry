using EldanToolkit.Project;
using Godot;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

public class IndexToolWrapper
{
	public static bool DoCompile(Dictionary<string, IEnumerable<string>> fileList, string outputPath) // Dictionary of base directories, with lists of relative file paths
	{
		try
		{
			if (!string.IsNullOrEmpty(ProgramSettings.IndexToolPath))
			{
				string xmlPath = Path.GetTempFileName(); // Creates empty temp file and gives path to it.
				IndexToolXML fileListXML = new IndexToolXML();

				fileListXML.folders = new ITFileFolder[fileList.Keys.Count];

				int i = 0;
				foreach (var dir in fileList)
				{
					ITFileFolder folder = new ITFileFolder();
					fileListXML.folders[i] = folder;
					i++;
					folder.basePath = dir.Key;
					folder.files = dir.Value.Select(f => new ITFile { filePath = f }).ToArray();
				}

				fileListXML.Write(xmlPath);

				ProcessStartInfo processStartInfo = new ProcessStartInfo { CreateNoWindow = true, FileName = ProgramSettings.IndexToolPath, UseShellExecute = false };
				processStartInfo.ArgumentList.Add("-inputxml=" + xmlPath);
				processStartInfo.ArgumentList.Add("-outputfile=" + outputPath);
				Process proc = Process.Start(processStartInfo);

				if (!proc.WaitForExit(5000))
				{
					proc.Close();
					return false;
				}

				return true;
			}
		}
		catch (Exception e)
		{
			GD.Print(e);
		}

		return false;
	}
}