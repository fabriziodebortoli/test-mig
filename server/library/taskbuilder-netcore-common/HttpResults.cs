using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Microarea.Common
{
    public class ErrorResult : ContentResult
    {
        public ErrorResult(string content = "")
        {
            this.Content = content;

            this.StatusCode = 500;
            this.ContentType = "text/plain";
        }
    }

    public class SuccessResult : ContentResult
    {
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

    public class NoAuthResult : ContentResult
    {
        public NoAuthResult(string content = "")
        {
            this.Content = content;

            this.StatusCode = 401;
            this.ContentType = "application/json";
        }
    }

    public class ForbiddenResult : ContentResult
    {
        public ForbiddenResult(string content = "")
        {
            this.Content = content;

            this.StatusCode = 403;
            this.ContentType = "application/json";
        }
    }

    public class FileNotFoundResult : ContentResult
    {
        public FileNotFoundResult(string content = "")
        {
            this.Content = content;

            this.StatusCode = 404;
            this.ContentType = "application/json";
        }
    }

}
