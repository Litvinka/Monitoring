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
    
    public partial class education__institution
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public education__institution()
        {
            this.curators_and_controlers = new HashSet<curators_and_controlers>();
            this.experts = new HashSet<experts>();
        }
    
        public int id { get; set; }
        public string full_name { get; set; }
        public string short_name { get; set; }
        public Nullable<int> audit_object_id { get; set; }
        public int district_id { get; set; }
        public string address { get; set; }
        public string email { get; set; }
        public string phone { get; set; }
        public Nullable<int> type_edu_id { get; set; }
        public Nullable<int> kind_edu_id { get; set; }
        public string OKPO { get; set; }
        public string UNP { get; set; }
        public string director { get; set; }
        public Nullable<int> is_application { get; set; }
        public Nullable<int> type_education_institution_id { get; set; }
        public Nullable<int> ownership_type_id { get; set; }
        public Nullable<int> department_subordination_id { get; set; }
        public Nullable<int> state_id { get; set; }
        public string UNP_superior_management { get; set; }
    
        public virtual audit_object audit_object { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<curators_and_controlers> curators_and_controlers { get; set; }
        public virtual department_subordination department_subordination { get; set; }
        public virtual district district { get; set; }
        public virtual kind_edu kind_edu { get; set; }
        public virtual ownership_type ownership_type { get; set; }
        public virtual state_edu state_edu { get; set; }
        public virtual type_edu type_edu { get; set; }
        public virtual type_education_institution type_education_institution { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<experts> experts { get; set; }
    }
}
