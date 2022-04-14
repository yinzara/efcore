// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;

namespace Microsoft.EntityFrameworkCore.Query;

/// <summary>
///     TODO
/// </summary>
public class EntityMappedToJsonProjectionExpression : Expression
{
    private readonly ColumnExpression _jsonColumn;
    private readonly IReadOnlyDictionary<IProperty, ColumnExpression> _propertyExpressionMap; // key properties that get mapped to normal columns
    private readonly IReadOnlyDictionary<IProperty, string> _propertyToJsonPathMap; // regular properties of the json-mapped entity, they are mapped into a json column with a specific json path
    private readonly Dictionary<INavigation, (string, EntityShaperExpression)> _ownedNavigationMap = new(); // nested owned navigations - entity shaper 

    /// <summary>
    ///     TODO
    /// </summary>
    public EntityMappedToJsonProjectionExpression(
        IEntityType entityType,
        ColumnExpression jsonColumn,
        IReadOnlyDictionary<IProperty, ColumnExpression> propertyExpressionMap,
        IReadOnlyDictionary<IProperty, string> propertyToJsonPathMap)
    {
        EntityType = entityType;
        _jsonColumn = jsonColumn;
        _propertyExpressionMap = propertyExpressionMap;
        _propertyToJsonPathMap = propertyToJsonPathMap;
    }

    /// <summary>
    ///     The entity type being projected out.
    /// </summary>
    public virtual IEntityType EntityType { get; }

    /// <inheritdoc />
    public sealed override ExpressionType NodeType
        => ExpressionType.Extension;

    /// <inheritdoc />
    public override Type Type
        => EntityType.ClrType;

    /// <inheritdoc />
    protected override Expression VisitChildren(ExpressionVisitor visitor)
    {
        var changed = false;

        var jsonColumn = (ColumnExpression)visitor.Visit(_jsonColumn);
        changed |= jsonColumn != _jsonColumn;

        var propertyExpressionMap = new Dictionary<IProperty, ColumnExpression>();
        foreach (var (property, columnExpression) in _propertyExpressionMap)
        {
            var newExpression = (ColumnExpression)visitor.Visit(columnExpression);
            changed |= newExpression != columnExpression;

            propertyExpressionMap[property] = newExpression;
        }

        return changed
            ? new EntityMappedToJsonProjectionExpression(EntityType, jsonColumn, propertyExpressionMap, _propertyToJsonPathMap)
            : this;
    }

    /// <summary>
    ///     TODO
    /// </summary>
    public virtual void AddNavigationBinding(INavigation navigation, string jsonPath, EntityShaperExpression entityShaper)
    {
        if (!EntityType.IsAssignableFrom(navigation.DeclaringEntityType)
            && !navigation.DeclaringEntityType.IsAssignableFrom(EntityType))
        {
            throw new InvalidOperationException(
                RelationalStrings.UnableToBindMemberToEntityProjection("navigation", navigation.Name, EntityType.DisplayName()));
        }

        _ownedNavigationMap[navigation] = (jsonPath, entityShaper);
    }

    ///// <summary>
    /////     TODO
    ///// </summary>
    //public virtual EntityShaperExpression? BindNavigation(INavigation navigation)
    //{
    //    if (!EntityType.IsAssignableFrom(navigation.DeclaringEntityType)
    //        && !navigation.DeclaringEntityType.IsAssignableFrom(EntityType))
    //    {
    //        throw new InvalidOperationException(
    //            RelationalStrings.UnableToBindMemberToEntityProjection("navigation", navigation.Name, EntityType.DisplayName()));
    //    }

    //    return _ownedNavigationMap.TryGetValue(navigation, out var expression)
    //        ? expression
    //        : null;
    //}

    /// <inheritdoc />
    public override string ToString()
        => $"EntityMappedToJsonProjectionExpression: {EntityType.ShortName()}";
}

/// <summary>
///     <para>
///         An expression that represents an entity in the projection of <see cref="SelectExpression" />.
///     </para>
///     <para>
///         This type is typically used by database providers (and other extensions). It is generally
///         not used in application code.
///     </para>
/// </summary>
public class EntityProjectionExpression : Expression
{
    private readonly IReadOnlyDictionary<IPropertyBase, ColumnExpression> _propertyExpressionMap;
    private readonly Dictionary<INavigation, EntityShaperExpression> _ownedNavigationMap = new();

    /// <summary>
    ///     Creates a new instance of the <see cref="EntityProjectionExpression" /> class.
    /// </summary>
    /// <param name="entityType">The entity type to shape.</param>
    /// <param name="propertyExpressionMap">A dictionary of column expressions corresponding to properties (or in some cases navigations) of the entity type.</param>
    /// <param name="discriminatorExpression">A <see cref="SqlExpression" /> to generate discriminator for each concrete entity type in hierarchy.</param>
    public EntityProjectionExpression(
        IEntityType entityType,
        IReadOnlyDictionary<IPropertyBase, ColumnExpression> propertyExpressionMap,
        SqlExpression? discriminatorExpression = null)
    {
        EntityType = entityType;
        _propertyExpressionMap = propertyExpressionMap;
        DiscriminatorExpression = discriminatorExpression;
    }

    /// <summary>
    ///     The entity type being projected out.
    /// </summary>
    public virtual IEntityType EntityType { get; }

    /// <summary>
    ///     A <see cref="SqlExpression" /> to generate discriminator for entity type.
    /// </summary>
    public virtual SqlExpression? DiscriminatorExpression { get; }

    /// <inheritdoc />
    public sealed override ExpressionType NodeType
        => ExpressionType.Extension;

    /// <inheritdoc />
    public override Type Type
        => EntityType.ClrType;

