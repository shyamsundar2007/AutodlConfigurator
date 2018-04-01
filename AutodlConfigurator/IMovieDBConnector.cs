using System.Collections.Generic;

namespace AutodlConfigurator
{
    /// <summary>
    /// Interface for a movie DB connector
    /// </summary>
    interface IMovieDbConnector
    {
        /// <summary>
        /// Gets list of movies from the movie DB
        /// </summary>
        List<Movie> GetListOfMovies();
    }
} 
