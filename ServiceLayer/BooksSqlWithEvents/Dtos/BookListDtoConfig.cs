// Copyright (c) 2019 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System;
using System.Linq;
using AutoMapper;
using DataLayerEvents.EfClasses;
using GenericServices.Configuration;

namespace ServiceLayer.BooksSqlWithEvents.Dtos
{
    class BookListDtoConfig : PerDtoConfig<BooksSql.Dtos.BookListDto, BookWithEvents>
    {
        public override Action<IMappingExpression<BookWithEvents, BooksSql.Dtos.BookListDto>> AlterReadMapping
        {
            get
            {
                return cfg => cfg
                    .ForMember(x => x.ReviewsCount, x => x.MapFrom(book => book.Reviews.Count()))
                    .ForMember(x => x.AuthorsOrdered, y => y.MapFrom(p => string.Join(", ",
                        p.AuthorsLink.OrderBy(q => q.Order).Select(q => q.Author.Name).ToList())))
                    .ForMember(x => x.ReviewsAverageVotes,
                        x => x.MapFrom(p => p.Reviews.Select(y => (double?)y.NumStars).Average()));
            }
        }
    }
}