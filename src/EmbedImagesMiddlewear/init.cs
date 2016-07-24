using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace EmbedImagesMiddlewear
{
    public class EmbedImages
    {
        private readonly IHostingEnvironment env;
        private RequestDelegate _next;
        private Regex regexImgSrc = new Regex(@"<img[^>]*?src\s*=\s*[""']?([^'"" >]+?)[ '""][^>]*?>", RegexOptions.Compiled);

        public EmbedImages(RequestDelegate next, IHostingEnvironment env)
        {
            _next = next;
            this.env = env;
        }

        public async Task Invoke(HttpContext context)
        {
            var acceptEncoding = context.Request.Headers["Accept-Encoding"];
            if (acceptEncoding.ToString().IndexOf("gzip", StringComparison.CurrentCultureIgnoreCase) < 0)
            {
                await _next?.Invoke(context);
                return;
            }

            using (var buffer = new MemoryStream())
            {
                var body = context.Response.Body;
                context.Response.Body = buffer;
                try
                {
                    await _next?.Invoke(context);
                    string bodyText;
                    buffer.Seek(0, SeekOrigin.Begin);
                    using (var reader = new StreamReader(buffer))
                    {
                        bodyText = await reader.ReadToEndAsync();
                    }
                    var urls = bodyText.ToImageLinks();
                    foreach (var url in urls)
                    {
                        var filePath = Path.Combine(env.WebRootPath, url.Insert(0, "."));
                        var file = new FileInfo(filePath);
                        //if (file.Length > this.maxBytes) continue;
                        var contentType = string.Empty;
                        if (new Microsoft.AspNetCore.StaticFiles.FileExtensionContentTypeProvider().TryGetContentType(filePath, out contentType))
                        {
                            var bytes = File.ReadAllBytes(filePath);
                            bodyText = bodyText.Replace(url, $"data:{contentType};base64,{Convert.ToBase64String(bytes)}");
                        }
                    }
                    using (var write = new MemoryStream(Encoding.UTF8.GetBytes(bodyText)))
                    {
                        write.WriteTo(body);
                    }
                }
                finally
                {
                    context.Response.Body = body;
                }
            }
        }
    }

    public static class BuilderExtension
    {
        public static void UseEmbedImages(this IApplicationBuilder app, IHostingEnvironment env)
        {
            app.UseMiddleware<EmbedImages>(env);
        }
    }
}