    /// <inheritdoc />
    protected override Expression VisitChildren(ExpressionVisitor visitor)
    {
        var changed = false;
        var propertyExpressionMap = new Dictionary<IPropertyBase, ColumnExpression>();
        foreach (var (property, columnExpression) in _propertyExpressionMap)
        {
            var newExpression = (ColumnExpression)visitor.Visit(columnExpression);
            changed |= newExpression != columnExpression;

            propertyExpressionMap[property] = newExpression;
        }

        var discriminatorExpression = (SqlExpression?)visitor.Visit(DiscriminatorExpression);
        changed |= discriminatorExpression != DiscriminatorExpression;

        return changed
            ? new EntityProjectionExpression(EntityType, propertyExpressionMap, discriminatorExpression)
            : this;
    }

    /// <summary>
    ///     Makes entity instance in projection nullable.
    /// </summary>
    /// <returns>A new entity projection expression which can project nullable entity.</returns>
    public virtual EntityProjectionExpression MakeNullable()
    {
        var propertyExpressionMap = new Dictionary<IPropertyBase, ColumnExpression>();
        foreach (var (property, columnExpression) in _propertyExpressionMap)
        {
            propertyExpressionMap[property] = columnExpression.MakeNullable();
        }

        // We don't need to process DiscriminatorExpression because they are already nullable
        return new EntityProjectionExpression(EntityType, propertyExpressionMap, DiscriminatorExpression);
    }

    /// <summary>
    ///     Updates the entity type being projected out to one of the derived type.
    /// </summary>
    /// <param name="derivedType">A derived entity type which should be projected.</param>
    /// <returns>A new entity projection expression which has the derived type being projected.</returns>
    public virtual EntityProjectionExpression UpdateEntityType(IEntityType derivedType)
    {
        if (!derivedType.GetAllBaseTypes().Contains(EntityType))
        {
            throw new InvalidOperationException(
                RelationalStrings.InvalidDerivedTypeInEntityProjection(
                    derivedType.DisplayName(), EntityType.DisplayName()));
        }

        var propertyExpressionMap = new Dictionary<IPropertyBase, ColumnExpression>();
        foreach (var (property, columnExpression) in _propertyExpressionMap)
        {
            // maumar:
            // TODO: any other possibilities here?
            var declaringEntityType = (property as IProperty)?.DeclaringEntityType ?? ((INavigation)property).DeclaringEntityType;

            if (derivedType.IsAssignableFrom(declaringEntityType)
                || declaringEntityType.IsAssignableFrom(derivedType))
            {
                propertyExpressionMap[property] = columnExpression;
            }
        }

        var discriminatorExpression = DiscriminatorExpression;
        if (DiscriminatorExpression is CaseExpression caseExpression)
        {
            var entityTypesToSelect = derivedType.GetTptDiscriminatorValues();
            var whenClauses = caseExpression.WhenClauses
                .Where(wc => entityTypesToSelect.Contains((string)((SqlConstantExpression)wc.Result).Value!))
                .ToList();

            discriminatorExpression = caseExpression.Update(operand: null, whenClauses, elseResult: null);
        }

        return new EntityProjectionExpression(derivedType, propertyExpressionMap, discriminatorExpression);
    }

    /// <summary>
    ///     Binds a property with this entity projection to get the SQL representation.
    /// </summary>
    /// <param name="property">A property to bind.</param>
    /// <returns>A column which is a SQL representation of the property.</returns>
    public virtual ColumnExpression BindProperty(IProperty property)
    {
        if (!EntityType.IsAssignableFrom(property.DeclaringEntityType)
            && !property.DeclaringEntityType.IsAssignableFrom(EntityType))
        {
            throw new InvalidOperationException(
                RelationalStrings.UnableToBindMemberToEntityProjection("property", property.Name, EntityType.DisplayName()));
        }

        return _propertyExpressionMap[property];
    }

    /// <summary>
    ///     Adds a navigation binding for this entity projection when the target entity type of the navigation is owned or weak.
    /// </summary>
    /// <param name="navigation">A navigation to add binding for.</param>
    /// <param name="entityShaper">An entity shaper expression for the target type.</param>
    public virtual void AddNavigationBinding(INavigation navigation, EntityShaperExpression entityShaper)
    {
        if (!EntityType.IsAssignableFrom(navigation.DeclaringEntityType)
            && !navigation.DeclaringEntityType.IsAssignableFrom(EntityType))
        {
            throw new InvalidOperationException(
                RelationalStrings.UnableToBindMemberToEntityProjection("navigation", navigation.Name, EntityType.DisplayName()));
        }

        _ownedNavigationMap[navigation] = entityShaper;
    }

    /// <summary>
    ///     Binds a navigation with this entity projection to get entity shaper for the target entity type of the navigation which was
    ///     previously added using <see cref="AddNavigationBinding(INavigation, EntityShaperExpression)" /> method.
    /// </summary>
    /// <param name="navigation">A navigation to bind.</param>
    /// <returns>An entity shaper expression for the target entity type of the navigation.</returns>
    public virtual EntityShaperExpression? BindNavigation(INavigation navigation)
    {
        if (!EntityType.IsAssignableFrom(navigation.DeclaringEntityType)
            && !navigation.DeclaringEntityType.IsAssignableFrom(EntityType))
        {
            throw new InvalidOperationException(
                RelationalStrings.UnableToBindMemberToEntityProjection("navigation", navigation.Name, EntityType.DisplayName()));
        }

        return _ownedNavigationMap.TryGetValue(navigation, out var expression)
            ? expression
            : null;
    }

    /// <inheritdoc />
    public override string ToString()
        => $"EntityProjectionExpression: {EntityType.ShortName()}";
}
