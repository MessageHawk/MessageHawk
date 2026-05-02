namespace MessageHawk.Domain.Entities;

public class MessageType
{
    public int Id { get; set; }
    public required string Code { get; set; }
    public required string DisplayName { get; set; }

    public ICollection<Interchange> Interchanges { get; set; } = new List<Interchange>();
}
