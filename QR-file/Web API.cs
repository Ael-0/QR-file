public class Web_API
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        //  точка конфігурації

        builder.Services.AddControllers();
        // підтримка контролерів ([ApiController]) — для побудови REST API
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();
        //Підключає Swagger/OpenAPI
        var app = builder.Build();
        //Створює екземпляр застосунку (WebApplication

        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }
        //налаштовується pipeline(послідовність middleware)

        app.UseHttpsRedirection();

        app.UseAuthorization();
        //немає UseAuthentication(), не працює 

        app.MapControllers();

        app.Run();

    }
}
