// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Metadata.Conventions;

/// <summary>
///     A convention that removes discriminators from non-TPH entity types and unmaps the inherited properties for TPT entity types.
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-conventions">Model building conventions</see> and
///     <see href="https://aka.ms/efcore-docs-inheritance">Entity type hierarchy mapping</see> for more information and examples.
/// </remarks>
public class EntityTypeHierarchyMappingConvention : IModelFinalizingConvention
{
    /// <summary>
    ///     Creates a new instance of <see cref="EntityTypeHierarchyMappingConvention" />.
    /// </summary>
    /// <param name="dependencies">Parameter object containing dependencies for this convention.</param>
    /// <param name="relationalDependencies"> Parameter object containing relational dependencies for this convention.</param>
    public EntityTypeHierarchyMappingConvention(
        ProviderConventionSetBuilderDependencies dependencies,
        RelationalConventionSetBuilderDependencies relationalDependencies)
    {
        Dependencies = dependencies;
        RelationalDependencies = relationalDependencies;
    }

    /// <summary>
    ///     Dependencies for this service.
    /// </summary>
    protected virtual ProviderConventionSetBuilderDependencies Dependencies { get; }

    /// <summary>
    ///     Relational provider-specific dependencies for this service.
    /// </summary>
    protected virtual RelationalConventionSetBuilderDependencies RelationalDependencies { get; }

    /// <inheritdoc />
    public virtual void ProcessModelFinalizing(
        IConventionModelBuilder modelBuilder,
        IConventionContext<IConventionModelBuilder> context)
    {
        var nonTphRoots = new HashSet<IConventionEntityType>();

        foreach (var entityType in modelBuilder.Metadata.GetEntityTypes())
        {
            if (entityType.BaseType == null)
            {
                continue;
            }

            var mappingStrategy = (string?)entityType[RelationalAnnotationNames.MappingStrategy];
            if (mappingStrategy == null)
            {
                mappingStrategy = (string?)entityType.GetRootType()[RelationalAnnotationNames.MappingStrategy];
            }
            
            if (mappingStrategy == RelationalAnnotationNames.TpcMappingStrategy)
            {
                nonTphRoots.Add(entityType.GetRootType());
                continue;
            }

            var tableName = entityType.GetTableName();
            if (tableName != null)
            {
                if (mappingStrategy == null)
                {
                    if (tableName != entityType.BaseType.GetTableName()
                        || entityType.GetSchema() != entityType.BaseType.GetSchema())
                    {
                        mappingStrategy = RelationalAnnotationNames.TptMappingStrategy;
                    }
                }

                if (mappingStrategy == RelationalAnnotationNames.TptMappingStrategy)
                {
                    var pk = entityType.FindPrimaryKey();
                    if (pk != null
                        && !entityType.FindDeclaredForeignKeys(pk.Properties)
                            .Any(fk => fk.PrincipalKey.IsPrimaryKey()
                            && fk.PrincipalEntityType.IsAssignableFrom(entityType)
                            && fk.PrincipalEntityType != entityType))
                    {
                        var closestMappedType = entityType.BaseType;
                        while (closestMappedType != null
                            && closestMappedType.GetTableName() == null)
                        {
                            closestMappedType = closestMappedType.BaseType;
                        }

                        if (closestMappedType != null)
                        {
                            entityType.Builder.HasRelationship(closestMappedType, pk.Properties, pk)?
                                .IsUnique(true);
                        }
                    }

                    nonTphRoots.Add(entityType.GetRootType());
                    continue;
                }
            }

            var viewName = entityType.GetViewName();
            if (viewName != null
                && (viewName != entityType.BaseType.GetViewName()
                    || entityType.GetViewSchema() != entityType.BaseType.GetViewSchema()))
            {
                nonTphRoots.Add(entityType.GetRootType());
            }
        }

        foreach (var root in nonTphRoots)
        {
            root.Builder.HasNoDiscriminator();
        }
    }
}
