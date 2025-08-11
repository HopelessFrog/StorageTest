using Storage.Common;

namespace Storage.Data.Entities;

public class Unit : IEntity, IArchivable
{
    public int Id { get; private set; }
    public string Name { get; set; } = string.Empty;

    public ArchiveState State { get; set; } = ArchiveState.Active;
}