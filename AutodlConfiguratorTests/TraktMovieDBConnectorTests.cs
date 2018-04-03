using System;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using AutodlConfigurator;

namespace AutodlConfiguratorTests
{
    [TestClass]
    public class TraktMovieDBConnectorTests
    {
        // Declare variables
        string clientID = "df6ae44105ec8f7f7c3c9a1f52fab29c3a6bc1f7538e4c0b53111a316b249b65";
        string clientSecret = "f86301337ab7bd6c95b4d30bfb9b885b9baf6b9350f1550982209978053c498d";
        string autodlFileWithName =
            @"C:\Users\ShyamV\Documents\Visual Studio 2017\Projects\AutodlConfigurator\AutodlConfigurator\accesstoken.txt";

        [TestMethod]
        public void TestInitialAuthorization()
        {            
            // Delete autodl file if exists
            if (File.Exists(autodlFileWithName))
                File.Delete(autodlFileWithName);

            // Create trakt client with initital authorization
            TraktMovieDBConnector traktMovieDBConnector = new TraktMovieDBConnector(clientID, clientSecret);            
            
            // Validate test
            Assert.IsTrue(traktMovieDBConnector.traktClient.IsValidForUseWithAuthorization);
            Assert.IsTrue(File.Exists(autodlFileWithName));
        }

        [TestMethod]
        public void TestSubsequentAuthorization()
        {
            // If autodl file doesn't exist, skip test
            if (!File.Exists(autodlFileWithName))
                return;

            // Create trakt client
            TraktMovieDBConnector traktMovieDbConnector = new TraktMovieDBConnector(clientID, clientSecret);

            // Validate test
            Assert.IsTrue(traktMovieDbConnector.traktClient.IsValidForUseWithAuthorization);
        }

        [TestMethod]
        public void TestGetListOfMovies()
        {
            // Create trakt client
            TraktMovieDBConnector traktMovieDBConnector = new TraktMovieDBConnector(clientID, clientSecret);

            // Validate test
            Assert.IsNotNull(traktMovieDBConnector.GetListOfMovies());
        }

        [TestMethod]
        public void TestRefreshAccessToken()
        {
            // If autodl file doesn't exist, skip test
            if (!File.Exists(autodlFileWithName))
                return;

            // Create trakt client and revoke access immediately
            TraktMovieDBConnector traktMovieDbConnector = new TraktMovieDBConnector(clientID, clientSecret);
            string oldAccessToken = traktMovieDbConnector.traktClient.Authorization.AccessToken;
            traktMovieDbConnector.RevokeAuthorizationAsync().Wait();

            // Create new instance of trakt DB connector
            traktMovieDbConnector = new TraktMovieDBConnector(clientID, clientSecret);
            string newAccessToken = traktMovieDbConnector.traktClient.Authorization.AccessToken;

            // Validate test
            Assert.AreNotEqual(oldAccessToken, newAccessToken);
        }
    }
}
