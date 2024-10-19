//DbContextFactory.cs

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace MudBlazorWeb2.Components.EntityFrameworkCore
{
    // Класс фабрики для создания экземпляров DbContext, используя dependency injection.
    public class DbContextFactory<TContext> : IDbContextFactory<TContext> where TContext : DbContext
    {
        // Приватное поле для хранения провайдера услуг.
        private readonly IServiceProvider _provider;

        // Конструктор, принимающий провайдер услуг.
        public DbContextFactory(IServiceProvider provider)
        {
            // Инициализация провайдера услуг.
            _provider = provider;
        }

        // Метод для создания экземпляра DbContext.
        public TContext CreateDbContext()
        {
            // Создание экземпляра TContext с помощью ActivatorUtilities.CreateInstance, используя провайдер услуг.
            // Это позволяет создавать экземпляры DbContext на demande, избегая проблем с параллельными операциями и конкуренцией.
            return ActivatorUtilities.CreateInstance<TContext>(_provider);
        }

    }
}
