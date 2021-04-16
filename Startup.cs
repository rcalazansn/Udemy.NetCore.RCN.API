using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using RCN.API.data;
using RCN.API.Swagger;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace RCN.API
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            this.Configuration = configuration;

        }

        public IApiVersionDescriptionProvider test(IServiceCollection services)
        {
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
            services.AddApiVersioning(
                options =>
                {
                    options.ReportApiVersions = true;
                });

            services.AddVersionedApiExplorer(
               options =>
               {
                   options.GroupNameFormat = "'v'VVV";

                   options.SubstituteApiVersionInUrl = true;
               });

            services.AddTransient<IConfigureOptions<SwaggerGenOptions>, ConfigureSwaggerOptions>();

            //configurando uso de cash
            //midware de cach
            services.AddResponseCaching();


            // Register the Swagger generator, defining 1 or more Swagger documents
            services.AddSwaggerGen(
               options =>
               {
                   // add a custom operation filter which sets default values
                   options.OperationFilter<SwaggerDefaultValues>();
               });


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
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IApiVersionDescriptionProvider provider)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();

            }

            #region  'configuração do Swagger'

            // Enable middleware to serve generated Swagger as a JSON endpoint.
            app.UseSwagger();

            // Enable middleware to serve generated Swagger as a JSON endpoint.
            app.UseSwagger();

            // Enable middleware to serve swagger-ui (HTML, JS, CSS, etc.),
            // specifying the Swagger JSON endpoint.

            app.UseSwaggerUI(options =>
           {
                   // build a swagger endpoint for each discovered API version
                   foreach (var description in provider.ApiVersionDescriptions)
               {
                   options.SwaggerEndpoint($"/swagger/{description.GroupName}/swagger.json", description.GroupName.ToUpperInvariant());
               }
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
