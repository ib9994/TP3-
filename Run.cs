using TradingBot.Info;
using TradingBot;
using System;
using System.Timers;





namespace TradingBot
{
    public class Run
	{
		static System.Threading.ManualResetEvent timerFired = new System.Threading.ManualResetEvent(false);

		public static void timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
		{
			Console.WriteLine("Timer Fired.");
			TradingBot.AlgoTrading.Bot();
		}


		public static void Main(string[] args)
		{
			//Démarrer Bot la première fois, avant le Timer
			AlgoTrading.Bot();

			//Debut Timer
			Timer timer = new Timer();
			timer.Interval = General.timerInterval * 60 * 1000; // Convertir ms en minutes
			timer.Elapsed += new ElapsedEventHandler(timer_Elapsed);
			timer.Enabled = true;

			timerFired.WaitOne();  
		}
	}
}