using System;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.Data.SqlClient;
using System.IO;
using System.Security.Cryptography;

namespace DupesMaintConsole
{
    internal class Program
	{
		//private static int _count;
		//private static DirectoryInfo _sourceDir;
		//private static Boolean _truncateCheckSum = false;
		private static readonly Model1 _popsModels = new Model1();
		//private static System.Diagnostics.Stopwatch _stopwatch;

		private static int Main(string[] args)
		{

			// Uses System.CommandLine beta library
			// see https://github.com/dotnet/command-line-api/wiki/Your-first-app-with-System.CommandLine

			RootCommand rootCommand = new RootCommand("DupesMaintConsole")
			{
				new Option("--folder", "The root folder of the tree to scan which must exist, 'F:/Picasa backup/c/photos'.")
					{
						Argument = new Argument<DirectoryInfo>().ExistingOnly(),
						Required = true
					},

				new Option("--replace", "Replace default (true) or append (false) to the db tables CheckSum & CheckSumDupes.")
					{
						Argument = new Argument<bool>(getDefaultValue: () => true),
						Required = false
					}

			};

			rootCommand.TreatUnmatchedTokensAsErrors = true;

			// setup the root command handler
			rootCommand.Handler = CommandHandler.Create((DirectoryInfo folder, bool replace) => {Process(folder, replace);});

			// call the method defined in the handler
			return rootCommand.InvokeAsync(args).Result;
		}


		public static void Process(DirectoryInfo folder, bool replace)
		{
			System.Diagnostics.Stopwatch _stopwatch = System.Diagnostics.Stopwatch.StartNew();

			Console.WriteLine($"{DateTime.Now}, INFO - target folder is {folder.FullName}\n\rTruncate tables is: {replace}.");
			Console.WriteLine($"{DateTime.Now}, INFO - press any key to start processing.");
			Console.ReadLine();

			if (replace)
			{
				// clear the CheckSum and CheckSumDups tables
				_popsModels.Database.ExecuteSqlCommand("truncate table CheckSum; truncate table CheckSumDups");
				_popsModels.SaveChanges();
				Console.WriteLine($"{DateTime.Now}, INFO - sqlcommands truncate table CheckSum; truncate table CheckSumDup were executed");
			}

			// main processing
			int fileCount = ProcessFiles(folder);

			_stopwatch.Stop();
			Console.WriteLine($"{DateTime.Now}, Total execution time: {_stopwatch.ElapsedMilliseconds / 60000} mins. # of files processed: {fileCount}.");
		}


		// process all the files matching the pattern in the the source directory tree
		private static int ProcessFiles(DirectoryInfo folder)
		{
			int _count = 0;

			System.Diagnostics.Stopwatch process100Watch = System.Diagnostics.Stopwatch.StartNew();

			FileInfo[] _files = folder.GetFiles("*", SearchOption.AllDirectories);

			// Process all the files in the source directory tree
			foreach (FileInfo fi in _files)
			{
				// calculate the SHA string for the file and return with the time taken in ms in a tuple
				(string SHA, int timerMs) = CalcSHA(fi);

				// insert row into CheckSum table
				CheckSum_ins(SHA, fi.FullName, fi.Extension, fi.CreationTimeUtc, fi.DirectoryName, fi.Length, timerMs);

				_count++;

				if (_count % 100 == 0)
				{
					process100Watch.Stop();
					Console.WriteLine($"{DateTime.Now}, INFO - {_count}. Last 100 in {process100Watch.ElapsedMilliseconds / 1000} secs. " +
						$"Completed: {(_count * 100) / _files.Length}%. " +
						$"Processing folder: {fi.DirectoryName}");
					process100Watch.Reset();
					process100Watch.Start();
				}
			}
			return _count;
		}


		// insert a new row into the CheckSum table
		private static void CheckSum_ins(string mySHA,
										string fullName,
										string fileExt,
										DateTime fileCreateDt,
										string directoryName,
										long fileLength,
										int timerMs)
		{
			// create the SqlParameters for the stored procedure
			SqlParameter _sHA = new SqlParameter("@SHA", mySHA);
			SqlParameter _folder = new SqlParameter("@Folder", directoryName);
			SqlParameter _theFileName = new SqlParameter("@TheFileName", fullName);
			SqlParameter _fileExt = new SqlParameter("@FileExt", fileExt);
			SqlParameter _fileSize = new SqlParameter("@FileSize", (int)fileLength);
			SqlParameter _fileCreateDt = new SqlParameter("@FileCreateDt", fileCreateDt);
			SqlParameter _timerMs = new SqlParameter("@TimerMs", timerMs);
			SqlParameter _notes = new SqlParameter("@Notes", DBNull.Value);

			// call the stored procedure
			_popsModels.Database.ExecuteSqlCommand("exec spCheckSum_ins	@SHA, @Folder,	@TheFileName,	@FileExt,   @FileSize,  @FileCreateDt,  @TimerMs,   @notes",
																		_sHA, _folder, _theFileName,	_fileExt,	_fileSize,	_fileCreateDt,	_timerMs,	_notes);
		}


		// calculate the SHA256 checksum for the file and return it with the elapsed processing time using a tuple
		private static (string SHA, int timerMs) CalcSHA(FileInfo fi)
		{
			System.Diagnostics.Stopwatch watch = System.Diagnostics.Stopwatch.StartNew();

			FileStream fs = fi.OpenRead();
			fs.Position = 0;

			// ComputeHash - returns byte array  
			byte[] bytes = SHA256.Create().ComputeHash(fs);

			// BitConverter used to put all bytes into one string, hyphen delimited  
			string bitString = BitConverter.ToString(bytes);
			
			watch.Stop();
			//Console.WriteLine($"{fi.Name}, length: {fi.Length}, execution time: {watch.ElapsedMilliseconds} ms");

			return (SHA: bitString, timerMs: (int)watch.ElapsedMilliseconds);
		}



	}
}
