using ScreenSound.Modelos;

namespace ScreenSound.Shared.Modelos.Modelos;

public interface IAvaliavel
{
    double Nota { get; set; }
}

public class AvaliacaoArtista : IAvaliavel
{
    public int ArtistaId { get; set; }
    public virtual Artista? Artista { get; set; }
    public int PessoaId { get; set; }
    public double Nota { get; set; }
}
