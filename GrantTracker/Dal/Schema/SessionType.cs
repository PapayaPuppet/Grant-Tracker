﻿using GrantTracker.Dal.Models.Views;
using Microsoft.EntityFrameworkCore;

namespace GrantTracker.Dal.Schema
{
	//Consider deriving all schema classes from something that requires setup, then all of the dropdown options by yet another subclass
	public class SessionType : DropdownOption
	{
		public virtual ICollection<Session> Sessions { get; set; }

		public static void Setup(ModelBuilder builder)
		{
			var entity = builder.Entity<SessionType>();

			entity.ToTable("SessionType", "GTkr")
				.HasComment("Lookup table for session types.")
				.HasKey(e => e.Guid);

			entity.HasIndex(e => e.Label)
				.IsUnique();

			/// /Relations

			entity.HasMany(e => e.Sessions)
				.WithOne(e => e.SessionType)
				.HasForeignKey(e => e.SessionTypeGuid);

			/// /Properties

			entity.Property(e => e.Guid)
				.IsRequired()
				.HasColumnName("SessionTypeGuid")
				.HasColumnType("uniqueidentifier")
				.HasDefaultValueSql("NEWID()");

			entity.Property(e => e.Abbreviation)
				.HasColumnType("nvarchar")
				.HasMaxLength(10)
				.HasComment("Abbreviation of the label for use in frontend dropdowns.");

			entity.Property(e => e.Label)
				.IsRequired()
				.HasColumnType("nvarchar")
				.HasMaxLength(50)
				.HasComment("Short textual description of the type for use in frontend dropdowns.");

			entity.Property(e => e.Description)
				.HasColumnType("nvarchar")
				.HasMaxLength(400)
				.HasComment("Extended description of the type for future use and ensuring the type is well explained in the event it's label is unhelpful.");

            DropdownOption.Setup(entity);
        }
	}
}