namespace Kurs_DB.Models;

public class PropertyModel
{
	private int _id;

	private string _name;

	private List<string> _values;

	public PropertyModel(int id, string name, List<string> values)
	{
		_id = id;
		_name = name;
		_values = values;
	}

	public int Id { get => _id; set => _id = value; }
	public string Name { get => _name; set => _name = value; }
	public List<string> Values { get => _values; set => _values = value; }
}