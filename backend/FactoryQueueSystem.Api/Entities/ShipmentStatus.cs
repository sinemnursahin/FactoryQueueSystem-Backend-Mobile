namespace FactoryQueueSystem.Api.Entities;

public enum ShipmentStatus
{
    OnTheWay = 0,
    InQueue = 1,
    CalledToScale = 2,
    AtScale = 3,
    Unloading = 4,
    UnloadingComplete = 5,
    Completed = 6
}
