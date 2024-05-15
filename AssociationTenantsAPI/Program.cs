using AssociationBusiness.Association.Concrete;
using AssociationBusiness.Association.Interface;
using AssociationBusiness.Handlers;
using AssociationBusiness.Handlers.Blogging;
using AssociationBusiness.Handlers.Components;
using AssociationBusiness.Handlers.Rows;
using AssociationEntities.Models;
using AssociationRepository.Association;
using AssociationRepository.Association.Menu;
using AssociationRepository.Association.PageData;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Reflection;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddScoped<IBlogBusiness, BlogBusiness>();
builder.Services.AddScoped<IBlogRepository, BlogRepository>();

builder.Services.AddScoped<ICustomMenuRepository, CustomMenuRepository>();
builder.Services.AddScoped<IPageRepository, PageRepository>();
builder.Services.AddScoped<IRowRepository, RowRepository>();
builder.Services.AddScoped<IComponentsRepository, ComponentsRepository>();
builder.Services.AddScoped<IContainerRepository, ContainerRepository>();
builder.Services.AddScoped<IComponentPropertiesRespository, ComponentPropertiesRespository>();
builder.Services.AddScoped<IBlogPagesRepository, BlogPagesRepository>();
builder.Services.AddScoped<IEventRespository, EventRepository>();

builder.Services.AddMediatR(m =>
{
    m.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());
});
builder.Services.AddTransient<GetAllBloggingsHandler>();
builder.Services.AddTransient<CreateMenuHandler>();

builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblies(
    typeof(GetAllBloggingsHandler).Assembly,
    typeof(CreateBloggingHandler).Assembly,
    typeof(CreateMenuHandler).Assembly,
    typeof(CreatePageHandler).Assembly,
    typeof(GetAllMenuHandler).Assembly,
    typeof(GetAllPageHandler).Assembly,
    typeof(GetMenuByIdHandler).Assembly,
    typeof(GetPageByIdHandler).Assembly,
    typeof(GetAllRowsByPageIdHandler).Assembly,
    typeof(CreateRowHandler).Assembly,
    typeof(DeleteRowByIdRequest).Assembly,
    typeof(CreateComponentHandler).Assembly,
    typeof(DeleteComponentByComponentIdHander).Assembly,
    typeof(DeleteComponentByContainerIdHandler).Assembly,
    typeof(GetComponentByContainerIdHandler).Assembly,
    typeof(CreateContainerHandle).Assembly,
    typeof(DeleteContainerByRowIdHandle).Assembly,
    typeof(GetAllContainersByIDHandle).Assembly,

    typeof(CreateComponentPropertiesHandler).Assembly,
    typeof(CreateComponentPropertyHandler).Assembly,
    typeof(DeleteComponentPropertiesByComponentId).Assembly,
    typeof(CreateBlogPageHandler).Assembly,
typeof(DeleteBlogPageByIdHandler).Assembly,
typeof(GetAllBlogPagesHandler).Assembly,
typeof(GetBlogPageByIdHandler).Assembly));

builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
    options.JsonSerializerOptions.WriteIndented = true;
});

var connectionString = builder.Configuration.GetConnectionString("DefaultConnectionString");
builder.Services.AddDbContext<AssociationContext>(x => x.UseSqlServer(connectionString));
builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
