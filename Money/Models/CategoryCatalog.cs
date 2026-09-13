using System.Globalization;
using System.Text;
namespace Money.Models;

public interface ICategoryVisual { string Name { get; } string Type { get; } string Color { get; } string? Icon { get; } }
public sealed record MainCategoryItem(long Id, string Name, string Type, string Color, string? Icon,
    int Order, bool Active, int SubcategoryCount = 0) : ICategoryVisual;
public sealed record SubcategoryItem(long Id, long MainCategoryId, string MainCategoryName,
    string Name, string Type, string Color, string? Icon, bool Active) : ICategoryVisual
{
    public string DisplayName => $"{MainCategoryName} / {Name}";
}
public sealed record CategorySeed(string Name, string Icon, string Type, int Order, IReadOnlyList<SubcategorySeed> Children);
public sealed record SubcategorySeed(string Name, string Icon, string? Pillar);
public static class CategoryCatalog
{
    public static string Normalize(string value) => string.Join(" ", new string(value.Trim().Normalize(NormalizationForm.FormD)
        .Where(c => CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark).ToArray())
        .Normalize(NormalizationForm.FormC).ToUpperInvariant().Split(' ', StringSplitOptions.RemoveEmptyEntries));
    public static IReadOnlyList<CategorySeed> All { get; } =
    [
        new("Alimentação", "Restaurant", "despesa", 0, [
            new("Lanches", "Fastfood", "VARIAVEIS_LAZER"),
            new("Restaurantes", "Restaurant", "VARIAVEIS_LAZER"),
            new("Supermercado", "ShoppingCart", "FIXAS_ESSENCIAIS"),
        ]),
        new("Animais de Estimação", "Pets", "despesa", 1, [
            new("Pet Shop", "Pets", "FIXAS_ESSENCIAIS"),
            new("Veterinário", "MedicalServices", "FIXAS_ESSENCIAIS"),
        ]),
        new("Casa/Moradia", "House", "despesa", 2, [
            new("Aluguel", "Key", "FIXAS_ESSENCIAIS"),
            new("Celular/Linha", "Smartphone", "FIXAS_ESSENCIAIS"),
            new("Condomínio", "Apartment", "FIXAS_ESSENCIAIS"),
            new("Energia Elétrica", "Bolt", "FIXAS_ESSENCIAIS"),
            new("Gás", "LocalFireDepartment", "FIXAS_ESSENCIAIS"),
            new("Internet", "Wifi", "FIXAS_ESSENCIAIS"),
            new("Lavanderia", "LocalLaundryService", "FIXAS_ESSENCIAIS"),
            new("Manutenção Residencial", "Build", "FIXAS_ESSENCIAIS"),
            new("Reforma/Reparo", "HomeRepairService", "FIXAS_ESSENCIAIS"),
            new("Serviços Domésticos", "CleaningServices", "FIXAS_ESSENCIAIS"),
            new("Telefone", "Phone", "FIXAS_ESSENCIAIS"),
            new("Água", "WaterDrop", "FIXAS_ESSENCIAIS"),
        ]),
        new("Compras", "ShoppingBag", "despesa", 3, [
            new("Acessórios", "Watch", "VARIAVEIS_LAZER"),
            new("Calçados", "Checkroom", "VARIAVEIS_LAZER"),
            new("Decoração", "Chair", "VARIAVEIS_LAZER"),
            new("Eletrônicos", "Devices", "VARIAVEIS_LAZER"),
            new("Informática", "Computer", "VARIAVEIS_LAZER"),
            new("Móveis", "Chair", "VARIAVEIS_LAZER"),
            new("Roupas", "Checkroom", "VARIAVEIS_LAZER"),
        ]),
        new("Educação", "School", "despesa", 4, [
            new("Cursos", "MenuBook", "VARIAVEIS_LAZER"),
            new("Livros", "MenuBook", "VARIAVEIS_LAZER"),
            new("Material Escolar", "Edit", "FIXAS_ESSENCIAIS"),
            new("Mensalidade Escolar", "School", "FIXAS_ESSENCIAIS"),
        ]),
        new("Financeiro", "AccountBalanceWallet", "despesa", 5, [
            new("Consórcios", "Groups", "FIXAS_ESSENCIAIS"),
            new("Impostos", "ReceiptLong", "FIXAS_ESSENCIAIS"),
            new("Investimentos", "TrendingUp", "INVESTIMENTOS"),
            new("Multas", "Gavel", "VARIAVEIS_LAZER"),
            new("Reserva Financeira", "Savings", "RESERVA_EMERGENCIA"),
            new("Tarifas Bancárias", "AccountBalance", "FIXAS_ESSENCIAIS"),
            new("Taxas", "RequestQuote", "VARIAVEIS_LAZER"),
        ]),
        new("Lazer", "Movie", "despesa", 6, [
            new("Cinema", "Movie", "VARIAVEIS_LAZER"),
            new("Jogos", "SportsEsports", "VARIAVEIS_LAZER"),
            new("Passeios", "Public", "VARIAVEIS_LAZER"),
            new("Streaming", "LiveTv", "VARIAVEIS_LAZER"),
            new("Viagens", "Flight", "VARIAVEIS_LAZER"),
        ]),
        new("Saúde", "MedicalServices", "despesa", 7, [
            new("Academia", "FitnessCenter", "VARIAVEIS_LAZER"),
            new("Consultas Médicas", "MedicalServices", "FIXAS_ESSENCIAIS"),
            new("Exames", "Biotech", "FIXAS_ESSENCIAIS"),
            new("Farmácia", "LocalPharmacy", "FIXAS_ESSENCIAIS"),
            new("Plano de Saúde", "HealthAndSafety", "FIXAS_ESSENCIAIS"),
        ]),
        new("Transporte", "DirectionsCar", "despesa", 8, [
            new("Combustível", "LocalGasStation", "FIXAS_ESSENCIAIS"),
            new("Manutenção Veículo", "Build", "FIXAS_ESSENCIAIS"),
            new("Seguro Veículo", "Shield", "FIXAS_ESSENCIAIS"),
            new("Transporte Público", "DirectionsBus", "FIXAS_ESSENCIAIS"),
            new("Uber / Táxi", "LocalTaxi", "VARIAVEIS_LAZER"),
        ]),
        new("Outros", "Category", "despesa", 9, [
            new("Doações", "VolunteerActivism", "VARIAVEIS_LAZER"),
            new("Presentes", "CardGiftcard", "VARIAVEIS_LAZER"),
            new("Outras Despesas", "MoreHoriz", "VARIAVEIS_LAZER"),
            new("Dízimos", "Church", "VARIAVEIS_LAZER"),
        ]),
        new("Receitas", "Payments", "receita", 10, [
            new("Salário", "Payments", null),
            new("Adiantamento Salarial", "Payments", null),
            new("Horas Extras", "Payments", null),
            new("Comissões", "Payments", null),
            new("Bônus", "Payments", null),
            new("Décimo Terceiro", "Payments", null),
            new("Férias", "Payments", null),
            new("Freelance", "Payments", null),
            new("Trabalho Autônomo", "Payments", null),
            new("Venda de Produtos", "Payments", null),
            new("Venda de Serviços", "Payments", null),
            new("Aluguel Recebido", "Payments", null),
            new("Dividendos", "Payments", null),
            new("Rendimentos de Investimentos", "Payments", null),
            new("Juros Recebidos", "Payments", null),
            new("Cashback", "Payments", null),
            new("Prêmios", "Payments", null),
            new("Bonificações", "Payments", null),
            new("Restituição de Imposto", "Payments", null),
            new("Reembolso", "Payments", null),
            new("Mesada", "Payments", null),
            new("Ajuda Familiar", "Payments", null),
            new("Doações Recebidas", "Payments", null),
            new("Herança", "Payments", null),
            new("Indenizações", "Payments", null),
            new("Resgate de Investimentos", "Payments", null),
            new("Venda de Bens", "Payments", null),
            new("Outras Receitas", "Payments", null),
        ])
    ];
    public static IReadOnlyList<string> Icons { get; } = All.Select(x => x.Icon)
        .Concat(All.SelectMany(x => x.Children.Select(c => c.Icon))).Append("Sell").Distinct().ToArray();
}
