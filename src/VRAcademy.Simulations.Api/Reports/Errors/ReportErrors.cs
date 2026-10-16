using Kodster.Results.Errors;

namespace VRAcademy.Simulations.Api.Reports.Errors;

public static class ReportErrors
{
    public static Error ReportNotFound(Guid simulationId) =>
        Error.NotFound("Reports.NotFound", $"Report for simulation with id {simulationId} not found.");
    
    public static Error SimulationIsFinished => 
        Error.Validation("Reports.SimulationIsFinished", "Simulation is finished.");
    
    public static Error SimulationCannotBeCompleted => 
        Error.Validation("Reports.CannotCompleteSimulation", "Simulation cannot be completed.");
    
    public static Error StepAlreadyCompleted(int stepId) => 
        Error.Validation("Reports.StepAlreadyCompleted", $"Step with id {stepId} already completed.");
    
    public static Error MaxStepsReached => 
        Error.Validation("Reports.MaxStepsReached", "Max steps reached.");
}
