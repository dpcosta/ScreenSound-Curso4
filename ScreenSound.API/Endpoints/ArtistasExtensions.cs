using Azure.Core;
using Microsoft.AspNetCore.Mvc;
using ScreenSound.API.Requests;
using ScreenSound.API.Response;
using ScreenSound.Banco;
using ScreenSound.Modelos;
using ScreenSound.Shared.Dados.IdentityModel;
using System.Net;
using System.Security.Claims;

namespace ScreenSound.API.Endpoints;

public static class ArtistasExtensions
{
    public static void AddEndPointsArtistas(this IEndpointRouteBuilder app)
    {

        var groupBuilder = app
            .MapGroup("/artistas")
            .WithTags("Artistas")
            .RequireAuthorization();

        #region Endpoint Artistas
        groupBuilder.MapGet("", ([FromServices] DAL<Artista> dal) =>
        {
            var listaDeArtistas = dal.Listar();
            if (listaDeArtistas is null)
            {
                return Results.NotFound();
            }
            var listaDeArtistaResponse = EntityListToResponseList(listaDeArtistas);
            return Results.Ok(listaDeArtistaResponse);
        });

        groupBuilder.MapGet("{nome}", ([FromServices] DAL<Artista> dal, string nome) =>
        {
            var artista = dal.RecuperarPor(a => a.Nome.ToUpper().Equals(nome.ToUpper()));
            if (artista is null)
            {
                return Results.NotFound();
            }
            return Results.Ok(EntityToResponse(artista));

        });

        groupBuilder.MapPost("", async ([FromServices] IHostEnvironment env, [FromServices] DAL<Artista> dal, [FromBody] ArtistaRequest artistaRequest) =>
        {

            var nome = artistaRequest.nome.Trim();
            var imagemArtista = DateTime.Now.ToString("ddMMyyyyhhss") + "." + nome + ".jpg";

            var path = Path.Combine(env.ContentRootPath,
                "wwwroot", "FotosPerfil", imagemArtista);

            using MemoryStream ms = new MemoryStream(Convert.FromBase64String(artistaRequest.fotoPerfil!));
            using FileStream fs = new(path, FileMode.Create);
            await ms.CopyToAsync(fs);

            var artista = new Artista(artistaRequest.nome, artistaRequest.bio) { FotoPerfil = $"/FotosPerfil/{imagemArtista}" };

            dal.Adicionar(artista);
            return Results.Ok();
        });

        groupBuilder.MapDelete("{id}", ([FromServices] DAL<Artista> dal, int id) =>
        {
            var artista = dal.RecuperarPor(a => a.Id == id);
            if (artista is null)
            {
                return Results.NotFound();
            }
            dal.Deletar(artista);
            return Results.NoContent();

        });

        groupBuilder.MapPut("", ([FromServices] DAL<Artista> dal, [FromBody] ArtistaRequestEdit artistaRequestEdit) =>
        {
            var artistaAAtualizar = dal.RecuperarPor(a => a.Id == artistaRequestEdit.Id);
            if (artistaAAtualizar is null)
            {
                return Results.NotFound();
            }
            artistaAAtualizar.Nome = artistaRequestEdit.nome;
            artistaAAtualizar.Bio = artistaRequestEdit.bio;
            dal.Atualizar(artistaAAtualizar);
            return Results.Ok();
        });


        groupBuilder.MapPost("avaliacao", (
            [FromServices] DAL<Artista> dalArtista,
            [FromServices] DAL<PessoaComAcesso> dalPessoa,
            [FromBody] AvaliacaoArtistaRequest request,
            HttpContext context) =>
        {
            var artista = dalArtista.RecuperarPor(a => a.Id == request.ArtistaId);
            if (artista is null) return Results.NotFound();

            var email = context.User
                .Claims.FirstOrDefault(c => c.Type.Equals(ClaimTypes.Email))?
                .Value ?? throw new InvalidOperationException("Não foi possível recuperar pessoa logada.");

            var pessoa = dalPessoa
                .RecuperarPor(p => p.Email!.Equals(email))
                ?? throw new InvalidOperationException("Não foi possível recuperar pessoa logada.");

            var avaliacao = artista
                .Avaliacoes
                .FirstOrDefault(a => a.ArtistaId == artista.Id && a.PessoaId == pessoa.Id);
            if (avaliacao is not null) avaliacao.Nota = request.Nota;
            else artista.AdicionarNota(request.Nota, pessoa.Id);

            dalArtista.Atualizar(artista);
            return Results.Created();
        });

        groupBuilder.MapGet("{id}/avaliacao", (int id,
            [FromServices] DAL<Artista> dalArtista,
            [FromServices] DAL<PessoaComAcesso> dalPessoa,
            HttpContext context) =>
        {
            var artista = dalArtista.RecuperarPor(a => a.Id == id);
            if (artista is null) return Results.NotFound();

            var email = context.User
                .Claims.FirstOrDefault(c => c.Type.Equals(ClaimTypes.Email))?
                .Value ?? throw new InvalidOperationException("Não foi possível recuperar pessoa logada.");

            var pessoa = dalPessoa
                .RecuperarPor(p => p.Email!.Equals(email))
                ?? throw new InvalidOperationException("Não foi possível recuperar pessoa logada.");

            var avaliacao = artista
                .Avaliacoes
                .FirstOrDefault(a => a.ArtistaId == artista.Id && a.PessoaId == pessoa.Id);
            if (avaliacao is not null) return Results.Ok(new { ArtistaId = artista.Id, avaliacao.Nota });
            else return Results.Ok(new { ArtistaId = artista.Id, Nota = 0 });

        });
        #endregion
    }

    private static ICollection<ArtistaResponse> EntityListToResponseList(IEnumerable<Artista> listaDeArtistas)
    {
        return listaDeArtistas.Select(a => EntityToResponse(a)).ToList();
    }

    private static ArtistaResponse EntityToResponse(Artista artista)
    {
        return new ArtistaResponse(artista.Id, artista.Nome, artista.Bio, artista.FotoPerfil)
        {
            Classificacao = artista.Avaliacoes
                .Select(a => a.Nota)
                .DefaultIfEmpty(0)
                .Average()
        };
    }


}
