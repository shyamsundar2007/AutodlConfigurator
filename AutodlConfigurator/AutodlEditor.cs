using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
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
        /// Entire autodl file name with path.
        /// </summary>
        private string AutodlFileNameWithPath => this.autodlFilePath + @"\" + this.autodlFileName;

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

            // Check if file exists on path, if not create one
            if (!File.Exists(AutodlFileNameWithPath))
            {
                AutodlLogger.Log(AutodlLogLevel.DEBUG, $"File does not exist. Creating file {AutodlFileNameWithPath}.");
                using (FileStream fs = File.Create(AutodlFileNameWithPath)) {}
            }
        }

        /// <summary>
        /// Fetch list of movies from autodl file.
        /// </summary>
        /// <returns>List of movies.</returns>
        public List<Movie> GetMoviesList()
        {
            // Initialize variables
            this.movieList = new List<Movie>(); // clear movieList since we are fetching again

            try
            {
                // Open autodl file
                AutodlLogger.Log(AutodlLogLevel.DEBUG, $"Trying to open file {AutodlFileNameWithPath}");
                using (StreamReader sr = new StreamReader(AutodlFileNameWithPath))
                {                  
                    // Read line by line
                    string line = sr.ReadLine();
                    while (line != null)
                    {
                        // Check for movie match and add to list
                        Match movieMatchResult = Regex.Match(line, @"^\[filter (.*?)\]$", RegexOptions.IgnoreCase);
                        if (movieMatchResult.Success)
                        {
                            Movie newMovie = new Movie(movieMatchResult.Groups[1].Value);
                            this.movieList.Add(newMovie);
                        }                        

                        // Read next line
                        line = sr.ReadLine();
                    }
                }
                AutodlLogger.Log(AutodlLogLevel.DEBUG, @"Successfully read movies from autodl file.");
            }
            catch (Exception e)
            {
                AutodlLogger.Log(AutodlLogLevel.ERROR, e.Message);
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

            // Iterate each movie in movieList
            foreach (Movie movie in moviesToAddList)
            {
                // Add movie to file if it doesn't already exist on file
                if (!this.movieList.Contains(movie))
                    moviesToBeWrittenList.Add(movie);
            }

            // Return early if no movies to add
            if (!moviesToBeWrittenList.Any())
            {
                AutodlLogger.Log(AutodlLogLevel.INFO, @"No new movies to be written to autodl file.");
                return;
            }

            // Log movies
            AutodlLogger.Log(AutodlLogLevel.INFO, @"Here are the movies to be written to the autodl file: ");
            moviesToBeWrittenList.ForEach(p => AutodlLogger.Log(AutodlLogLevel.INFO, p.Name));

            // Write movies to file
            try
            {                
                AutodlLogger.Log(AutodlLogLevel.DEBUG, $"Trying to write movies to {AutodlFileNameWithPath}");
                using (StreamWriter sw = new StreamWriter(AutodlFileNameWithPath, true))
                {
                    foreach (Movie movie in moviesToBeWrittenList)
                    {
                        sw.WriteLine(Environment.NewLine + "[filter " + movie.Name + "]");
                        sw.WriteLine("shows = " + movie.Name);
                        AddCommonParametersToAutodlConfig(sw);
                    }                    
                }
                AutodlLogger.Log(AutodlLogLevel.DEBUG, @"Successfully wrote movies autodl file.");
            }
            catch (Exception e)
            {
                AutodlLogger.Log(AutodlLogLevel.ERROR, e.Message);
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