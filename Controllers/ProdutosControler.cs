using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using RCN.API.data;

namespace RCN.API.Controllers
{

    [ApiController]
    //incluindo a rota da aplicação com a versão dos servisos
    //cada serviço será criado com sua respectiva versão na rota
   [Route("/api/v{version:apiVersion}/[controller]")]
 
     public class ProdutosController: ControllerBase
    {
        private readonly IProdutoRepository Repository;

    public ProdutosController(IProdutoRepository reporitory)
    {
        Repository = reporitory;
    }

       [HttpPost]
       // mapeando a versão do serviço
       [ApiVersion("1.0")]        
       public IActionResult Criar([FromBody]Produto produto)
        {
            if(produto.Codigo == null)
            return BadRequest("Código do produto não informado!");

            if(string.IsNullOrEmpty(produto.Descricao))
            return BadRequest("Descrição do produto não informada!");

            Repository.Inserir(produto);
            return Created(nameof(Criar),produto);
        }


       [HttpPut]
       [ApiVersion("1.0")] 
        public IActionResult Atualizar([FromBody]Produto produto)
        {
            var prod = Repository.Obter(produto.Id);
            if(prod == null) return NotFound();

            if(produto.Codigo == null)  
                    return BadRequest("Código do produto não informado!");

            if(string.IsNullOrEmpty(produto.Descricao))
                    return BadRequest("Descrição do produto não informada!");


            Repository.Editar(produto);
            return NoContent();
        }


        [HttpDelete("{id}")]
        [ApiVersion("1.0")] 
        public IActionResult Apagar(int id)
        {
            var produto = Repository.Obter(id);
            if(produto == null) return NotFound();

            Repository.Excluir(produto);
            return Ok();
        }

        
        [HttpGet]
        [ApiVersion("1.0")] 
        //configurando para o serviso use cache
        // o duratin define o tempo que ficara usando os dados do cache antes de voltar a fazer uma requisição ao servidor
        // obs o cache só funciona em métodos GET
        [ResponseCache(Duration=30)]
        public IActionResult Obter()
        {
            var lista = Repository.Obter();
            return Ok(lista);
        }

        
        [HttpGet("{id}")]
        [ApiVersion("1.0")] 
        public IActionResult Obter(int id)
        {
            var produto = Repository.Obter(id);
            if(produto == null) return NotFound();

            return Ok(produto);
        }

        [HttpGet("{codigo}")]
        [ApiVersion("2.0")] 
        public IActionResult ObterPorCodigo(string codigo)
        {
            return Ok("Metodo ObterPorCodigo Versão 2.0");
        }

/*
        [HttpGet]
        [ApiVersion("3.0")] 
        public IActionResult ObterTodos()
        {
           List<string> lista = new List<string>();

            for(int i =0; i< 1000; i++)
            {
                lista.Add($"indixe : {i}");
            }

            return Ok(string.Join(" , ", lista));
        }
        */
        
    }
}