﻿using Discord;
using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;

namespace Aspid
{
    /// <summary>
    /// Html Converter. Converts HTML string and URLs to image bytes
    /// taken and slightly tweaked from https://github.com/andrei-m-code/net-core-html-to-image
    /// check it out, this thing helped me a lot
    /// </summary>
    public class HtmlConverter
    {
        private static string toolFilename = "wkhtmltoimage";
        private static string directory;
        private static string toolFilepath;

        static HtmlConverter()
        {
            directory = AppContext.BaseDirectory;

            //Check on what platform we are
            if (System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.Windows))
            {
                toolFilepath = Path.Combine(directory, toolFilename + ".exe");

                if (!File.Exists(toolFilepath))
                {
                    var assembly = typeof(HtmlConverter).GetTypeInfo().Assembly;
                    var type = typeof(HtmlConverter);
                    var ns = type.Namespace;

                    using var resourceStream = assembly.GetManifestResourceStream($"{ns}.{toolFilename}.exe");
                    using var fileStream = File.OpenWrite(toolFilepath);
                    resourceStream.CopyTo(fileStream);
                }
            }
            else if (System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.Linux))
            {
                //Check if wkhtmltoimage package is installed on this distro in using which command
                toolFilepath = "/home/xentellion/HtI/usr/local/bin/" + toolFilename;
            }
            else
            {
                //OSX not implemented
                throw new Exception("OSX Platform not implemented yet");
            }
        }

        /// <summary>
        /// Converts HTML string to image
        /// </summary>
        /// <param name="html">HTML string</param>
        /// <param name="width">Output document width</param>
        /// <param name="format">Output image format</param>
        /// <param name="quality">Output image quality 1-100</param>
        /// <returns></returns>
        public byte[] FromHtmlString(string html, int width = 1024, ImageFormat format = ImageFormat.Png, int quality = 100)
        {
            var filename = Path.Combine(directory, $"{Guid.NewGuid()}.html");
            File.WriteAllText(filename, html);
            var bytes = FromUrl(filename, width, format, quality);
            File.Delete(filename);
            return bytes;
        }

        /// <summary>
        /// Converts HTML page to image
        /// </summary>
        /// <param name="url">Valid http(s):// URL</param>
        /// <param name="width">Output document width</param>
        /// <param name="format">Output image format</param>
        /// <param name="quality">Output image quality 1-100</param>
        /// <returns></returns>
        public byte[] FromUrl(string url, int width = 1024, ImageFormat format = ImageFormat.Png, int quality = 100)
        {
            var imageFormat = format.ToString().ToLower();
            var filename = Path.Combine(directory, $"{Guid.NewGuid().ToString()}.{imageFormat}");

            string args;

            if (IsLocalPath(url))
            {
                args = $"--quality {quality} --width {width} -f {imageFormat} \"{url}\" \"{filename}\"";
            }
            else
            {
                args = $"--quality {quality} --width {width} -f {imageFormat} {url} \"{filename}\"";
            }

            Process process = Process.Start(new ProcessStartInfo(toolFilepath, args)
            {
                WindowStyle = ProcessWindowStyle.Hidden,
                CreateNoWindow = true,
                UseShellExecute = false,
                WorkingDirectory = directory,
                RedirectStandardError = true
            });

            process.ErrorDataReceived += (object sender, DataReceivedEventArgs e) => throw new Exception(e.Data);
            process.WaitForExit();

            if (File.Exists(filename))
            {
                var bytes = File.ReadAllBytes(filename);
                File.Delete(filename);
                return bytes;
            }

            throw new Exception("Something went wrong. Please check input parameters");
        }

        private static bool IsLocalPath(string path)
        {
            if (path.StartsWith("http"))
            {
                return false;
            }

            return new Uri(path).IsFile;
        }
    }
}