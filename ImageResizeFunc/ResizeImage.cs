using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;

namespace ImageResizeFunc;

public sealed class ResizeImage(ILogger<ResizeImage> logger)
{
    [Function("ResizeImage")]
    [BlobOutput("thumbnails/{name}", Connection = "AzureWebJobsStorage")]
    public async Task<byte[]> Run([BlobTrigger("uploads/{name}", Connection = "AzureWebJobsStorage")] byte[] imageBytes, string name)
    {
        logger.LogInformation("[Triggered] Yeni resim geldi: {Name}", name);
        
        using var inputStream = new MemoryStream(imageBytes);
        
        using var image = await Image.LoadAsync(inputStream);
        
        image.Mutate(x => x.Resize(new ResizeOptions
        {
            Size =  new Size(200, 200),
            Mode = ResizeMode.Max
        }));

        using var outputStream = new MemoryStream();
        await image.SaveAsJpegAsync(outputStream);
        
        logger.LogInformation("[Success] {Name} başarıyla boyutlandırıldı ve thumbnails klasörüne gönderildi.", name);
        
        return outputStream.ToArray();
    }
}