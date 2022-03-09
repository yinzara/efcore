// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;

namespace Microsoft.EntityFrameworkCore.Query;


/// <summary>
/// TODO
/// </summary>
public class JsonEntityExpression : SqlExpression
{
    private readonly List<string> _jsonPath;

    private readonly Dictionary<INavigation, RelationalEntityShaperExpression> _ownedNavigationMap = new();

    /// <summary>
    /// TODO
    /// </summary>
    public virtual IReadOnlyDictionary<IProperty, SqlExpression> KeyPropertyExpressionMap { get; }

    /// <summary>
    /// TODO
    /// </summary>
    public virtual ColumnExpression JsonColumn { get; }

    /// <summary>
    /// TODO
    /// </summary>
    public virtual IEntityType EntityType { get; }

    /// <summary>
    /// TODO
    /// </summary>
    public JsonEntityExpression(
        ColumnExpression jsonColumn,
        IEntityType entityType,
        Type type,
        RelationalTypeMapping? typeMapping,
        IReadOnlyDictionary<IProperty, SqlExpression> keyPropertyExpressionMap)
        : this(jsonColumn, entityType, type, typeMapping, keyPropertyExpressionMap, new List<string>())
    {
    }

    /// <summary>
    /// TODO
    /// </summary>
    public JsonEntityExpression(
        ColumnExpression jsonColumn,
        IEntityType entityType,
        Type type,
        RelationalTypeMapping? typeMapping,
        IReadOnlyDictionary<IProperty, SqlExpression> keyPropertyExpressionMap,
        List<string> jsonPath)
        : base(type, typeMapping)
    {
        JsonColumn = jsonColumn;
        EntityType = entityType;
        _jsonPath = jsonPath;
        KeyPropertyExpressionMap = keyPropertyExpressionMap;
    }

    /// <summary>
    /// TODO
    /// </summary>
    public RelationalEntityShaperExpression BindNavigation(INavigation navigation)
    {
        if (_ownedNavigationMap.TryGetValue(navigation, out var result))
        {
            return result;
        }

        var pathSegment = navigation.Name;
        var entityType = navigation.TargetEntityType;

        var newPath = _jsonPath.ToList();
        newPath.Add(pathSegment);

        var newKeyPropertyExpressionMap = new Dictionary<IProperty, SqlExpression>();

        var primaryKey = entityType.FindPrimaryKey();
        if (primaryKey == null || primaryKey.Properties.Count != KeyPropertyExpressionMap.Count)
        {
            throw new InvalidOperationException("shouldnt happen");
        }

        // TODO: do this properly this is sooooo hacky rn
        var oldValues = KeyPropertyExpressionMap.Values.ToList();
        for (var i = 0; i < primaryKey.Properties.Count; i++)
        {
            newKeyPropertyExpressionMap[primaryKey.Properties[i]] = oldValues[i];
        }

        var jsonEntityExpression = new JsonEntityExpression(JsonColumn, entityType, Type, TypeMapping, newKeyPropertyExpressionMap, newPath);

        return new RelationalEntityShaperExpression(entityType, jsonEntityExpression, nullable: true);
    }

    /// <summary>
    /// TODO
    /// </summary>
    public void AddNavigationBinding(INavigation navigation, RelationalEntityShaperExpression entityShaperExpression)
    {
        _ownedNavigationMap[navigation] = entityShaperExpression;
    }

    /// <summary>
    /// TODO
    /// </summary>
    public SqlExpression BindProperty(IProperty property)
    {
        if (KeyPropertyExpressionMap.TryGetValue(property, out var keyMapping))
        {
            return keyMapping;
        }

        // maumar - make this nicer
        var jsonPath = string.Join(".", _jsonPath);
        if (!string.IsNullOrEmpty(jsonPath))
        {
            jsonPath = "$." + jsonPath;
        }
        else
        {
            jsonPath = "$";
        }


        jsonPath += "." + property.Name;

        // TODO: make into constant rather than fragment later
        // also this is sql server specific
        // also make it into jsonpath access expression or some such
        return new SqlUnaryExpression(
            ExpressionType.Convert,
            new SqlFunctionExpression(
                "JSON_VALUE",
                new SqlExpression[] { JsonColumn, new SqlFragmentExpression("'" + jsonPath + "'") },
                true,
                new bool[] { true, false },
                property.ClrType, null),
            property.ClrType,
            null);
    }

    /// <summary>
    /// TODO
    /// </summary>
    public IReadOnlyList<string> GetPath()
        => _jsonPath.AsReadOnly();

    /// <inheritdoc />
    protected override Expression VisitChildren(ExpressionVisitor visitor)
        => this;

