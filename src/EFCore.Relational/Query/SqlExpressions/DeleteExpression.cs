// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query.SqlExpressions;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
public sealed class DeleteExpression : Expression, IPrintableExpression
{
    public DeleteExpression(TableExpression table, SqlExpression? predicate = null)
    {
        Table = table;
        Predicate = predicate;
    }

    public TableExpression Table { get; }

    public SqlExpression? Predicate { get; }

    /// <inheritdoc />
    public override Type Type
        => typeof(object);

    /// <inheritdoc />
    public sealed override ExpressionType NodeType
        => ExpressionType.Extension;

    protected override Expression VisitChildren(ExpressionVisitor visitor)
    {
        throw new NotImplementedException();
        //var table = (TableExpression)visitor.Visit(Table);
        //var predicate = (SqlExpression?)visitor.Visit(Predicate);

        //return table != Table || predicate != Predicate
        //    ? new DeleteExpression(table, predicate)
        //    : this;
    }

    public DeleteExpression Update(TableExpression table, SqlExpression? predicate)
        => table != Table || predicate != Predicate
            ? new DeleteExpression(table, predicate)
            : this;

    /// <inheritdoc />
    public void Print(ExpressionPrinter expressionPrinter)
    {
        expressionPrinter.AppendLine($"DELETE FROM {Table.Name} AS {Table.Alias}");
        if (Predicate != null)
        {
            expressionPrinter.Append("WHERE ");
            expressionPrinter.Visit(Predicate);
        }
    }

    /// <inheritdoc />
    public override bool Equals(object? obj)
        => obj != null
            && (ReferenceEquals(this, obj)
                || obj is DeleteExpression deleteExpression
                && Equals(deleteExpression));

    private bool Equals(DeleteExpression deleteExpression)
        => Table == deleteExpression.Table
        && (Predicate == null
            ? deleteExpression.Predicate == null
            : Predicate == deleteExpression.Predicate);

    /// <inheritdoc />
    public override int GetHashCode() => HashCode.Combine(Table, Predicate);
}
