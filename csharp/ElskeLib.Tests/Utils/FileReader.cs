/*
 * Copyright (c) Johannes Knittel
 *
 * This source code is licensed under the MIT license found in the
 * LICENSE file in the root directory of this source tree.
 */

using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text.Json;
using System.Threading;

namespace ElskeLib.Tests.Utils
{
    public static class FileReader
    {

        private static StreamReader GetReader(string fn)
        {
            var isGzip = fn.EndsWith(".gz", StringComparison.OrdinalIgnoreCase);
            return new StreamReader(isGzip
                ? new BufferedStream(new GZipStream(File.OpenRead(fn), CompressionMode.Decompress))
                : File.OpenRead(fn));
        }

        public static StreamWriter GetWriter(string fn)
        {
            if(File.Exists(fn))
                File.Delete(fn);
            var isGzip = fn.EndsWith(".gz", StringComparison.OrdinalIgnoreCase);
            return new StreamWriter(isGzip
                ? new BufferedStream(new GZipStream(File.OpenWrite(fn), CompressionMode.Compress))
                : File.OpenWrite(fn));
        }


        public static BinaryReader GetBinaryReader(string fn)
        {
            var isGzip = fn.EndsWith(".gz", StringComparison.OrdinalIgnoreCase);
            return new BinaryReader(isGzip
                ? new BufferedStream(new GZipStream(File.OpenRead(fn), CompressionMode.Decompress))
                : File.OpenRead(fn));
        }

        public static BinaryWriter GetBinaryWriter(string fn)
        {
            if (File.Exists(fn))
                File.Delete(fn);
            var isGzip = fn.EndsWith(".gz", StringComparison.OrdinalIgnoreCase);
            return new BinaryWriter(isGzip
                ? new BufferedStream(new GZipStream(File.OpenWrite(fn), CompressionMode.Compress))
                : File.OpenWrite(fn));
        }


        /// <summary>
        /// read file line by line and decompress gzip stream if file extension indicates .gz file
        /// </summary>
        /// <param name="fn"></param>
        /// <returns></returns>
        public static IEnumerable<string> ReadLines(string fn)
        {
            using (var reader = GetReader(fn))
            {
                string l;
                while ((l = reader.ReadLine()) != null)
                    yield return l;
            }
        }

        public static IEnumerable<string> ReadLines(string fn, CancellationToken token)
        {
            using (var reader = GetReader(fn))
            {
                string l;
                while (!token.IsCancellationRequested && (l = reader.ReadLine()) != null)
                    yield return l;
            }
        }

        public static IEnumerable<T> ReadLines<T>(string fn)
        {
            using (var reader = GetReader(fn))
            {
                string l;
                while ((l = reader.ReadLine()) != null)
                {
                    yield return JsonSerializer.Deserialize<T>(l);
                }
            }
        }

        public static void WriteLines(string fn, IEnumerable<string> lines)
        {
            using (var writer = GetWriter(fn))
            {
                foreach (var line in lines)
                {
                    writer.WriteLine(line);
                }
            }
        }

        public static void WriteLines<T>(string fn, IEnumerable<T> objects)
        {
            using (var writer = GetWriter(fn))
            {
                foreach (var o in objects)
                {
                    writer.WriteLine(JsonSerializer.Serialize(o));
                }
            }
        }
    }
}
