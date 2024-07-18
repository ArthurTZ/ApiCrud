using ApiCrud.Data;
using Microsoft.EntityFrameworkCore;

namespace ApiCrud.Estudantes
{
    public static class EstudantesRotas
    {
        public static void AddRotasEstudantes(this WebApplication app)
        {
            RouteGroupBuilder rotasEstudantes = app.MapGroup(prefix: "estudantes");

            // Post
            rotasEstudantes.MapPost(pattern: "", handler: async (AddEstudanteRequest request, AppDBContext context, CancellationToken ct) =>
            {
                var check_if_exists = await context.Estudantes.AnyAsync(estudante => estudante.Nome == request.Nome, ct);

                if (check_if_exists)
                    return Results.Conflict(error: "Ja Existe esse Estudante!");



                var novoEstudante = new Estudante(request.Nome);
                await context.Estudantes.AddAsync(novoEstudante, ct);

                await context.SaveChangesAsync(ct);

                var estudanteRetorno = new EstudanteDto(novoEstudante.Id, novoEstudante.Nome);

                return Results.Ok(novoEstudante);
            });


            // Retonar Estudantes -> ReadUsers

            rotasEstudantes.MapGet(pattern: "",
                handler: async (AppDBContext context, CancellationToken ct) =>
                {
                    var estudantes = await context.Estudantes
                    .Where(estudante => estudante.Ativo)
                    .Select(estudante => new EstudanteDto(estudante.Id, estudante.Nome))
                    .ToListAsync(ct);
                    return estudantes;
                });

            // Atualizar Estudantes -> UpdateUsers
            rotasEstudantes.MapPut(pattern: "{id}", handler: async (Guid id,AddEstudanteRequest request, AppDBContext context, CancellationToken ct) =>
            {
                var estudante = await context.Estudantes.SingleOrDefaultAsync(
                    estudante => estudante.Id == id
                    );

                if (estudante == null) return Results.NotFound();

                estudante.AtualizarNome(request.Nome);

                await context.SaveChangesAsync(ct);

                return Results.Ok(new EstudanteDto(estudante.Id, estudante.Nome));

            });


            rotasEstudantes.MapDelete(pattern: "{id}", handler: async (Guid id,AppDBContext context, CancellationToken ct) =>
            {
                var estudante = await context.Estudantes.SingleOrDefaultAsync(
                    estudante => estudante.Id == id, ct
                    );

                if (estudante == null)
                {
                    return Results.NotFound();
                }

                estudante.DesativarEstudante();

                await context.SaveChangesAsync(ct);

                return Results.Ok();
            });




        }
    }
}
