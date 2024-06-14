using Google.Protobuf.Reflection;

namespace ArmoniK.Extension.CSharp.Client.Common.Enum;

public enum SortDirection
{
    /// <summary>* Unspecified. Do not use.</summary>
    [OriginalName("SORT_DIRECTION_UNSPECIFIED")]
    Unspecified,

    /// <summary>* Ascending.</summary>
    [OriginalName("SORT_DIRECTION_ASC")] Asc,

    /// <summary>* Descending.</summary>
    [OriginalName("SORT_DIRECTION_DESC")] Desc
}