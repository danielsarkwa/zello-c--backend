using Microsoft.AspNetCore.Builder;

namespace Zello.Infrastructure.Interfaces;

public interface ISwaggerService {
    void SaveSwaggerYaml(IApplicationBuilder app, string filePath);
}
