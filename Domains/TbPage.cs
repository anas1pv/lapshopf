using System;

namespace lapshop.Domains
{
    public class TbPage
    {
        public int PageId { get; set; }
        public string Title { get; set; } = null!;
        public string Description { get; set; } = null!;
        public string? MetaKeyWord { get; set; }
        public string? MetaDescriptiuon { get; set; }
        public string? ImageName { get; set; }
        public int CurrentState { get; set; }
        public DateTime? CreatedDate { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public string? UpdatedBy { get; set; }
    }
}
