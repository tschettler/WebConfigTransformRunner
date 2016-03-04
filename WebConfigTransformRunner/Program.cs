namespace WebConfigTransformRunner
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;

    using Microsoft.Web.XmlTransform;

    public class Program
    {
        public static void Main(string[] args)
        {
            var root = Directory.GetCurrentDirectory();
            string config = null;
            var outputDir = root;

            switch (args.Length)
            {
                case 0:
                    config = GetConfigFile(root);

                    break;
                case 1:
                    if (args[0].EndsWith("help", StringComparison.OrdinalIgnoreCase))
                    {
                        Console.WriteLine("WebConfigTransformRunner.exe");
                        Console.WriteLine("WebConfigTransformRunner.exe ConfigFileName");
                        Console.WriteLine("WebConfigTransformRunner.exe OutputDirectory");
                        Console.WriteLine("WebConfigTransformRunner.exe ConfigFileName OutputDirectory");
                        Environment.Exit(1);
                    }

                    if (Path.GetExtension(args[0]) == ".config")
                    {
                        config = args[0];
                    }
                    else
                    {
                        config = GetConfigFile(root);
                        outputDir = args[0];
                    }

                    break;
                case 2:
                    config = args[0];
                    outputDir = args[1];
                    break;
            }

            if (config == null)
            {
                Console.WriteLine("Could not find a config file to transform in : {0}", root);
                Environment.Exit(1);
            }

            if (!File.Exists(config) || !Directory.Exists(outputDir))
            {
                Console.WriteLine("The config or output directory do not exist!");
                Environment.Exit(2);
            }

            var fileNameFormat = outputDir == root ? "{0}.transformed" : "{0}";
            var files = GetTransformableFiles(root, config);
            using (var doc = new XmlTransformableDocument())
            {
                Console.WriteLine("Using {0}", config);
                foreach (var file in files)
                {
                    doc.Load(config);
                    using (var tranform = new XmlTransformation(file))
                    {
                        var transformed = false;
                        try
                        {
                            Console.WriteLine("Applying transform for {0}", Path.GetFileName(file));
                            transformed = tranform.Apply(doc);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex.Message);
                            Environment.Exit(3);
                        }

                        if (transformed)
                        {
                            var filename = string.Format(fileNameFormat, Path.GetFileName(file));
                            var outputfile = Path.Combine(outputDir, filename);
                            doc.Save(outputfile);
                            Console.WriteLine("Saved to {0}", outputfile);
                        }
                        else
                        {
                            Console.WriteLine("Could not apply transform");
                            Environment.Exit(3);
                        }
                    }
                }
            }

            Console.WriteLine("Completed!");
        }

        private static string GetConfigFile(string root)
        {
            var files = new[] { "app.config", "web.config" };
            var config = files.SelectMany(e => Directory.GetFiles(root, e)).FirstOrDefault();
            return config;
        }

        private static IEnumerable<string> GetTransformableFiles(string root, string config)
        {
            var filepattern = Path.GetFileName(config).Replace(".", ".*.");
            var files = Directory.GetFiles(root, filepattern);

            return files;
        } 
    }
}
