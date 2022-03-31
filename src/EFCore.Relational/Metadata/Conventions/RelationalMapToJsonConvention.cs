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
            //if (name == RelationalAnnotationNames.MapToJson)
            //{
            //    if (annotation?.Value as bool? == true)
            //    {




            //    }
            //    else
            //    {
            //    }
            //}

            if (name == RelationalAnnotationNames.MapToJsonColumnName)
            {
                var tableName = entityTypeBuilder.Metadata.GetTableName();
                if (tableName == null)
                {
                    throw new InvalidOperationException("need table name");
                }

                if (!string.IsNullOrEmpty(annotation?.Value as string))
                {
                    foreach (var navigation in entityTypeBuilder.Metadata.GetNavigations()
                        .Where(n => n.ForeignKey.IsOwnership
                            && n.DeclaringEntityType == entityTypeBuilder.Metadata
                            && n.TargetEntityType.IsOwned()))
                    {
                        var mapToJsonAnnotation = navigation.TargetEntityType.FindAnnotation(RelationalAnnotationNames.MapToJsonColumnName);
                        if (mapToJsonAnnotation == null || mapToJsonAnnotation.Value != annotation.Value)
                        {
                            navigation.TargetEntityType.SetAnnotation(RelationalAnnotationNames.MapToJsonColumnName, annotation.Value);
                            //navigation.TargetEntityType.SetAnnotation(RelationalAnnotationNames.TableName, tableName);
                        }
                    }
                }
                else
                {
                    // TODO: unwind everything
                }
            }

            // if table name changed for entity which owns other entities mapped to 
            //if (name == RelationalAnnotationNames.TableName)
            //{

            //}


        }
    }
}
