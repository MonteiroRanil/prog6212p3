using System.IO;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using WebApplication1.Controllers;
using WebApplication1.Data;
using WebApplication1.Models;
using WebApplication1.ViewModels;
using Xunit;

namespace WebApplication1.Tests
{
    public class LecturerControllerTests
    {
        private ApplicationDbContext GetInMemoryDbContext()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            return new ApplicationDbContext(options);
        }

        private LecturerController GetController(ApplicationDbContext context, ApplicationUser user)
        {
            var mockUserManager = new Mock<UserManager<ApplicationUser>>(
                Mock.Of<IUserStore<ApplicationUser>>(), null, null, null, null, null, null, null, null);

            mockUserManager.Setup(um => um.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
                           .ReturnsAsync(user);

            var controller = new LecturerController(mockUserManager.Object, context);

            var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id)
            }, "mock"));

            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = claimsPrincipal }
            };

            return controller;
        }

        [Fact]
        public async Task SubmitClaim_CalculatesTotalAmountCorrectly()
        {
            var context = GetInMemoryDbContext();
            var user = new ApplicationUser { Id = "user1", HourlyRate = 100, FirstName = "Test", LastName = "User" };
            var controller = GetController(context, user);

            var model = new CreateClaimViewModel
            {
                HoursWorked = 5,
                Notes = "Test note",
                Documents = new List<IFormFile>() // empty list is fine
            };

            await controller.SubmitClaim(model);

            var claim = await context.LecturerClaims.FirstOrDefaultAsync();
            Assert.NotNull(claim);
            Assert.Equal(500, claim.TotalAmount);
            Assert.Equal("Test note", claim.Notes);
        }

        [Fact]
        public async Task SubmitClaim_SavesMultipleDocuments()
        {
            var context = GetInMemoryDbContext();
            var user = new ApplicationUser { Id = "user1", HourlyRate = 100, FirstName = "Test", LastName = "User" };
            var controller = GetController(context, user);

            // Mock multiple files
            var files = new List<IFormFile>();
            for (int i = 0; i < 2; i++)
            {
                var fileMock = new Mock<IFormFile>();
                var content = $"Dummy content {i}";
                var fileName = $"test{i}.pdf";
                var ms = new MemoryStream();
                var writer = new StreamWriter(ms);
                writer.Write(content);
                writer.Flush();
                ms.Position = 0;

                fileMock.Setup(f => f.OpenReadStream()).Returns(ms);
                fileMock.Setup(f => f.FileName).Returns(fileName);
                fileMock.Setup(f => f.Length).Returns(ms.Length);
                fileMock.Setup(f => f.ContentType).Returns("application/pdf");

                files.Add(fileMock.Object);
            }

            var model = new CreateClaimViewModel
            {
                HoursWorked = 2,
                Notes = "With multiple documents",
                Documents = files
            };

            await controller.SubmitClaim(model);

            var documents = await context.ClaimDocuments.ToListAsync();
            Assert.Equal(2, documents.Count);
            Assert.Contains(documents, d => d.FileName == "test0.pdf");
            Assert.Contains(documents, d => d.FileName == "test1.pdf");
        }
    }
}
