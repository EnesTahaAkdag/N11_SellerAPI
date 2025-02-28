using Dapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using N11_SellerAPI.Models;
using N11_SellerAPI.ViewModels;
using System.Globalization;
using System.Text.RegularExpressions;

namespace N11_SellerAPI.Controllers
{
	[Route("[controller]")]
	[ApiController]
	public class N11Controller : ControllerBase
	{
		private readonly N11_SellerInfosContext _context;
		private readonly string _connectionString;

		public N11Controller(N11_SellerInfosContext context)
		{
			_context = context;
			_connectionString = "Data Source=PRASOFT\\SQLEXPRESS;Initial Catalog=N11_SellerInfos;Persist Security Info=True;Trusted_Connection=True;TrustServerCertificate=Yes";
		}

		[HttpPost("GetRandomUrl")]
		public async Task<IActionResult> GetRandomUrl()
		{
			const string selectQuery = @"
                SELECT TOP 1 Id, StoreLink AS Link
                FROM [N11_SellerInfos].[dbo].[Stores]
                WHERE Checked = 0
                ORDER BY NEWID()";

			try
			{
				await using var connection = new SqlConnection(_connectionString);
				await connection.OpenAsync();

				var store = await connection.QuerySingleOrDefaultAsync<StoreModel>(selectQuery);

				if (store == null)
					return NotFound(new { success = false, message = "Rastgele URL Bulunamadı" });

				const string updateQuery = @"
                    UPDATE [N11_SellerInfos].[dbo].[Stores]
                    SET Checked = 1
                    WHERE Id = @Id";

				await connection.ExecuteAsync(updateQuery, new { Id = store.Id });

				return Ok(new
				{
					success = true,
					message = "Rastgele URL Başarıyla Getirildi",
					Url = store.Link
				});
			}
			catch (Exception ex)
			{
				return StatusCode(500, new { success = false, message = "Bir hata oluştu.", error = ex.Message });
			}
		}

		[HttpPost("SellerMarketInformation")]
		public async Task<IActionResult> SellerMarketInformation([FromBody] SellerMarketInformationModel model)
		{
			try
			{
				if (model == null)
					return BadRequest(new { success = false, ErrorMessage = "Veriler Boş Geldi" });

				if (string.IsNullOrEmpty(model.ProductCount) && string.IsNullOrEmpty(model.Category))
					return BadRequest(new { success = true, message = "Mağzada ürün yok", errors = ModelState });

				if (!ModelState.IsValid)
					return BadRequest(new { success = false, message = "Geçersiz veri.", errors = ModelState });

				var dataControl = await _context.Stores.FirstOrDefaultAsync(m => m.StoreLink == model.Link);
				if (dataControl == null)
					return NotFound(new { success = false, message = "Böyle bir mağaza yok." });

				dataControl.StoreName = model.StoreName;
				dataControl.Category = model.Category;

				if (!string.IsNullOrEmpty(model.StoreScore))
				{
					if (decimal.TryParse(model.StoreScore, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal storeScoreValue))

						dataControl.StoreScore = decimal.Parse((storeScoreValue).ToString("N1", new CultureInfo("tr-TR")));
					else
						return BadRequest(new { success = false, message = "StoreScore değeri geçersiz." });
				}

				if (!string.IsNullOrEmpty(model.RatingScore))
				{
					if (decimal.TryParse(model.RatingScore, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal ratingScoreValue))
						dataControl.RatingScore = decimal.Parse((ratingScoreValue).ToString("N1", new CultureInfo("tr-TR")));
					else
						return BadRequest(new { success = false, message = "RatingScore değeri geçersiz." });
				}

				var turkishCulture = new CultureInfo("tr-TR");
				var ratingCountMatch = Regex.Match(model.RatingCount, "\\d+");

				if (!ratingCountMatch.Success)
					return BadRequest(new { success = false, message = "RatingCount içerisinden sayı alınamadı." });
				dataControl.RatingCount = int.Parse(ratingCountMatch.Value, turkishCulture);

				model.ProductCount = model.ProductCount.Replace(",", "");

				if (int.TryParse(model.ProductCount, NumberStyles.Any, turkishCulture, out int productCountValue))
					dataControl.ProductCount = productCountValue;
				else
					return BadRequest(new { success = false, message = "ProductCount değeri geçersiz." });

				await _context.SaveChangesAsync();
				return Ok(new { success = true, message = "Mağaza Bilgileri Başarıyla Kaydedildi" });
			}
			catch (Exception ex)
			{
				return StatusCode(500, new
				{
					success = false,
					message = "Bir hata oluştu.",
					error = ex.Message
				});
			}
		}

