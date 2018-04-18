using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MusicStoreUI.Models;
using MusicStoreUI.Services;
using MusicStoreUI.Services.HystrixCommands;
using Steeltoe.CircuitBreaker.Hystrix;
using Steeltoe.CircuitBreaker.Hystrix.Exceptions;
using System;
using System.Threading.Tasks;

namespace MusicStoreUI.Controllers
{
    public class StoreController : Controller
    {
        private readonly AppSettings _appSettings;
        private readonly GetGenres _genres;
		private readonly ILogger _logger;

        public StoreController(
			GetGenres genres, 
			IOptions<AppSettings> options, 
			ILogger<StoreController> logger)
        {
            _appSettings = options.Value;
            _genres = genres;
			_logger = logger;
        }

     
        // GET: /Store/
        public async Task<IActionResult> Index()
        {
            var genres = await _genres.GetGenresAsync();

            return View(genres);
        }

        // GET: /Store/Browse?genre=Disco
        public async Task<IActionResult> Browse(
            [FromServices] Services.HystrixCommands.GetGenre genreCommand,
            string genre)
        {
            var genreModel = await genreCommand.GetGenreAsync(genre);
            return View(genreModel);
        }

        public async Task<IActionResult> Details(
            [FromServices]  Services.HystrixCommands.GetAlbum albumCommand,
            int id)
        {
            var album = await albumCommand.GetAlbumAsync(id);
            return View(album);
        }
    }
}
