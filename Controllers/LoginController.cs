using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Kurs_DB.Models;
using MySqlConnector;
using System.Collections.Specialized;
using System.Security.Cryptography;
using System.Text;

namespace Kurs_DB.Controllers
{
	public class LoginController : Controller
	{
		private readonly MySqlConnection _connection;
		private readonly ILogger<LoginController> _logger;
		private readonly LoginDAL lAL;
		public LoginController(ILogger<LoginController> logger, MySqlConnection connection)
		{
			_logger = logger;
			_connection = connection;
			lAL = new(_connection);
		}

		public IActionResult Index(string message)
		{
			return View(model: message);
		}

		public IActionResult AdminManagment()
		{
			List<LoginModel> models = lAL.GetAllFromDB().Select(x => { x.Password = Decrypt(x.Password); return x; }).ToList();
			return View(models);
		}
		[HttpPost]
		public IActionResult Confirmation(string login, string password)
		{
			var users = lAL.GetAllFromDB();
			foreach (var user in users)
			{
				if ((user.Login == login) && (Decrypt(user.Password) == password)) //need to decrypt
				{
					if (user.Login == "superuser")
						HttpContext.Session.SetString("Role", "Superuser");
					HttpContext.Session.SetString("Role", "Admin");
					HttpContext.Session.SetString("Username", user.Login);
				}
			}
			if (HttpContext.Session.GetString("Role") == "user")
				return RedirectToAction("Index", "Login", new { message = "Wrong" });

			return RedirectToAction("Index", "WaterMeter");
		}

		public IActionResult Logout()
		{
			HttpContext.Session.Clear();
			return RedirectToAction("Index", "WaterMeter");
		}

		[HttpPost]
		public IActionResult Add(string login, string password)
		{
			if (lAL.GetAllFromDB().Where(x => x.Login.Equals(login, StringComparison.OrdinalIgnoreCase)).Any())
			{
				HttpContext.Session.SetString("Error",
				"Неможливо додати адміна, у таблиці уже присутній такий адмін");
			}
			else
			{
				if (login != "superuser" && lAL.GetAllFromDB().Select(x => x.Login != login).Any())
				{
					lAL.Add(login, Encrypt(password));
				}
			}
			return RedirectToAction("AdminManagment");
		}

		[HttpPost]
		public IActionResult Delete(int admin_id)
		{
			lAL.Delete(admin_id);

			return RedirectToAction("AdminManagment");
		}

		[HttpGet]
		public IActionResult Search(string search_value)
		{
			var admins = lAL.GetAllFromDB().Select(x => { x.Password = Decrypt(x.Password); return x; }).ToList();
			List<LoginModel> temp = [];
			if (search_value != null)
			{
				string[] search_split = search_value.Split(" ");

				foreach (LoginModel a in admins)
				{
					foreach (string s in search_split)
					{
						if (a.Login.Contains(s, StringComparison.OrdinalIgnoreCase))
						{
							temp.Add(a);
						}
					}
				}
			}
			Tuple<List<LoginModel>, string> tuple = new(temp, search_value);
			return View("Search", tuple);
		}

		[HttpPost]
		public IActionResult Edit(int admin_id)
		{
			LoginModel Model = lAL.GetAdminById(admin_id);
			Model.Password = Decrypt(Model.Password);
			return View(Model);
		}

		[HttpPost]
		public IActionResult SaveEditChanges(int admin_id, string login, string password)
		{
			if (lAL.GetAllFromDB().Where(x => x.Login.Equals(login, StringComparison.OrdinalIgnoreCase)
			&& admin_id != x.Id).Any())
			{
				HttpContext.Session.SetString("Error",
				$"Неможливо змінити логін адміна на {login}, у таблиці уже присутній такий адмін");
			}
			else
			{
				lAL.Edit(admin_id, login, Encrypt(password));
			}

			return RedirectToAction("AdminManagment");
		}

		[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
		public IActionResult Error()
		{
			return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
		}

		private static string Encrypt(string clearText)
		{
			string encryptionKey = "MAKV2SPBNI99212";
			byte[] clearBytes = Encoding.Unicode.GetBytes(clearText);
			using (Aes encryptor = Aes.Create())
			{
				Rfc2898DeriveBytes pdb = new Rfc2898DeriveBytes(encryptionKey, new byte[] { 0x49, 0x76, 0x61, 0x6e, 0x20, 0x4d, 0x65, 0x64, 0x76, 0x65, 0x64, 0x65, 0x76 });
				encryptor.Key = pdb.GetBytes(32);
				encryptor.IV = pdb.GetBytes(16);
				using (MemoryStream ms = new MemoryStream())
				{
					using (CryptoStream cs = new CryptoStream(ms, encryptor.CreateEncryptor(), CryptoStreamMode.Write))
					{
						cs.Write(clearBytes, 0, clearBytes.Length);
						cs.Close();
					}
					clearText = Convert.ToBase64String(ms.ToArray());
				}
			}
			return clearText;
		}

		private static string Decrypt(string cipherText)
		{
			string encryptionKey = "MAKV2SPBNI99212";
			byte[] cipherBytes = Convert.FromBase64String(cipherText);
			using (Aes encryptor = Aes.Create())
			{
				Rfc2898DeriveBytes pdb = new Rfc2898DeriveBytes(encryptionKey, new byte[] { 0x49, 0x76, 0x61, 0x6e, 0x20, 0x4d, 0x65, 0x64, 0x76, 0x65, 0x64, 0x65, 0x76 });
				encryptor.Key = pdb.GetBytes(32);
				encryptor.IV = pdb.GetBytes(16);
				using (MemoryStream ms = new MemoryStream())
				{
					using (CryptoStream cs = new CryptoStream(ms, encryptor.CreateDecryptor(), CryptoStreamMode.Write))
					{
						cs.Write(cipherBytes, 0, cipherBytes.Length);
						cs.Close();
					}
					cipherText = Encoding.Unicode.GetString(ms.ToArray());
				}
			}
			return cipherText;
		}

	}

}
