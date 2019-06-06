namespace xstudio.Httpd
{
    public interface IHttpListener
    {
        void Start(string host, ushort port);
        void Stop();
        void Shutdown();
    }
}