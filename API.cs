using TradingBot.Info;
using Newtonsoft.Json;
using System;
using System.Linq;
using System.Timers; 
using System.ComponentModel;

namespace TradingBot
{
	public class API
	{
		/// <summary>
		/// Le robot prend une paire de devises, vérifie les transactions précédentes pour cette paire.
		/// Seul le contraire de la dernière transaction peut être exécuté (technique de maximisation de la quantité de stock).
		/// Si le prix a changé selon le pourcentage souhaité, une transaction est exécutée. 
		/// </summary>

		public static string Url { get; set; } = "https://www.binance.com/api/";

		/// <summary>
		/// Tester la connectivité à l'API Rest de Binance
		/// </summary>
		/// <returns>Valeur booléenne (vrai/faux) basée sur le succès.</returns>
		public static bool Ping()
		{
			string apiRequestUrl = $"{Url}v1/ping";

			string response = Server.webRequest(apiRequestUrl, "GET", null);
			if (response == "{}")
				return true;
			else
				return false;
		}

		/// <summary>
		/// Tester la connectivité à l'API Rest de Binance et obtenir l'heure actuelle du serveur.
		/// </summary>
		/// <returns>Chaîne de l'heure actuelle en millisecondes</returns>
		public static string Time()
		{
			string apiRequestUrl = $"{Url}v1/time";

			var response = Server.webRequest(apiRequestUrl, "GET", null);
			return response;
		}


		/// <summary>
		/// Obtenir les prix de tous les symboles
		/// </summary>
		/// <returns>Prix actuels de tous les symbols</returns>
		public static SymbolPrice[] GetAllPrices()
		{
			string apiRequestUrl = $"{Url}v1/ticker/allPrices";

			var response = Server.webRequest(apiRequestUrl, "GET", null);
			var parsedResponse = JsonConvert.DeserializeObject<SymbolPrice[]>(response);

			return parsedResponse;
		}


		/// <summary>
		/// Obtenir le prix actuel pour le symbole donné
		/// </summary>
		/// <param name="symbol">Asset symbol e.g.ETHBTC</param>
		/// <returns>The price. Double.</returns>
		public static double GetOnePrice(string symbol)
		{
			var symbolPrices = GetAllPrices();

			SymbolPrice symbolPrice = (from sym in symbolPrices where sym.Symbol == symbol select sym).FirstOrDefault();
			if (symbolPrice == null)
			{
				throw new ApplicationException($"No symbol, {symbol}, exists");
			}

			return symbolPrice.Price;
		}


		/// <summary>
		/// Obtenir des informations sur le compte, y compris tous les soldes d'actifs
		/// </summary>
		/// <param name="recvWindow">Intervalle (en millisecondes) dans lequel la demande doit être traitée dans un certain nombre de millisecondes ou être rejetée par le serveur. Valeur par défaut : 5000 millisecondes</param>
		/// <returns>Objet AccountInformation comprenant les soldes d'actifs actuels</returns>
		public static AccountInformation GetAccountInformation()
		{
			string apiRequestUrl = $"{Url}v3/account";

			string query = $"";
			query = $"{query}&timestamp={Server.getTimeStamp()}";

			var signature = Server.getSignature(General.SecretKey, query);
			query += "&signature=" + signature;

			apiRequestUrl += "?" + query;

			var response = Server.webRequest(apiRequestUrl, "GET", General.ApiKey);

			var parsedResponse = JsonConvert.DeserializeObject<AccountInformation>(response);
			return parsedResponse;
		}

		/// <summary>
		/// Obtenez vos ordres ouverts pour le symbole donné
		/// </summary>
		/// <param name="symbol">Asset symbol e.g.ETHBTC</param>
		/// <returns>Une liste d'objets Order avec les données de l'ordre</returns>
		public static Order[] GetOpenOrders(string symbol)
		{
			string apiRequestUrl = $"{Url}v3/openOrders";

			string query = $"symbol={symbol}";

			query = $"{query}&timestamp={Server.getTimeStamp()}";

			var signature = Server.getSignature(General.SecretKey, query);

			query += "&signature=" + signature;

			apiRequestUrl += "?" + query;

			var response = Server.webRequest(apiRequestUrl, "GET", General.ApiKey);
			var parsedResponse = JsonConvert.DeserializeObject<Order[]>(response);
			return parsedResponse;
		}

		/// <summary>
		/// Obtenir des transactions pour un compte et un symbole spécifiques
		/// </summary>
		/// <param name="symbol">Asset symbol e.g.ETHBTC</param>
		/// <returns>Une liste de Trades</returns>
		public static Trades[] GetMyTrades(string symbol)
		{
			string apiRequestUrl = $"{Url}v3/myTrades";

			string query = $"symbol={symbol}";

			query = $"{query}&timestamp={Server.getTimeStamp()}";

			var signature = Server.getSignature(General.SecretKey, query);
			query += "&signature=" + signature;

			apiRequestUrl += "?" + query;

			var response = Server.webRequest(apiRequestUrl, "GET", General.ApiKey);
			var parsedResponse = JsonConvert.DeserializeObject<Trades[]>(response);
			return parsedResponse;
		}

