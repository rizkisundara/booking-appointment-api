using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.Response
{
    public class GlobalResponse<T>
    {
        public string? Message { get; set; }
        public T? Data { get; set; }
        public string? Error { get; set; }

        public static GlobalResponse<T> Success(T data, string? message = null)
        {
            return new GlobalResponse<T>
            {
                Message = message ?? "Success",
                Data = data,
                Error = null
            };
        }

        public static GlobalResponse<T> Failed(string error, string? message = null)
        {
            return new GlobalResponse<T>
            {
                Message = message ?? "Failed",
                Data = default,
                Error = error
            };
        }

        public static GlobalResponse<T> NotFound(string? message = null)
        {
            return new GlobalResponse<T>
            {
                Message = message ?? "Data Not Found",
                Data = default,
                Error = null
            };
        }

        public static GlobalResponse<T> InternalError(string error = "An unexpected error occurred", string? message = null)
        {
            return new GlobalResponse<T>
            {
                Message = message ?? "Internal Server Error",
                Data = default,
                Error = error
            };
        }
    }
}
