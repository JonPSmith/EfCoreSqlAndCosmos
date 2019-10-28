// Copyright (c) 2019 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore;

namespace Test.CosmosTestDb
{
    [Owned]
    public class CosmosReview
    {
        public string VoterName { get; set; }
        public int NumStars { get; set; }
        public string Comment { get; set; }
    }
}