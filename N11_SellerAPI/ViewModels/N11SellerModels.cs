using System.ComponentModel.DataAnnotations;

namespace N11_SellerAPI.ViewModels
{
	public class StoreModel
	{
		public int Id { get; set; }
		public string Link { get; set; }
	}

	public class PreliminaryInformationFormModel
	{
		public string Link { get; set; }

		public string SellerName { get; set; }

		public string Address { get; set; }

		public string Phone { get; set; }  

		public string Mersis { get; set; } 

		public string Email { get; set; }
	}

	public class SellerMarketInformationModel
	{
		public string Link { get; set; }

		public string StoreName { get; set; }

		public string Category { get; set; }

		public string StoreScore { get; set; }

		public string RatingScore { get; set; }

		public string RatingCount { get; set; }

		public string ProductCount { get; set; }
	}
}
