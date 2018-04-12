using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using TraktApiSharp;
using TraktApiSharp.Authentication;
using TraktApiSharp.Enums;
using TraktApiSharp.Objects.Basic;
using TraktApiSharp.Objects.Get.Collection;
using TraktApiSharp.Objects.Get.Watchlist;

namespace AutodlConfigurator
{
    /// <summary>
    /// Class providing interface to Trakt API.
    /// </summary>
    public class TraktMovieDbConnector : IMovieDbConnector
    {
        /// <summary>
        /// Trakt client.
        /// </summary>
        public TraktClient TraktClient;

        /// <summary>
        /// Trakt client ID.
        /// </summary>
        private string _clientId;

        /// <summary>
        /// Trakt client secret.
        /// </summary>
        private string _clientSecret;

        /// <summary>
        /// Full path that the access token is saved in.
        /// </summary>
        private string _accessTokenPathWithName = Path.Combine(Directory.GetCurrentDirectory(), "accesstoken.txt");

        /// <summary>
        /// Constructor for TraktMovieDBConnector.
        /// </summary>
        /// <param name="clientId">Trakt API Client ID.</param>
        /// <param name="clientSecret">Trakt API Client Secret.</param>
        public TraktMovieDbConnector(string clientId, string clientSecret)
        {
            // Assign internal data
            this._clientId = clientId;
            this._clientSecret = clientSecret;            

            // Load access token from file if generated
            // Else generate access token from user
            if (File.Exists(this._accessTokenPathWithName))
            {
                AutodlLogger.Log(AutodlLogLevel.DEBUG,  $"Trying to read accesstoken file {this._accessTokenPathWithName}");
                try
                {
                    using (StreamReader sr = new StreamReader(this._accessTokenPathWithName))
                    {
                        // Temp store for access and refresh tokens
                        string accessToken = null;
                        string refreshToken = null;

                        // Throw error if file does not have access token
                        if (sr.EndOfStream)
                            throw new IOException();

                        // Read access and refresh tokens from file
                        accessToken = sr.ReadLine();
                        if (!sr.EndOfStream)
                            refreshToken = sr.ReadLine();
                        TraktClient = new TraktClient(this._clientId, this._clientSecret)
                        {
                            Authorization = TraktAuthorization.CreateWith(accessToken, refreshToken)
                        };
                        AutodlLogger.Log(AutodlLogLevel.DEBUG, @"Successfully read access token and refresh token from file.");
                    }
                }
                catch (Exception e)
                {
                    AutodlLogger.Log(AutodlLogLevel.ERROR, e.Message);
                    throw;
                }
            }
            else
            {
                AutodlLogger.Log(AutodlLogLevel.DEBUG, @"Trying to generate new authorization token from user.");
                TraktClient = new TraktClient(this._clientId, this._clientSecret);
                this.GenerateAccessTokenAsync().Wait();
                AutodlLogger.Log(AutodlLogLevel.DEBUG, @"Successfully generated new authorization token from user.");
            }

            // Refresh authorization if needed
            this.RefreshAuthorizationAsync().Wait();

            // Force OAuth on all requests
            this.TraktClient.Configuration.ForceAuthorization = true;
        }

        /// <summary>
        /// Refreshes access token if expired.
        /// </summary>
        /// <returns></returns>
        private async Task RefreshAuthorizationAsync()
        {
            // Validate data
            if (null == this.TraktClient)
                throw new NullReferenceException();

            // Check if client needs a new auth token
            bool tokenExpired = await this.TraktClient.Authentication.CheckIfAccessTokenWasRevokedOrIsNotValidAsync(this.TraktClient.Authorization.AccessToken);

            // Get new access token if expired
            if (tokenExpired)
            {
                AutodlLogger.Log(AutodlLogLevel.DEBUG, @"Token expired. Refreshing new token.");
                await this.TraktClient.DeviceAuth.RefreshAuthorizationAsync();
            }
            else
            {
                AutodlLogger.Log(AutodlLogLevel.DEBUG, @"Token is valid. No need to refresh token.");
            }
        }

        /// <summary>
        /// Revokes authorization of trakt client.
        /// </summary>
        /// <returns></returns>
        public async Task RevokeAuthorizationAsync()
        {
            // Validate data
            if (null == this.TraktClient)
                throw new NullReferenceException();

            // Revoke authorization
            AutodlLogger.Log(AutodlLogLevel.DEBUG, @"Revoking authorization token for user.");
            await this.TraktClient.DeviceAuth.RevokeAuthorizationAsync();
        }

