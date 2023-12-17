namespace Kurs_DB.Models;

public class LoginModel
{
	private int _id;

	private string _login;

	private string _password;

	public LoginModel(int id, string name, string password)
	{
		_id = id;
		_login = name;
		_password = password;
	}

	public int Id { get => _id; set => _id = value; }
	public string Login { get => _login; set => _login = value; }
	public string Password { get => _password; set => _password = value; }
}