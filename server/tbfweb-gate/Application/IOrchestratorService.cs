using System.Collections.Generic;
using TaskBuilderNetCore.Documents.Model;

namespace Microarea.TbfWebGate.Application
{
    public interface IOrchestratorService
    {
        string CloseComponent(CallerContext context);
        string CloseDocument(CallerContext context);
        IEnumerable<string> GetAllComponents();
        IEnumerable<string> GetAllDocuments();
        string GetComponent(CallerContext context);
        string GetDocument(CallerContext context);
    }
}