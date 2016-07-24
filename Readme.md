This is a simple aspnet middlewear that converts your small images to base64 encoded data tags in html.

I wouldn't actually recommend doing this, I mostly did this to see what is possible in aspnet core middlwear. There is much [evidence](http://davidbcalhoun.com/2011/when-to-base64-encode-images-and-when-not-to/) to suggest that it is a bad idea

How to use:

```csharp
public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {

            app.UseEmbedImages(env);

        }

```

## Before

`<img src="/path/to/img.jpg" />`

## After

`<img src="data:img/jpeg;base64,=randomdatahere" />`