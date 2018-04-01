using System;
using System.Collections;
using System.Collections.Generic;

namespace AutodlConfigurator
{
    /// <summary>
    /// Represents a movie class.
    /// </summary>
    public class Movie : IEquatable<Movie>
    {
        // TODO: Add more properties like year
        /// <summary>
        /// Name of the movie.
        /// </summary>
        public string Name;

        /// <summary>
        /// Constructor for movie.
        /// </summary>
        public Movie(string name)
        {
            this.Name = name;
        }

        public bool Equals(Movie other)
        {
            return this.Name.Equals(other.Name);
        }
    }
}