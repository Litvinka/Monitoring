//------------------------------------------------------------------------------
// <auto-generated>
//     Этот код создан по шаблону.
//
//     Изменения, вносимые в этот файл вручную, могут привести к непредвиденной работе приложения.
//     Изменения, вносимые в этот файл вручную, будут перезаписаны при повторном создании кода.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Monitoring.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class message
    {
        public int id { get; set; }
        public string title { get; set; }
        public string text { get; set; }
        public int recipient_id { get; set; }
        public int author_id { get; set; }
        public Nullable<System.DateTime> date_create { get; set; }
        public int state_id { get; set; }
    
        public virtual system_event_state system_event_state { get; set; }
        public virtual User User { get; set; }
        public virtual User User1 { get; set; }
    }
}
