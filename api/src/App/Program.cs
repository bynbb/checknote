using Checknote.Api;
using Microsoft.AspNetCore.Builder;

WebApplication app = ChecknoteApi.Create(args);
app.Run();
