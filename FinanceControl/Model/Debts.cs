//------------------------------------------------------------------------------
// <auto-generated>
//     Этот код создан по шаблону.
//
//     Изменения, вносимые в этот файл вручную, могут привести к непредвиденной работе приложения.
//     Изменения, вносимые в этот файл вручную, будут перезаписаны при повторном создании кода.
// </auto-generated>
//------------------------------------------------------------------------------

namespace FinanceControl.Model
{
    using System;
    using System.Collections.Generic;
    
    public partial class Debts
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Debts()
        {
            this.DebtsTransactions = new HashSet<DebtsTransactions>();
        }
    
        public int DebtID { get; set; }
        public Nullable<int> UserID { get; set; }
        public decimal Amount { get; set; }
        public string ToWho { get; set; }
        public string Status { get; set; }
        public string Comment { get; set; }
    
        public virtual Users Users { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<DebtsTransactions> DebtsTransactions { get; set; }
    }
}
