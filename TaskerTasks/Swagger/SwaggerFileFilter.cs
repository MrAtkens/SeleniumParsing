using System;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace TaskerTasks.Swagger
{
    public class SwaggerFileFilter : IOperationFilter  
    {  
        public void Apply(OpenApiOperation operation, OperationFilterContext context)  
        {  
            const string fileUploadMime = "multipart/form-data";  
            if (operation.RequestBody == null || !operation.RequestBody.Content.Any(x => x.Key.Equals(fileUploadMime, StringComparison.InvariantCultureIgnoreCase)))  
                return;  
  
            var fileParams = context.MethodInfo.GetParameters().Where(p => p.ParameterType == typeof(IFormFile));  
            operation.RequestBody.Content[fileUploadMime].Schema.Properties =  
                fileParams.ToDictionary(k => k.Name, v => new OpenApiSchema()  
                {  
                    Type = "string",  
                    Format = "binary"  
                });  
        }  
    }  
}