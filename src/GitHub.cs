using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Serialization.Json;
using System.Security.Cryptography;
using System.Text;
using System.Xml;
using System.Xml.Linq;

struct Content
{
    internal string Url;
    internal bool Update;
}

static class GitHub
{
    static readonly WebClient client = new() { BaseAddress = "https://api.github.com/repos/flarialmc/newcdn/" };

    static readonly SHA1 sha = SHA1.Create();

    internal static Content Get(string url, string path)
    {
        try
        {
            client.Headers[HttpRequestHeader.UserAgent] = "Flarial.Loader";
            using var stream = client.OpenRead($"contents/{url}");
            using var reader = JsonReaderWriterFactory.CreateJsonReader(stream, XmlDictionaryReaderQuotas.Max);
            var _ = XElement.Load(reader);
            return new() { Url = _.Element("download_url").Value, Update = !_.Element("sha").Value.Equals(Hash(path), StringComparison.OrdinalIgnoreCase) };
        }
        catch (Exception _) when (_ is ArgumentNullException || _ is WebException) { return default; }
    }

    static string Hash(string path)
    {
        try
        {
            var blob = File.ReadAllBytes(path);
            var header = Encoding.UTF8.GetBytes($"blob {blob.Length}\0");

            var buffer = new byte[header.Length + blob.Length];
            Buffer.BlockCopy(header, 0, buffer, 0, header.Length);
            Buffer.BlockCopy(blob, 0, buffer, header.Length, blob.Length);

            return BitConverter.ToString(sha.ComputeHash(buffer)).Replace("-", string.Empty);
        }
        catch { return null; }
    }
}