        /// <inheritdoc />
        /// <summary>
        /// Get list of movies from Trakt wishlist.
        /// </summary>
        public List<Movie> GetListOfMovies()
        {
            // Declare local vairables
            List<Movie> moviesList = new List<Movie>();
            List<Movie> moviesCollectedList;
            List<Movie> prunedMoviesList = new List<Movie>();

            // Get trakt movie watchlist
            AutodlLogger.Log(AutodlLogLevel.DEBUG, @"Trying to get watchlist of Trakt user.");
            Task<TraktPaginationListResult<TraktWatchlistItem>> getTraktMoviesTask = Task.Run(() => this.GetTraktWatchlistMoviesAsync());
            getTraktMoviesTask.Wait();
            TraktPaginationListResult<TraktWatchlistItem> traktMovieListResult = getTraktMoviesTask.Result;
            AutodlLogger.Log(AutodlLogLevel.DEBUG, @"Successfully received watchlist of Trakt user.");

            // Get movies that have already been collected
            Task<List<Movie>> getTraktCollectedMoviesListTask = Task.Run(() => this.GetTraktMoviesCollectedAsync());
            getTraktCollectedMoviesListTask.Wait();
            moviesCollectedList = getTraktCollectedMoviesListTask.Result;            

            // Add trakt movies to movieList
            foreach (TraktWatchlistItem traktWatchlistItem in traktMovieListResult)
                if ((DateTime.Now.Year - 2) <= traktWatchlistItem.Movie.Year) // TODO: Remove magic number
                    moviesList.Add(new Movie(traktWatchlistItem.Movie.Title)); // TODO: Let client choose year as parameter
            AutodlLogger.Log(AutodlLogLevel.DEBUG, @"Added trakt watchlist to movie list.");            

            // Prune away movies already collected
            foreach (Movie movie in moviesList)
            {
                if (!moviesCollectedList.Contains(movie))
                    prunedMoviesList.Add(movie);
            }

            // Return movie list
            return prunedMoviesList;
        }

        /// <summary>
        /// Gets trakt movie watchlist through Trakt API.
        /// </summary>
        /// <returns>List of TraktWatchListItems</returns>
        private async Task<TraktPaginationListResult<TraktWatchlistItem>> GetTraktWatchlistMoviesAsync()
        {
            try
            {
                AutodlLogger.Log(AutodlLogLevel.DEBUG, @"Trying to get Trakt watchlist.");
                var movieTraktList =
                    await this.TraktClient.Users.GetWatchlistAsync(@"shyamsundar2007", TraktSyncItemType.Movie); // TODO: Remove hardcoding
                AutodlLogger.Log(AutodlLogLevel.DEBUG, @"Successfully retrieved Trakt watchlist.");
                return movieTraktList;
            }
            catch (Exception e)
            {
                AutodlLogger.Log(AutodlLogLevel.ERROR, e.Message);
                throw;
            }
        }

        /// <summary>
        /// Generates access token for trakt client.
        /// </summary>
        /// <returns></returns>
        private async Task GenerateAccessTokenAsync()
        {
            // Get access token from Trakt
            TraktDevice device = await this.TraktClient.DeviceAuth.GenerateDeviceAsync();
            AutodlLogger.Log(AutodlLogLevel.INFO, $"Please go to {device.VerificationUrl} and enter code {device.UserCode} on the page.");
            TraktAuthorization authorization = await this.TraktClient.DeviceAuth.PollForAuthorizationAsync();

            // Write access token to file
            AutodlLogger.Log(AutodlLogLevel.INFO, $"Successfully received access token: {authorization.AccessToken}");
            AutodlLogger.Log(AutodlLogLevel.DEBUG, $"Trying to write access token and refresh token to file {this._accessTokenPathWithName}");
            try
            {
                using (StreamWriter sw = new StreamWriter(this._accessTokenPathWithName))
                {
                    sw.WriteLine(authorization.AccessToken);
                    sw.WriteLine(authorization.RefreshToken);
                }
                AutodlLogger.Log(AutodlLogLevel.DEBUG, $"Successfully wrote access token and refresh token to file {this._accessTokenPathWithName}");
            }
            catch (Exception e)
            {
                AutodlLogger.Log(AutodlLogLevel.ERROR, e.Message);
                throw;
            }
        }

        /// <summary>
        /// Gets list of movies already collected by user.
        /// </summary>
        /// <returns></returns>
        private async Task<List<Movie>> GetTraktMoviesCollectedAsync()
        {
            // Declare local vairables
            List<Movie> moviesList = new List<Movie>();

            try
            {
                // Retrieve collected movies from Trakt API
                AutodlLogger.Log(AutodlLogLevel.DEBUG, @"Trying to retrieve movies already collected from Trakt.");
                var collectedMovieTraktList = await this.TraktClient.Users.GetCollectionMoviesAsync(@"shyamsundar2007");    // TODO: Remove hardcoding
                AutodlLogger.Log(AutodlLogLevel.DEBUG, @"Successfully retrieved movies already collected from Trakt.");

                // Convert Trakt movie to Autodl movie
                foreach (var collectedMovie in collectedMovieTraktList)
                {
                    moviesList.Add(new Movie(collectedMovie.Movie.Title));
                }

                return moviesList;
            }
            catch (Exception e)
            {
                AutodlLogger.Log(AutodlLogLevel.ERROR, e.Message);
                throw;
            }
        }
    }
}
