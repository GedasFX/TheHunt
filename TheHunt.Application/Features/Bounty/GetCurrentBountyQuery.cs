// using System;
// using System.Threading;
// using System.Threading.Tasks;
// using Google.Protobuf.WellKnownTypes;
// using MediatR;
//
// namespace TheHunt.Application.Features.Bounty;
//
// public class GetCurrentBountyQueryHandler : IRequestHandler<GetCurrentBountyQuery, BountyDto>
// {
//     public Task<BountyDto> Handle(GetCurrentBountyQuery request, CancellationToken cancellationToken)
//     {
//         return Task.FromResult(new BountyDto
//         {
//             Challenge = "Obtain a Smoke battlestaff",
//             Password = "iban14",
//             Until = Timestamp.FromDateTime(new DateTime(2022, 10, 16, 00, 00, 00, DateTimeKind.Utc)),
//         });
//     }
// }