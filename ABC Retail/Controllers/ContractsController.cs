using ABC_Retail.Services.Storage;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ABC_Retail.Controllers
{
    public class ContractsController : Controller
    {
        private readonly FileShareStorageService _fileShareService;

        public ContractsController(FileShareStorageService fileShareService)
        {
            _fileShareService = fileShareService;
        }

        // GET: Upload form
        public IActionResult Upload() => View();

        // POST: Upload file
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Upload(IFormFile file)
        {
            if (file != null && file.Length > 0)
            {
                using var stream = file.OpenReadStream();
                await _fileShareService.UploadContractAsync(stream, file.FileName);
            }
            return RedirectToAction(nameof(Index));
        }

        // GET: List all files
        public async Task<IActionResult> Index()
        {
            var files = await _fileShareService.ListContractsAsync();
            return View(files);
        }

        //download file
        public async Task<IActionResult> Download(string fileName)
        {
            if (string.IsNullOrEmpty(fileName))
                return BadRequest("Filename must be provided.");

            var fileBytes = await _fileShareService.DownloadContractAsync(fileName);

            if (fileBytes == null)
                return NotFound();

            return File(fileBytes, "application/octet-stream", fileName);
        }
    }
}