		/// <summary>
		/// Obtenir le dernier Trade
		/// </summary>
		/// <returns>La dernière transaction. Objet Trades</returns>
		/// <param name="symbol">Symbol.</param>
		public static Trades GetLastTrade(string symbol)
		{
			var parsedResponse = GetMyTrades(symbol);

			if (parsedResponse.Length != 0)
				return parsedResponse[parsedResponse.Length - 1];
			else
				return null;
		}

		/// <summary>
		/// Place un ordre
		/// </summary>
		/// <returns>L'objet de l'ordre</returns>
		/// <param name="symbol">Symbole de la monnaie tradé, eg BCCETH</param>
		/// <param name="side">Sens de l'ordre , acheter ou vendre</param>
		/// <param name="type">Type de commande, voir Set.OrderTypes </param>
		/// <param name="timeInForce">La durée de la commande sera active pour</param>
		/// <param name="quantity">Montant à trader</param>
		/// <param name="price">Prix auquel acheter</param>

		public static Order PlaceOrder(string symbol, OrderSides side, OrderTypes type, TimesInForce timeInForce, double quantity, double price)
		{
			string apiRequestUrl = "";

			if (General.testCase == true)
				apiRequestUrl = $"{Url}v3/order/test";
			else
				apiRequestUrl = $"{Url}v3/order";


			string query = $"symbol={symbol}&side={side}&type={type}&timeInForce={timeInForce}&quantity={quantity}&price={price}";

			query = $"{query}&timestamp={Server.getTimeStamp()}";

			var signature = Server.getSignature(General.SecretKey, query);
			query += "&signature=" + signature;

			apiRequestUrl += "?" + query;
			var response = Server.webRequest(apiRequestUrl, "POST", General.ApiKey);

			var parsedResponse = JsonConvert.DeserializeObject<Order>(response);
			return parsedResponse;
		}

		/// <summary>
		/// Place un ordre au marché. (Ordre au prix actuel du marché, ne nécessite pas de paramètres de prix ou de timeInForce).
		/// </summary>
		/// <returns>L'objet de l'ordre</returns>
		/// <param name="symbol">Symbole de la monnaie a trader, eg BCCETH.</param>
		/// <param name="side">Sens de l'odre , acheter ou vendre.</param>
		/// <param name="quantity">Montant à trader.</param>

		public static Order PlaceMarketOrder(string symbol, OrderSides side, double quantity)
		{
			string apiRequestUrl = ""; 

			if (General.testCase == true)
				apiRequestUrl = $"{Url}v3/order/test";
			else
				apiRequestUrl = $"{Url}v3/order";

			string query = $"symbol={symbol}&side={side}&type={OrderTypes.MARKET}&quantity={quantity}";
			query = $"{query}&timestamp={Server.getTimeStamp()}";

			var signature = Server.getSignature(General.SecretKey, query);
			query += "&signature=" + signature;

			apiRequestUrl += "?" + query;
			var response = Server.webRequest(apiRequestUrl, "POST", General.ApiKey);

			var parsedResponse = JsonConvert.DeserializeObject<Order>(response);
			return parsedResponse;
		}


		/***********

		//Ping
		var pingTest = Ping();

		//Temps
		var timeTest = Time();

		///Retrouver vos informations de compte
		//Retourné sous forme d'objet, voir Set.cs pour l'analyser
		var accountInfo = GetAccountInformation();

		///Retrouver les prix de toutes les paires de devises disponibles
		//Retourné sous forme de liste d'objets, voir Set.cs ou la fonction GetOnePrice() pour l'analyser
		var allPrices = GetAllPrices();

		///Retrouver les prix d'une paire de devises/symbole spécifique
		var onePrice = GetOnePrice("BCCETH");

		///Retrouver tous les ordres ouverts sur votre compte
		var openOrders = GetOpenOrders("BCCETH");

		///Obtenez l'historique de vos transactions liées à une paire de devises/un symbole spécifique.
		//Retourné sous la forme d'une liste d'objets, voir Set.cs ou la fonction GetLastTrade() pour l'analyser.
		var trades = GetMyTrades("XRPETH");

		///Obtenez votre dernière transaction pour une paire de devises/un symbole spécifique.
		//Retourné sous forme d'objet, voir Set.cs pour l'analyser
		var lastTrade = GetLastTrade("XRPETH");

		///Placez n'importe quel type d'ordre accepté, défini en phase "test" car non utilisé.
		//var order = PlaceOrder("BCCETH", OrderSides.SELL, OrderTypes.MARKET, TimesInForce.GTC, 0.01, 2.09) ;

		///Placer un ordre de marché
		var marketOrder = PlaceMarketOrder("BCCETH", OrderSides.SELL, 0.01);

		**********/

	}
}
