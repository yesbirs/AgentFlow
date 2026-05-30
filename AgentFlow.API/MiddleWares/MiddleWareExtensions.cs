namespace AgentFlow.API.MiddleWares
{
    public static class MiddleWareExtensions
    {
        public static void UseGlobalExceptionHandler(this IApplicationBuilder app)
        {
            app.UseMiddleware<GlobalExceptionMiddleWare>();
        }
    }
}