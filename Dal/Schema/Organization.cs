using Microsoft.EntityFrameworkCore;

namespace GrantTracker.Dal.Schema
{
	public class Organization
	{
		public Guid OrganizationGuid { get; set; }
		public string Name { get; set; }

		public ICollection<OrganizationYear> Years { get; set; }

		public static void Setup(ModelBuilder builder)
		{
			var entity = builder.Entity<Organization>();

			entity.ToTable("Organization", "GTkr")
				.HasComment("Organizations such as sites, a group of administrators, etc.")
				.HasKey(e => e.OrganizationGuid);

			entity.HasIndex(e => e.Name).IsUnique();

			entity.Property(e => e.Name)
				.HasMaxLength(100);
		}
	}
}