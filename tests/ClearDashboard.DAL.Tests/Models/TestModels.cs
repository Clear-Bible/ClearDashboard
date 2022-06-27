using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClearDashboard.DAL.Tests.Models
{
    public class Person
    {
        public Guid Id { get; set; }
        public Guid? SyncId { get; set; }
        public string? Name { get; set; }
        public DateTimeOffset Created { get; set; }
    }

    public class GrandParent : Person
    {
        public GrandParent()
        {
            Children = new HashSet<Parent>();
        }
       public virtual ICollection<Parent> Children { get; set; }
    }

    public class Parent : Person
    {
        public Parent()
        {
            Children = new HashSet<Child>();
        }
        public virtual ICollection<Child> Children { get; set; }
    }

    public class Child : Person
    {
        
    }
}
