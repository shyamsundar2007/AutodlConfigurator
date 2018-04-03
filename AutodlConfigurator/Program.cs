using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NLog;

namespace AutodlConfigurator
{
    class Program
    {               
        static void Main(string[] args)
        {
            // Declare variables
            string clientID = "df6ae44105ec8f7f7c3c9a1f52fab29c3a6bc1f7538e4c0b53111a316b249b65";
            string clientSecret = "f86301337ab7bd6c95b4d30bfb9b885b9baf6b9350f1550982209978053c498d";
            string autodlFilePath =
                @"C:\Users\ShyamV\Documents\Visual Studio 2017\Projects\AutodlConfigurator\AutodlConfiguratorTests\OutputFiles";
            string autodlFileName = @"autodl.cfg";
            string uploadWatchDir = @"/Volumes/Plex_4TB/Downloads/Torrents/movies_watch/";

            // Create traktMovieDbConnector instance
            AutodlLogger.Log(AutodlLogLevel.INFO, @"Gettng the Trakt client started up...");
            IMovieDbConnector trakMovieDbConnector = new TraktMovieDbConnector(clientID, clientSecret);

            // Get list of movies from Trackt
            AutodlLogger.Log(AutodlLogLevel.INFO, @"Getting the list of movies from the Trakt watchlist...");
            List<Movie> moviesFromTraktList = trakMovieDbConnector.GetListOfMovies();

            // Write movies to autodl
            AutodlLogger.Log(AutodlLogLevel.INFO, $"Updating the autodl config file...");
            AutodlEditor autodlEditor = new AutodlEditor(autodlFilePath, autodlFileName, uploadWatchDir);
            autodlEditor.GetMoviesList(); // TODO: This function call needs to be made better; have to manually call
            autodlEditor.WriteMoviesList(moviesFromTraktList);
            AutodlLogger.Log(AutodlLogLevel.INFO, @"Successfully updated autodl config file!");

            Console.ReadLine();
        }
    }
}
