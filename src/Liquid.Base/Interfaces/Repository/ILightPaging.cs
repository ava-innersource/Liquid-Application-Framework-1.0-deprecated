using System.Collections.Generic;

namespace Liquid.Interfaces
{

    /// <summary>
    /// Public interface for pagination features on database queries
    /// </summary>
    public interface ILightPaging<T>
    {
        ICollection<T> Data { get; set; }
        int Page { get; set; }
        int TotalPages { get; set; }
        int TotalCount { get; set; }
        int ItemsPerPage { get; set; }
        string ContinuationToken { get; set; }
    }
}