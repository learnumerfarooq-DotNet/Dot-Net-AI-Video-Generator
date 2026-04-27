namespace AiContentFactory.Domain.Common;

public abstract class AggregateRoot
{
    protected AggregateRoot() { }
    protected AggregateRoot(Guid id) => Id = id;
    public Guid Id { get; set; }
}
