
using System.Collections.Specialized;

namespace Kurs_DB.Models;

public class WaterMeterModel
{
	private int _id;

	private string _name;

	private string _manufacturer;

	private NameValueCollection _properties;

	public WaterMeterModel(int id, string name, string manufacturer, NameValueCollection properties)
	{
		_id = id;
		_name = name;
		_manufacturer = manufacturer;
		_properties = properties;
	}

	public int Id { get => _id; set => _id = value; }
	public string Name { get => _name; set => _name = value; }
	public string Manufacturer { get => _manufacturer; set => _manufacturer = value; }
	public NameValueCollection Properties { get => _properties; set => _properties = value; }
}