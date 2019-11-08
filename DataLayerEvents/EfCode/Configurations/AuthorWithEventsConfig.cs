// Copyright (c) 2019 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using DataLayerEvents.EfClasses;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DataLayerEvents.EfCode.Configurations
{
    public class AuthorWithEventsConfig : IEntityTypeConfiguration<AuthorWithEvents>
    {
        public void Configure (EntityTypeBuilder<AuthorWithEvents> entity)
        {
            entity.HasKey(p => p.AuthorId);
            entity.Property(b => b.Name).HasField("_name");
        }
    }
}