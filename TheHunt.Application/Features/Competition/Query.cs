using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using HotChocolate;
using HotChocolate.Data;
using HotChocolate.Types;
using TheHunt.Application.Helpers;
using TheHunt.Domain;
using TheHunt.Domain.Models;

namespace TheHunt.Application.Features.Competition;

public class Query
{
    [UsePaging]
    [UseProjection]
    public IQueryable<CompetitionDto> GetCompetitions([Service] AppDbContext context, [Service] IRequestContextAccessor contextAccessor)
    {
        // return context.Competitions;
        return context.Competitions
            .Where(c => c.IsListed || c.Members!.Any(m => m.UserId == contextAccessor.Context.UserId))
            .Select(c => new CompetitionDto
            {
            });
    }

    public record CompetitionDto
    {
        public long Id { get; set; }

        public string Name { get; set; } = null!;
        public string Description { get; set; } = null!;

        public bool IsListed { get; set; }

        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        public ICollection<CompetitionUserDto>? Members { get; set; }


        public record CompetitionUserDto
        {
            [Column("is_admin")]
            public bool IsAdmin { get; set; }

            [Column("is_moderator")]
            public bool IsModerator { get; set; }

            [Column("registration_date")]
            public DateTime RegistrationDate { get; set; }


            // [ForeignKey(nameof(UserId))]
            // public User? User { get; set; }
        }
    }
}