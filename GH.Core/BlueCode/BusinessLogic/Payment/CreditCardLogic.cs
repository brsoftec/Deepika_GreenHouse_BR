using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PayPal.Api;

namespace Core.Common
{
	/// <summary>
	/// Include PayPal processor
	/// </summary>
	public class CreditCardLogic
    {
		public APIContext ApiContext { get; set; }

		public CreditCardLogic(APIContext apiContext)
		{
			ApiContext = apiContext;
		}

		/// <summary>
		/// Create and validate credit card number
		/// </summary>
		/// <param name="type"></param>
		/// <param name="number"></param>
		/// <param name="cvv"></param>
		/// <param name="month"></param>
		/// <param name="year"></param>
		/// <returns>CreditCard</returns>
		public CreditCard CreateCreditCard(string idCreditCard)
		{
			if (idCreditCard == null) throw new ArgumentNullException(nameof(idCreditCard));
			return CreditCard.Get(ApiContext, idCreditCard);
		}

		/// <summary>
		/// Create and validate credit card number
		/// </summary>
		/// <param name="type"></param>
		/// <param name="number"></param>
		/// <param name="cvv"></param>
		/// <param name="month"></param>
		/// <param name="year"></param>
		/// <returns>CreditCard</returns>
		public CreditCard CreateCreditCard(string type, string number, string cvv, int month, int year)
		{
			CreditCard cc = null;
			var credCard = new CreditCard();
			credCard.type = type;
			credCard.number = number;
			credCard.cvv2 = cvv;
			credCard.expire_month = month;
			credCard.expire_year = year;
			//CrdtCard = credCard.Create(Api);
			try
			{
				cc = credCard.Create(ApiContext);
				return cc;
			}
			catch (Exception ex)
			{
				throw new Exception("Invalid credit card");
			}
		}

		/// <summary>
		/// WARNING, This will charge cutomer $0.01 and refund back, you will lose $0.01 in your account for refund fee
		/// </summary>
		/// <param name="email"></param>
		/// <param name="idCreditCard"></param>
		/// <returns></returns>
		public Refund ValidateCreditCard(string email, string idCreditCard)
		{
			// Retrieve cc from PayPal
			//var retrievedCard = CreditCard.Get(ApiContext, idCreditCard);

			var payment = CreatePayment(email, "0.01", "Credit card validation for " + email, idCreditCard);

			if (payment != null)
			{
				var sale = payment.transactions[0].related_resources[0].sale;

				var refund = new Refund()
				{
					amount = new Amount()
					{
						currency = "USD",
						total = "0.01"
					}
				};

				try
				{
					// Prevent DUPLICATE_REQUEST_ID, "PayPal-Request-Id header was already used."
					// When perform multiple request on 1 Call
					ApiContext.ResetRequestId();

					return sale.Refund(ApiContext, refund);
					// TODO Record refund information to database
					//response.id // refund id
					//response.sale_id
					//response.parent_payment
				}
				catch (Exception ex)
				{
					throw new Exception("Cannot process credit card, please check again");
				}
			}
			throw new Exception("Cannot process credit card, please check again");
		}

		/// <summary>
		/// Perform a payment
		/// </summary>
		/// <param name="email"></param>
		/// <param name="orderAmount"></param>
		/// <param name="orderDescription"></param>
		/// <param name="creditCardId"></param>
		/// <returns></returns>
		public Payment CreatePayment(string email, string orderAmount, string orderDescription, string creditCardId)
		{
			Payment pay = null;
			ApiContext.ResetRequestId();
			var transaction = new Transaction()
			{
				amount = new Amount()
				{
					currency = "USD",
					total = orderAmount
				},
				description = orderDescription,
				item_list = new ItemList()
				{
					items = new List<Item>()
					{
						new Item()
						{
							name = orderDescription,
							currency = "USD",
							price = orderAmount,
							quantity = "1"
						}
					}
				} //,
				  //invoice_number = Common.GetRandomInvoiceNumber()
			};

			// A resource representing a Payer that funds a payment.
			var payer = new Payer()
			{
				payment_method = "credit_card",
				funding_instruments = new List<FundingInstrument>()
				{
					new FundingInstrument()
					{
						credit_card_token = new CreditCardToken()
						{
							credit_card_id = creditCardId
						}
					}
				},
				payer_info = new PayerInfo
				{
					email = email
				}
			};

			// A Payment resource; create one using the above types and intent as `sale` or `authorize`
			var payment = new Payment()
			{
				intent = "sale",
				payer = payer,
				transactions = new List<Transaction>() { transaction }
			};

			//try
			//{
			pay = payment.Create(ApiContext);
			//}
			//catch (Exception)
			//{
			//	throw new Exception("An error occurred while processing your credit card.");
			//}
			return pay;
		}

	}


	public static class CreditCardCheck
	{
		public static bool LuhnCheck(this string cardNumber)
		{
			return LuhnCheck(cardNumber.Select(c => c - '0').ToArray());
		}

		private static bool LuhnCheck(this int[] digits)
		{
			return GetCheckValue(digits) == 0;
		}

		private static int GetCheckValue(this int[] digits)
		{
			return digits.Select((d, i) => i % 2 == digits.Length % 2 ? ((2 * d) % 10) + d / 5 : d).Sum() % 10;
		}
	}

}
