using EldanToolkit.Project;
using Godot;
using System.IO;

public class FileImporter
{
	public static void Import(string path, string dest)
	{
		string baseName = path.GetBaseName();

		ImportFile file = ImportFile.Load(path);
		file.FixConvertData();
		ImportFile.Save(path, file);

		file.Import(path, dest);
	}
}