		[HttpPost("PreliminaryInformationForm")]
		public async Task<IActionResult> PreliminaryInformationForm([FromBody] PreliminaryInformationFormModel model)
		{
			if (model == null)
				return BadRequest(new { success = false, ErrorMessage = "Veriler Boş Geldi" });

			if (!ModelState.IsValid)
				return BadRequest(new { success = false, message = "Geçersiz veri.", errors = ModelState });

			var existingSeller = await _context.Stores
				.FirstOrDefaultAsync(m => m.SellerName == model.SellerName);
			if (existingSeller != null)
				return BadRequest(new { success = false, message = "Bu 'SellerName' başka bir mağaza tarafından kullanılıyor." });

			var dataControl = await _context.Stores.FirstOrDefaultAsync(m => m.StoreLink == model.Link);
			if (dataControl == null)
				return NotFound(new { success = false, message = "Böyle bir mağaza yok." });

			if (!string.IsNullOrWhiteSpace(model.SellerName) && model.SellerName.StartsWith("Satıcı İsim/Unvanı:"))
				dataControl.SellerName = model.SellerName.Replace("Satıcı İsim/Unvanı:", "").Trim();
			else if (string.IsNullOrWhiteSpace(model.SellerName))
				dataControl.SellerName = null;

			if (!string.IsNullOrWhiteSpace(model.Address) && model.Address.StartsWith("Satıcı’nın Açık Adresi:"))
				dataControl.Address = model.Address.Replace("Satıcı’nın Açık Adresi:", "").Trim();
			else if (string.IsNullOrWhiteSpace(model.Address))
				dataControl.Address = null;

			if (!string.IsNullOrWhiteSpace(model.Mersis) && model.Mersis.StartsWith("Satıcı Mersis veya Vergi Kimlik No:"))
			{
				var mersisValue = model.Mersis.Replace("Satıcı Mersis veya Vergi Kimlik No:", "").Trim();
				dataControl.Mersis = string.IsNullOrEmpty(mersisValue) ? null : mersisValue;
			}
			else if (string.IsNullOrWhiteSpace(model.Mersis))
				dataControl.Mersis = null;

			if (!string.IsNullOrWhiteSpace(model.Phone) && model.Phone.StartsWith("Satıcı’nın Telefonu:"))
			{
				var phoneValue = model.Phone.Replace("Satıcı’nın Telefonu:", "").Trim();
				dataControl.Phone = string.IsNullOrEmpty(phoneValue) ? null : phoneValue;
			}
			else if (string.IsNullOrWhiteSpace(model.Phone))
				dataControl.Phone = null;

			if (!string.IsNullOrWhiteSpace(model.Email) && model.Email.StartsWith("Satıcı E-Posta Adresi:"))
				dataControl.Email = model.Email.Replace("Satıcı E-Posta Adresi:", "").Trim();
			else if (string.IsNullOrWhiteSpace(model.Email))
				dataControl.Email = null;

			try
			{
				await _context.SaveChangesAsync();
				return Ok(new { success = true, message = "Ön Bilgiler Başarıyla Kaydedildi" });
			}
			catch (Exception ex)
			{
				return StatusCode(500, new { success = false, message = "Bir hata oluştu.", error = ex.Message });
			}
		}
	}
}
