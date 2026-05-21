using AGS.SmartShift.Application.Contracts.Planning;
using MediatR;

namespace AGS.SmartShift.Application.Features.SyncProposals;

public sealed record DismissSyncProposalCommand(Guid ProposalId) : IRequest<SyncProposalDto>;
