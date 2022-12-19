using PowerArgs;

using Purge.Common;

namespace BlobPurge
{
    [TabCompletion]
    public class Args
    {
        [HelpHook, ArgShortcut("-h"), ArgShortcut("--help"), ArgShortcut("-?"), ArgDescription("Shows Help")]
        public bool Help { get; set; }

        [ArgShortcut("-a"), ArgShortcut("--account"), ArgDescription("The name of the storage account from which to purge blobs"), ArgRequired(PromptIfMissing = true), ArgPosition(1)]
        public string? AccountName { get; set; }

        [ArgShortcut("-c"), ArgShortcut("--container"), ArgDescription("The name of the container within the storage account from which to purge blobs"), ArgRequired(PromptIfMissing = true), ArgPosition(2)]
        public string? ContainerName { get; set; }

        [ArgShortcut("-p"), ArgShortcut("--prefix"), ArgDescription("The prefix match to use for purging blobs"), ArgRequired(IfNot = "AgeDays")]
        public string? Prefix { get; set; }

        [ArgShortcut("-d"), ArgShortcut("--days"), ArgDescription("The max age of a blob before it is purged"), ArgRequired(IfNot = "Prefix"), GreaterThanZeroValidator]
        public int? AgeDays { get; set; }

        [ArgShortcut("-s"), ArgDescription("The Subscription ID in which the Storage Account lives, otherwise will use your default subscription (see az account list)")]
        public string? SubscriptionId { get; set; }

        [ArgShortcut("--chunksize"), ArgDescription("The number of blobs to process in parallel"), ArgDefaultValue(10), GreaterThanZeroValidator]
        public int ChunkSize { get; set; }

        [ArgShortcut("-y"), ArgShortcut("--confirm"), ArgDescription("Don't prompt for confirmation before starting deletion")]
        public bool Confirm { get; set; } = false;

        [ArgShortcut("--whatif"), ArgDescription("Doesn't actually purge but rather lists the blobs that *would be* purged if the tool were ran without --whatif")]
        public bool WhatIf { get; set; } = false;

        [ArgShortcut("-v"), ArgShortcut("--verbose"), ArgDescription("Outputs more detailed messages as the tool executes")]
        public bool Verbose { get; set; } = false;

    }
}
