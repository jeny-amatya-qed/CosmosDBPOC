﻿using System;
using System.Threading.Tasks;
using System.Configuration;
using System.Collections.Generic;
using System.Net;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Fluent;
using System.Linq;
using CosmosGettingStartedTutorial.Repositories.Interfaces;
using CosmosGettingStartedTutorial.Repositories;
using CosmosGettingStartedTutorial.Models;

namespace CosmosGettingStartedTutorial
{
  class Program
  {
    // The Azure Cosmos DB endpoint for running this sample.
    private static readonly string EndpointUri = ConfigurationManager.AppSettings["EndPointUri"];

    // The primary key for the Azure Cosmos account.
    private static readonly string PrimaryKey = ConfigurationManager.AppSettings["PrimaryKey"];

    //// The Cosmos client instance
    //private CosmosClient cosmosClient;

    //// The database we will create
    //private Database database;

    //// The container we will create.
    //private Container container;

    private IApplicationRepository ApplicationRepository { get; set; }
    private IApiRequestRepository ApiRequestRepository { get; set; }

    public static async Task Main(string[] args)
    {
      try
      {
        Console.WriteLine("Beginning operations...\n");
        Program p = new Program();

        await p.GetStartedDemoAsync();
      }
      catch (CosmosException de)
      {
        Exception baseException = de.GetBaseException();
        Console.WriteLine("{0} error occurred: {1}", de.StatusCode, de);
      }
      catch (Exception e)
      {
        Console.WriteLine("Error: {0}", e);
      }
      finally
      {
        Console.WriteLine("End of demo, press any key to exit.");
        Console.ReadKey();
      }
    }

    public async Task GetStartedDemoAsync()
    {
      Console.Write("Initialising repositories...");

      ApplicationRepository = new ApplicationRepository(EndpointUri, PrimaryKey, "Application");
      ApiRequestRepository = new ApiRequestRepository(EndpointUri, PrimaryKey, "ApiRequest");

      Console.WriteLine("Done.");
      Console.Write("Initialising dbs, containers...");
      await ApplicationRepository.Initialise();
      await ApiRequestRepository.Initialise();

      Console.WriteLine("Done.");

      WriteOptionsToConsole();

      ConsoleKeyInfo key = Console.ReadKey(true);

      while (key.KeyChar != ' ' && key.KeyChar != 'x' && key.KeyChar != 'X')
      {
        Console.WriteLine();

        switch (key.KeyChar)
        {
          case 'a':
            Console.Write("Please enter UserId: ");
            var userId = Console.ReadLine();

            var applications = await ApplicationRepository.GetAllApplicationsByUser(userId);
            WriteApplicationsToConsole(applications.ToList());
            break;

          case 'b':
            Console.Write("Please enter AppId: ");
            var appId = Console.ReadLine();

            if (!string.IsNullOrEmpty(appId))
            {
              var app = await ApplicationRepository.GetApplication(appId);
              WriteApplicationToConsole(app);
            }
            break;

          case 'c':
            Console.Write("Please enter UserId: ");
            userId = Console.ReadLine();
            Console.Write("Please enter AppId: ");
            appId = Console.ReadLine();
            Console.Write("Please enter a name for the application: ");
            var appName = Console.ReadLine();

            if (!string.IsNullOrEmpty(userId) && !string.IsNullOrEmpty(appId) && !string.IsNullOrEmpty(appName))
            {
              var app = await ApplicationRepository.CreateApplication(new Application()
              {
                Id = $"{appId}.1",
                PartitionKey = appId,
                UserId = userId,
                AppId = appId,
                AppName = appName,
                ApiKeys = new ApiKey[] { }
              });

              WriteApplicationToConsole(app);
            }
            break;

          case 'd':
            Console.Write("Please enter AppId: ");
            appId = Console.ReadLine();

            Console.Write("Please enter a name for the new key: ");
            var keyName = Console.ReadLine();

            var now = DateTime.UtcNow;

            if (!string.IsNullOrEmpty(appId) && !string.IsNullOrEmpty(keyName))
            {
              var app = await ApplicationRepository.CreateApiKey(appId, new ApiKey()
              {
                Name = keyName,
                Scopes = [],
                Value = Guid.NewGuid().ToString(),
                Created = now,
                Updated = now
              });

              WriteApplicationToConsole(app);
            }
            break;

          case 'e':
            Console.Write("Please enter AppId: ");
            appId = Console.ReadLine();

            Console.Write("Please enter the existing value for the key to be regenerated: ");
            var apiKey = Console.ReadLine();

            if (!string.IsNullOrEmpty(appId) && !string.IsNullOrEmpty(apiKey))
            {
              var app = await ApplicationRepository.RegenerateApiKey(appId, apiKey);

              WriteApplicationToConsole(app);
            }
            break;

          case 'f':
            Console.Write("Please enter AppId: ");
            appId = Console.ReadLine();
            Console.Write("Please enter ApiKey: ");
            apiKey = Console.ReadLine();
            Console.Write("Please enter a new label name for the api key: ");
            var keyLabel = Console.ReadLine();

            if (!string.IsNullOrEmpty(appId) && !string.IsNullOrEmpty(apiKey) && !string.IsNullOrEmpty(keyLabel))
            {
              var app = await ApplicationRepository.UpdateApiLabel(appId, apiKey, keyLabel);

              WriteApplicationToConsole(app);
            }
            break;

          case 'g':
            Console.Write("Please enter AppId: ");
            appId = Console.ReadLine();
            Console.Write("Please enter ApiKey: ");
            apiKey = Console.ReadLine();

            if (!string.IsNullOrEmpty(appId) && !string.IsNullOrEmpty(apiKey))
            {
              await ApplicationRepository.RetireApiKey(appId, apiKey);
            }
            break;
        }

        WriteOptionsToConsole();

        key = Console.ReadKey(true);
      }

      ExitDemo();
    }

    private void WriteOptionsToConsole()
    {
      Console.WriteLine();
      Console.WriteLine("OPTIONS:");
      Console.WriteLine();
      Console.WriteLine("a: Get Applications by userId");
      Console.WriteLine("b: Get Application by appId");
      Console.WriteLine("c: Create application");
      Console.WriteLine("d: Create Api key");
      Console.WriteLine("e: Regenerate Api key");
      Console.WriteLine("f: Update Api key label");
      Console.WriteLine("g: Delete (retire) Api key");

      Console.WriteLine("x or SPACE: Exit");
    }

    private void ExitDemo()
    {
      Console.WriteLine("Demo exiting...");

      // dispose of the Cosmos clients...
      ApplicationRepository.Dispose();
      ApiRequestRepository.Dispose();

      Environment.Exit(0);
    }
    
    private void WriteApplicationsToConsole(List<Application> apps)
    {
      foreach (var a in apps)
      {
        WriteApplicationToConsole(a);
      }
    }

    private void WriteApplicationToConsole(Application app)
    {
      Console.WriteLine();
      Console.WriteLine($"{app}");
    }
  }
}