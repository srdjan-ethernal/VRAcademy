namespace VRAcademy.Simulations.Api.Reports.Models;

public sealed class Step
{
    public Step(int id, bool isSuccess, float finishedInSeconds)
    {
        Id = id;
        IsSuccess = isSuccess;
        FinishedInSeconds = finishedInSeconds;
    }
    
    public int Id { get; set; }
    public bool IsSuccess { get; private set; }
    public float FinishedInSeconds { get; private set; }
}
