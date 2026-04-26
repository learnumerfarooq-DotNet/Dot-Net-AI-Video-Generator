namespace AiContentFactory.Application.Studio;

public interface IStudioTool
{
    string Name { get; }
    string Description { get; }
    string ParametersSchema { get; } // JSON Schema
    Task<string> ExecuteAsync(string arguments, CancellationToken cancellationToken);
}

public sealed class StudioToolRegistry
{
    private readonly IEnumerable<IStudioTool> _tools;

    public StudioToolRegistry(IEnumerable<IStudioTool> tools)
    {
        _tools = tools;
    }

    public IEnumerable<IStudioTool> GetTools() => _tools;

    public IStudioTool? GetTool(string name) => _tools.FirstOrDefault(t => t.Name == name);
}
