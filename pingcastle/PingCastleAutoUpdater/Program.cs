﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.UI;
using System.Web.Script.Serialization;
using System.Net;
using System.IO;
using System.Reflection;
using System.IO.Compression;
using System.Diagnostics;

namespace PingCastleAutoUpdater
{
	class Program
	{
		class Release
		{
			public string name { get; set; }
			public bool prerelease { get; set; }
			public DateTime published_at { get; set; }
			public List<Asset> assets { get; set; }
		}

		class Asset
		{
			public string name { get; set; }
			public int size { get; set; }
			public string browser_download_url { get; set; }
		}

		const string fileNameLastDownload = "LastDownloadedRelease.txt";

		static void Main(string[] args)
		{
			Program program = new Program();
			program.Run(args);
		}

		bool forceDownload = false;
		bool preview = false;
		int numberOfDaysToWay = 0;
		string releaseInfoUrl = "https://api.github.com/repos/vletoux/pingcastle/releases";

		void Run(string[] args)
		{
			Trace.WriteLine("Before parsing arguments");
			for (int i = 0; i < args.Length; i++)
			{
				switch (args[i])
				{
					case "--api-url":
						if (i + 1 >= args.Length)
						{
							WriteInRed("argument for --api-url is mandatory");
							return;
						}
						releaseInfoUrl = args[++i];
						break;
					case "--force-download":
						forceDownload = true;
						break;
					case "--help":
						DisplayHelp();
						return;
					case "--use-preview":
						preview = true;
						break;
					case "--wait-for-days":
						if (i + 1 >= args.Length)
						{
							WriteInRed("argument for --wait-for-days is mandatory");
							return;
						}
						{
							if (!int.TryParse(args[++i], out numberOfDaysToWay))
							{
								WriteInRed("argument for --wait-for-days is not a valid value (typically: 30)");
								return;
							}
						}
						break;
					default:
						WriteInRed("unknow argument: " + args[i]);
						DisplayHelp();
						return;
				}
			}
			Console.WriteLine("Do not forget that there are other command line switches like --help that you can use");
			Console.WriteLine("Running on " + Environment.Version);
			Console.WriteLine();
			Console.WriteLine("Getting the list of releases");
			string releaseInfo = GET(releaseInfoUrl);
			Console.WriteLine("Done");
			string lastRelease = null;
			if (File.Exists(fileNameLastDownload) && !forceDownload)
			{
				if (forceDownload)
				{
					Console.WriteLine("Download is forced");
				}
				lastRelease = File.ReadAllText(fileNameLastDownload);
				Console.WriteLine("Current release is: " + lastRelease);
			}
			else
			{
				Console.WriteLine("No previous download");
			}

			JavaScriptSerializer jsonSerializer = new JavaScriptSerializer();
			IEnumerable<Release> releases = jsonSerializer.Deserialize<List<Release>>(releaseInfo);
			if (numberOfDaysToWay > 0)
			{
				Console.WriteLine("Only releases older than " + numberOfDaysToWay + " day(s) are selected");
				releases = releases.Where(r => r.published_at.AddDays(numberOfDaysToWay) < DateTime.Now);
			}
			if (!preview)
			{
				releases = releases.Where(r => r.prerelease == false);
			}
			else
			{
				Console.WriteLine("Prerelease are included");
			}
			releases = releases.OrderByDescending(i => i.published_at);
			if (releases.Count() == 0)
			{
				Console.WriteLine("There is no release matching the requirements");
				return;
			}
			Release release = releases.First();
			Console.WriteLine("Latest release is: " + release.name);
			if (release.name == lastRelease)
			{
				Console.WriteLine("This is the latest one. Program is stopping.");
				return;
			}
			string downloadUrl = release.assets.First().browser_download_url;
			Console.WriteLine("Downloading " + downloadUrl);

			ProceedReleaseInstall(downloadUrl);
			
			// success ! 
			Console.WriteLine("Saving status");
			File.WriteAllText(fileNameLastDownload, release.name);
			Console.WriteLine("Update with success!");
		}

