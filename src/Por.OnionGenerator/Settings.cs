using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using Mono.Options;

namespace Por.OnionGenerator
{
	sealed class Settings
	{
		public readonly string AppName = AppDomain.CurrentDomain.FriendlyName;
		public Regex ToMatch { get; set; }
		public string OutFilename { get; set; }
		public string InFilename { get; set; }
		public string CheckDir { get; set; }
		public string BaseDir { get; set; }
		public int WorkerCount { get; set; }
		public List<string> ExtraArgs { get; set; }
		public int Verbosity { get; set; }
		public bool ShouldShowHelp { get; set; }
		
		OptionSet options;
		
		public Settings()
		{
			options = new OptionSet() {
				{ "m|match=", "create hidden service directories for onion addresses found matching {REGEX}",
					v => { if (v != null) ToMatch = new Regex(v, RegexOptions.Compiled); } },
				{ "o|out=", "append generated keys to {FILE}\nexclusive of -i,-c",
					v => { if (v != null) OutFilename = v; } },
				{ "i|in=", "find matching addresses from a previous run that was saved to {FILE}\nexclusive of -o,-c, requires -m",
					v => { if (v != null) InFilename = v; } },
				{ "c|check=", "verify that {DIR} is a valid onion directory whose hostname matches its private_key\nexclusive of -i,-o",
					v => { if (v != null) CheckDir = v; } },
				{ "b|basedir=", "use {DIR} as the base working directory",
					v => { if (v != null) BaseDir = v; } },
				{ "w|workers=", "spawn {NUM} worker threads to generate keys\ndefault 2*num_proc, ignored if -i",
					(int v) => WorkerCount = v },
				{ "v", "increase output verbosity",
					v => { if (v != null) ++Verbosity; } },
				{ "h|help",  "show this message and exit", 
					v => ShouldShowHelp = v != null }
			};
			BaseDir = ".";
			WorkerCount = Environment.ProcessorCount > 1 ? Environment.ProcessorCount * 2 : 1;
		}
		
		public bool TryParse(string[] args)
		{
			try
			{
				ExtraArgs = options.Parse(args);
			}
			catch (OptionException e)
			{
				ShowOptionsError(e.Message);
				return false;
			}
			return true;
		}

		public void ShowOptionsError(string message)
		{
			Console.Error.Write(AppName + ": ");
			Console.Error.WriteLine(message);
			Console.Error.WriteLine("Try `" + AppName + " --help' for more information.");
		}
		
		public void ShowHelp(TextWriter o)
		{
			o.WriteLine("Usage: " + AppName + " [-v] [-b] [[[-i|-o] filename] [-m regex] [-w number]|-c dir]");
			o.WriteLine("Brute-forces the creation of many RSA key-pairs attempting to find one whose");
			o.WriteLine("Tor onion address matches a given pattern. For the creation of vanity");
			o.WriteLine("onion addresses or burning through excess entropy.");
			o.WriteLine();
			o.WriteLine("Options:");
			options.WriteOptionDescriptions(o);
		}
	}
}
