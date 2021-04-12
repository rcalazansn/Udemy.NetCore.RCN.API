using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using RCN.API.data;

namespace RCN.API
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            this.Configuration = configuration;

        }

        public IApiVersionDescriptionProvider test(IServiceCollection services){
            var provider = services.BuildServiceProvider().GetRequiredService<IApiVersionDescriptionProvider>();
            return provider;
        }
        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {

            services.AddControllers();

            services.AddMvc()
            //configura compatibilidade de versão do MVC
            .SetCompatibilityVersion(CompatibilityVersion.Version_3_0)
            //configura a posibilidade de retornar um XML
            .AddXmlSerializerFormatters();

            // mapeando a injeção de dependencia do dbcontex
            // dessa forma sempre que a classe for instanciada o construtor obtem o contexto
            services.AddDbContext<ProdutoContext>(opt =>
                opt.UseInMemoryDatabase(databaseName: "produtoInMemory")
                .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking));

            // fazendo o mapeamento da injeção de dependencia 
            // do IProdutoRepository  para a clase concreta 
            // dessa forma o controler não sabe (tem visibilidade) qual clase sera implementada
            services.AddTransient<IProdutoRepository, ProdutoRepository>();   

            //instansiando o apiversion
            services.AddApiVersioning();

            //configurando uso de cash
            //midware de cach
            services.AddResponseCaching();


            // Register the Swagger generator, defining 1 or more Swagger documents
            services.AddSwaggerGen();


            //cconfigurando midware de compresão de dados
            services.AddResponseCompression(opt =>
            {
                //injetando dependencia para o midwere GZIP responsavel pela compressão
                //opt.Providers.Add<GzipCompressionProvider>();

                //o Brotli tem uma maior compressão que o Gzip
                opt.Providers.Add<BrotliCompressionProvider>();
                opt.EnableForHttps = true;
            });


        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                                                                     
            }

                #region  'configuração do Swagger'
                    
                // Enable middleware to serve generated Swagger as a JSON endpoint.
                app.UseSwagger(c =>
                {
                    c.SerializeAsV2 = true;
                });

                // Enable middleware to serve generated Swagger as a JSON endpoint.
                app.UseSwagger();

                // Enable middleware to serve swagger-ui (HTML, JS, CSS, etc.),
                // specifying the Swagger JSON endpoint.
                app.UseSwaggerUI(c =>
                {
                    c.SwaggerEndpoint("/swagger/v1/swagger.json","Pai Produtos ");
                    c.RoutePrefix = string.Empty;
                }); 

                 #endregion  


            app.UseHttpsRedirection();

            //instansia os midware
            app.UseResponseCaching();

            app.UseResponseCompression();
            // fim dos midwares inseridos manualmente
            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
