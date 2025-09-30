using System.ComponentModel.DataAnnotations.Schema;

namespace ItemShopHub.Data;

[Table("CategoryProduct")]
public class CategoryProductLink
{
    public long CategoryId { get; set; }
    public long ProductId { get; set; }
}
