// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
//
// Generated with Bot Builder V4 SDK Template for Visual Studio EchoBot v4.6.2

using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;

namespace mediaStreamingChatbot
{
    public class Program
    {
        public static void Main(string[] args)
        {
            //Here I am adding some data for my classes since i dont have a DB as of yet 
            Creator Creator = new Creator();

            Creator.First = "Jenny";
            Creator.Last = "Henderson";
            Creator.ChannelName = "lostAdventures";
            Creator.NumOfSubs = 200000;
            Creator.UserID = 125487;

            CreateWebHostBuilder(args).Build().Run();
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                        .ConfigureLogging((logging) =>
                            {
                                 logging.AddDebug();
                                 logging.AddConsole();
                            })
                .UseStartup<Startup>();
    }
}
