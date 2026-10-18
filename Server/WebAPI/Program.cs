using FileRepositories;
using RepositoryContracts;
using Services;
using WebAPI;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi

builder.Services.AddControllers();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

// Register repositories
builder.Services.AddScoped<IUserRepository, UserFileRepository>();
builder.Services.AddScoped<IPostRepository, PostFileRepository>();
builder.Services.AddScoped<ICommentRepository, CommentFileRepository>();
builder.Services.AddScoped<IReactionRepository, ReactionFileRepository>();
builder.Services.AddScoped<ISubforumRepository, SubforumFileRepository>();

// Register services
builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<PostService>();
builder.Services.AddScoped<CommentService>();
builder.Services.AddScoped<ReactionService>();
builder.Services.AddScoped<SubforumService>();

var app = builder.Build();

app.UseExceptionHandler();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.MapControllers();

app.Run();