using Godot;

public class FileImporter
{
	public static void Import(string path, string dest)
	{
		string baseName = path.GetBaseName();

		ImportFile file = ImportFile.Load(path);
		ImportFile.Save(path, file);

		file.Import(path, dest);
	}
}
