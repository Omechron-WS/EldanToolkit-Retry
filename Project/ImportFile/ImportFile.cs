using Godot;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml;
using System.Xml.Serialization;

[XmlRoot("ImportFile")]
public class ImportFile
{
	public static ImportFile Load(string path)
	{
		using var reader = XmlReader.Create(path);
		XmlSerializer serializer = new XmlSerializer(typeof(ImportFile), TypeList());
		return (ImportFile)serializer.Deserialize(reader);
	}

	public static void Save(string path, ImportFile file)
	{
		XmlWriterSettings settings = new XmlWriterSettings { Indent = true };
		using var writer = XmlWriter.Create(path, settings);
		XmlSerializer serializer = new XmlSerializer(typeof(ImportFile), TypeList());
		serializer.Serialize(writer, file);
	}

	public static Type[] TypeList()
	{
		// use reflection to get all derived types
		return Assembly.GetExecutingAssembly().GetTypes().Where(
			t => typeof(ConvertData).IsAssignableFrom(t)).ToArray();

	}

	public void Import(string source, string dest)
	{
		if(File.Exists(dest))
		{
			File.Delete(dest);
		}
		if(type == ConvertType.Copy)
		{
			File.Copy(source.GetBaseName(), dest); // includes path, excludes .import
		}
		else if(convertData != null)
		{
			convertData.Import(source, dest);
		}
	}

	public enum ConvertType
	{
		[XmlEnum("Copy")]
		Copy,
		PNG2TEX
	}

	public void FixConvertData()
	{
		switch(type)
		{
			case ConvertType.Copy:
				convertData = null;
				break;
			case ConvertType.PNG2TEX:
				convertData = convertData as PNG2TEX ?? new PNG2TEX();
				break;
		}
	}

	[XmlAttribute("ConvertType")]
	public ConvertType type;

	[XmlElement("ConvertData")]
	public ConvertData convertData = null;
}

[XmlType("ConvertData")]
public abstract class ConvertData
{
	public abstract void Import(string source, string dest);
}

[XmlType("PNG2TEX")]
public class PNG2TEX : ConvertData
{
	[XmlAttribute("Depth")]
	public int depth = 1; // for LUTs and 3D textures
	[XmlAttribute("Mipmaps")]
	public int mipmaps = 1;

	public enum TexType
	{
		JPG1,
		JPG2,
		JPG3,
		ARGB1,
		ARGB2,
		RGB,
		GRAYSCALE,
		DXT1,
		DXT3,
		DXT5,
	}
	[XmlAttribute("TexType")]
	public TexType texType = TexType.DXT3;

	public override void Import(string source, string dest)
	{
		string srcfile = source.GetBaseName();
		if(srcfile.GetExtension().ToLower() != "tex")
		{
			return;
		}
		NexusVaultWrapper.DoConvert(srcfile.GetBaseName() + ".png", dest, $"--png2tex-mipmaps {mipmaps}", $"--png2tex-depth {depth}", $"--png2tex-type {texType.ToString()}");
	}
}