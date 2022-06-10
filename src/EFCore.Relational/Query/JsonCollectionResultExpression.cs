// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query
{
    /// <summary>
    /// TODO
    /// </summary>
    public class JsonCollectionResultExpression2 : Expression, IPrintableExpression
    {
        /// <summary>
        /// TODO
        /// </summary>
        public JsonCollectionResultExpression2(
            JsonQueryExpression jsonQueryExpression,
            INavigation navigation,
            Type elementType)
        {
            JsonQueryExpression = jsonQueryExpression;
            Navigation = navigation;
            ElementType = elementType;
        }

        /// <summary>
        /// TODO
        /// </summary>
        public virtual JsonQueryExpression JsonQueryExpression { get; }

        /// <summary>
        /// TODO
        /// </summary>
        public virtual INavigation Navigation { get; }

        /// <summary>
        /// TODO
        /// </summary>
        public virtual Type ElementType { get; }

        /// <inheritdoc />
        public override Type Type
            => typeof(IEnumerable<>).MakeGenericType(ElementType);

        /// <inheritdoc />
        public override ExpressionType NodeType
            => ExpressionType.Extension;

        /// <inheritdoc />
        protected override Expression VisitChildren(ExpressionVisitor visitor)
        {
            var jsonQueryExpression = (JsonQueryExpression)visitor.Visit(JsonQueryExpression);

            return Update(jsonQueryExpression);
        }

        /// <summary>
        ///     Creates a new expression that is like this one, but using the supplied children. If all of the children are the same, it will
        ///     return this expression.
        /// </summary>
        /// <param name="jsonQueryExpression">The <see cref="JsonQueryExpression" /> property of the result.</param>
        /// <returns>This expression if no children changed, or an expression with the updated children.</returns>
        public virtual JsonCollectionResultExpression2 Update(JsonQueryExpression jsonQueryExpression)
            => jsonQueryExpression != JsonQueryExpression
                ? new JsonCollectionResultExpression2(jsonQueryExpression, Navigation, ElementType)
                : this;

        /// <inheritdoc />
        public virtual void Print(ExpressionPrinter expressionPrinter)
        {
            expressionPrinter.AppendLine("JsonCollectionResultExpression:");
            expressionPrinter.Visit(JsonQueryExpression);
            expressionPrinter.AppendLine();
            if (Navigation != null)
            {
                expressionPrinter.Append($"Navigation: {Navigation.Name} ");
            }

            expressionPrinter.Append("ElementType: ").Append(ElementType.ShortDisplayName());
            expressionPrinter.Append(")");
        }
    }

    /// <summary>
    /// TODO
    /// </summary>
    public class JsonCollectionResultExpression : Expression, IPrintableExpression
    {
        /// <summary>
        /// TODO
        /// </summary>
        public JsonCollectionResultExpression(
            JsonProjectionExpression jsonProjectionExpression,
            INavigation navigation,
            Type elementType)
        {
            JsonProjectionExpression = jsonProjectionExpression;
            Navigation = navigation;
            ElementType = elementType;
        }

        /// <summary>
        /// TODO
        /// </summary>
        public virtual JsonProjectionExpression JsonProjectionExpression { get; }

        /// <summary>
        /// TODO
        /// </summary>
        public virtual INavigation Navigation { get; }

        /// <summary>
        /// TODO
        /// </summary>
        public virtual Type ElementType { get; }

        /// <inheritdoc />
        public override Type Type
            => typeof(IEnumerable<>).MakeGenericType(ElementType);

        /// <inheritdoc />
        public override ExpressionType NodeType
            => ExpressionType.Extension;

        /// <inheritdoc />
        protected override Expression VisitChildren(ExpressionVisitor visitor)
        {
            var jsonProjectionExpression = (JsonProjectionExpression)visitor.Visit(JsonProjectionExpression);

            return Update(jsonProjectionExpression);
        }

        /// <summary>
        ///     Creates a new expression that is like this one, but using the supplied children. If all of the children are the same, it will
        ///     return this expression.
        /// </summary>
        /// <param name="jsonProjectionExpression">The <see cref="JsonProjectionExpression" /> property of the result.</param>
        /// <returns>This expression if no children changed, or an expression with the updated children.</returns>
        public virtual JsonCollectionResultExpression Update(JsonProjectionExpression jsonProjectionExpression)
            => jsonProjectionExpression != JsonProjectionExpression
                ? new JsonCollectionResultExpression(jsonProjectionExpression, Navigation, ElementType)
                : this;

        /// <inheritdoc />
        public virtual void Print(ExpressionPrinter expressionPrinter)
        {
            expressionPrinter.AppendLine("JsonCollectionResultExpression:");
            using (expressionPrinter.Indent())
            {
                expressionPrinter.Append("JsonProjectionExpression:");
                expressionPrinter.Visit(JsonProjectionExpression);
                expressionPrinter.AppendLine();
                if (Navigation != null)
                {
                    expressionPrinter.Append("Navigation:").AppendLine(Navigation.ToString()!);
                }

                expressionPrinter.Append("ElementType:").AppendLine(ElementType.ShortDisplayName());
            }
        }
    }
}
