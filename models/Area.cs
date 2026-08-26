namespace TestDevBackMiddle.Models;

public class Area
{
    public int Id { get; set; }

    public int IdArea { get; set; }

    public string AreaName { get; set; } = string.Empty;

    public int? StatusArea { get; set; }

    public DateTime? CreateDate { get; set; }
}