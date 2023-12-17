using Kurs_DB.Models;
using MySqlConnector;

class LoginDAL
{
	private readonly MySqlConnection _connection;

	public LoginDAL(MySqlConnection connection)
	{
		_connection = connection;
	}

	public List<LoginModel> GetAllFromDB()
	{
		List<LoginModel> temp = [];

		string query = @"SELECT * FROM admins";
		using (MySqlCommand cmd = new(query, _connection))
		{
			_connection.Open();
			using (MySqlDataReader sdr = cmd.ExecuteReader())
			{
				while (sdr.Read())
				{
					temp.Add(new LoginModel
					(
						Convert.ToInt32(sdr["admin_id"]),
						sdr["admin_login"].ToString(),
						sdr["admin_password"].ToString()
					));
				}

			}
		}
		_connection.Close();
		return temp;
	}
	public LoginModel GetAdminById(int admin_id)
	{
		LoginModel temp = null;
		string query = @"SELECT * FROM admins
			WHERE admin_id = @admin_id";
		using (MySqlCommand cmd = new(query, _connection))
		{
			cmd.Parameters.AddWithValue("@admin_id", admin_id);
			_connection.Open();
			using (MySqlDataReader sdr = cmd.ExecuteReader())
			{
				while (sdr.Read())
				{
					temp = new(admin_id,
					sdr["admin_login"].ToString(),
					sdr["admin_password"].ToString());
				}
			}
		}
		_connection.Close();
		return temp;
	}
	public void Add(string admin_login, string admin_password)
	{
		string query = @"INSERT INTO admins(admin_login, admin_password)
							VALUES(@admin_login, N@admin_password)";
		using (MySqlCommand cmd = new(query, _connection))
		{
			cmd.Parameters.AddWithValue("@admin_login", admin_login);
			cmd.Parameters.AddWithValue("@admin_password", admin_password);

			_connection.Open();
			cmd.ExecuteNonQuery();
		}
		_connection.Close();
	}

	public void Edit(int admin_id, string admin_login, string admin_password)
	{
		string query = @"UPDATE admins SET admin_login = @admin_login, admin_password = @admin_password
		WHERE admin_id = @admin_id";
		using (MySqlCommand cmd = new(query, _connection))
		{
			MySqlParameter[] parametrs = [new MySqlParameter("@admin_id" , admin_id),
			new MySqlParameter("@admin_login", admin_login), new MySqlParameter("@admin_password", admin_password)];
			cmd.Parameters.AddRange(parametrs);

			_connection.Open();
			cmd.ExecuteNonQuery();
		}
		_connection.Close();
	}

	public void Delete(int admin_id)
	{
		string query = @"DELETE FROM admins WHERE admin_id = @admin_id";
		using (MySqlCommand cmd = new(query, _connection))
		{
			cmd.Parameters.AddWithValue("@admin_id", admin_id);
			_connection.Open();
			cmd.ExecuteNonQuery();
		}
		_connection.Close();
	}
}
