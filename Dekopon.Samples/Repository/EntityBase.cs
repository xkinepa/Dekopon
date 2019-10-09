using Dekopon.Attributes;

namespace Dekopon.Repository
{
    [Where(Clause = "deleted = 0")]
    [Delete(Set = "deleted = 1")]
    public abstract class EntityBase
    {
        [Key(IsIdentity = true), Generated]
        public long Id { get; set; }
        public int Deleted { get; set; }
    }
}