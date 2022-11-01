# Azure Storage Blob Purge tool

Sometimes our best ideas come back to bite us. In some cases that's our wallet.
Have you been collecting data from a service for a long time, but maybe forgot to turn off the collection, then you get a huge bill for your storage account? This tool is here to help.

## Getting started
1. **Required**: [.NET 6.0 **SDK**](https://dotnet.microsoft.com/en-us/download/dotnet/6.0)
1. Clone the repo
1. Explore the tool with `dotnet blobpurge -h`

```
Usage - BlobPurge <AccountName> <ContainerName> -options

GlobalOption                       Description
Help (-h, -?, --help)              Shows Help
AccountName* (-a, --account)       The name of the storage account from which to purge blobs
ContainerName* (-c, --container)   The name of the container within the storage account from which to purge blobs
Prefix (-p, --prefix)              The prefix match to use for purging blobs
AgeDays (-d, --days)               The max age of a blob before it is purged
SubscriptionId (-s)                The Subscription ID in which the Storage Account lives, otherwise will use your default subscription (see az account list)
ChunkSize (--chunksize)            The number of blobs to process in parallel [Default='10']
Confirm (-y, --confirm)            Don't prompt for confirmation before starting deletion
WhatIf (--whatif)                  Doesn't actually purge but rather lists the blobs that *would be* purged if the tool were ran without --whatif
Verbose (-v, --verbose)            Outputs more detailed messages as the tool executes
```

I suggest using `--whatif` when you're testing your Prefix (-p) or Age (-d) filters as this will ensure no deletes _actually_ get executed.

This tool will use your Azure CLI, Visual Studio, or other interactive credentials if they're found on your system, else (I think?) it'll prompt you for creds and use those.
If the account you're after isn't in your default subscription, you can specify its subscription ID with `-s`

## ** WARNING **
Deleting blobs is an irreversible operation, _especially_ if you don't have Soft Delete turned on in your storage account/container. USE ACCORDINGLY

Good luck & enjoy!