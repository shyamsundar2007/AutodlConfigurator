using System;
using System.Collections.Generic;
using AutodlConfigurator;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AutodlConfiguratorTests
{
    [TestClass]
    public class AutodlEditorTests
    {
        // Initialize global variables
        string autodlFilePath =
            "C:\\Users\\ShyamV\\Documents\\Visual Studio 2017\\Projects\\AutodlConfigurator\\AutodlConfiguratorTests\\TestFiles";

        [TestMethod]
        public void ReadAutodlFileTestMethod1()
        {
            // Initialize variables
            AutodlEditor autodlEditor = new AutodlEditor(this.autodlFilePath, "readonlyAutodlTest1.cfg");
            List<Movie> expectedMovieList = new List<Movie>()
            {
                new Movie("The Untamed"),
                new Movie("Nil Battey Sannata"),
                new Movie("Aligarh"),
                new Movie("They Call Me Jeeg Robot"),
                new Movie("The Survivalist"),
                new Movie("Life, Animated"),
                new Movie("Bokeh"),
                new Movie("The Last Animals"),
                new Movie("Ready Player One"),
                new Movie("Rukh")
            };

            // Read movies from autodl file
            List<Movie> movieList = autodlEditor.GetMoviesList();

            // Verify result
            Assert.IsNotNull(movieList);
            Assert.AreEqual(10, movieList.Count);
            foreach (Movie movie in movieList)
            {
                Assert.IsTrue(expectedMovieList.Contains(movie));
            }
        }

        [TestMethod]
        public void WriteAutodlFileTestMethod1()
        {
            // Initialize variables
            AutodlEditor autodlEditor = new AutodlEditor(this.autodlFilePath, "writeAutodlTest1.cfg");
            List<Movie> moviesToAdd = new List<Movie>()
            {
                new Movie("Interstellar"),
                new Movie("Rukh"),
                new Movie("Arrival"),
                new Movie("Stranger Things")
            };

            // Read movies from the file
            List<Movie> readMovieList = autodlEditor.GetMoviesList();

            try
            {
                // Write movies to file
                autodlEditor.WriteMoviesList(moviesToAdd);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }            
        }
    }
}
