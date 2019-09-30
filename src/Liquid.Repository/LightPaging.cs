using Liquid.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace Liquid.Repository
{
    public class LightPaging<T> : ILightPaging<T>
    {
        public ICollection<T> Data { get; set; }
        public int Page { get; set; }
        public int TotalPages { get; set; }
        public int TotalCount { get; set; }
        public int ItemsPerPage { get; set; }
        public string ContinuationToken { get; set; }
    }
}
