﻿// See https://aka.ms/new-console-template for more information
using Azure;
using Azure.Identity;
using Azure.ResourceManager.Storage;
using Azure.Storage;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;

using BlobPurge;

using System.Net;

var cts = new CancellationTokenSource();

Args input;
try
{
    input = PowerArgs.Args.Parse<Args>(args);

    if (input is null || input.Help)
    {
        // Help output will be printed by PowerArgs
        return;
    }
}
catch (PowerArgs.ArgException ex)
{
    WriteError(ex.Message);
    return;
}

var retryOptions = new BlobClientOptions();
retryOptions.Retry.Mode = Azure.Core.RetryMode.Exponential;
retryOptions.Retry.MaxDelay = TimeSpan.FromMinutes(1);
retryOptions.Retry.Delay = TimeSpan.FromSeconds(3);

WriteVerbose("Connecting to container...");
DateTimeOffset expiryTime = DateTimeOffset.UtcNow.AddDays(1);

var container = new BlobContainerClient(new Uri($"https://{input.AccountName}.blob.core.windows.net/{input.ContainerName}"), new DefaultAzureCredential(true), retryOptions);
Azure.AsyncPageable<BlobItem> blobs = container.GetBlobsAsync(BlobTraits.Metadata, BlobStates.None, input.Prefix, cts.Token);

if (!input.Confirm)
{
    Prompt(@"About to start deleting blobs, THIS OPERATION CANNOT BE UNDONE.");
}

var checkDays = input.AgeDays.HasValue;
var maxAgeDays = input.AgeDays.GetValueOrDefault();

Write("Getting blobs to purge...");

await foreach (Azure.Page<BlobItem> p in blobs.AsPages(pageSizeHint: input.ChunkSize))
{
    try
    {
        if (!cts.IsCancellationRequested)
        {
            var deleteTasks = new List<Task>(p.Values.Count);
            Write($"Deleting {p.Values.Count} blob(s)...");
            foreach (BlobItem? b in p.Values)
            {
                if (!cts.IsCancellationRequested && !b.Deleted)
                {
                    DateTimeOffset dateToCheck = DateTimeOffset.UtcNow;
                    if (checkDays)
                    {
                        dateToCheck = b.Properties.LastModified ?? b.Properties.CreatedOn.GetValueOrDefault(DateTimeOffset.UtcNow);
                    }

                    if (!checkDays || (DateTimeOffset.UtcNow - dateToCheck).TotalDays > maxAgeDays)
                    {
                        WriteVerboseFunc(() =>
                        {
                            var msg = $"Signaling delete for {b.Name}";
                            if (checkDays)
                            {
                                msg += $@" ({b.Properties.CreatedOn})";
                            }

                            return $@"{msg} ...";
                        });

                        if (!input.WhatIf)
                        {
                            deleteTasks.Add(container.DeleteBlobAsync(b.Name, DeleteSnapshotsOption.IncludeSnapshots, cancellationToken: cts.Token));
                        }
                    }
                    else
                    {
                        WriteVerboseFunc(() =>
                        {
                            var msg = $"SKIPPED {b.Name}";
                            if (checkDays)
                            {
                                msg += $@" ({b.Properties.CreatedOn})";
                            }

                            return $@"{msg} ...";
                        });
                    }
                }
            }

            WriteVerbose($"Waiting for {deleteTasks.Count} deletion(s) to complete...");
            await Task.WhenAll(deleteTasks);
        }
    }
    catch (RequestFailedException e) when (e.Status == (int)HttpStatusCode.NotFound) { }
}

void Write(string message)
{
    if (input.WhatIf)
    {
        message = $"[WHATIF] {message}";
    }

    Console.WriteLine(message);
}

void WriteVerboseFunc(Func<string> messageFactory)
{
    if (input.Verbose)
    {
        WriteVerbose(messageFactory());
    }
}

void WriteVerbose(string message)
{
    if (input.Verbose)
    {
        Console.ForegroundColor = ConsoleColor.Cyan;
        Write(message);
        Console.ResetColor();
    }
}

void WriteError(string message)
{
    Console.ForegroundColor = ConsoleColor.Red;
    Console.Error.WriteLine(message);
    Console.ResetColor();
}

string? Prompt(string message)
{
    Console.ForegroundColor = ConsoleColor.Green;
    Write(message);
    Write("\tPress Enter to continue...");
    Console.ResetColor();

    return Console.ReadLine();
}