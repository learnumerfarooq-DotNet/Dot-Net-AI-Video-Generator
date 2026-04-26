using System.Text.Json;
using AiContentFactory.Application.Studio;
using Microsoft.Extensions.DependencyInjection;

namespace AiContentFactory.Infrastructure.Tools;

public sealed class ScheduleTaskTool(IServiceProvider serviceProvider) : IStudioTool
{
    public string Name => "schedule_task";
    public string Description => "Queue a future task or job in the Quartz scheduler for a specific agent.";

    public string ParametersSchema => @"
    {
        ""type"": ""object"",
        ""properties"": {
            ""taskName"": { ""type"": ""string"", ""description"": ""Descriptive name of the task to run."" },
            ""agentKey"": { ""type"": ""string"", ""description"": ""The key of the agent that should run this task (e.g. trend-agent, script-agent)."" },
            ""triggerTime"": { ""type"": ""string"", ""description"": ""When to run (e.g. 'Today 7:00 PM', 'In 2 hours', '0 8 * * *')."" },
            ""notes"": { ""type"": ""string"", ""description"": ""Additional context or instructions for the agent when it runs."" }
        },
        ""required"": [""taskName"", ""agentKey"", ""triggerTime""]
    }";

    public async Task<string> ExecuteAsync(string arguments, CancellationToken cancellationToken)
    {
        try
        {
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var args = JsonSerializer.Deserialize<ScheduleTaskArgs>(arguments, options);
            if (args == null) return "Error: Could not parse arguments.";

            var request = new CreateManualScheduleRequest(
                args.TaskName, 
                args.AgentKey, 
                args.TriggerTime, 
                args.Notes ?? string.Empty, 
                true);

            using var scope = serviceProvider.CreateScope();
            var store = scope.ServiceProvider.GetRequiredService<IStudioWorkspaceStore>();
            var result = await store.CreateManualScheduleAsync(request, cancellationToken);

            return $"Successfully scheduled task '{result.Name}' for agent '{result.AgentKey}' at '{result.Trigger}'. Job ID: {result.Id}";
        }
        catch (Exception ex)
        {
            return $"Error scheduling task: {ex.Message}";
        }
    }

    private sealed record ScheduleTaskArgs(string TaskName, string AgentKey, string TriggerTime, string? Notes);
}
