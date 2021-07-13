using System;
using System.Threading.Tasks;
using Catalog.Api.Controllers;
using Catalog.Api.DTOs;
using Catalog.Api.Entities;
using Catalog.Api.Repositories;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Catalog.UnitTests
{
  public class ItemsControllerTests
  {

    private readonly Mock<IItemsRepository> _repositoryStub = new();
    private readonly Mock<ILogger<ItemsController>> _loggerStub = new();
    private readonly Random _rand = new();

    [Fact]
    public async Task GetItemAsync_WithUnexistingItem_ReturnsNotFound()
    {
      // Arrange
      _repositoryStub.Setup(repo => repo.GetItemAsync(It.IsAny<Guid>()))
        .ReturnsAsync((Item)null);

      var controller = new ItemsController(_repositoryStub.Object, _loggerStub.Object);

      // Act
      var result = await controller.GetItemAsync(Guid.NewGuid());

      // Assert
      result.Result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task GetItemAsync_WithExistingItem_ReturnsExpectedItem()
    {
      // Arrange
      var expectedItem = CreateRandomItem();
      _repositoryStub.Setup(repo => repo.GetItemAsync(It.IsAny<Guid>()))
        .ReturnsAsync(expectedItem);

      var controller = new ItemsController(_repositoryStub.Object, _loggerStub.Object);

      // Act
      var result = await controller.GetItemAsync(Guid.NewGuid());

      // Assert
      result.Value.Should().BeEquivalentTo(
        expectedItem,
        options => options.ComparingByMembers<Item>());
    }

    [Fact]
    public async Task GetItemsAsync_WithExistingItems_ReturnsAllItems()
    {
      // Arrange
      var expectedItems = new[] { CreateRandomItem(), CreateRandomItem(), CreateRandomItem() };

      _repositoryStub.Setup(repo => repo.GetItemsAsync())
        .ReturnsAsync(expectedItems);

      var controller = new ItemsController(_repositoryStub.Object, _loggerStub.Object);

      // Act
      var actualItems = await controller.GetItemsAsync();

      // Assert
      actualItems.Should().BeEquivalentTo(
        expectedItems,
        options => options.ComparingByMembers<Item>()
      );
    }

    [Fact]
    public async Task CreateItemAsync_WithItemToCreate_ReturnsCreatedItem()
    {
      // Arrange
      var itemToCreate = new CreateItemDto()
      {
        Name = Guid.NewGuid().ToString(),
        Price = _rand.Next(1000),
      };

      var controller = new ItemsController(_repositoryStub.Object, _loggerStub.Object);

      // Act
      var result = await controller.CreateItemAsync(itemToCreate);

      // Assert
      var createdItem = (result.Result as CreatedAtActionResult).Value as ItemDto;
      itemToCreate.Should().BeEquivalentTo(
        createdItem,
        options => options.ComparingByMembers<ItemDto>().ExcludingMissingMembers()
      );
      createdItem.Id.Should().NotBeEmpty();
      createdItem.CreatedDate.Should().BeCloseTo(DateTimeOffset.UtcNow, 1000);
    }


    private Item CreateRandomItem()
    {
      return new()
      {
        Id = Guid.NewGuid(),
        Name = Guid.NewGuid().ToString(),
        Price = _rand.Next(1000),
        CreatedDate = DateTimeOffset.UtcNow
      };
    }
  }
}
