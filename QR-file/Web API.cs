public class Web_API
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        //  ����� ������������

        builder.Services.AddControllers();
        // �������� ���������� ([ApiController]) � ��� �������� REST API
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();
        //ϳ������ Swagger/OpenAPI
        var app = builder.Build();
        //������� ��������� ���������� (WebApplication

        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }
        //������������� pipeline(����������� middleware)

        app.UseHttpsRedirection();

        app.UseAuthorization();
        //���� UseAuthentication(), �� ������ 

        app.MapControllers();

        app.Run();

    }
}
