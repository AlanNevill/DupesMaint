using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace DupesMaintConsole
{
    class Program
    {
		private static int _count;
        private static DirectoryInfo _sourceDir;
        private static Boolean _truncateCheckSum = false;
        private static readonly Model1 _popsModels = new Model1();

        static void Main(string[] args)
        {
            var mainWatch = System.Diagnostics.Stopwatch.StartNew();

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

			Console.WriteLine($"INFO - target folder is {_sourceDir.FullName}, and truncate tables is: {_truncateCheckSum}.");
			Console.WriteLine("INFO - press any key to start processing.");
			_ = Console.ReadLine();

			Clear_CheckSum_CheckSumDupes();
			ProcessFiles(_sourceDir);

			mainWatch.Stop();
			Console.WriteLine($"Total execution time: {mainWatch.ElapsedMilliseconds / 1000} seconds");
			Console.WriteLine("Finished - press any key to close");
			_ = Console.ReadLine();


		}


		static bool ValidateFolder(string folderArg)
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



		static void Clear_CheckSum_CheckSumDupes()
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
		static void ProcessFiles(DirectoryInfo sourceDir)
		{
			var process100Watch = System.Diagnostics.Stopwatch.StartNew();

			// Process all the jpg files in the source directory tree
			foreach (FileInfo fi in sourceDir.GetFiles("*.jpg", SearchOption.AllDirectories))
			{
				// calculate the SHA string for the file and return with the time taken in ms in a tuple
				var (SHA, timerMs) = CalcSHA(fi);

				// insert row into CheckSum table
				CheckSum_ins(SHA, fi.FullName, fi.Extension, fi.CreationTimeUtc, fi.DirectoryName, fi.Length, timerMs);

				_count++;

				if (_count % 100 == 0)
				{
					process100Watch.Stop();
					Console.WriteLine($"INFO - {_count} processed in {process100Watch.ElapsedMilliseconds /1000} secs. Processing folder {fi.DirectoryName}");
					process100Watch.Reset();
					process100Watch.Start();
				}
			}
		}


		// insert a new row into the CheckSum table
		private static void CheckSum_ins(	string mySHA,
											string fullName,
											string fileExt,
											DateTime fileCreateDt,
											string directoryName,
											long fileLength,
											int timerMs)
		{
			// create the SqlParameters for the stored procedure
			var _sHA = new SqlParameter("@SHA", mySHA);
			var _folder = new SqlParameter("@Folder", directoryName);
			var _theFileName = new SqlParameter("@TheFileName", fullName);
			var _fileExt = new SqlParameter("@FileExt", fileExt);
			var _fileSize = new SqlParameter("@FileSize", (int)fileLength);
			var _fileCreateDt = new SqlParameter("@FileCreateDt", fileCreateDt);
			var _timerMs = new SqlParameter("@TimerMs", timerMs);
			var _notes = new SqlParameter("@Notes", DBNull.Value);

			// call the stored procedure
			_popsModels.Database.ExecuteSqlCommand("exec spCheckSum_ins	@SHA, @Folder,	@TheFileName,	@FileExt,   @FileSize,  @FileCreateDt,  @TimerMs,   @notes",
																		_sHA, _folder,	_theFileName,	_fileExt,   _fileSize,  _fileCreateDt,  _timerMs,   _notes);
		}


		// calculate the SHA256 checksum for the file and return it with the elapsed processing time using a tuple
		static (string SHA, int timerMs) CalcSHA(FileInfo fi)
		{
			var watch = System.Diagnostics.Stopwatch.StartNew();

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
