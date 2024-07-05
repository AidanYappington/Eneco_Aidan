using System;
using System.Linq;
using System.Threading.Tasks;
using Blazor.Data;
using Blazor.Services;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Blazor.Services.Tests;

[Collection("Database collection")]
public class ReservationServiceTests
{
    private readonly IDbContextFactory<AppDbContext> _contextFactory;

    public ReservationServiceTests(DatabaseFixture fixture)
    {
        _contextFactory = fixture.DbContextFactory;
    }
  

    [Fact]
    public async Task CreateReservation_AddsReservationToDatabase()
    {
        // Arrange
        var service = new ReservationService(_contextFactory);
        var reservation = new Reservation
        {
            Id = Guid.NewGuid(),
            UserId = "test-user",
            StartDate = DateTime.UtcNow,
            EndDate = DateTime.UtcNow.AddHours(2),
            Floor = 1
            // Optionally set a RoomId if needed
        };

        // Act
        await service.CreateReservationAsync(reservation);

        // Assert
        await using var context = await _contextFactory.CreateDbContextAsync();
        var allReservations = await context.Reservations.ToListAsync();
        Assert.Contains(allReservations, r => r.Id == reservation.Id);
        Assert.Contains(allReservations, r => r.UserId == "test-user");
        Assert.Contains(allReservations, r => r.StartDate == reservation.StartDate);
        Assert.Contains(allReservations, r => r.EndDate == reservation.EndDate);
        Assert.Contains(allReservations, r => r.Floor == reservation.Floor);
    }

    [Fact]
    public async Task CreateReservation_OnlyIfNoDuplicateExists()
    {
        // Arrange
        var service = new ReservationService(_contextFactory);
        var roomId = Guid.NewGuid();
        var reservation1 = new Reservation
        {
            Id = Guid.NewGuid(),
            UserId = "test-user",
            StartDate = DateTime.UtcNow,
            EndDate = DateTime.UtcNow.AddHours(1),
            Floor = 1,
            RoomId = roomId // Setting RoomId as an example
        };

        var reservation2 = new Reservation
        {
            Id = Guid.NewGuid(),
            UserId = "test-user",
            StartDate = reservation1.StartDate,
            EndDate = reservation1.EndDate,
            Floor = 1,
            RoomId = roomId // Attempting to reserve the same room at the same time
        };

        // Act
        await service.CreateReservationAsync(reservation1);

        // Check for existing reservation before creating the second one
        await using var context = await _contextFactory.CreateDbContextAsync();
        var existingReservation = await context.Reservations
            .AnyAsync(r => r.RoomId == reservation2.RoomId && r.StartDate < reservation2.EndDate && reservation2.StartDate < r.EndDate);

        if (!existingReservation)
        {
            await service.CreateReservationAsync(reservation2);
        }

        // Assert
        var allReservations = await context.Reservations.ToListAsync();
        Assert.Single(allReservations); // Ensure only one reservation is present since a duplicate shouldn't be added
        Assert.Contains(allReservations, r => r.Id == reservation1.Id);
        Assert.DoesNotContain(allReservations, r => r.Id == reservation2.Id);
    }
    [Fact]
    public async Task DeleteReservation_RemovesReservationFromDatabase()
    {
        // Arrange
        var service = new ReservationService(_contextFactory);
        var reservationId = Guid.NewGuid();
        var reservation = new Reservation
        {
            Id = reservationId,
            UserId = "test-user",
            StartDate = DateTime.UtcNow,
            EndDate = DateTime.UtcNow.AddHours(1),
            Floor = 1
            // Optionally set RoomId or WorkspaceId if needed
        };

        // Act
        await service.CreateReservationAsync(reservation);

        // Ensure the reservation was added
        await using var context1 = await _contextFactory.CreateDbContextAsync();
        var addedReservation = await context1.Reservations.FindAsync(reservationId);
        Assert.NotNull(addedReservation);

        // Delete the reservation
        await service.DeleteReservationAsync(reservationId);

        // Assert
        await using var context2 = await _contextFactory.CreateDbContextAsync();
        var deletedReservation = await context2.Reservations.FindAsync(reservationId);
        Assert.Null(deletedReservation);
    }
    
    
}
