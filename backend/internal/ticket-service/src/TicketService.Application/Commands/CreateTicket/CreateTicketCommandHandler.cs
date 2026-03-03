using TicketService.Application.Exceptions;
using TicketService.Application.Interfaces;
using TicketService.Application.Models;
using TicketService.Domain.Entities;

namespace TicketService.Application.Commands.CreateTicket;

public sealed class CreateTicketCommandHandler
{
    private readonly ITicketRepository _ticketRepository;
    private readonly ITicketUnitOfWork _ticketUnitOfWork;

    public CreateTicketCommandHandler(
        ITicketRepository ticketRepository,
        ITicketUnitOfWork ticketUnitOfWork)
    {
        _ticketRepository = ticketRepository;
        _ticketUnitOfWork = ticketUnitOfWork;
    }

    public async Task<CreateTicketResult> Handle(
        CreateTicketCommand command,
        CancellationToken cancellationToken = default)
    {
        Validate(command);

        var ticket = new Ticket(
            Guid.NewGuid(),
            command.FullName,
            command.Email,
            command.BirthDate,
            command.PhoneNumber,
            command.IdentityDocumentFileName,
            command.DriverLicenseFileName,
            command.AvatarUrl,
            DateTime.UtcNow);

        await _ticketRepository.AddAsync(ticket, cancellationToken);
        await _ticketUnitOfWork.SaveChangesAsync(cancellationToken);

        return new CreateTicketResult(ticket.ToDto());
    }

    private static void Validate(CreateTicketCommand command)
    {
        if (string.IsNullOrWhiteSpace(command.FullName))
        {
            throw new ValidationException("Full name is required.");
        }

        if (string.IsNullOrWhiteSpace(command.Email))
        {
            throw new ValidationException("Email is required.");
        }

        if (command.BirthDate == default)
        {
            throw new ValidationException("Birth date is required.");
        }

        if (command.BirthDate > DateOnly.FromDateTime(DateTime.UtcNow))
        {
            throw new ValidationException("Birth date cannot be in the future.");
        }

        if (string.IsNullOrWhiteSpace(command.PhoneNumber))
        {
            throw new ValidationException("Phone number is required.");
        }
    }
}
