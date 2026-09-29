using ArchUnitNET.Domain;
using ArchUnitNET.Loader;
using ArticlesService.Application;
using ArticlesService.ArticlesAPI;
using ArticlesService.Domain;
using ArticlesService.Infrastructure.Cache;
using ArticlesService.Infrastructure.DataAccess;
using ReflectionAssembly = System.Reflection.Assembly;

namespace ArticlesService.Tests.ArchitectureTests
{
    public abstract class BaseTest
    {
        protected static readonly ReflectionAssembly DomainAssembly = typeof(IDomainAssemblyMarker).Assembly;
        protected static readonly ReflectionAssembly ApplicationAssembly = typeof(IApplicationAssemblyMarker).Assembly;
        protected static readonly ReflectionAssembly PresentationAssembly = typeof(IPresentationAssemblyMarker).Assembly;

        protected static readonly ReflectionAssembly InfrastructureCacheAssembly = typeof(IInfrastructureCacheAssemblyMarker).Assembly;
        protected static readonly ReflectionAssembly InfrastructureDataAccessAssembly = typeof(IInfrastructureDataAccessAssemblyMarker).Assembly;
              
        protected static readonly Architecture Architecture = new ArchLoader()
            .LoadAssemblies(
                DomainAssembly,
                ApplicationAssembly,
                PresentationAssembly,
                InfrastructureCacheAssembly,
                InfrastructureDataAccessAssembly)
            .Build();
    }
}
