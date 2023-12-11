namespace Pipeline.Services;

public interface IDataSource
{
    void Listen(Action<FileProcessContext> listener);
    
    Task DeleteAsync(FileProcessContext context);
}
