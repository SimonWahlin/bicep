// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
using System.Collections.Immutable;
using Bicep.Core.TypeSystem.Providers;
using Bicep.Core.TypeSystem.Providers.ThirdParty;
using Bicep.Core.TypeSystem.Types;
using static Bicep.Core.TypeSystem.Providers.ThirdParty.ThirdPartyResourceTypeLoader;

namespace Bicep.Core.Semantics.Namespaces
{
    public static class ThirdPartyNamespaceType
    {
        public static NamespaceSettings Settings { get; } = new(
            IsSingleton: true,
            BicepProviderName: string.Empty,
            ConfigurationType: null,
            ArmTemplateProviderName: string.Empty,
            ArmTemplateProviderVersion: string.Empty);

        public static NamespaceType Create(string name, string aliasName, IResourceTypeProvider resourceTypeProvider)
        {
            // NamespaceConfig is not null
            if (resourceTypeProvider is ThirdPartyResourceTypeProvider thirdPartyProvider && thirdPartyProvider.GetNamespaceConfiguration() is NamespaceConfiguration namespaceConfig && namespaceConfig != null)
            {
                // Include the `deprecated` decorator in the list of available decorators for the namespace.
                var decorators = ImmutableArray<Decorator>.Empty.Add(new Decorator("deprecated", new FunctionOverload("deprecated", ReturnType: TypeSymbolValidationFlags.Default, new Parameter("message", TypeSymbolValidationFlags.Default, "The deprecation message."), new FunctionFlags()), null, null));

                return new NamespaceType(
                    aliasName,
                    new NamespaceSettings(
                        IsSingleton: namespaceConfig.IsSingleton,
                        BicepProviderName: namespaceConfig.Name,
                        ConfigurationType: namespaceConfig.ConfigurationObject,
                        ArmTemplateProviderName: namespaceConfig.Name,
                        ArmTemplateProviderVersion: namespaceConfig.Version),
                    ImmutableArray<TypeProperty>.Empty,
                    ImmutableArray<FunctionOverload>.Empty,
                    ImmutableArray<BannedFunction>.Empty,
                    decorators,
                    resourceTypeProvider);
            }

            // NamespaceConfig is required to be set for 3PProviders
            throw new ArgumentException($"Please provide the following required Settings properties: Name, Version, & IsSingleton.");

        }
    }
}