		private static void WriteInRed(string data)
		{
			Console.ForegroundColor = ConsoleColor.Red;
			Console.WriteLine(data);
			Trace.WriteLine("[Red]" + data);
			Console.ResetColor();
		}

		// Returns JSON string
		static string GET(string url)
		{
			// github forces TLS 1.2 which is not enabled by default in .net
			System.Net.ServicePointManager.SecurityProtocol |= SecurityProtocolType.Tls12;
			Version version = Assembly.GetExecutingAssembly().GetName().Version;
			HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
			request.UserAgent = "PingCastleAutoUpdater " + version.ToString();
			try
			{
				WebResponse response = request.GetResponse();
				using (Stream responseStream = response.GetResponseStream())
				{
					StreamReader reader = new StreamReader(responseStream, System.Text.Encoding.UTF8);
					return reader.ReadToEnd();
				}
			}
			catch (WebException ex)
			{
				WebResponse errorResponse = ex.Response;
				if (errorResponse != null)
				{
					using (Stream responseStream = errorResponse.GetResponseStream())
					{
						StreamReader reader = new StreamReader(responseStream, System.Text.Encoding.GetEncoding("utf-8"));
						String errorText = reader.ReadToEnd();
						Console.WriteLine(errorText);
						// log errorText
					}
				}
				throw;
			}
		}

		static void ProceedReleaseInstall(string url)
		{
			Version version = Assembly.GetExecutingAssembly().GetName().Version;
			HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
			request.UserAgent = "PingCastleAutoUpdater " + version.ToString();
			try
			{
				WebResponse response = request.GetResponse();
				using (Stream responseStream = response.GetResponseStream())
				using (var archive = new ZipArchive(responseStream, ZipArchiveMode.Read))
				{
					foreach (var entry in archive.Entries)
					{
						// do not save .config file except if it doesn't exists 
						// and do not overwrite the updater file because it's running !
						if (entry.FullName.EndsWith(".config", StringComparison.OrdinalIgnoreCase))
						{
							if (!File.Exists(entry.FullName))
							{
								performCopy(entry);
							}
						}
						else
						{
							performCopy(entry);
						}
					}
				}
			}
			catch (WebException ex)
			{
				WebResponse errorResponse = ex.Response;
				using (Stream responseStream = errorResponse.GetResponseStream())
				{
					StreamReader reader = new StreamReader(responseStream, System.Text.Encoding.GetEncoding("utf-8"));
					String errorText = reader.ReadToEnd();
					Console.WriteLine(errorText);
					// log errorText
				}
				throw;
			}
		}

		static void performCopy(ZipArchiveEntry entry)
		{
			using (var e = entry.Open())
			{
				Console.WriteLine("Saving " + entry.FullName);
				if (File.Exists(entry.FullName))
				{
					string exePath = new Uri(System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase).LocalPath;
					// if we try to overwrite the current exe, it will fail
					// the trick is to move the current assembly to a new file
					if (string.Compare(new FileInfo(entry.FullName).FullName,exePath,StringComparison.OrdinalIgnoreCase) == 0)
					{
						string bakFileName = entry.FullName + ".bak";
						if (File.Exists(bakFileName))
							File.Delete(bakFileName);
						File.Move(entry.FullName, bakFileName);
					}
				}
				using (var fileStream = File.Create(entry.FullName))
				{
					e.CopyTo(fileStream);
					fileStream.Close();
				}
				
			}
		}

		private static void DisplayHelp()
		{
			Console.WriteLine("switch:");
			Console.WriteLine("  --help              : display this message");
			Console.WriteLine("");
			Console.WriteLine("  --api-url http://xx : use an alternative url for checking for updates");
			Console.WriteLine("  --force-download    : download the latest release even if it is not the most recent. Useful for tests");
			Console.WriteLine("  --use-preview       : download preview release if it is the most recent");
			Console.WriteLine("  --wait-for-days  30 : ensure the releases has been made public for at least X days");
		}
	}
}
