using System.Net;
using System.Net.Http;
using YTMediaControllerSrv.Logging;
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
        private readonly ILogger Logger;

        public BackendServer(string host, int port, UISocketServer uiSockerServer, ILogger logger)
        {
            YTDLP = new YtdlpExec(videoCache);
            this.uiSockerServer = uiSockerServer;
            this.endpoint = $"http://{host}:{port}/";
            this.Logger = logger;
        }

        public void Start()
        {
            var pipeline = new MiddlewarePipeline();

            ILogger networkLogger = new Logger("NETWORK");

            pipeline.Use(new LoggingMiddleware(networkLogger).Invoke);
            pipeline.Use(new CORSMiddleware().Invoke);
            pipeline.Use(new MobileRouterMiddleware(uiSockerServer, YTDLP, endpoint, networkLogger).Invoke);
            pipeline.Use(new HLSMiddleware(httpClient, this.endpoint, networkLogger).Invoke);
            pipeline.Use(new DASHMiddleware(httpClient, videoCache, networkLogger).Invoke);
            pipeline.Use(new BaseMiddleware().Invoke);

            var middlewarePipeline = pipeline.Build();
            server = new CreateHttpServer(endpoint, Logger);
            server.OnRequest += async (ctx, req, rs) => await middlewarePipeline(ctx);
            Logger.Info($"Backend server started at {endpoint}");
        }

        public void Stop()
        {
            server.Stop();
        }
    }
}
