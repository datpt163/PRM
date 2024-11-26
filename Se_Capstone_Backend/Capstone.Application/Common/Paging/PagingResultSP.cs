using Microsoft.EntityFrameworkCore;


namespace Capstone.Application.Common.Paging
{
    public class PagingResultSP<T> : List<T>
    {
        public PagingSP? Paging { get; set; }
        public IList<T> Data { get; set; }
        public string ErrorMessage { get; set; } = string.Empty;
        public int Count { get; set; } 

        public PagingResultSP(List<Module.Projects.Response.ProjectDTO> projects)
        {
        }

        public PagingResultSP(IList<T> data, int totalCount, int pageIndex, int pageSize)
        {
            Paging = new PagingSP(totalCount, pageIndex, pageSize);
            Data = data;
        }

        public PagingResultSP(IEnumerable<T> data, int totalCount, int pageIndex, int pageSize, bool isList)
        {
            Paging = new PagingSP(totalCount, pageIndex, pageSize);
            Data = data.ToList();
        }

        public PagingResultSP()
        {
        }

        //public static async Task<PagingResultSP<T>> CreateAsyncSP(IQueryable<T> query, int pageIndex, int pageSize)
        //{
        //    var items = await query.ToListAsync();
        //    var count = items.FirstOrDefault()?.TotalRows ?? 0;

        //    return new PagingResultSP<T>(items, count, pageIndex, pageSize);
        //}

        public static async Task<PagingResultSP<T>> CreateAsyncLinq(IQueryable<T> query, int totalRow, int pageIndex, int pageSize)
        {
            var items = await query.ToListAsync();

            return new PagingResultSP<T>(items, totalRow, pageIndex, pageSize);
        }
    }
}
