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
    
    public partial class Reminders
    {
        public int ReminderID { get; set; }
        public Nullable<int> UserID { get; set; }
        public string ReminderDescription { get; set; }
        public System.DateTime ReminderDate { get; set; }
        public bool IsCompleted { get; set; }
    
        public virtual Users Users { get; set; }
    }
}
