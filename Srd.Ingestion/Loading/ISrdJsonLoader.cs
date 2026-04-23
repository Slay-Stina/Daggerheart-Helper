using Srd.Ingestion.Domain;

namespace Srd.Ingestion.Loading;

public interface ISrdJsonLoader
{
    Task<SrdCatalog> LoadAsync(string jsonDirectoryPath, CancellationToken cancellationToken = default);
}

