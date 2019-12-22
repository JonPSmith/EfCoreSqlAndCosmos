// Copyright (c) 2019 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System;
using System.Linq;
using AutoMapper;
using DataLayerEvents.EfClasses;
using GenericServices.Configuration;

namespace ServiceLayer.BooksSqlWithEvents.Dtos
{
    class DeleteBookDtoConfig : PerDtoConfig<BooksSql.Dtos.DeleteBookDto, BookWithEvents>
    {
        public override Action<IMappingExpression<BookWithEvents, BooksSql.Dtos.DeleteBookDto>> AlterReadMapping
        {
            get
            {
                return cfg => cfg
                    .ForMember(x => x.AuthorsOrdered, y => y.MapFrom(p => string.Join(", ",
                        p.AuthorsLink.OrderBy(q => q.Order).Select(q => q.Author.Name).ToList())));
            }
        }
    }
}