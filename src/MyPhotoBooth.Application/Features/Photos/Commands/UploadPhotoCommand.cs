using MediatR;
using Microsoft.AspNetCore.Http;
using MyPhotoBooth.Application.Common.DTOs;
using MyPhotoBooth.Application.Common.Requests;

namespace MyPhotoBooth.Application.Features.Photos.Commands;

public record UploadPhotoCommand(
    IFormFile File,
    string? Description,
    string UserId
) : ICommand<PhotoUploadResponse>;
