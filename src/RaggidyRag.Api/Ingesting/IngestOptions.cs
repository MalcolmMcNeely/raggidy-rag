namespace RaggidyRag.Api.Ingesting;

public sealed class IngestOptions
{
    // Relative to the content root, the project folder under dotnet run, so the default reaches the Edict repo beside this one.
    public string DocumentsFolder { get; set; } = "../../../Edict/docs";
}
