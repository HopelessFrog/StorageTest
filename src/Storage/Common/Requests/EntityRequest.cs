using Storage.Common.Base;

namespace Storage.Common.Requests;

public record EntityRequest(int Id);

public class EntityRequestValidator : BaseEntityRequestValidator<EntityRequest>
{ }