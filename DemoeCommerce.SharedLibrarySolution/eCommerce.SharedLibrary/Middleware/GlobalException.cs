using eCommerce.SharedLibrary.Logs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualBasic;
using System.Net;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text.Json;

namespace eCommerce.SharedLibrary.Middleware
{
    public class GlobalException(RequestDelegate next)
    {
        public async Task InvokeAsync(HttpContext context)
        {
            string message = "sorry, Internal Server Error Occured.Kindly Try Again";
            int statusCode = (int)HttpStatusCode.InternalServerError;
            string title = "Error";

            try
            {
                await next(context);

                if (context.Response.StatusCode == StatusCodes.Status429TooManyRequests)
                {
                    title = "Warning";
                    message = "Too Many Request Made.";
                    statusCode = (int)StatusCodes.Status429TooManyRequests;
                    await ModifyHeader(context, title, message, statusCode);

                }

                if (context.Response.StatusCode == StatusCodes.Status401Unauthorized)
                {
                    title = "Alert";
                    message = "You Are Not Authorized to access.";
                    statusCode = (int)StatusCodes.Status401Unauthorized;
                    await ModifyHeader(context, title, message, statusCode);

                }

                if (context.Response.StatusCode == StatusCodes.Status403Forbidden)
                {
                    title = "Out Of Access";
                    message = "You Are Not Allowed/required to access.";
                    statusCode = (int)StatusCodes.Status403Forbidden;
                    await ModifyHeader(context, title, message, statusCode);

                }
            }
            catch (Exception ex)
            {
                LogException.LogExceptions(ex);

                if(ex is TaskCanceledException || ex is TimeoutException)
                {
                    title = "Out Of Time";
                    message = "Request Time Out....Try Again";
                    statusCode = StatusCodes.Status408RequestTimeout;
                }

               await ModifyHeader(context, title, message,statusCode);
            }


        }
        private static async Task ModifyHeader(HttpContext context, string title, string message, int statusCode)
        {
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsync(JsonSerializer.Serialize(new ProblemDetails()
            {
                Detail = message,
                Status = statusCode,
                Title = title

            }), CancellationToken.None);

            return;
        }
    }
}
