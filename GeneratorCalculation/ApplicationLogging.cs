using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace GeneratorCalculation
{
	class ApplicationLogging
	{
		public static readonly ILoggerFactory LoggerFactory;

		static ApplicationLogging()
		{
			//var loggerFactory = LoggerFactory.Create(
			//	builder => builder
			//		// add console as logging target
			//		.AddConsole()
			//		// set minimum level to log
			//		.SetMinimumLevel(LogLevel.Debug)
			//);

			//throw new NotImplementedException();
			string configFilePath = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(System.AppContext.BaseDirectory), "appsettings.json");
			if (System.IO.File.Exists(configFilePath) == false)
				Console.Error.WriteLine($"{configFilePath} doesn't exist.");
			var builder = new ConfigurationBuilder()
				.SetBasePath(Directory.GetCurrentDirectory())
				.AddJsonFile(configFilePath, optional: true);
			var configuration = builder.Build();

			LoggerFactory = Microsoft.Extensions.Logging.LoggerFactory.Create(b =>
			{
				b.AddConfiguration(configuration.GetSection("Logging")).AddSimpleConsole();
			});
		}
	}
}
