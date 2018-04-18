using Microsoft.Extensions.Logging;
using MusicStoreUI.Models;
using Steeltoe.Common.Discovery;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace MusicStoreUI.Services
{
    public class MusicStoreService : IMusicStore
    {
        private const string TOP_SELLING_URL = "TopSelling";
        private const string GENRES_URL = "Genres";
        private const string GENRE_URL = "Genre";
        private const string ALBUMS_URL = "Albums";
        private const string ALBUM_URL = "Album";
        private const string ARTISTS_URL = "Artists";
        private const string ARTIST_URL = "Artist";

        private readonly HttpClient _httpClient;

        public MusicStoreService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<Genre> GetGenreAsync(string genre)
        {
            var response = await _httpClient.GetAsync($"{GENRE_URL}?name={genre}");
            var result = Genre.From(await response.Content.ReadAsAsync<GenreJson>());

            response = await _httpClient.GetAsync($"{ALBUMS_URL}?genre={genre}");
            result.Albums = Album.From(await response.Content.ReadAsAsync<List<AlbumJson>>());
            return result;
        }

        public async Task<Genre> GetGenreAsync(int id)
        {
            var response = await _httpClient.GetAsync($"{GENRE_URL}?id={id}");
            var result = Genre.From(await response.Content.ReadAsAsync<GenreJson>());
            return result;
        }

        public async Task<List<Genre>> GetGenresAsync()
        {
            var response = await _httpClient.GetAsync(GENRES_URL);
            var result = Genre.From(await response.Content.ReadAsAsync<List<GenreJson>>());
            return result;
        }

        public async Task<List<Album>> GetTopSellingAlbumsAsync(int count = 6)
        {
            var response = await _httpClient.GetAsync($"{TOP_SELLING_URL}?count={count}");
            var result = Album.From(await response.Content.ReadAsAsync<List<AlbumJson>>());
            return result;
        }

        // GET: /Store/Album?id=#
        public async Task<Album> GetAlbumAsync(int id)
        {
            var response = await _httpClient.GetAsync($"{ALBUM_URL}?id={id}");
            var result = Album.From(await response.Content.ReadAsAsync<AlbumJson>());
            return result;
        }

        public async Task<Album> GetAlbumAsync(string title)
        {
            var response = await _httpClient.GetAsync($"{ALBUM_URL}?title={title}");
            var result = Album.From(await response.Content.ReadAsAsync<AlbumJson>());
            return result;
        }

        public async Task<List<Album>> GetAllAlbumsAsync()
        {
            var response = await _httpClient.GetAsync($"{ALBUMS_URL}?genre=All");
            var result = Album.From(await response.Content.ReadAsAsync<List<AlbumJson>>());
            return result;
        }

        public async Task<Artist> GetArtistAsync(int id)
        {
            var response = await _httpClient.GetAsync($"{ARTIST_URL}?id={id}");
            var result = Artist.From(await response.Content.ReadAsAsync<ArtistJson>());
            return result;
        }

        public async Task<List<Artist>> GetAllArtistsAsync()
        {
            var response = await _httpClient.GetAsync(ARTISTS_URL);
            var result = Artist.From(await response.Content.ReadAsAsync<List<ArtistJson>>());
            return result;
        }

        public async Task<bool> AddAlbumAsync(Album album)
        {
            var response = await _httpClient.PostAsJsonAsync(ALBUM_URL, AlbumJson.From(album));
            response.EnsureSuccessStatusCode();
            return true;
        }

        public async Task<bool> UpdateAlbumAsync(Album album)
        {
            var response = await _httpClient.PutAsJsonAsync(ALBUM_URL, AlbumJson.From(album));
            response.EnsureSuccessStatusCode();
            return true;
        }

        public async Task<bool> RemoveAlbumAsync(Album album)
        {
            var response = await _httpClient.DeleteAsync($"{ALBUM_URL}/{album.AlbumId}");
            response.EnsureSuccessStatusCode();
            return true;
        }
    }
}
