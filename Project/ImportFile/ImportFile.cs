using Godot;
using System.IO;
using System.Xml;
using System.Xml.Serialization;

[XmlRoot("ImportFile")]
public class ImportFile
{
	public static ImportFile Load(string path)
	{
		XmlReader reader = XmlReader.Create(path);
		XmlSerializer serializer = new XmlSerializer(typeof(ImportFile));
		ImportFile file = (ImportFile)serializer.Deserialize(reader);
		reader.Close();
		return file;
	}

	public static void Save(string path, ImportFile file)
	{
		XmlWriterSettings settings = new XmlWriterSettings();
		settings.Indent = true;
		XmlWriter writer = XmlWriter.Create(path, settings);
		XmlSerializer serializer = new XmlSerializer(typeof(ImportFile));
		serializer.Serialize(writer, file);
		writer.Close();
	}

	public void Import(string source, string dest)
	{
		if(type == ConvertType.Copy)
		{
			File.Copy(source.GetBaseName(), dest); // includes path, excludes .import
		}
	}

	public enum ConvertType
	{
		[XmlEnum("Copy")]
		Copy,
		[XmlEnum("Tex")]
		Tex
	}

	[XmlAttribute("ConvertType")]
	public ConvertType type;
}