using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using GoogleCloudMessaging.Ccs;
using System.Threading;

namespace UnitTests.GoogleCloudMessaging.Ccs
{
    [TestClass]
    public class CcsClientTests
    {
        //[Ignore]
        [TestMethod]
        public void TestMethod1()
        {
            string senderId = "api-project-478231157746";
            string apiKey = "AIzaSyBWZ4vOgCTehXFUOMCublOvp8l6KU3avYA";

            var client = new CcsClient(senderId, apiKey, true);
            client.BeginConnect();
            Thread.Sleep(50000000);
            
        }

        // send while receiving
        // send while sending
    }
}
