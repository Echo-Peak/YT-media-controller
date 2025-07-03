using System.Net;
using System.Net.Http;
using YTMediaControllerSrv.Server.Middleware;
using YTMediaControllerSrv.YTDLP;

namespace YTMediaControllerSrv.Server
{
    internal class BackendServer
    {
        public CreateHttpServer server { get; set; }
        private UISocketServer uiSockerServer { get; set; }
        private YtdlpExec YTDLP { get; set; }
        private string endpoint = string.Empty;
        private HttpClient httpClient = new HttpClient();
        private VideoCache videoCache = new VideoCache();

        public BackendServer(string host, int port, UISocketServer uiSockerServer)
        {
            YTDLP = new YtdlpExec(videoCache);
            this.uiSockerServer = uiSockerServer;
            this.endpoint = $"http://{host}:{port}/";
        }

        public void Start()
        {
            var pipeline = new MiddlewarePipeline();

            pipeline.Use(new CORSMiddleware().Invoke);
            pipeline.Use(new MobileRouterMiddleware(uiSockerServer, YTDLP, endpoint).Invoke);
            pipeline.Use(new HLSMiddleware(httpClient, this.endpoint).Invoke);
            pipeline.Use(new DASHMiddleware(httpClient, videoCache).Invoke);
            pipeline.Use(new BaseMiddleware().Invoke);

            var middlewarePipeline = pipeline.Build();
            server = new CreateHttpServer(endpoint);
            server.OnRequest += async (ctx, req, rs) => await middlewarePipeline(ctx);
            Logger.Info($"Backend server started at {endpoint}");
        }

        public void Stop()
        {
            server.Stop();
        }
    }
}
