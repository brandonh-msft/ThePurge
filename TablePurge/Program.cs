// See https://aka.ms/new-console-template for more information
using Azure;
using Azure.Data.Tables;
using Azure.Identity;
using Azure.ResourceManager.Storage;

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

var retryOptions = new TableClientOptions();
retryOptions.Retry.Mode = Azure.Core.RetryMode.Exponential;
retryOptions.Retry.MaxDelay = TimeSpan.FromMinutes(1);
retryOptions.Retry.Delay = TimeSpan.FromSeconds(3);

WriteVerbose("Connecting to table...");
DateTimeOffset expiryTime = DateTimeOffset.UtcNow.AddDays(1);

var table = new TableClient(new Uri($"https://{input.AccountName}.table.core.windows.net"), input.TableName, new DefaultAzureCredential(true), retryOptions);
var entityPages = table.QueryAsync<TableEntity>(cancellationToken: cts.Token);

if (!input.Confirm)
{
    Prompt(@"About to start deleting rows, THIS OPERATION CANNOT BE UNDONE.");
}

Write("Getting entities to delete...");

await foreach (Page<TableEntity> p in entityPages.AsPages(pageSizeHint: input.ChunkSize))
{
    try
    {
        if (!cts.IsCancellationRequested)
        {
            var deleteTasks = new List<Task>(p.Values.Count);
            Write($"Checking {p.Values.Count} entity(ies)...");
            foreach (TableEntity? e in p.Values)
            {
                if (!cts.IsCancellationRequested)
                {
                    DateTimeOffset dateToCheck;
                    DateTimeOffset? possiblyNullDateFromTable = null;
                    try
                    {
                        possiblyNullDateFromTable = e.GetDateTimeOffset(input.DateTimeColumnName);
                    }
                    catch
                    {
                    }

                    if (possiblyNullDateFromTable is not null)
                    {
                        dateToCheck = possiblyNullDateFromTable.Value;
                    }
                    else
                    {
                        var dateTimeStringValue = e.GetString(input.DateTimeColumnName);
                        if (string.IsNullOrWhiteSpace(dateTimeStringValue))
                        {
                            WriteError($@"{input.DateTimeColumnName} for {e.PartitionKey} | {e.RowKey} had no value");
                            continue;
                        }
                        else if (!DateTimeOffset.TryParse(dateTimeStringValue, out dateToCheck))
                        {
                            WriteError($@"Unable to parse DateTime value in column {input.DateTimeColumnName} for {e.PartitionKey} | {e.RowKey} into a DateTime object. Value is '{dateTimeStringValue}'");
                            continue;
                        }
                    }

                    if ((DateTimeOffset.UtcNow - dateToCheck).TotalDays > input.AgeDays)
                    {
                        WriteVerbose($"Signaling delete for {e.PartitionKey} | {e.RowKey} ({dateToCheck})...");

                        if (!input.WhatIf)
                        {
                            deleteTasks.Add(table.DeleteEntityAsync(e.PartitionKey, e.RowKey, cancellationToken: cts.Token));
                        }
                    }
                    else
                    {
                        WriteVerbose($"SKIPPED {e.PartitionKey} | {e.RowKey} ({dateToCheck})...");
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