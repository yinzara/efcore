// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Metadata.Conventions
{
    /// <summary>
    /// TODO
    /// </summary>
    public class RelationalMapToJsonConvention : IEntityTypeAnnotationChangedConvention
    {
        /// <summary>
        /// TODO
        /// </summary>
        public void ProcessEntityTypeAnnotationChanged(
            IConventionEntityTypeBuilder entityTypeBuilder,
            string name,
            IConventionAnnotation? annotation,
            IConventionAnnotation? oldAnnotation,
            IConventionContext<IConventionAnnotation> context)
        {
            if (name == RelationalAnnotationNames.MapToJsonColumnName)
            {
                // TODO: if json type name was specified (via attribute) propagate it here and add it as annotation on the entity itself
                // needed for postgres, since it has two json types
                var tableName = entityTypeBuilder.Metadata.GetTableName();
                if (tableName == null)
                {
                    throw new InvalidOperationException("need table name");
                }

                var jsonColumnName = annotation?.Value as string;
                if (!string.IsNullOrEmpty(jsonColumnName))
                {
                    foreach (var navigation in entityTypeBuilder.Metadata.GetNavigations()
                        .Where(n => n.ForeignKey.IsOwnership
                            && n.DeclaringEntityType == entityTypeBuilder.Metadata
                            && n.TargetEntityType.IsOwned()))
                    {
                        var currentJsonColumnName = navigation.TargetEntityType.MappedToJsonColumnName();
                        if (currentJsonColumnName == null || currentJsonColumnName != jsonColumnName)
                        {
                            navigation.TargetEntityType.SetAnnotation(RelationalAnnotationNames.MapToJsonColumnName, jsonColumnName);
                        }
                    }
                }
                else
                {
                    // TODO: unwind everything
                }
            }
        }
    }
}
