namespace VRAcademy.Simulations.Api.Reports.Models;

public sealed class Report
{
    public const int MaxSteps = 20;
    
    public Report(
        Guid simulationId, 
        SimulationScenario simulationScenario, 
        SimulationKind simulationKind, 
        bool? isSuccess, 
        float? finishedInSeconds, 
        List<Step> steps)
    {
        SimulationId = simulationId;
        SimulationScenario = simulationScenario;
        SimulationKind = simulationKind;
        IsSuccess = isSuccess;
        FinishedInSeconds = finishedInSeconds;
        Steps = steps;
    }

    public Guid SimulationId { get; private set; }
    public SimulationScenario SimulationScenario { get; private set; }
    public SimulationKind SimulationKind { get; private set; }
    public bool? IsSuccess { get; private set; }
    public float? FinishedInSeconds { get; private set; }
    public List<Step> Steps { get; private set; }
    
    public bool IsFinished => FinishedInSeconds is not null;

    public void FinishStep(Step step) => Steps.Add(step);
    public void FinishSimulation(bool isSuccess, float finishedInSeconds) => (IsSuccess, FinishedInSeconds) = (isSuccess, finishedInSeconds);
}
