using Kurs_DB.Models;
using MySqlConnector;

class ManufacturerDAL
{
	private readonly MySqlConnection _connection;

	public ManufacturerDAL(MySqlConnection connection)
	{
		_connection = connection;
	}

	public List<ManufacturerModel> GetAllFromDB()
	{
		List<ManufacturerModel> temp = [];

		string query = @"SELECT * FROM manufacturers";
		using (MySqlCommand cmd = new(query, _connection))
		{
			_connection.Open();
			using (MySqlDataReader sdr = cmd.ExecuteReader())
			{
				while (sdr.Read())
				{
					temp.Add(new ManufacturerModel
					(
						Convert.ToInt32(sdr["manufacturer_id"]),
						sdr["manufacturer_name"].ToString()
					));
				}

			}
		}
		_connection.Close();
		return temp;
	}
	public ManufacturerModel GetManufacturerById(int manufacturer_id)
	{
		ManufacturerModel temp = null;
		string query = @"SELECT * FROM manufacturers
			WHERE manufacturer_id = @manufacturer_id";
		using (MySqlCommand cmd = new(query, _connection))
		{
			cmd.Parameters.AddWithValue("@manufacturer_id", manufacturer_id);
			_connection.Open();
			using (MySqlDataReader sdr = cmd.ExecuteReader())
			{
				while (sdr.Read())
				{
					temp = new(manufacturer_id,
					sdr["manufacturer_name"].ToString());
				}
			}
		}
		_connection.Close();
		return temp;
	}
	public void Add(string manufacturer_name)
	{
		string query = @"INSERT INTO manufacturers(manufacturer_name)
							VALUES(@manufacturer_name)";
		using (MySqlCommand cmd = new(query, _connection))
		{
			cmd.Parameters.AddWithValue("@manufacturer_name", manufacturer_name);

			_connection.Open();
			cmd.ExecuteNonQuery();
		}
		_connection.Close();
	}

	public void Edit(int manufacturer_id, string manufacturer_name)
	{
		string query = @"UPDATE manufacturers SET manufacturer_name = @manufacturer_name
		WHERE manufacturer_id = @manufacturer_id";
		using (MySqlCommand cmd = new(query, _connection))
		{
			MySqlParameter[] parametrs = [new MySqlParameter("@manufacturer_id" , manufacturer_id), 
			new MySqlParameter("@manufacturer_name", manufacturer_name)];
			cmd.Parameters.AddRange(parametrs);

			_connection.Open();
			cmd.ExecuteNonQuery();
		}
		_connection.Close();
	}

	public void Delete(int manufacturer_id)
	{
		string query = @"DELETE FROM manufacturers WHERE manufacturer_id = @manufacturer_id";
		using (MySqlCommand cmd = new(query, _connection))
		{
			cmd.Parameters.AddWithValue("@manufacturer_id", manufacturer_id);
			_connection.Open();
			cmd.ExecuteNonQuery();
		}
		_connection.Close();
	}
}
