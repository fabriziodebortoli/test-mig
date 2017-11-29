using Microarea.AdminServer.Controllers.Helpers;
using Microarea.AdminServer.Controllers.Helpers.All;
using Microarea.Common;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Microarea.AdminServer.Controllers
{
	//================================================================================
	[Consumes("application/json", otherContentTypes: "multipart/form-data")]
	public class TBFSController : Controller
    {
		IHostingEnvironment _env;
		AppOptions _settings;
		IJsonHelper jsonHelper;

		//-----------------------------------------------------------------------------	
		public TBFSController(IHostingEnvironment env, IOptions<AppOptions> settings, IJsonHelper jsonHelper)
		{
			this._env = env;
			this._settings = settings.Value;
			this.jsonHelper = jsonHelper;
		}

		[HttpPost("api/tbfs/init")]
		//-----------------------------------------------------------------------------	
		public async Task<IActionResult> ApiTBFSInit(IFormFile file)
		{
			if (file == null || file.Length == 0)
				return Content("file not selected");

			string uploadSubFolder = this._settings.TBFS.UploadFolder;
			string uploadFolder = Path.Combine(Directory.GetCurrentDirectory(), uploadSubFolder);

			try
			{
				if (!Directory.Exists(uploadFolder))
				{
					Directory.CreateDirectory(uploadFolder);
				}

				var path = Path.Combine(
							Directory.GetCurrentDirectory(), uploadSubFolder,
							file.FileName);

				using (var stream = new FileStream(path, FileMode.Create))
				{
					await file.CopyToAsync(stream);
				}
			}
			catch 
			{
				this.jsonHelper.AddJsonCouple<bool>("result", false);
				return new ContentResult { Content = jsonHelper.WriteFromKeysAndClear(), StatusCode = 500, ContentType = "application/json" };
			}

			this.jsonHelper.AddJsonCouple<bool>("result", true);
			this.jsonHelper.AddJsonCouple<string>("message", String.Format("File {0} has been uploaded", file.FileName));
			return new ContentResult { Content = jsonHelper.WriteFromKeysAndClear(), StatusCode = 200, ContentType = "application/json" };
		}

        [HttpPost("api/tbfs/create")]
        //-----------------------------------------------------------------------------	
        public IActionResult ApiTBFSCreate()
        {
            try
            {
                MetaDataManagerTool metadata = new MetaDataManagerTool("I-M4");
                metadata.InsertAllStandardMetaDataInDB();
            }
            catch (Exception e)
            {
                this.jsonHelper.AddJsonCouple<bool>("result", false);
                this.jsonHelper.AddJsonCouple<string>("message", String.Format("An error occurred: {0}", e.Message));
                return new ContentResult { Content = jsonHelper.WriteFromKeysAndClear(), StatusCode = 500, ContentType = "application/json" };
            }

            this.jsonHelper.AddJsonCouple<bool>("result", true);
            this.jsonHelper.AddJsonCouple<string>("message", "Test ended");
            return new ContentResult { Content = jsonHelper.WriteFromKeysAndClear(), StatusCode = 200, ContentType = "application/json" };

        }

        [HttpPost("api/tbfs/test")]
		//-----------------------------------------------------------------------------	
		public IActionResult ApiTBFSTest()
		{
			try
			{

			}
			catch (Exception e)
			{
				this.jsonHelper.AddJsonCouple<bool>("result", false);
				this.jsonHelper.AddJsonCouple<string>("message", String.Format("An error occurred: {0}", e.Message));
				return new ContentResult { Content = jsonHelper.WriteFromKeysAndClear(), StatusCode = 500, ContentType = "application/json" };
			}

			this.jsonHelper.AddJsonCouple<bool>("result", true);
			this.jsonHelper.AddJsonCouple<string>("message", "Test ended");
			return new ContentResult { Content = jsonHelper.WriteFromKeysAndClear(), StatusCode = 200, ContentType = "application/json" };
		}
	}

}
