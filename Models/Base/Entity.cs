using System;
namespace Models.Base
{
    public abstract class Entity
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public DateTime? CreationDate { get; set; } = null;

    }
}
