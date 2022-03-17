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
            if (name == RelationalAnnotationNames.MapToJson)
            {
                if (annotation?.Value as bool? == true)
                {
                }
                else
                {
                }
            }

            if (name == RelationalAnnotationNames.JsonColumnName)
            {
            }
        }
    }
}
