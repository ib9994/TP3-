using TradingBot.Info;
using System;
using System.Linq;



namespace TradingBot
{
	public class AlgoTrading
	{


		public static void Bot()
		{
			string symbol = General.C1 + General.C2;

			//Obtenir les informations de compte
			var accInfo = API.GetAccountInformation();

			//Vérifier les fonds du compte pour les deux monnaies
			AssetBalance Currency1 = (from coin in accInfo.Balances where coin.Asset == General.C1 select coin).FirstOrDefault();
			var freeCurrency1 = Currency1.Free;

			AssetBalance Currency2 = (from coin in accInfo.Balances where coin.Asset == General.C2 select coin).FirstOrDefault();
			var freeCurrency2 = Currency2.Free;

			//Obtenir le prix du dernier trade BCCBTC
			var lastTrade = API.GetLastTrade(symbol);
			var lastTradePrice = 0.0;
			var lastTradeSide = OrderSides.SELL; //si lastTrade == null, cela signifie acheter la Devise1


			//obtenir le prix et l'OrderSide de la dernière transaction (le cas échéant)
			if (lastTrade != null)
			{
				lastTradePrice = lastTrade.Price;

				if (lastTrade.isBuyer == true)
					lastTradeSide = OrderSides.BUY;
				else
					lastTradeSide = OrderSides.SELL;
			}

			//Regarder le prix actuel
			var currentPrice = API.GetOnePrice(symbol);

			//Calculer le pourcentage de changement de prix réel
			var priceChange = 100 * (currentPrice - lastTradePrice) / currentPrice;

			Console.WriteLine("Current Price is " + currentPrice);
			Console.WriteLine("Price Change is " + priceChange);

			//Creer un ordre
			Order marketOrder = null;

			if (lastTradeSide == OrderSides.BUY && priceChange > General.percentageChange)
			{
				//si la dernière commande était un achat et que le prix a augmenté.
				//Vendre C1
				marketOrder = API.PlaceMarketOrder(symbol, OrderSides.SELL, General.quatityPerTrade);

			}
			else if (lastTradeSide == OrderSides.SELL && priceChange < -General.percentageChange)
			{
				//si le dernier ordre était une vente, et que le prix a baissé.
				//Acheter C1
				marketOrder = API.PlaceMarketOrder(symbol, OrderSides.BUY, General.quatityPerTrade);
			}


			//Déclarations
			if (marketOrder == null)
			{
				Console.WriteLine("No trade was made.");
				var actualLastTrade = API.GetLastTrade(symbol);
				if (actualLastTrade.isBuyer == true)
				{
					Console.WriteLine(actualLastTrade.Qty + " of " + General.C1 + " was previously bought for " + actualLastTrade.Price);
				}
				else if (actualLastTrade.isBuyer == false)
				{
					Console.WriteLine(actualLastTrade.Qty + " of " + General.C1 + " was previously sold for " + actualLastTrade.Price);
				}
			}
			else
			{
				var newLastTrade = API.GetLastTrade(symbol);
				if (marketOrder.Side == OrderSides.BUY)
				{
					Console.WriteLine(newLastTrade.Qty + " of " + General.C1 + " was bought for " + newLastTrade.Price);
				}
				else if (marketOrder.Side == OrderSides.SELL)
				{
					Console.WriteLine(newLastTrade.Qty + " of " + General.C1 + " was sold for " + newLastTrade.Price);
				}
			}

		}
	}
}