    /// <summary>
    /// TODO
    /// </summary>
    protected override void Print(ExpressionPrinter expressionPrinter)
    {
        expressionPrinter.Append("JsonEntityExpression(entity: " + EntityType.Name + "  Path: " + string.Join(".", _jsonPath) + ")");
    }

    /// <inheritdoc />
    public override bool Equals(object? obj)
    {
        if (obj is JsonEntityExpression jsonEntityExpression)
        {
            var result = true;
            result = result && JsonColumn.Equals(jsonEntityExpression.JsonColumn);
            result = result && _jsonPath.Count == jsonEntityExpression._jsonPath.Count;

            if (result)
            {
                result = result && _jsonPath.Zip(jsonEntityExpression._jsonPath, (l, r) => l == r).All(x => true);
            }

            return result;
        }
        else
        {
            return false;
        }
    }

    /// <inheritdoc />
    public override int GetHashCode()
        => base.GetHashCode(); // TODO
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
    private readonly IReadOnlyDictionary<IPropertyBase, SqlExpression> _propertyExpressionMap;
    private readonly Dictionary<INavigation, EntityShaperExpression> _ownedNavigationMap = new();
    private readonly IReadOnlyDictionary<IPropertyBase, SqlExpression> _jsonPropertyPathMap;

    /// <summary>
    ///     Creates a new instance of the <see cref="EntityProjectionExpression" /> class.
    /// </summary>
    /// <param name="entityType">The entity type to shape.</param>
    /// <param name="propertyExpressionMap">A dictionary of column expressions corresponding to properties (or in some cases navigations) of the entity type.</param>
    /// <param name="discriminatorExpression">A <see cref="SqlExpression" /> to generate discriminator for each concrete entity type in hierarchy.</param>
    public EntityProjectionExpression(
        IEntityType entityType,
        IReadOnlyDictionary<IPropertyBase, SqlExpression> propertyExpressionMap,
        SqlExpression? discriminatorExpression = null)
    {
        EntityType = entityType;
        _propertyExpressionMap = propertyExpressionMap;
        DiscriminatorExpression = discriminatorExpression;

        _jsonPropertyPathMap = new Dictionary<IPropertyBase, SqlExpression>();
    }

    ///// <summary>
    /////     TODO
    ///// </summary>
    //public EntityProjectionExpression(
    //    IEntityType entityType,
    //    ColumnExpression jsonColumn,
    //    IReadOnlyDictionary<IPropertyBase, ColumnExpression> propertyExpressionMap,
    //    IReadOnlyDictionary<IPropertyBase, SqlExpression> jsonPropertyPathMap)
    //{
    //    EntityType = entityType;
    //    _propertyExpressionMap = propertyExpressionMap;
    //    DiscriminatorExpression = null;

    //    JsonColumn = jsonColumn;
    //    _jsonPropertyPathMap = jsonPropertyPathMap;
    //}

    /// <summary>
    ///     The entity type being projected out.
    /// </summary>
    public virtual IEntityType EntityType { get; }

    /// <summary>
    ///     A <see cref="SqlExpression" /> to generate discriminator for entity type.
    /// </summary>
    public virtual SqlExpression? DiscriminatorExpression { get; }

    /// <summary>
    ///     TODO
    /// </summary>
    public virtual ColumnExpression? JsonColumn { get; }

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
        var propertyExpressionMap = new Dictionary<IPropertyBase, SqlExpression>();
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
        var propertyExpressionMap = new Dictionary<IPropertyBase, SqlExpression>();
        foreach (var (property, sqlExpression) in _propertyExpressionMap)
        {
            // maumar: fix this for json (i.e. when the sql expression is not a column
            var nullable = (sqlExpression as ColumnExpression)?.MakeNullable() ?? sqlExpression;
            propertyExpressionMap[property] = nullable;
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

        var propertyExpressionMap = new Dictionary<IPropertyBase, SqlExpression>();
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
    public virtual ColumnExpression BindKeyProperty(IProperty property)
    {
        if (!EntityType.IsAssignableFrom(property.DeclaringEntityType)
            && !property.DeclaringEntityType.IsAssignableFrom(EntityType))
        {
            throw new InvalidOperationException(
                RelationalStrings.UnableToBindMemberToEntityProjection("property", property.Name, EntityType.DisplayName()));
        }

        return (ColumnExpression)_propertyExpressionMap[property];
    }

    /// <summary>
    ///     TODO
    /// </summary>
    public virtual SqlExpression BindProperty2(IProperty property)
    {
        if (!EntityType.IsAssignableFrom(property.DeclaringEntityType)
            && !property.DeclaringEntityType.IsAssignableFrom(EntityType))
        {
            throw new InvalidOperationException(
                RelationalStrings.UnableToBindMemberToEntityProjection("property", property.Name, EntityType.DisplayName()));
        }

        if (_jsonPropertyPathMap.ContainsKey(property))
        {
            return JsonColumn!;
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
