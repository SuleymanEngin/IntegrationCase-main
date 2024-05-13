namespace Integration.Common;

public sealed class Item
{
    public int Id { get; set; }
    public string Content { get; set; }

    public override string ToString()
    {
        return $"{Id}:{Content}";
    }
}