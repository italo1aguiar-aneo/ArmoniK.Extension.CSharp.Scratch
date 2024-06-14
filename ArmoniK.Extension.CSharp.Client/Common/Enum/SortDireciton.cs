using Google.Protobuf.Reflection;

namespace ArmoniK.Extension.CSharp.Client.Common.Enum;

/// <summary>
///     Defines the directions in which sorting can be applied.
/// </summary>
public enum SortDirection
{
    /// <summary>
    ///     Unspecified sort direction. This value should not be used as it does not represent a valid sorting order.
    /// </summary>
    [OriginalName("SORT_DIRECTION_UNSPECIFIED")]
    Unspecified,

    /// <summary>
    ///     Ascending sort order where elements progress from the lowest value to the highest.
    /// </summary>
    [OriginalName("SORT_DIRECTION_ASC")] Asc,

    /// <summary>
    ///     Descending sort order where elements progress from the highest value to the lowest.
    /// </summary>
    [OriginalName("SORT_DIRECTION_DESC")] Desc
}