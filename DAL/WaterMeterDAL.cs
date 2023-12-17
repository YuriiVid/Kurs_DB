using System.Collections.Specialized;
using Kurs_DB.Models;
using Microsoft.AspNetCore.Mvc;
using MySqlConnector;

class WaterMeterDAL

{
	private readonly MySqlConnection _connection;

	public WaterMeterDAL(MySqlConnection connection)
	{
		_connection = connection;
	}

	public List<WaterMeterModel> GetAllFromDB()
	{
		List<WaterMeterModel> temp = [];

		string query = @"SELECT wm.*, m.manufacturer_name FROM water_meters wm 
			INNER JOIN manufacturers m ON m.manufacturer_id = wm.manufacturer_id";
		using (MySqlCommand cmd = new(query, _connection))
		{
			_connection.Open();
			using (MySqlDataReader sdr = cmd.ExecuteReader())
			{
				while (sdr.Read())
				{
					temp.Add(new WaterMeterModel
					(
						Convert.ToInt32(sdr["water_meter_id"]),
						sdr["water_meter_name"].ToString(),
						sdr["manufacturer_name"].ToString(),
						[]
					));
				}
			}

			foreach (var wm in temp)
			{
				wm.Properties = GetWMPropertiesById(cmd, wm.Id);
			}
		}
		_connection.Close();
		return temp;
	}

	public NameValueCollection GetWMPropertiesById(MySqlCommand cmd, int waterMeter_id)
	{
		NameValueCollection temp = [];

		cmd.Parameters.Clear();
		cmd.CommandText = $@"SELECT property_name, property_value FROM water_meter_properties wmp 
							INNER JOIN properties p ON wmp.property_id = p.property_id
							INNER JOIN property_values pv ON wmp.property_value_id = pv.property_value_id
							WHERE water_meter_id = @water_meter_id";
		cmd.Parameters.AddWithValue("@water_meter_id", waterMeter_id);
		using (MySqlDataReader sdr = cmd.ExecuteReader())
		{
			while (sdr.Read())
			{
				temp.Add(sdr["property_name"].ToString(), sdr["property_value"].ToString());
			}
		}
		return temp;
	}

	public WaterMeterModel GetModelById(int waterMeter_id)
	{
		WaterMeterModel temp = null;
		string query = @"SELECT wm.*, m.manufacturer_name FROM water_meters wm 
			INNER JOIN manufacturers m ON m.manufacturer_id = wm.manufacturer_id
			WHERE water_meter_id = @water_meter_id";
		using (MySqlCommand cmd = new(query, _connection))
		{
			cmd.Parameters.AddWithValue("@water_meter_id", waterMeter_id);
			_connection.Open();
			using (MySqlDataReader sdr = cmd.ExecuteReader())
			{
				while (sdr.Read())
				{
					temp = new(waterMeter_id,
					sdr["water_meter_name"].ToString(),
					sdr["manufacturer_name"].ToString(),
					[]);
				}
			}
			temp.Properties = GetWMPropertiesById(cmd, waterMeter_id);
		}
		_connection.Close();
		return temp;
	}

	public void Add(string name, int manufacturer_id, NameValueCollection properties)
	{
		string query = @"INSERT INTO water_meters(water_meter_name, manufacturer_id)
							VALUES(@water_meter_name, @manufacturer_id)";
		using (MySqlCommand cmd = new(query, _connection))
		{
			MySqlParameter[] parametrs = [new MySqlParameter("@water_meter_name", name),
			new MySqlParameter("@manufacturer_id" , manufacturer_id)];
			cmd.Parameters.AddRange(parametrs);

			_connection.Open();
			cmd.ExecuteNonQuery();

			long waterMeter_id = cmd.LastInsertedId;

			foreach (string p_name in properties)
			{
				cmd.Parameters.Clear();
				cmd.CommandText = @"SELECT property_id FROM properties WHERE property_name = @property_name;" +
							"SELECT property_value_id FROM property_values WHERE property_value = @property_value";
				cmd.Parameters.AddWithValue("@property_name", p_name);
				cmd.Parameters.AddWithValue("@property_value", properties[p_name]);

				using (MySqlDataReader sdr = cmd.ExecuteReader())
				{
					while (sdr.Read())
					{
						cmd.Parameters.AddWithValue("@property_id", sdr["property_id"]);
					}
					sdr.NextResult();
					while (sdr.Read())
					{
						cmd.Parameters.AddWithValue("@property_value_id", sdr["property_value_id"]);
					}
				}

				cmd.Parameters.AddWithValue("@water_meter_id", waterMeter_id);
				cmd.CommandText = @"INSERT INTO water_meter_properties(water_meter_id, property_id, property_value_id)
							VALUES(@water_meter_id, @property_id, @property_value_id)";
				cmd.ExecuteNonQuery();
			}
		}
		_connection.Close();
	}

	public void Edit(int waterMeter_id, string name, int manufacturer_id, NameValueCollection properties)
	{
		string query = @"UPDATE water_meters SET water_meter_name = @water_meter_name,
		manufacturer_id = @manufacturer_id WHERE water_meter_id = @water_meter_id";
		using (MySqlCommand cmd = new(query, _connection))
		{
			MySqlParameter[] parametrs = [new MySqlParameter("@water_meter_name", name),
			new MySqlParameter("@manufacturer_id" , manufacturer_id), new MySqlParameter("@water_meter_id", waterMeter_id)];
			cmd.Parameters.AddRange(parametrs);

			_connection.Open();
			cmd.ExecuteNonQuery();

			foreach (string p_name in properties)
			{
				cmd.Parameters.Clear();
				cmd.CommandText = @"SELECT property_id FROM properties WHERE property_name = @property_name;" +
							"SELECT property_value_id FROM property_values WHERE property_value = @property_value";
				cmd.Parameters.AddWithValue("@property_name", p_name);
				cmd.Parameters.AddWithValue("@property_value", properties[p_name]);

				using (MySqlDataReader sdr = cmd.ExecuteReader())
				{
					while (sdr.Read())
					{
						cmd.Parameters.AddWithValue("@property_id", sdr["property_id"]);
					}
					sdr.NextResult();
					while (sdr.Read())
					{
						cmd.Parameters.AddWithValue("@property_value_id", sdr["property_value_id"]);
					}
				}
				cmd.Parameters.AddWithValue("@water_meter_id", waterMeter_id);
				cmd.CommandText = @"UPDATE water_meter_properties SET property_value_id = @property_value_id
				WHERE water_meter_id = @water_meter_id AND property_id = @property_id";
				
				cmd.ExecuteNonQuery();
			}
		}
		_connection.Close();
	}

	public void Delete(int waterMeter_id)
	{
		string query = @"DELETE FROM water_meter_properties WHERE water_meter_id = @water_meter_id";
		using (MySqlCommand cmd = new(query, _connection))
		{
			cmd.Parameters.AddWithValue("@water_meter_id", waterMeter_id);
			_connection.Open();
			cmd.ExecuteNonQuery();

			cmd.CommandText = @"DELETE FROM water_meters WHERE water_meter_id = @water_meter_id";
			cmd.ExecuteNonQuery();
		}
		_connection.Close();
	}
}
