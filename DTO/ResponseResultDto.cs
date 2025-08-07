using Syncfusion.EJ2.Base;

namespace ProjectManagement.App.DTO
{
    public class ResponseResultDto<T> 
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public T Data { get; set; }
    }

    public class ResponseResultDto
    {
        public bool Success { get; set; }

        public string Message { get; set; } = string.Empty;
    }
}
