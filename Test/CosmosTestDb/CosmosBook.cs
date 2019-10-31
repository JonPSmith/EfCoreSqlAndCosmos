// Copyright (c) 2019 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;

namespace Test.CosmosTestDb
{
    public class CosmosBook
    {
        public int CosmosBookId { get; set; }
        public string Title { get; set; }
        public double Price { get; set; }
        public DateTime PublishedDate { get; set; }

        //----------------------------------
        //relationships 

        public ICollection<CosmosReview> Reviews { get; set; }
    }
}