namespace WeddingApp_Test.Application.Interfaces;

public interface IReminderProcessor
{
    Task ProcessAsync(CancellationToken cancellationToken = default);
}
