using TradingBot.Info;
using System;
using System.Net;
using System.Text;
using Newtonsoft.Json;
using System.Security.Cryptography;


namespace TradingBot
{
	public static class Server
	{
		/// <summary>
		/// Prend la date et retourne le timestamp
		/// </summary>
		/// <returns>Historique en millisecondes</returns>.
		/// <param name="date">Date actuelle.</param>
		/// Ne fonctionne que pour ajouter l'horodatage à l'Url si le timestamp saisi est UTC.
		/// Sinon, recvWindow est nécessaire pour les autres timestamps, inférieurs à UTC uniquement !
		public static long DateToTimestamp(DateTime date)
		{
			var timeSt = (long)(date - new DateTime(1970, 1, 1)).TotalMilliseconds;
			return timeSt;
		}

		/// <summary>
		/// Obtient le timestamp actuel pour UTC
		/// </summary>
		/// <returns>Retourne le timestamp en millisecondes.</returns>
		public static long getTimeStamp()
		{
			var timeSt = (long)(DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalMilliseconds;
			return timeSt;
		}

		/// <summary>
		/// Tester la connectivité à l'API Rest de Binance et obtenir l'objet de l'heure actuelle du serveur.
		/// </summary>
		/// <returns>Objet de l'heure actuelle en millisecondes</returns>.
		/// Ne doit pas être utilisé pour le paramètre timestamp dans l'url, le délai peut être trop long.
		public static ServerTime getServerTimeObject()
		{
			string apiRequestUrl = "https://www.binance.com/api/v1/time";

			string response = webRequest(apiRequestUrl, "GET", null);
			var serverTime = JsonConvert.DeserializeObject<ServerTime>(response);
			return serverTime;
		}

		/// <summary>
		/// Obtenir l'heure actuelle du serveur
		/// </summary>
		/// <returns>Longueur du temps actuel en millisecondes</returns>
		public static long getServerTime()
		{
			var serverTime = getServerTimeObject();
			return serverTime.Time;
		}


		/// <summary>
		/// Convertit les octets en chaîne de caractères.
		/// </summary>
		/// <returns>String</returns>
		/// <param name="buff">Variable qui contient des octets</param>
		public static string ByteToString(byte[] buff) 
		{
			string str = "";
			for (int i = 0; i < buff.Length; i++)
				str += buff[i].ToString("X2");
			return str;
		}


		public static string addReceiveWindow(string query)
		{
			query = $"{query}&recvWindow={5}";
			return query;
		}

		/// <summary>
		/// Créer une requete http web avec la signature si nécessaire
		/// </summary>
		/// <param name="url">Url à demander. String.</param>
		/// <param name="method">Type de requete http. Eg; GET/POST. String.</param>
		/// <param name="ApiKey">Clé publique à ajouter à l'en-tête si elle est signée. String.</param>
		/// <returns>Reponse de requete http</returns>
		public static string webRequest(string requestUrl, string method, string ApiKey)
		{
			try
			{
				var request = (HttpWebRequest)WebRequest.Create(requestUrl);
				request.Method = method;
				request.Timeout = 5000;  
				if (ApiKey != null)
				{
					request.Headers.Add("X-MBX-APIKEY", ApiKey);
				}

				var webResponse = (HttpWebResponse)request.GetResponse();
				if (webResponse.StatusCode != HttpStatusCode.OK)
				{
					throw new Exception($"Did not return OK 200. Returned: {webResponse.StatusCode}");
				}

				var encoding = ASCIIEncoding.ASCII;
				string responseText = null;

				using (var reader = new System.IO.StreamReader(webResponse.GetResponseStream(), encoding))
				{
					responseText = reader.ReadToEnd();
				}

				return responseText;
			}
			catch (WebException webEx)
			{
				if (webEx.Response != null)
				{
					Encoding encoding = ASCIIEncoding.ASCII;
					using (var reader = new System.IO.StreamReader(webEx.Response.GetResponseStream(), encoding))
					{
						string responseText = reader.ReadToEnd();
						throw new Exception(responseText);
					}
				}
				throw;
			}
			catch
			{
				return "Error";
			}
		}

		/// <summary>
		/// Creer la signature   
		/// </summary>
		/// <param name="SecretKey">Code secret pour la signature HMAC. String.</param>
		/// <param name="query">Text à signer. String.</param>
		/// <returns>Signature</returns>
		public static string getSignature(string SecretKey, string query)
		{
			Encoding encoding = Encoding.UTF8;
			var keyByte = encoding.GetBytes(SecretKey);
			using (var hmacsha256 = new HMACSHA256(keyByte))
			{
				hmacsha256.ComputeHash(encoding.GetBytes(query));
				return ByteToString(hmacsha256.Hash);
			}

		}

	}
}
