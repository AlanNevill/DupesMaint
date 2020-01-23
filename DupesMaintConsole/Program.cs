using System;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Security.Cryptography;

namespace DupesMaintConsole
{
	internal class Program
	{
		private static int _count;
		private static DirectoryInfo _sourceDir;
		private static Boolean _truncateCheckSum = false;
		private static readonly Model1 _popsModels = new Model1();
		private static System.Diagnostics.Stopwatch _stopwatch;

		private static void Main(string[] args)
		{
			_stopwatch = System.Diagnostics.Stopwatch.StartNew();

			if (string.IsNullOrEmpty(args[0]))
			{
				Console.WriteLine("ERROR - the target folder was not specified");
				return;
			}

			// check for 2nd argument supplied requesting truncation of CheckSum and CheckSumDups tables
			if (args.Length == 2)
			{
				if (args[1].ToLower() == "true")
				{
					_truncateCheckSum = true;
				}
			}

			// check that the root folder exists
			if (!ValidateFolder(args[0]))
			{
				Console.WriteLine("ERROR - the target folder not found.");
				return;
			}

			Console.WriteLine($"INFO - target folder is {_sourceDir.FullName}\n\rTruncate tables is: {_truncateCheckSum}.");
			Console.WriteLine("INFO - press any key to start processing.");
			_ = Console.ReadLine();

			Clear_CheckSum_CheckSumDupes();
			ProcessFiles(_sourceDir);

			_stopwatch.Stop();
			Console.WriteLine($"Total execution time: {_stopwatch.ElapsedMilliseconds / 60000} mins. # files processed: {_count}.");
			Console.WriteLine("Finished - press any key to close");
			Console.ReadLine();


		}

		private static bool ValidateFolder(string folderArg)
		{
			// Specify the root directory you want to scan.
			DirectoryInfo di = new DirectoryInfo(@folderArg);
			try
			{
				// Determine whether the directory exists.
				if (di.Exists)
				{
					// Indicate that the directory already exists.
					Console.WriteLine($"INFO - [{di.FullName}] - path exists.");
					_sourceDir = di;
					return true;
				}

				Console.WriteLine($"ERROR - {folderArg} directory does not exist.");
				return false;

			}
			catch (Exception e)
			{
				Console.WriteLine($"ERROR - The process failed: {e.ToString()}");
				throw;
			}
		}

		private static void Clear_CheckSum_CheckSumDupes()
		{

			// if command line argument 2 is set to true
			if (_truncateCheckSum)
			{
				// clear the CheckSum and CheckSumDups tables
				_popsModels.Database.ExecuteSqlCommand("truncate table CheckSum; truncate table CheckSumDups");
				_popsModels.SaveChanges();
				Console.WriteLine("INFO - truncate table CheckSum; truncate table CheckSumDup were executed");
			}
		}


		// process all the files matching the pattern in the the source directory tree
		private static void ProcessFiles(DirectoryInfo sourceDir)
		{
			System.Diagnostics.Stopwatch process100Watch = System.Diagnostics.Stopwatch.StartNew();

			FileInfo[] _files = sourceDir.GetFiles("*", SearchOption.AllDirectories);

			// Process all the jpg files in the source directory tree
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
					Console.WriteLine($"INFO - {_count}. Last 100 in {process100Watch.ElapsedMilliseconds / 1000} secs. " +
						$"Completed: {(_count * 100) / _files.Length}%. " +
						$"Elapsed: {_stopwatch.ElapsedMilliseconds / 60000} min. " +
						$"Processing folder: {fi.DirectoryName}");
					process100Watch.Reset();
					process100Watch.Start();
				}
			}
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
																		_sHA, _folder, _theFileName, _fileExt, _fileSize, _fileCreateDt, _timerMs, _notes);
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
