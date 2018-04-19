using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Microarea.Common
{
    public class ErrorResult : ActionResult
    {

        //
        // Summary:
        //     Gets or set the content representing the body of the response.
        public string Content { get; set; }
        //
        // Summary:
        //     Gets or sets the Content-Type header for the response.
        public string ContentType { get; }
        //
        // Summary:
        //     Gets or sets the HTTP status code.
        public int? StatusCode { get; }

        public ErrorResult(string content = "")
        {
            this.Content = content;

            this.StatusCode = 500;
            this.ContentType = "text/plain";
        }
    }

    public class SuccessResult : ActionResult
    {

        //
        // Summary:
        //     Gets or set the content representing the body of the response.
        public string Content { get; set; }
        //
        // Summary:
        //     Gets or sets the Content-Type header for the response.
        public string ContentType { get; }
        //
        // Summary:
        //     Gets or sets the HTTP status code.
        public int? StatusCode { get; }

        public SuccessResult(string content = "")
        {
            this.Content = content;

            if (string.IsNullOrEmpty(this.Content))
                this.StatusCode = 204;
            else
                this.StatusCode = 200;

            this.ContentType = "application/json";
        }
    }

    public class NoAuthResult : ActionResult
    {

        //
        // Summary:
        //     Gets or set the content representing the body of the response.
        public string Content { get; set; }
        //
        // Summary:
        //     Gets or sets the Content-Type header for the response.
        public string ContentType { get; }
        //
        // Summary:
        //     Gets or sets the HTTP status code.
        public int? StatusCode { get; }

        public NoAuthResult(string content = "")
        {
            this.Content = content;

            this.StatusCode = 401;
            this.ContentType = "application/json";
        }
    }

    public class ForbiddenResult : ActionResult
    {

        //
        // Summary:
        //     Gets or set the content representing the body of the response.
        public string Content { get; set; }
        //
        // Summary:
        //     Gets or sets the Content-Type header for the response.
        public string ContentType { get; }
        //
        // Summary:
        //     Gets or sets the HTTP status code.
        public int? StatusCode { get; }

        public ForbiddenResult(string content = "")
        {
            this.Content = content;

            this.StatusCode = 403;
            this.ContentType = "application/json";
        }
    }

}
