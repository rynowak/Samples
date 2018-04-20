using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MusicStoreService
{
    public class ResiliencyMiddleware
    {
        private readonly RequestDelegate _next;
        private Random _rand;
        private IOptionsSnapshot<ResiliencyOptions> _options;

        public ResiliencyMiddleware(RequestDelegate next,
                                    IOptionsSnapshot<ResiliencyOptions> options)
        {
            _next = next;
            _rand = new Random();
            _options = options;
        }

        public Task InvokeAsync(HttpContext context)
        {
            var resiliency = _options.Value.ResiliencyPercent;

            if(_rand.Next(0, 100) >= resiliency)
            {
                throw new Exception("Computer says no.");
            }

            return this._next(context);
        }
    }
}
