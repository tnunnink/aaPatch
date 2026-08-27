using aaPatch.Model;

namespace aaPatch.Extensions;

/// <summary>
/// Provides extension methods for working with collections of <see cref="ObjectData"/>.
/// Includes methods for filtering, projection, patching, and querying object data collections.
/// </summary>
public static class ObjectExtensions
{
    /// <summary>
    /// Provides extension methods for working with collections of <see cref="ObjectData"/>.
    /// Enables filtering, projection, patching, and querying operations on object data collections.
    /// </summary>
    /// <param name="data">The collection of ObjectData to extend with additional operations.</param>
    extension(IEnumerable<ObjectData> data)
    {
        /// <summary>
        /// Filters a collection of ObjectData based on the specified ObjectExpression.
        /// </summary>
        /// <param name="expression">
        /// The filtering criteria provided as an ObjectExpression.
        /// When null, no filtering is applied, and the original collection is returned.
        /// </param>
        /// <returns>
        /// Returns a filtered IEnumerable of ObjectData based on the provided ObjectExpression.
        /// If the expression is null, the original collection is returned.
        /// </returns>
        public IEnumerable<ObjectData> Filter(ObjectExpression? expression)
        {
            if (expression is null)
            {
                return data;
            }

            var predicate = expression.Compile<ObjectData, bool>();
            return data.Where(predicate);
        }

        /// <summary>
        /// Filters the collection of ObjectData, returning only those objects that contain the specified attributes.
        /// </summary>
        /// <param name="attributes">
        /// A read-only collection of attribute names to filter by. Only objects that have all of these attributes
        /// will be included in the resulting collection.
        /// </param>
        /// <returns>
        /// Returns a filtered IEnumerable of ObjectData containing only the objects that have all the specified attributes.
        /// </returns>
        public IEnumerable<ObjectData> Having(IReadOnlyCollection<string> attributes)
        {
            foreach (var item in data)
            {
                if (item.Has(attributes))
                    yield return item;
            }
        }

        /// <summary>
        /// Applies a collection of ObjectPatch instances to the current set of ObjectData.
        /// Each ObjectPatch modifies the attributes of ObjectData based on the defined rules in the patch.
        /// </summary>
        /// <param name="patches">
        /// A collection of ObjectPatch instances representing the changes to be applied to the attributes
        /// of the ObjectData objects.
        /// </param>
        /// <returns>
        /// Returns an IEnumerable of ObjectData with the applied patches. Each ObjectData in the result is updated
        /// according to the rules specified by the provided patches.
        /// </returns>
        public IEnumerable<ObjectData> Patch(IReadOnlyCollection<ObjectPatch> patches)
        {
            foreach (var item in data)
            {
                var results = item.Select(x => patches.Aggregate(x, (a, p) => p.Apply(a)));
                yield return new ObjectData(results);
            }
        }

        /// <summary>
        /// Projects a collection of ObjectData by applying the specified projections
        /// to transform the attributes of each object in the collection.
        /// </summary>
        /// <param name="projections">
        /// A read-only collection of ObjectProjection instances used to transform
        /// the attributes of each ObjectData in the collection.
        /// </param>
        /// <returns>
        /// Returns a collection of ObjectData where each object is transformed
        /// based on the provided projections.
        /// </returns>
        public IEnumerable<ObjectData> Project(IReadOnlyCollection<ObjectProjection> projections)
        {
            foreach (var item in data)
            {
                var attributes = projections.SelectMany(p => p.Project(item));
                yield return new ObjectData(attributes);
            }
        }
    }
}