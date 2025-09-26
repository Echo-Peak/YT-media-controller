namespace YTMediaControllerUpdaterSrv.Types
{
    internal class ChannelsDto
    {
        public ChannelVersionInfo Alpha { get; set; }
        public ChannelVersionInfo Beta { get; set; }
        public ChannelVersionInfo Stable { get; set; }

        public ChannelVersionInfo Select(string channel)
        {
            if (channel == "alpha" && Alpha != null) { 
                return Alpha;
            }

            if (channel == "beta" && Beta != null)
            {
                return Beta;
            }

            if (channel == "stable" && Stable != null)
            {
                return Stable;
            }

            return null;
        }
    }
}
