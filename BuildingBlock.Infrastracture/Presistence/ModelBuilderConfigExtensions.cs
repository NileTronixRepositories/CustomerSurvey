using BuildingBlock.Application.Abstraction.Persistence;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace BuildingBlock.Infrastracture.Presistence
{
    public static class ModelBuilderConfigExtensions
    {
        public static void ApplyWriteConfigurations(this ModelBuilder modelBuilder, Assembly assembly)
        {
            var configs = assembly.GetTypes()
                .Where(t => !t.IsAbstract && !t.IsGenericTypeDefinition)
                .Where(t => typeof(IWriteEntityConfiguration).IsAssignableFrom(t))
                .ToList();

            foreach (var cfgType in configs)
            {
                var iface = cfgType.GetInterfaces()
                    .First(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IEntityTypeConfiguration<>));

                var entityType = iface.GetGenericArguments()[0];
                var cfgInstance = Activator.CreateInstance(cfgType);

                var applyGeneric = typeof(ModelBuilder).GetMethods()
                    .Single(m => m.Name == nameof(ModelBuilder.ApplyConfiguration) && m.GetParameters().Length == 1)
                    .MakeGenericMethod(entityType);

                applyGeneric.Invoke(modelBuilder, new[] { cfgInstance });
            }
        }

        public static void ApplyReadConfigurations(this ModelBuilder modelBuilder, Assembly assembly)
        {
            var configs = assembly.GetTypes()
                .Where(t => !t.IsAbstract && !t.IsGenericTypeDefinition)
                .Where(t => typeof(IReadEntityConfiguration).IsAssignableFrom(t))
                .ToList();

            foreach (var cfgType in configs)
            {
                var iface = cfgType.GetInterfaces()
                    .First(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IEntityTypeConfiguration<>));

                var entityType = iface.GetGenericArguments()[0];
                var cfgInstance = Activator.CreateInstance(cfgType);

                var applyGeneric = typeof(ModelBuilder).GetMethods()
                    .Single(m => m.Name == nameof(ModelBuilder.ApplyConfiguration) && m.GetParameters().Length == 1)
                    .MakeGenericMethod(entityType);

                applyGeneric.Invoke(modelBuilder, new[] { cfgInstance });
            }
        }
    }
}