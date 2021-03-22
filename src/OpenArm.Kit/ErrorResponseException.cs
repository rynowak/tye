using System;

namespace OpenArm
{
    public class ErrorResponseException : Exception
    {
        public int HttpStatusCode { get; }
        public ExtendedError Error { get; }

        public ErrorResponseException(int httpStatusCode, ExtendedError error, Exception? innerException = null)
            : base(error.Message, innerException)
        {
            HttpStatusCode = httpStatusCode;
            Error = error;
        }

        public ErrorResponseException(int httpStatusCode, string code, string message, Exception? innerException = null)
            : this(httpStatusCode, new ExtendedError(){ Code = code, Message = message, }, innerException)
        {
        }

        public static ErrorResponseException BadRequest(string code, string message)
            => new ErrorResponseException(400, code, message);
    }
}
