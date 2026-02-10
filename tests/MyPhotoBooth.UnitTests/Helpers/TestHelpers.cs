using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Moq;
using MyPhotoBooth.Application.Common.Requests;
using MyPhotoBooth.Application.Interfaces;

namespace MyPhotoBooth.UnitTests.Helpers;

public static class TestHelpers
{
    public static Mock<IPhotoRepository> CreatePhotoRepositoryMock()
    {
        return new Mock<IPhotoRepository>();
    }

    public static Mock<IFileStorageService> CreateFileStorageServiceMock()
    {
        var mock = new Mock<IFileStorageService>();
        mock.Setup(x => x.BuildStoragePath(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>()))
            .Returns((string userId, string filename, bool isThumbnail)
                => $"/storage/{userId}/{(isThumbnail ? "thumbs" : "original")}/{filename}");
        return mock;
    }

    public static Mock<IImageProcessingService> CreateImageProcessingServiceMock(
        bool isValidImage = true,
        int width = 1920,
        int height = 1080)
    {
        var mock = new Mock<IImageProcessingService>();
        mock.Setup(x => x.IsValidImageFile(It.IsAny<Stream>(), It.IsAny<string>()))
            .Returns(isValidImage);
        mock.Setup(x => x.ProcessImageAsync(It.IsAny<Stream>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ProcessedImageResult(
                new MemoryStream(),
                new MemoryStream(),
                width,
                height,
                null));
        return mock;
    }

    public static FormFile CreateTestFormFile(string fileName = "test.jpg", long size = 1024, string contentType = "image/jpeg")
    {
        return new FormFile(new MemoryStream(new byte[size]), 0, size, "file", fileName)
        {
            Headers = new HeaderDictionary(),
            ContentType = contentType
        };
    }

    public static Stream CreateTestImageStream(int sizeInBytes = 1024)
    {
        return new MemoryStream(new byte[sizeInBytes]);
    }

    public static IConfiguration CreateConfiguration(Dictionary<string, string?>? settings = null)
    {
        var mock = new Mock<IConfiguration>();
        var mockSection = new Mock<IConfigurationSection>();

        var defaultSettings = new Dictionary<string, string?>
        {
            ["StorageSettings:MaxFileSizeMB"] = "50",
            ["JwtSettings:AccessTokenExpirationMinutes"] = "15"
        };

        if (settings != null)
        {
            foreach (var setting in settings)
            {
                defaultSettings[setting.Key] = setting.Value;
            }
        }

        mock.Setup(x => x["StorageSettings:MaxFileSizeMB"]).Returns(defaultSettings["StorageSettings:MaxFileSizeMB"]);
        mock.Setup(x => x["JwtSettings:AccessTokenExpirationMinutes"]).Returns(defaultSettings["JwtSettings:AccessTokenExpirationMinutes"]);

        return mock.Object;
    }
}
