using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Kurs_DB.Models;
using MySqlConnector;
using System.Collections.Specialized;
using System.Linq;
using Microsoft.AspNetCore.Components.Endpoints.Infrastructure;

namespace Kurs_DB.Controllers
{
	public class WaterMeterController : Controller
	{
		private readonly MySqlConnection _connection;
		private readonly ILogger<WaterMeterController> _logger;
		private readonly WaterMeterDAL dAL;
		private readonly ManufacturerDAL mAL;
		private readonly PropertyDAL pAL;
		public WaterMeterController(ILogger<WaterMeterController> logger, MySqlConnection connection)
		{
			_logger = logger;
			_connection = connection;
			dAL = new(_connection);
			mAL = new(_connection);
			pAL = new(_connection);
		}

		public IActionResult Index()
		{
			if (string.IsNullOrEmpty(HttpContext.Session.GetString("Role")))
				HttpContext.Session.SetString("Role", "user");
			Tuple<List<WaterMeterModel>, List<ManufacturerModel>, List<PropertyModel>> tupleModel = new(dAL.GetAllFromDB(), mAL.GetAllFromDB(), pAL.GetAllFromDB());
			return View(tupleModel);
		}

		[HttpPost]
		public IActionResult Add(string model_name, int manufacturer_id, IFormCollection form)
		{
			if (dAL.GetAllFromDB().Where(x => x.Name.Equals(model_name, StringComparison.OrdinalIgnoreCase)).Any())
			{
				HttpContext.Session.SetString("Error",
				"Неможливо додати модель, у таблиці уже присутня така модель");
			}
			else
			{
				List<PropertyModel> props = pAL.GetAllFromDB();
				NameValueCollection properties = [];
				foreach (var key in form.Keys)
				{
					if (props.Any(p => p.Name == key))
					{
						properties.Add(key, form[key]);
					}
				}
				dAL.Add(model_name, manufacturer_id, properties);
			}
			return RedirectToAction("Index");
		}

		[HttpPost]
		public IActionResult Delete(int waterMeter_id)
		{
			dAL.Delete(waterMeter_id);

			return RedirectToAction("Index");
		}

		[HttpGet]
		public IActionResult Search(string search_value)
		{
			var WaterMeters = dAL.GetAllFromDB();
			List<WaterMeterModel> temp = [];
			if (search_value != null)
			{
				string[] search_split = search_value.Split(" ");

				foreach (WaterMeterModel wm in WaterMeters)
				{
					List<string> haystack = [wm.Name, wm.Manufacturer];
					haystack.AddRange(wm.Properties.Cast<string>().Select(x => wm.Properties[x]));
					bool Add = true;
					foreach (string s in search_split)
					{
						if (!haystack.Any(x => x.Contains(s, StringComparison.OrdinalIgnoreCase)))
						{
							Add = false;
						}
					}
					if (Add) temp.Add(wm);
				}
			}
			Tuple<List<WaterMeterModel>, string> tuple = new(temp, search_value);
			return View("Search", tuple);
		}

		[HttpPost]
		public IActionResult Filter(string manufacturer_name, IFormCollection form)
		{
			List<WaterMeterModel> wm = dAL.GetAllFromDB();
			List<PropertyModel> props = pAL.GetAllFromDB();
			NameValueCollection properties = [];
			foreach (var key in form.Keys)
			{
				if (props.Any(p => p.Name == key) && form[key] != "")
				{
					properties.Add(key, form[key]);
				}
			}
			if (manufacturer_name != null)
				wm = wm.Where(x => x.Manufacturer == manufacturer_name).ToList();

			if (properties != null)
			{
				foreach (string p_key in properties)
					wm = wm.Where(x => x.Properties[p_key] == properties[p_key]).ToList();
			}
			Tuple<List<WaterMeterModel>, List<ManufacturerModel>, List<PropertyModel>> tuple = new(wm, mAL.GetAllFromDB(), props);
			return View("Index", tuple);
		}

		[HttpPost]
		public IActionResult Edit(int waterMeter_id)
		{
			Tuple<List<ManufacturerModel>, List<PropertyModel>, WaterMeterModel> tupleModel = new(mAL.GetAllFromDB(), pAL.GetAllFromDB(), dAL.GetModelById(waterMeter_id));

			return View(tupleModel);
		}

		[HttpPost]
		public IActionResult SaveEditChanges(int waterMeter_id, string model_name, int manufacturer_id, IFormCollection form)
		{
			if (dAL.GetAllFromDB().Where(x => x.Name.Equals(model_name, StringComparison.OrdinalIgnoreCase)
			&& waterMeter_id != x.Id).Any())
			{
				HttpContext.Session.SetString("Error",
				$"Неможливо змінити назву моделі на {model_name}, у таблиці уже присутній такий виробник");
			}
			else
			{
				List<PropertyModel> props = pAL.GetAllFromDB();
				NameValueCollection properties = [];
				foreach (var key in form.Keys)
				{
					if (props.Any(p => p.Name == key))
					{
						properties.Add(key, form[key]);
					}
				}
				dAL.Edit(waterMeter_id, model_name, manufacturer_id, properties);
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
