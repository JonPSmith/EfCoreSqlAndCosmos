// Copyright (c) 2019 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System;
using System.ComponentModel.DataAnnotations;

namespace DataLayer.EfClassesNoSql
{
    public class BookNoSql
    {
        private const string IdStart = "booklist/";

        [Key]
        public string Id
        {
            get { return CreateIdString(); }
            set { SetBookIdFromId(value); }
        }

        public int BookId { get; set; }
        public string Title { get; set; }
        public DateTime PublishedOn { get; set; }
        public decimal Price { get; set; }
        public decimal ActualPrice { get; set; }
        public string PromotionPromotionalText { get; set; }
        public string AuthorsOrdered { get; set; }
        public int ReviewsCount { get; set; }
        public double? ReviewsAverageVotes { get; set; }


        private void SetBookIdFromId(string id)
        {
            BookId = int.Parse(id.Substring(IdStart.Length));
        }

        //Note: to allow orderby it needs to be in format D10, i.e. has leading zeros
        private string CreateIdString()
        {
            return IdStart + BookId.ToString("D10");
        }
    }
}