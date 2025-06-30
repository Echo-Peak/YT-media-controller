using System.Net;
using System.Net.Http;
using YTMediaControllerSrv.Server.Middleware;
using YTMediaControllerSrv.Streaming;

namespace YTMediaControllerSrv.Server
{
    internal class BackendServer
    {
        public CreateHttpServer server { get; set; }
        private UISocketServer uiSockerServer { get; set; }
        private string endpoint = string.Empty;
        private StreamingService streamingService { get; set; }
        private HttpClient httpClient = new HttpClient(new HttpClientHandler
        {
            AutomaticDecompression = DecompressionMethods.Deflate
        });

        public BackendServer(string host, int port, UISocketServer uiSockerServer)
        {
            this.uiSockerServer = uiSockerServer;
            this.endpoint = $"http://{host}:{port}/";
            streamingService = new StreamingService(
                new DASHStreamer(),
                new HLSStreamer(httpClient, this.endpoint)
                );
        }

        public void Start()
        {
            var pipeline = new MiddlewarePipeline();

            pipeline.Use(new CORSMiddleware().Invoke);
            pipeline.Use(new MobileRouterMiddleware(streamingService, uiSockerServer, endpoint).Invoke);
            pipeline.Use(new HLSMiddleware(streamingService).Invoke);
            pipeline.Use(new DASHMiddleware(streamingService, httpClient).Invoke);
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
