using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Writers;
using Swashbuckle.AspNetCore.Swagger;
using System;
using System.IO;
using Zello.Infrastructure.Interfaces;

namespace Zello.Infrastructure.Services;

public class SwaggerService : ISwaggerService {
    public void SaveSwaggerYaml(IApplicationBuilder app, string filePath) {
        if (string.IsNullOrWhiteSpace(filePath)) {
            throw new ArgumentNullException(nameof(filePath), "File path cannot be null or empty");
        }

        var swaggerProvider = app.ApplicationServices.GetRequiredService<ISwaggerProvider>();
        var swagger = swaggerProvider.GetSwagger("v1");

        var directoryPath = Path.GetDirectoryName(filePath);
        if (directoryPath != null) {
            Directory.CreateDirectory(directoryPath);
        } else {
            throw new ArgumentException("Invalid file path provided", nameof(filePath));
        }

        using var streamWriter = File.CreateText(filePath);
        var yamlWriter = new OpenApiYamlWriter(streamWriter);
        swagger.SerializeAsV3(yamlWriter);
    }
}
