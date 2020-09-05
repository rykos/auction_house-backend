using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace ah_backend
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args)
        {
            string path = default, key = default;
            return Host.CreateDefaultBuilder(args).ConfigureWebHostDefaults(webBuilder =>
            {
                webBuilder.UseStartup<Startup>().UseKestrel().ConfigureServices((context, services) =>
                {
                    path = context.Configuration["Certificate:Path"];
                    key = context.Configuration["Certificate:Key"];
                }).ConfigureKestrel(serverOptions =>
                {
                    serverOptions.ConfigureHttpsDefaults(x =>
                    {
                        if (path != default && key != default)
                        {
                            var clientCertificate = new X509Certificate2(path, key);
                            x.ServerCertificate = clientCertificate;
                        }
                    });
                });
            });
        }
    }
}
