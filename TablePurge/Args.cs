using PowerArgs;

using Purge.Common;

[TabCompletion]
public class Args
{
    [HelpHook, ArgShortcut("-h"), ArgShortcut("--help"), ArgShortcut("-?"), ArgDescription("Shows Help")]
    public bool Help { get; set; }

    [ArgShortcut("-a"), ArgShortcut("--account"), ArgDescription("The name of the storage account from which to purge table entities"), ArgRequired(PromptIfMissing = true), ArgPosition(0)]
    public string? AccountName { get; set; }

    [ArgShortcut("-t"), ArgShortcut("--table"), ArgDescription("The name of the table within the storage account from which to purge entities"), ArgRequired(PromptIfMissing = true), ArgPosition(1)]
    public string? TableName { get; set; }

    [ArgShortcut("-d"), ArgShortcut("--days"), ArgDescription("The max age of an entity before it is purged"), ArgRequired(PromptIfMissing = true), ArgPosition(2), GreaterThanZeroValidator]
    public int AgeDays { get; set; }

    [ArgShortcut("-c"), ArgShortcut("--columnName"), ArgDescription("The name of the column containing the timestamp of the entity used for comparison to AgeDays"), ArgRequired, ArgDefaultValue("Timestamp")]
    public string DateTimeColumnName { get; set; } = "Timestamp";

    [ArgShortcut("-s"), ArgDescription("The Subscription ID in which the Storage Account lives, otherwise will use your default subscription (see az account list)")]
    public string? SubscriptionId { get; set; }

    [ArgShortcut("--chunksize"), ArgDescription("The number of blobs to process in parallel"), ArgDefaultValue(10), GreaterThanZeroValidator]
    public int ChunkSize { get; set; }

    [ArgShortcut("-y"), ArgShortcut("--confirm"), ArgDescription("Don't prompt for confirmation before starting deletion")]
    public bool Confirm { get; set; } = false;

    [ArgShortcut("--whatif"), ArgDescription("Doesn't actually purge but rather lists the entities that *would be* deleted if the tool were ran without --whatif")]
    public bool WhatIf { get; set; } = false;

    [ArgShortcut("-v"), ArgShortcut("--verbose"), ArgDescription("Outputs more detailed messages as the tool executes")]
    public bool Verbose { get; set; } = false;

}
