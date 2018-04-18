using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Model = MusicStoreUI.Models;
using Steeltoe.CircuitBreaker.Hystrix;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using System;

namespace MusicStoreUI.Services.HystrixCommands
{
    public class GetGenre : HystrixCommand<Model.Genre>
    {      
        private IMusicStore _storeService;
        private AppSettings _appSettings;
        private ILogger _logger;
        private int _intId;
        private string _name;

        public GetGenre(
            IHystrixCommandOptions options,
            IMusicStore storeService,
            IOptions<AppSettings> appsettings,
            ILogger<GetGenre> logger
            ) : base(options)
        {
            _storeService = storeService;
            _appSettings = appsettings.Value;
            _logger = logger;
        }

        public async Task<Model.Genre> GetGenreAsync(int id)
        {
            _intId = id;
            return await ExecuteAsync();
        }
        public async Task<Model.Genre> GetGenreAsync(string name)
        {
            _name = name;
            return await ExecuteAsync();
        }

        protected override async Task<Model.Genre> RunAsync()
        {
            var result = await FetchFromStoreAsync();
            if (result != null)
            {
                _logger.LogInformation("Genre returned from store!");
            }
            return result;
        }

        private async Task<Model.Genre> FetchFromStoreAsync()
        {
            if (string.IsNullOrEmpty(_name))
            {
                return await _storeService.GetGenreAsync(_intId);
            }
            else
            {
                return await _storeService.GetGenreAsync(_name);
            }
        }
    }
}
