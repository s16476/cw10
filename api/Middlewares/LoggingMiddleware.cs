using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace api.Middlewares
{
    public class LoggingMiddleware
    {

        private readonly RequestDelegate _next;
        public LoggingMiddleware(RequestDelegate next)
        {
            _next = next;
        }
        public async Task InvokeAsync(HttpContext context)
        {
            context.Request.EnableBuffering();
            if (context != null)
            {
                string path = context.Request.Path;
                string method = context.Request.Method;
                string queryString = context.Request.QueryString.ToString();
                string bodyStr = "";

                using (var reader = new StreamReader(context.Request.Body, Encoding.UTF8, true, 1024, true))
                {
                    bodyStr = await reader.ReadToEndAsync();
                    context.Request.Body.Position = 0;
                }

                using (StreamWriter writetext = File.AppendText("requestsLog.txt"))
                {

                    writetext.WriteLine("TIME: " + DateTime.Now.ToString());
                    writetext.WriteLine("PATH: " + path);
                    writetext.WriteLine("METHOD: " + method);
                    writetext.WriteLine("QUERY: " + queryString);
                    writetext.WriteLine("BODY: " + bodyStr);
                    writetext.WriteLine("==================================");
                }
            }

            //Our code
            await _next(context);
        }

    }
}
