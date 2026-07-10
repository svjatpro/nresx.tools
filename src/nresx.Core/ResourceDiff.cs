using System.Collections.Generic;
using System.Linq;

namespace nresx.Core;

/// <summary>
/// The key-based difference between two resource files (or two element sets): which keys were added,
/// removed, or changed. Elements present in both sides with equal values are omitted. Comparison is by
/// <see cref="ResourceElement.Key"/>; when a side has duplicate keys, the first occurrence wins.
/// </summary>
public class ResourceDiff
{
    /// <summary>Elements whose key exists in the second side but not the first.</summary>
    public IReadOnlyList<ResourceElement> AddedElements { get; }

    /// <summary>Elements whose key exists in the first side but not the second.</summary>
    public IReadOnlyList<ResourceElement> RemovedElements { get; }

    /// <summary>Elements whose key exists on both sides but whose <see cref="ResourceElement.Value"/> differs, paired (first, second).</summary>
    public IReadOnlyList<(ResourceElement First, ResourceElement Second)> ChangedElements { get; }

    /// <summary>True when there are no added, removed, or changed elements - the two sides are equivalent by key and value.</summary>
    public bool AreSame => AddedElements.Count == 0 && RemovedElements.Count == 0 && ChangedElements.Count == 0;

    private ResourceDiff(
        IReadOnlyList<ResourceElement> added,
        IReadOnlyList<ResourceElement> removed,
        IReadOnlyList<(ResourceElement First, ResourceElement Second)> changed )
    {
        AddedElements = added;
        RemovedElements = removed;
        ChangedElements = changed;
    }

    /// <summary>
    /// Compares two element sets by key. Added = in <paramref name="second"/> only; Removed = in
    /// <paramref name="first"/> only; Changed = same key, different value. Order follows the source
    /// documents (added in second's order, removed/changed in first's order).
    /// </summary>
    public static ResourceDiff Compare( IEnumerable<ResourceElement> first, IEnumerable<ResourceElement> second )
    {
        var firstList = first?.ToList() ?? new List<ResourceElement>();
        var secondList = second?.ToList() ?? new List<ResourceElement>();

        var firstByKey = FirstOccurrenceByKey( firstList );
        var secondByKey = FirstOccurrenceByKey( secondList );

        var added = new List<ResourceElement>();
        var seenAdded = new HashSet<string>();
        foreach ( var e in secondList )
        {
            var key = e.Key ?? string.Empty;
            if ( !firstByKey.ContainsKey( key ) && seenAdded.Add( key ) )
                added.Add( e );
        }

        var removed = new List<ResourceElement>();
        var seenRemoved = new HashSet<string>();
        var changed = new List<(ResourceElement First, ResourceElement Second)>();
        var seenChanged = new HashSet<string>();
        foreach ( var e in firstList )
        {
            var key = e.Key ?? string.Empty;
            if ( !secondByKey.TryGetValue( key, out var other ) )
            {
                if ( seenRemoved.Add( key ) )
                    removed.Add( e );
            }
            else if ( e.Value != other.Value && seenChanged.Add( key ) )
            {
                changed.Add( (e, other) );
            }
        }

        return new ResourceDiff( added, removed, changed );
    }

    private static Dictionary<string, ResourceElement> FirstOccurrenceByKey( IEnumerable<ResourceElement> elements )
    {
        var map = new Dictionary<string, ResourceElement>();
        foreach ( var e in elements )
        {
            var key = e.Key ?? string.Empty;
            if ( !map.ContainsKey( key ) )
                map[key] = e;
        }
        return map;
    }
}
