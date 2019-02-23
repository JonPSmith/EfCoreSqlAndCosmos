// Copyright (c) 2019 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System;
using System.Collections.Immutable;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using DataLayer.EfClassesNoSql;
using DataLayer.EfClassesSql;
using DataLayer.EfCode;
using DataLayer.SqlCode;
using Microsoft.EntityFrameworkCore;

[assembly: InternalsVisibleTo("Test")]
namespace DataLayer.NoSqlCode.Internal
{
    internal class ApplyChangeToNoSql
    {
        private static readonly MapperConfiguration SqlToNoSqlMapper = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<Book, BookListNoSql>()
                .ForMember(p => p.AuthorsOrdered,
                    m => m.MapFrom(s => UdfDefinitions.AuthorsStringUdf(s.BookId)))
                .ForMember(p => p.ReviewsAverageVotes,
                m => m.MapFrom(s => UdfDefinitions.AverageVotesUdf(s.BookId)));
            cfg.CreateMap<BookListNoSql, BookListNoSql>();
        });

        private readonly NoSqlDbContext _noSqlContext;
        private readonly DbContext _sqlContext;

        public ApplyChangeToNoSql(DbContext sqlContext, NoSqlDbContext noSqlContext)
        {
            _sqlContext = sqlContext ?? throw new ArgumentNullException(nameof(sqlContext));
            _noSqlContext = noSqlContext ?? throw new ArgumentNullException(nameof(noSqlContext)); ;
        }

        public bool UpdateNoSql(IImmutableList<BookChangeInfo> booksToUpdate)
        {
            if (_noSqlContext == null || !booksToUpdate.Any()) return false;

            foreach (var bookToUpdate in booksToUpdate)
            {
                switch (bookToUpdate.State)
                {
                    case EntityState.Deleted:
                    {
                        var noSqlBook = _noSqlContext.Find<BookListNoSql>(bookToUpdate.BookId);
                        _noSqlContext.Remove(noSqlBook);
                        break;
                    }
                    case EntityState.Modified:
                    {
                        //Note: You need to read the actual Cosmos entity because of the extra columns like id, _rid, etc.
                        //Version 3 might make attach work https://github.com/aspnet/EntityFrameworkCore/issues/13633
                        var noSqlBook = _noSqlContext.Find<BookListNoSql>(bookToUpdate.BookId);
                        var update = _sqlContext.Set<Book>()
                            .ProjectTo<BookListNoSql>(SqlToNoSqlMapper)
                            .Single(x => x.BookId == bookToUpdate.BookId);
                        SqlToNoSqlMapper.CreateMapper().Map(update, noSqlBook);
                        break;
                    }
                    case EntityState.Added:
                        var newBook = _sqlContext.Set<Book>()
                            .ProjectTo<BookListNoSql>(SqlToNoSqlMapper)
                            .Single(x => x.BookId == bookToUpdate.BookId);
                        _noSqlContext.Add(newBook);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            return true;
        }

        public async Task<bool> UpdateNoSqlAsync(IImmutableList<BookChangeInfo> booksToUpdate)
        {
            if (_noSqlContext == null || !booksToUpdate.Any()) return false;

            foreach (var bookToUpdate in booksToUpdate)
            {
                switch (bookToUpdate.State)
                {
                    case EntityState.Deleted:
                    {
                        var noSqlBook = await _noSqlContext.FindAsync<BookListNoSql>(bookToUpdate.BookId);
                        _noSqlContext.Remove(noSqlBook);
                        break;
                    }
                    case EntityState.Modified:
                    {
                        //Note: You need to read the actual Cosmos entity because of the extra columns like id, _rid, etc.
                        //Version 3 might make attach work https://github.com/aspnet/EntityFrameworkCore/issues/13633
                        var noSqlBook = await _noSqlContext.FindAsync<BookListNoSql>(bookToUpdate.BookId);
                        var update = await _sqlContext.Set<Book>()
                            .ProjectTo<BookListNoSql>(SqlToNoSqlMapper)
                            .SingleAsync(x => x.BookId == bookToUpdate.BookId);
                        SqlToNoSqlMapper.CreateMapper().Map(update, noSqlBook);
                        break;
                    }
                    case EntityState.Added:
                        var newBook = await _sqlContext.Set<Book>()
                            .ProjectTo<BookListNoSql>(SqlToNoSqlMapper)
                            .SingleAsync(x => x.BookId == bookToUpdate.BookId);
                        _noSqlContext.Add(newBook);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            return true;
        }
    }
}