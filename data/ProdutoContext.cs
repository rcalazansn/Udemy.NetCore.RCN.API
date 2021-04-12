using Microsoft.EntityFrameworkCore;

namespace RCN.API.data
{
    public class ProdutoContext:DbContext
    {
  
     public DbSet<Produto> Produtos { get; set; }

     public ProdutoContext(DbContextOptions option)
     :base(option)
     {
         
     }

    }
}