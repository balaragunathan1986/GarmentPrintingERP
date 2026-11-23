using Erp.Domain.Model;

namespace Erp.Domain.Model.Production
{
    public class MontlyExpensesModel : AuditFields
    {
        public int Id { get; set; }
        public Guid MontlyExpenses_Id { get; set; }
        public string MontlyExpenses_Number { get; set; } = string.Empty;
        public DateTime MontlyExpenses { get; set; }
        public int Company_Id { get; set; }
        public Guid? StyleId { get; set; }
        public Guid? JobOrder_Id { get; set; }
        public string? Remarks { get; set; }
        public List<MontlyExpensesDetailModel> MontlyExpenses_Details { get; set; } = new();
    }

    public class MontlyExpensesDetailModel : AuditFields
    {
        public int Id { get; set; }
        public Guid MontlyExpenses_Details_Id { get; set; }
        public Guid MontlyExpenses_Id { get; set; }
        public Guid Charge_Id { get; set; }
        public string AddLessType { get; set; } = "+";
        public decimal Percentage { get; set; }
        public decimal Amount { get; set; }
        public string? Remarks { get; set; }
    }
}
