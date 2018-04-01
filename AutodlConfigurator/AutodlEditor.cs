using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;

namespace AutodlConfigurator
{
    /// <summary>
    /// Provides functions to edit autodl files
    /// </summary>
    public class AutodlEditor
    {
        /// <summary>
        /// Complete file path of the autodl file.
        /// </summary>
        private string autodlFilePath;

        /// <summary>
        /// Name of the autodl file.
        /// </summary>
        private string autodlFileName;

        /// <summary>
        /// List of movies
        /// </summary>
        private List<Movie> movieList;

        /// <summary>
        /// Default match category.
        /// </summary>
        private string defaultMatchCategories = "MovieHD";

        /// <summary>
        /// Default match sites.
        /// </summary>
        private string defaultMatchSites = "ar";

        /// <summary>
        /// Default minimum size.
        /// </summary>
        private string defaultMinSize = "1GB";

        /// <summary>
        /// Default maximum size.
        /// </summary>
        private string defaultMaxSize = "10GB";

        /// <summary>
        /// Default upload type.
        /// </summary>
        private string defaultUploadType = "watchdir";

        /// <summary>
        /// Default resolutions.
        /// </summary>
        private string defaultResolutions = "720p, 1080p";

        /// <summary>
        /// Default upload diretory path.
        /// </summary>
        private string defaultUploadWatchDir = "/";
        // TODO: Make the above into an object

        /// <summary>
        /// Construtor for autodl editor
        /// </summary>
        public AutodlEditor(string filePath, string fileName, string uploadWatchDir = null)
        {
            // Assign internal variables
            this.autodlFilePath = filePath;
            this.autodlFileName = fileName;
            if (null != uploadWatchDir)
                this.defaultUploadWatchDir = uploadWatchDir;
            this.movieList = new List<Movie>();
        }

        /// <summary>
        /// Fetch list of movies from autodl file.
        /// </summary>
        /// <returns>List of movies.</returns>
        public List<Movie> GetMoviesList()
        {
            // Initialize variables
            string autodlFileNameWithPath = this.autodlFilePath + "\\" + this.autodlFileName;
            this.movieList = new List<Movie>(); // clear movieList since we are fetching again

            try
            {
                // Open autodl file
                using (StreamReader sr = new StreamReader(autodlFileNameWithPath))
                {                  
                    // Read line by line
                    string line = sr.ReadLine();
                    while (line != null)
                    {
                        // Check for movie match and add to list
                        Match movieMatchResult = Regex.Match(line, @"^\[filter (.*?)\]$", RegexOptions.IgnoreCase);
                        if (movieMatchResult.Success)
                        {
                            Console.WriteLine("Match for {0}", movieMatchResult.Groups[1].Value);
                            Movie newMovie = new Movie(movieMatchResult.Groups[1].Value);
                            this.movieList.Add(newMovie);
                        }                        

                        // Read next line
                        line = sr.ReadLine();
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }

            // Return list of movies
            return this.movieList;
        }

        /// <summary>
        /// Writes movie list to autodl file.
        /// </summary>
        /// <param name="movieList">List of movies to write.</param>
        public void WriteMoviesList(List<Movie> moviesToAddList)
        {
            // Declare variables
            List<Movie> moviesToBeWrittenList = new List<Movie>();
            string autodlFileNameWithPath = this.autodlFilePath + "\\" + this.autodlFileName;

            // Iterate each movie in movieList
            foreach (Movie movie in moviesToAddList)
            {
                // Add movie to file if it doesn't already exist on file
                if (!this.movieList.Contains(movie))
                    moviesToBeWrittenList.Add(movie);
            }

            // Write movies to file
            try
            {
                using (StreamWriter sw = new StreamWriter(autodlFileNameWithPath, true))
                {
                    foreach (Movie movie in moviesToBeWrittenList)
                    {
                        sw.WriteLine("\n[filter " + movie.Name + "]");
                        sw.WriteLine("shows = " + movie.Name);
                        AddCommonParametersToAutodlConfig(sw);
                    }                    
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }            
        }

        /// <summary>
        /// Adds common parameters to the autodl config file
        /// </summary>
        /// <param name="sw">Stream to be written to</param>
        private void AddCommonParametersToAutodlConfig(StreamWriter sw)
        {
            // TODO: To make these parameters generic
            sw.WriteLine("match-categories = " + this.defaultMatchCategories);
            sw.WriteLine("match-sites = " + this.defaultMatchSites);
            sw.WriteLine("min-size = " + this.defaultMinSize);
            sw.WriteLine("max-size = " + this.defaultMaxSize);
            sw.WriteLine("resolutions = " + this.defaultResolutions);
            sw.WriteLine("upload-type = " + this.defaultUploadType);
            sw.WriteLine("upload-watch-dir = " + this.defaultUploadWatchDir);
        }
    }

}