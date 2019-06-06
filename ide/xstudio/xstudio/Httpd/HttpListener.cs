using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Text;
using HPSocketCS;

namespace xstudio.Httpd
{
    public class HttpListener : IHttpListener
    {
        public delegate void RequestCallback(HttpRequest req);

        private readonly RequestCallback _cb;

        private HttpServer httpd;

        public HttpListener(RequestCallback cb)
        {
            _cb = cb;
            httpd = new HttpServer();

            httpd.OnHeadersComplete += OnHeadersComplete;
            httpd.OnBody += OnBody;
            httpd.OnMessageComplete += OnMessageComplete;
            httpd.OnParseError += OnParseError;
        }

        public void Stop()
        {
            if (httpd.IsStarted)
            {
                httpd.Stop();
            }
        }

        public void Shutdown()
        {
            if (httpd != null)
            {
                httpd.Destroy();
                httpd = null;
            }
        }

        public void Start(string host, ushort port)
        {
            if (!httpd.IsStarted)
            {
                httpd.IpAddress = host;
                httpd.Port = port;
                httpd.Start();
            }
        }

        private HttpParseResult OnParseError(IntPtr connId, int errorCode, string errorDesc)
        {
            return HttpParseResult.Ok;
        }

        private HttpParseResult OnMessageComplete(IntPtr connId)
        {
            if (_cb != null)
            {
                var req = new HttpRequest(httpd, connId);
                _cb(req);
            }

            return HttpParseResult.Ok;
        }

        private HttpParseResult OnPointerDataBody(IntPtr connId, IntPtr pData, int length)
        {
            return HttpParseResult.Ok;
        }

        private HttpParseResult OnBody(IntPtr connId, byte[] bytes)
        {
            var stream = httpd.GetExtra<MemoryStream>(connId);
            if (stream == null)
            {
                stream = new MemoryStream();
                httpd.SetExtra(connId, stream);
            }
            stream.Write(bytes, 0, bytes.Length);
            return HttpParseResult.Ok;
        }

        private HttpParseResultEx OnHeadersComplete(IntPtr connId)
        {
            return HttpParseResultEx.Ok;
        }
    }

    public class HttpRequest
    {
        private readonly IntPtr _handle;
        private readonly HttpServer _httpd;


        public HttpRequest(HttpServer httpd, IntPtr handle)
        {
            _httpd = httpd;
            _handle = handle;

            Method = httpd.GetMethod(handle);
            Path = httpd.GetUrlField(handle, HttpUrlField.Path);
            Query = httpd.GetUrlField(handle, HttpUrlField.QueryString);

            //set UserHost
            Host = httpd.GetHost(handle);

            //set UserHostAddress
            var ip = string.Empty;
            ushort port = 0;
            if (httpd.GetRemoteAddress(handle, ref ip, ref port))
            {
                UserHostAddress = string.Format("{0}:{1}", ip, port);
            }

            //set headers
            foreach (var header in httpd.GetAllHeaders(handle))
            {
                if (Headers == null)
                {
                    Headers = new NameValueCollection();
                }
                Headers[header.Name] = header.Value;
            }

            var stream = httpd.GetExtra<MemoryStream>(handle);
            if (stream != null)
            {
                RequestBody = stream.ToArray();
                stream.Dispose();
                httpd.RemoveExtra(handle);
            }
            else
            {
                RequestBody = new byte[0];
            }
        }

        public string Method { get; private set; }
        public string Path { get; private set; }
        public string Query { get; private set; }
        public string Host { get; private set; }
        public string UserHostAddress { get; private set; }
        public NameValueCollection Headers { get; private set; }
        public byte[] RequestBody { get; private set; }

        public void Respond(HttpStatusCode code, IDictionary<string, string> headers, byte[] body)
        {
            if (body == null)
            {
                body = new byte[0];
            }

            if (headers == null)
            {
                headers = new Dictionary<string, string>();
            }
            headers["Content-Length"] = body.Length.ToString();
            var theaders = new List<THeader>();
            theaders.AddRange(headers.Select(pair => new THeader {Name = pair.Key, Value = pair.Value}));

            _httpd.SendResponse(_handle, code, code.ToString().ToUpper(), theaders.ToArray(), body, body.Length);

            if (_httpd.IsKeepAlive(_handle) == false)
            {
                _httpd.Release(_handle);
            }
        }

        public void Respond(HttpStatusCode code, IDictionary<string, string> headers, string body)
        {
            Respond(code, headers, Encoding.UTF8.GetBytes(body + ""));
        }
    }
}