using System;
using System.Collections.Generic;
using System.Linq;
using FluentValidation.Results;
using NLog;
using NzbDrone.Common.Extensions;
using NzbDrone.Common.Http;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Indexers.Newznab;
using NzbDrone.Core.Parser;

namespace NzbDrone.Core.Indexers.Headphones
{
    public class Headphones : HttpIndexerBase<HeadphonesSettings>
    {
        private readonly IHeadphonesCapabilitiesProvider _capabilitiesProvider;

        public override string Name => "Headphones VIP";

        public override DownloadProtocol Protocol => DownloadProtocol.Usenet;

        public override int PageSize => _capabilitiesProvider.GetCapabilities(Settings).DefaultPageSize;

        public Headphones(IHeadphonesCapabilitiesProvider capabilitiesProvider, IHttpClient httpClient, IIndexerStatusService indexerStatusService, IConfigService configService, IParsingService parsingService, Logger logger)
            : base(httpClient, indexerStatusService, configService, parsingService, logger)
        {
            _capabilitiesProvider = capabilitiesProvider;
        }

        public override IIndexerRequestGenerator GetRequestGenerator()
        {
            return new HeadphonesRequestGenerator(_capabilitiesProvider)
            {
                PageSize = PageSize,
                Settings = Settings
            };
        }

        public override IParseIndexerResponse GetParser()
        {
            return new NewznabRssParser();
        }

        public override HttpRequest GetDownloadRequest(string link)
        {
            var request = new HttpRequest(link)
            {
                Credentials = new BasicNetworkCredential(Settings.Username, Settings.Password)
            };

            return request;
        }

        protected override void Test(List<ValidationFailure> failures)
        {
            base.Test(failures);

            if (failures.Any())
            {
                return;
            }

            failures.AddIfNotNull(TestCapabilities());
        }

        protected virtual ValidationFailure TestCapabilities()
        {
            try
            {
                var capabilities = _capabilitiesProvider.GetCapabilities(Settings);

                if (capabilities.SupportedSearchParameters != null && capabilities.SupportedSearchParameters.Contains("q"))
                {
                    return null;
                }

                return new ValidationFailure(string.Empty, "Indexer does not support required search parameters");
            }
            catch (Exception ex)
            {
                _logger.Warn(ex, "Unable to connect to indexer: " + ex.Message);

                return new ValidationFailure(string.Empty, "Unable to connect to indexer, check the log for more details");
            }
        }
    }
}
