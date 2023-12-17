using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Kurs_DB.Models;
using MySqlConnector;

namespace Kurs_DB.Controllers
{
	public class PropertyController : Controller
	{
		private readonly MySqlConnection _connection;
		private readonly ILogger<PropertyController> _logger;
		private readonly PropertyDAL pAL;
		private readonly WaterMeterDAL wAL;
		public PropertyController(ILogger<PropertyController> logger, MySqlConnection connection)
		{
			_logger = logger;
			_connection = connection;
			pAL = new(_connection);
			wAL = new(_connection);
		}

		public IActionResult Index()
		{
			List<PropertyModel> Model = new(pAL.GetAllFromDB());
			return View(Model);
		}

		[HttpPost]
		public IActionResult Add(string property_name, IFormCollection form)
		{
			if (pAL.GetAllFromDB().Where(x => x.Name.Equals(property_name, StringComparison.OrdinalIgnoreCase)).Any())
			{
				HttpContext.Session.SetString("Error", "Неможливо додати характеристику, у таблиці вже існує така характеристика");
			}
			else
			{
				List<string> prop_values = [];
				string new_property_values = form["new_property_value"];
				if (!string.IsNullOrEmpty(new_property_values))
					prop_values.AddRange(new_property_values.Split(",").Select(p => p.Trim()));

				prop_values = prop_values.Distinct().ToList();

				if (prop_values.Contains(property_name))
					prop_values = prop_values.Where(x => x != property_name).ToList();

				pAL.Add(property_name, prop_values);
			}
			return RedirectToAction("Index");
		}

		[HttpPost]
		public IActionResult Delete(int property_id)
		{
			if (wAL.GetAllFromDB().Select(x => x.Properties).Where(x => x.AllKeys
			.Any(x => x == pAL.GetPropertyById(property_id).Name)).Any())
			{
				HttpContext.Session.SetString("Error",
				 "Неможливо видалити характеристику, у таблиці присутні записи, які її використовують");
			}
			else
			{
				pAL.Delete(property_id);
			}
			return RedirectToAction("Index");
		}

		[HttpGet]
		public IActionResult DeletePropertyValue(int property_id, string property_value)
		{
			property_value ??= "";
			var property = pAL.GetPropertyById(property_id);
			if (wAL.GetAllFromDB().Where(x => x.Properties.GetValues(property.Name).Contains(property_value)).Any())
			{
				HttpContext.Session.SetString("Error", 
				"Неможливо видалити значення характеристики, у таблиці присутні записи, які її використовують");
			}
			else
			{
				pAL.DeletePropertyValue(property_id, property_value);
			}
			return RedirectToAction("Edit", "Property", new { property_id });
		}

		[HttpGet]
		public IActionResult Search(string search_value)
		{
			var Properties = pAL.GetAllFromDB();
			List<PropertyModel> temp = [];
			if (search_value != null)
			{
				string[] search_split = search_value.Split(" ");

				foreach (PropertyModel p in Properties)
				{
					List<string> haystack = [p.Name];
					haystack.AddRange(p.Values);
					bool Add = true;
					foreach (string s in search_split)
					{
						if (!haystack.Any(x => x.Contains(s, StringComparison.OrdinalIgnoreCase)))
						{
							Add = false;
						}
					}
					if (Add) temp.Add(p);
				}
			}
			Tuple<List<PropertyModel>, string> tuple = new(temp, search_value);
			return View("Search", tuple);
		}

		[HttpPost]
		public IActionResult Filter(string property_name)
		{
			List<PropertyModel> properties = pAL.GetAllFromDB();

			if (property_name != null)
				properties = properties.Where(x => x.Name == property_name).ToList();

			return View("Index", properties);
		}

		[HttpPost]
		public IActionResult PostEdit(int property_id)
		{
			return View("Edit", pAL.GetPropertyById(property_id));
		}

		public IActionResult Edit(int property_id)
		{
			PropertyModel model = pAL.GetPropertyById(property_id);
			Console.WriteLine(ViewData.Values);
			return View(model);
		}

		[HttpPost]
		public IActionResult SaveEditChanges(int property_id, string property_name, IFormCollection form)
		{
			if (pAL.GetAllFromDB().Where(x => x.Name.Equals(property_name, StringComparison.OrdinalIgnoreCase)
			&& x.Id != property_id).Any())
			{
				HttpContext.Session.SetString("Error",
				$"Неможливо змінити ім'я характеристики на {property_name}, у таблиці вже існує така характеристика");
			}
			else
			{
				string Error = null;
				List<PropertyModel> p = pAL.GetAllFromDB();
				List<string> prop_values = [];
				foreach (var key in form.Keys)
				{
					if (p.Any(p => p.Values.Contains(key)))
					{
						prop_values.Add(form[key]);
					}
				}
				string new_property_values = form["new_property_value"];
				if (!string.IsNullOrEmpty(new_property_values))
					prop_values.AddRange(new_property_values.Split(",").Select(p => p.Trim()));

				List<string> duplicateElements = [];
				foreach (string p_value in prop_values)
				{
					if (prop_values.Where(x => x.Equals(p_value, StringComparison.OrdinalIgnoreCase)).Count() > 1
					 && !duplicateElements.Where(x=>x.Equals(p_value, StringComparison.OrdinalIgnoreCase)).Any())
					{
						Error += $"Неможливо додати значення характеристики {p_value}, бо воно уже існує у списку.";
						duplicateElements.Add(p_value);
					}
				}
				prop_values = prop_values.Distinct().ToList();

				if (prop_values.Contains(property_name))
				{
					Error += $"Неможливо додати значення характеристики {property_name}, яке є її іменем.";
					prop_values = prop_values.Where(x => x != property_name).ToList();
				}

				if (Error != null)
					HttpContext.Session.SetString("Error", Error);
				else
				{
					pAL.Edit(property_id, property_name, prop_values);
				}
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
