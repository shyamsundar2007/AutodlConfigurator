using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Bson;
using TraktApiSharp;
using TraktApiSharp.Authentication;
using TraktApiSharp.Enums;
using TraktApiSharp.Objects.Basic;
using TraktApiSharp.Objects.Get.Users.Lists;
using TraktApiSharp.Objects.Get.Watchlist;
using TraktApiSharp.Requests.Params;

namespace AutodlConfigurator
{
    /// <summary>
    /// Class providing interface to Trakt API.
    /// </summary>
    public class TraktMovieDBConnector : IMovieDbConnector
    {
        /// <summary>
        /// Trakt client.
        /// </summary>
        public TraktClient traktClient;

        /// <summary>
        /// Trakt client ID.
        /// </summary>
        private string clientID;

        /// <summary>
        /// Trakt client secret.
        /// </summary>
        private string clientSecret;

        /// <summary>
        /// Full path that the access token is saved in.
        /// </summary>
        private string accessTokenPathWithName =
            @"C:\Users\ShyamV\Documents\Visual Studio 2017\Projects\AutodlConfigurator\AutodlConfigurator\accesstoken.txt";

        /// <summary>
        /// Constructor for TraktMovieDBConnector.
        /// </summary>
        /// <param name="clientID">Trakt API Client ID.</param>
        /// <param name="clientSecret">Trakt API Client Secret.</param>
        public TraktMovieDBConnector(string clientID, string clientSecret)
        {
            // Assign internal data
            this.clientID = clientID;
            this.clientSecret = clientSecret;            

            // Load access token from file if generated
            // Else generate access token from user
            if (File.Exists(this.accessTokenPathWithName))
            {
                try
                {
                    using (StreamReader sr = new StreamReader(this.accessTokenPathWithName))
                    {
                        traktClient = new TraktClient(this.clientID, this.clientSecret)
                        {
                            Authorization = TraktAuthorization.CreateWith(sr.ReadLine())
                            // TODO: Need ot check if token needs to be refreshed
                        };
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    throw;
                }
            }
            else
            {
                traktClient = new TraktClient(this.clientID, this.clientSecret);
                this.generateAccessTokenAsync().Wait();
            }

            // Force OAuth on all requests
            this.traktClient.Configuration.ForceAuthorization = true;
        }

        /// <summary>
        /// Get list of movies from Trakt wishlist.
        /// </summary>
        public List<Movie> GetListOfMovies()
        {
            // Declare local vairables
            List<Movie> moviesList = new List<Movie>();

            // Get trakt movie watchlist
            Task<TraktPaginationListResult<TraktWatchlistItem>> getTraktMoviesTask = Task.Run(() => this.getTraktWatchlistMoviesAsync());
            getTraktMoviesTask.Wait();
            TraktPaginationListResult<TraktWatchlistItem> traktMovieListResult = getTraktMoviesTask.Result;

            // Add trakt movies to movieList
            foreach (TraktWatchlistItem traktWatchlistItem in traktMovieListResult)
                if ((DateTime.Now.Year - 2) <= traktWatchlistItem.Movie.Year) // TODO: Remove magic number
                    moviesList.Add(new Movie(traktWatchlistItem.Movie.Title)); // TODO: Let client choose year as parameter

            Console.WriteLine(moviesList);

            return moviesList;
        }

        /// <summary>
        /// Gets trakt movie watchlist through Trakt API.
        /// </summary>
        /// <returns>List of TraktWatchListItems</returns>
        private async Task<TraktPaginationListResult<TraktWatchlistItem>> getTraktWatchlistMoviesAsync()
        {
            try
            {
                var movieTraktList =
                    await this.traktClient.Users.GetWatchlistAsync(@"shyamsundar2007", TraktSyncItemType.Movie); // TODO: Remove hardcoding
                Console.WriteLine(movieTraktList);
                return movieTraktList;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        /// <summary>
        /// Generates access token for trakt client.
        /// </summary>
        /// <returns></returns>
        private async Task generateAccessTokenAsync()
        {
            // Get access token from Trakt
            TraktDevice device = await this.traktClient.DeviceAuth.GenerateDeviceAsync();
            Console.WriteLine("Please go to {0} and enter code {1} on the page.", device.VerificationUrl, device.UserCode);
            TraktAuthorization authorization = await this.traktClient.DeviceAuth.PollForAuthorizationAsync();

            // Write access token to file
            Console.WriteLine("Successfully received access token: {0}", authorization.AccessToken);
            try
            {
                using (StreamWriter sw = new StreamWriter(this.accessTokenPathWithName))
                {
                    sw.WriteLine(authorization.AccessToken);
                    // TODO: Save refresh token as well
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }
    }
}
