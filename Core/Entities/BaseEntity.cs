namespace Core.Entities
{
    public abstract class BaseEntity<TId>
    {
        public TId Id { get; protected set; } = default!;
        public DateTime CreatedAt { get; protected set; } = DateTime.Now;
        public DateTime? UpdatedAt { get; protected set; }

        protected void UpdateTimestamps()
        {
            UpdatedAt = DateTime.Now;
        }
    }
}
