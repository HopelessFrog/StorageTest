using Storage.Common;

namespace Storage.Data.Entities;

public interface IArchivable
{
    ArchiveState State { get; }
}