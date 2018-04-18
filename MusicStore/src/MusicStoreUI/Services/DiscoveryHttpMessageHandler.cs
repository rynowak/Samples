// Copyright 2017 the original author or authors.
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using Microsoft.Extensions.Logging;
using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Steeltoe.Common.Discovery
{
    public class DiscoveryHttpMessageHandler : DelegatingHandler
    {
		// note that Random isn't really thread safe when used this way
        // but it's close enough for our purposes.
        protected static Random _random = new Random();

        private readonly IDiscoveryClient _client;
        private readonly ILogger _logger;
        
        public DiscoveryHttpMessageHandler(
			IDiscoveryClient client, 
			ILogger<DiscoveryHttpClientHandler> logger)
        {
            if (client == null)
            {
                throw new ArgumentNullException(nameof(client));
            }

			if (logger == null)
			{
				throw new ArgumentNullException(nameof(logger));
			}

            _client = client;
			_logger = logger;
		}

        protected virtual Uri LookupService(Uri current)
        {
			_logger.LogDebug("Looking up service for {url}", current);
            if (!current.IsDefaultPort)
            {
                return current;
            }

            var instances = _client.GetInstances(current.Host);
            if (instances.Count > 0)
            {
                var index = _random.Next(instances.Count);
				var result = instances[index].Uri;
                current = new Uri(result, current.PathAndQuery);
				_logger.LogDebug("Resolved {url} to {service}", current, result);
            }
   
            return current;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
			var current = request.RequestUri;
			try
			{
				request.RequestUri = LookupService(current);
				return await base.SendAsync(request, cancellationToken);
			}
			finally
			{
				request.RequestUri = current;
			}

        }
    }
}