using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Kurs_DB.Models;
using MySqlConnector;

namespace Kurs_DB.Controllers
{
	public class ManufacturerController : Controller
	{
		private readonly MySqlConnection _connection;
		private readonly ILogger<ManufacturerController> _logger;
		private readonly ManufacturerDAL mAL;
		private readonly WaterMeterDAL wAL;
		public ManufacturerController(ILogger<ManufacturerController> logger, MySqlConnection connection)
		{
			_logger = logger;
			_connection = connection;
			mAL = new(_connection);
			wAL = new(_connection);
		}

		public IActionResult Index()
		{
			List<ManufacturerModel> Model = new(mAL.GetAllFromDB());
			return View(Model);
		}

		[HttpPost]
		public IActionResult Add(string manufacturer_name)
		{
			if (mAL.GetAllFromDB().Where(x => x.Name.Equals(manufacturer_name, StringComparison.OrdinalIgnoreCase)).Any())
			{
				HttpContext.Session.SetString("Error",
				"Неможливо додати виробника, у таблиці уже присутній такий виробник");
			}
			else
			{
				mAL.Add(manufacturer_name);
			}
			return RedirectToAction("Index");
		}

		[HttpPost]
		public IActionResult Delete(int manufacturer_id)
		{
			if (wAL.GetAllFromDB().Where(x => x.Manufacturer.Equals(mAL.GetManufacturerById(manufacturer_id).Name, StringComparison.OrdinalIgnoreCase)).Any())
			{
				HttpContext.Session.SetString("Error", "Неможливо видалити виробника, у таблиці присутні записи, які його використовують");
			}
			else
			{
				mAL.Delete(manufacturer_id);
			}
			return RedirectToAction("Index");
		}

		[HttpGet]
		public IActionResult Search(string search_value)
		{
			var manufacturers = mAL.GetAllFromDB();
			List<ManufacturerModel> temp = [];
			if (search_value != null)
			{
				string[] search_split = search_value.Split(" ");

				foreach (ManufacturerModel m in manufacturers)
				{
					foreach (string s in search_split)
					{
						if (m.Name.Contains(s, StringComparison.OrdinalIgnoreCase))
						{
							temp.Add(m);
						}
					}
				}
			}
			Tuple<List<ManufacturerModel>, string> tuple = new(temp, search_value);
			return View("Search", tuple);
		}

		[HttpPost]
		public IActionResult Filter(string manufacturer_name)
		{
			List<ManufacturerModel> model = mAL.GetAllFromDB();
			if (manufacturer_name != null)
				model = model.Where(x => x.Name == manufacturer_name).ToList();

			return View("Index", model);
		}

		[HttpPost]
		public IActionResult Edit(int manufacturer_id)
		{
			ManufacturerModel Model = mAL.GetManufacturerById(manufacturer_id);
			return View(Model);
		}

		[HttpPost]
		public IActionResult SaveEditChanges(int manufacturer_id, string manufacturer_name)
		{
			if (mAL.GetAllFromDB().Where(x => x.Name.Equals(manufacturer_name, StringComparison.OrdinalIgnoreCase)
			&& manufacturer_id != x.Id).Any())
			{
				HttpContext.Session.SetString("Error",
				$"Неможливо змінити назву виробника на {manufacturer_name}, у таблиці уже присутній такий виробник");
			}
			else
			{
				mAL.Edit(manufacturer_id, manufacturer_name);
			}

			return RedirectToAction("Index");
		}

		[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
		public IActionResult Error()
		{
			return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
		}
	}
}
