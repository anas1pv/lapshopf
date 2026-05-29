using System;

namespace lapshop.Domains
{
    public class TbItemEvaluation
    {
        public int EvaluationId { get; set; }
        public int ItemId { get; set; }
        public string CustomerName { get; set; } = null!;
        public string CustomerEmail { get; set; } = null!;
        public int Rating { get; set; } // 1 to 5
        public string ReviewText { get; set; } = null!;
        public DateTime CreatedDate { get; set; }

        public virtual TbItem Item { get; set; } = null!;
    }
}
