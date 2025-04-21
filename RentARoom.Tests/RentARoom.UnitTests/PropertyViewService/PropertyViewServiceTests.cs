using NSubstitute;
using RentARoom.DataAccess.Repository.IRepository;
using RentARoom.Models;
using RentARoom.Models.DTOs;
using RentARoom.Services.IServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace RentARoom.Tests.RentARoom.UnitTests
{
    [Trait("Category", "Unit")]
    public class PropertyViewServiceTests
    {
        [Fact] 
        public void PropertyViewService_GetViewsPerDay_Should_ReturnViewCount()
        {
            // Arrange
            var unitOfWork = Substitute.For<IUnitOfWork>();
            var propertyViewRepository = Substitute.For<IPropertyViewRepository>();

            var propertyId = 1;
            var views = new List<PropertyView>
            {
                new PropertyView { PropertyId = propertyId, ViewedAt = new DateTime(2023, 1, 1) },
                new PropertyView { PropertyId = propertyId, ViewedAt = new DateTime(2023, 1, 1) },
                new PropertyView { PropertyId = propertyId, ViewedAt = new DateTime(2023, 1, 2) },
                new PropertyView { PropertyId = 2, ViewedAt = new DateTime(2023, 1, 1) },
            };

            // Configure the UnitOfWork to return the mocked repository
            unitOfWork.PropertyView.Returns(propertyViewRepository);

            // Configure the repository to return the filtered views using Expression
            propertyViewRepository.Find(Arg.Any<Expression<Func<PropertyView, bool>>>())
                .Returns(views.Where(v => v.PropertyId == propertyId));

            var service = new PropertyViewService(unitOfWork);

            // Act
            var result = service.GetViewsPerDay(propertyId).ToList();

            // Assert
            // Need to cast to PropertyViewStatsDTO to access vars
            Assert.Equal(2, result.Count);
            Assert.Equal("2023-01-01", ((PropertyViewStatsDTO)result[0]).Date);
            Assert.Equal(2, ((PropertyViewStatsDTO)result[0]).Views);
            Assert.Equal("2023-01-02", ((PropertyViewStatsDTO)result[1]).Date);
            Assert.Equal(1, ((PropertyViewStatsDTO)result[1]).Views);
        }

        [Fact]
        public void PropertyViewService_GetViewsPerDay_Should_ReturnEmptyList_WhenInvalidPropertyId()
        {
            // Arrange
            var unitOfWork = Substitute.For<IUnitOfWork>();
            var service = new PropertyViewService(unitOfWork);

            // Act
            var result = service.GetViewsPerDay(0);

            // Assert
            Assert.Empty(result);
        }


        [Fact]
        public async Task PropertyViewService_LogPropertyViewAsync_Should_LogPropertyViewAndReturnTotalViews()
        {
            // Arrange
            var unitOfWork = Substitute.For<IUnitOfWork>();
            var propertyViewRepository = Substitute.For<IPropertyViewRepository>();

            var propertyId = 1;
            var views = new List<PropertyView>
            {
                new PropertyView { PropertyId = propertyId, ViewedAt = new DateTime(2023, 1, 1) },
                new PropertyView { PropertyId = propertyId, ViewedAt = new DateTime(2023, 1, 2) },
            };

            unitOfWork.PropertyView.Returns(propertyViewRepository);

            // Configure Find to return the initial views
            propertyViewRepository.Find(Arg.Any<Expression<Func<PropertyView, bool>>>())
                .Returns(views);

            var service = new PropertyViewService(unitOfWork);

            // Act
            var result = await service.LogPropertyViewAsync(propertyId);

            // Assert
            // Verify Add and Save calls
            unitOfWork.PropertyView.Received(1).Add(Arg.Is<PropertyView>(pv => pv.PropertyId == propertyId));
            unitOfWork.Received(1).Save();

            // Verify the returned count --> doesn't explicitly check the new view added to the db (thats repo functionality)
            Assert.Equal(2, result);
        }

        [Fact]
        public async Task PropertyViewService_LogPropertyViewAsync_Should_ReturnZero_WhenInvalidPropertyId()
        {
            // Arrange
            var unitOfWork = Substitute.For<IUnitOfWork>();
            var service = new PropertyViewService(unitOfWork);

            // Act
            var result = await service.LogPropertyViewAsync(0);

            // Assert
            Assert.Equal(0, result);
        }


    }
}