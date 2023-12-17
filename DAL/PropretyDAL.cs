using Kurs_DB.Models;
using MySqlConnector;

class PropertyDAL
{
	private readonly MySqlConnection _connection;

	public PropertyDAL(MySqlConnection connection)
	{
		_connection = connection;
	}

	public List<PropertyModel> GetAllFromDB()
	{
		List<PropertyModel> temp = [];

		string query = @"SELECT * FROM properties p";
		using (MySqlCommand cmd = new(query, _connection))
		{
			_connection.Open();
			using (MySqlDataReader sdr = cmd.ExecuteReader())
			{
				while (sdr.Read())
				{
					temp.Add(new PropertyModel
					(
						Convert.ToInt32(sdr["property_id"]),
						sdr["property_name"].ToString(),
						[]
					));
				}
			}

			foreach (var property in temp)
			{
				property.Values = GetPropertyValuesById(cmd, property.Id);
			}
		}
		_connection.Close();
		return temp;
	}

	public List<string> GetPropertyValuesById(MySqlCommand cmd, int property_id)
	{
		List<string> temp = [];
		cmd.Parameters.Clear();
		cmd.CommandText = "SELECT property_value FROM property_values WHERE property_id = @property_id";
		cmd.Parameters.AddWithValue("@property_id", property_id);
		using (MySqlDataReader sdr = cmd.ExecuteReader())
		{
			while (sdr.Read())
			{
				temp.Add(sdr["property_value"].ToString());
			}
		}
		return temp;
	}

	public PropertyModel GetPropertyById(int property_id)
	{
		PropertyModel temp = null;
		string query = @"SELECT * FROM properties p WHERE property_id = @property_id";
		using (MySqlCommand cmd = new(query, _connection))
		{
			cmd.Parameters.AddWithValue("@property_id", property_id);
			_connection.Open();
			using (MySqlDataReader sdr = cmd.ExecuteReader())
			{
				while (sdr.Read())
				{
					temp = new(property_id,
					sdr["property_name"].ToString(),
					[]);
				}
			}
			temp.Values = GetPropertyValuesById(cmd, property_id);
		}
		_connection.Close();
		return temp;
	}

	public void Add(string property_name, List<string> property_values)
	{
		string query = @"INSERT INTO properties(property_name) VALUES(@property_name)";
		using (MySqlCommand cmd = new(query, _connection))
		{
			cmd.Parameters.AddWithValue("@property_name", property_name);

			_connection.Open();
			cmd.ExecuteNonQuery();

			long property_id = cmd.LastInsertedId;

			foreach (string p_value in property_values)
			{
				cmd.Parameters.Clear();
				cmd.CommandText = @"INSERT INTO property_values(property_id, property_value)
				VALUES(@property_id, @property_value)";

				cmd.Parameters.AddWithValue("@property_id", property_id);
				cmd.Parameters.AddWithValue("@property_value", p_value);

				cmd.ExecuteNonQuery();
			}
		}
		_connection.Close();
	}

	public void Edit(int property_id, string property_name, List<string> property_values)
	{
		string query = @"UPDATE properties SET property_name = @property_name
		WHERE property_id = @property_id";
		using (MySqlCommand cmd = new(query, _connection))
		{
			cmd.Parameters.AddWithValue("@property_id", property_id);
			cmd.Parameters.AddWithValue("@property_name", property_name);

			_connection.Open();
			cmd.ExecuteNonQuery();
			int k = 0;
			foreach (string p_value in property_values)
			{
				cmd.Parameters.Clear();

				cmd.CommandText = @"SELECT property_value_id FROM property_values 
				WHERE property_id = @property_id LIMIT 1 OFFSET @k";
				cmd.Parameters.AddWithValue("@property_id", property_id);
				cmd.Parameters.AddWithValue("@k", k);
				int property_value_id = 0;
				k++;

				using (MySqlDataReader sdr = cmd.ExecuteReader())
				{
					while (sdr.Read())
					{
						property_value_id = Convert.ToInt32(sdr["property_value_id"]);
					}
				}

				if (property_value_id != 0)
				{
					cmd.CommandText = @"UPDATE property_values SET property_value = @property_value
				WHERE property_value_id = @property_value_id";

					cmd.Parameters.AddWithValue("@property_value_id", property_value_id);
				}
				else
				{
					cmd.CommandText = @"INSERT INTO property_values(property_id, property_value)
				VALUES(@property_id, @property_value)";
				}

				cmd.Parameters.AddWithValue("@property_value", p_value);

				cmd.ExecuteNonQuery();
			}
		}
		_connection.Close();
	}

	public void Delete(int property_id)
	{
		string query = @"DELETE FROM property_values WHERE property_id = @property_id";
		using (MySqlCommand cmd = new(query, _connection))
		{
			cmd.Parameters.AddWithValue("@property_id", property_id);
			_connection.Open();
			cmd.ExecuteNonQuery();

			cmd.CommandText = @"DELETE FROM properties WHERE property_id = @property_id";
			cmd.ExecuteNonQuery();
		}
		_connection.Close();
	}

	public void DeletePropertyValue(int property_id, string property_value)
	{
		string query = @"DELETE FROM property_values WHERE property_id = @property_id
		 AND property_value = @property_value";
		using (MySqlCommand cmd = new(query, _connection))
		{
			cmd.Parameters.AddWithValue("@property_id", property_id);
			cmd.Parameters.AddWithValue("@property_value", property_value);
			_connection.Open();
			cmd.ExecuteNonQuery();
		}
		_connection.Close();
	}
}
