using Microsoft.AspNetCore.Builder;

namespace Zello.Domain.Abstractions;

public interface ISwaggerService {
    void SaveSwaggerYaml(IApplicationBuilder app, string filePath);
}
