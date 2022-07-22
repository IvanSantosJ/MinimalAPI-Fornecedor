namespace MinimalAPI.Models
{
    public class Fornecedor
    {
        public Guid Id { get; set; }
        public string Nome { get; set; }
        public string CNPJ { get; set; }
        public string CNPJ_Formatado { get; set; }
        public bool Ativo { get; set; }
    }
}
